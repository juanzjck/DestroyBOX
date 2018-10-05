using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Sounder;

public class SounderEffect : ScriptableObject 
{
	AudioSource audioSource = null;

	[SerializeField]
	int sampleRate = 48000;
		
	/// <summary>The wave table to use. Is the base timbre of the sound.</summary>
	public Oscillator.WaveForm waveForm;

	/// <summary>If true use the amplitude curve to generate the sound. Else regenerates the curve from AmplitudeData</summary>
	public bool amplitudeManualCurve = false;
	/// <summary>If true use the frequency curve to generate the sound. Else regenerates the curve from FrequencyData</summary>
	public bool frequencyManualCurve = false;

	/// <summary>Amplitude of the flanger. Flanger is an offset copy of the waveform. Suggested values are between .0f and 1.0f</summary>
	public float flangerAmplitude = .0f;
	/// <summary>Starting offset of the flanger.</summary>
	public float flangerOffset = .0f;
	/// <summary>Increase the offset of the flanger by this much per second</summary>
	public float flangerOffsetDelta = .0f;
		
	/// <summary>Waveform of the Low Frequency Oscillator.</summary>
	public Oscillator.WaveForm LFOForm;
	/// <summary>Amplitude of the low frequency Oscillator</summary>
	public float LFOAmplitude = .0f;
	/// <summary>Frequency of the low frequency Oscillator</summary>
	public float LFOFrequency = 3.0f;

	/// <summary>Low pass filter. Values higher than this are attenuated.</summary>
	public float lowPass = 0;
	/// <summary>Resonance of the low and high pass filters. Higher values make the filters more aggressive</summary>
	public float Q = 1.0f;
	/// <summary>High pass filter. Values lower than this are attenuated.</summary>
	public float highPass = 0;

	/// <summary>Delay in seconds before starting the sound effect.</summary>
	public float delay = .0f;

	/// <summary>Resets most variables to their starting values after this much time has passed. If zero or negative does nothing.</summary>
	public float phaseReset = .0f;

	/// <summary>Echoes the sound every echoTime seconds</summary>
	public float echoTime = .0f;
	/// <summary>How loud the echo is as a percent of the original sound</summary>
	public float echoValue = .5f;
	/// <summary>How many times to echo. If less than 0 echoes until very quiet.</summary>
	public int echoCount = -1;

	public int EchoCount 
	{ 
		get 
		{
			if(echoCount > -1)
				return echoCount;
			if(echoTime < float.Epsilon || echoValue < float.Epsilon)
				return 0;
			int c = 0;
			float v = echoValue;
			while(v > .05f && c < 20)
			{
				v *= echoValue;
				c++;
			}
			return c; 
		} 
	}
		
	[SerializeField]
	/// <summary>Stores the Attack, Decay, Sustain, and Realease envelope. Unused if amplitudeManualCurve == true</summary>
	public AmplitudeData AmplitudeData = new AmplitudeData();
	[SerializeField]
	/// <summary>Stores the data that is used to create the frequency curve. Unused if frequencyManualCurve == true</summary>
	public FrequencyData FrequencyData = new FrequencyData();

	[SerializeField]
	AnimationCurve amplitudeAniCurve;
	[SerializeField]
	AnimationCurve frequencyAniCurve;

	/// <summary>Any effects in this list are compiled into the sound effect on build. Unexpected results may occur if the effects have different sample rates. Will infinitely loop on build if there is recursion</summary>
	public List<SounderEffect> SubEffects = new List<SounderEffect>();

	public bool ContainsSub(SounderEffect test)
	{
		if(SubEffects == null)
			return false;
		if(SubEffects.Contains(test))
			return true;
		for(int iii = 0; iii < SubEffects.Count; iii++)
			if(SubEffects[iii] != null && SubEffects[iii].ContainsSub(test))
				return true;
		return false;
	}
	
	/// <summary>Animation Curve used as the volume envelope.</summary>
	public AnimationCurve AmplitudeCurve
	{
		get
		{ 
			if(amplitudeAniCurve == null || amplitudeAniCurve.length < 2)
				amplitudeAniCurve = AmplitudeData.GetCurve();
			return amplitudeAniCurve; 
		}
		set { amplitudeAniCurve = value; }
	}
	
	/// <summary>Animation Curve that represents the frequency over time.</summary>
	public AnimationCurve FrequencyCurve
	{
		get
		{
			if(frequencyAniCurve == null || frequencyAniCurve.length < 2)
				frequencyAniCurve = FrequencyData.GetCurve(Duration);
			return frequencyAniCurve;
		}
		set { frequencyAniCurve = value; }
	}

	public float Duration
	{
		get
		{
			float longestSub = -1.0f;
			for(int iii = 0; SubEffects != null && iii < SubEffects.Count; iii++)
				if(SubEffects[iii] != null && SubEffects[iii].Duration > longestSub)
					longestSub = SubEffects[iii].Duration;

			float t = CoreDuration + echoTime * EchoCount;
			return t > longestSub ? t : longestSub;
		}
	}
	float CoreDuration
	{
		get
		{
			float t;
			if(!amplitudeManualCurve)
				t = AmplitudeData.Duration;
			else
				t = AmplitudeCurve.keys[AmplitudeCurve.keys.Length -1].time;
			t += delay;
			return t;
		}
	}

#if UNITY_EDITOR
	[System.NonSerialized]
	public float LastBuildTime = .0f;
#endif
	/// <summary>How many samples per second. Lower values produce sounds that are muddier but much faster to generate</summary>
	public int SampleRate { get { return sampleRate; } set { sampleRate = Mathf.Clamp(value, 20, 50000); } }
	/// <summary>How long the Effect is in samples.</summary>
	public int SampleLength { get { return Mathf.CeilToInt(Duration * SampleRate); } }
	/// <summary>How long the Effect is in seconds.</summary>
	public float SecondsLength { get { if(AmplitudeCurve.keys.Length < 1) return .0f; return AmplitudeCurve.keys[AmplitudeCurve.keys.Length -1].time; } }

	[System.NonSerialized]
	Filter highFilter = null;
	[System.NonSerialized]
	Filter lowFilter = null;
	[System.NonSerialized]
	AudioClip audioClip = null;

	/// <summary>Copies the data from other into this Effect</summary>
	public void Copy(SounderEffect other)
	{
		name = other.name;

		SampleRate = other.SampleRate;
		waveForm = other.waveForm;

		amplitudeManualCurve = other.amplitudeManualCurve;
		frequencyManualCurve = other.frequencyManualCurve;

		flangerAmplitude = other.flangerAmplitude;
		flangerOffset = other.flangerOffset;
		flangerOffsetDelta = other.flangerOffsetDelta;

		LFOForm = other.LFOForm;
		LFOAmplitude = other.LFOAmplitude;
		LFOFrequency = other.LFOFrequency;

		delay = other.delay;

		echoTime = other.echoTime;
		echoValue = other.echoValue;
		echoCount = other.echoCount;

		lowPass = other.lowPass;
		Q = other.Q;
		highPass = other.highPass;

		AmplitudeData.Copy(other.AmplitudeData);
		FrequencyData.Copy(other.FrequencyData);

		if(amplitudeManualCurve)
			amplitudeAniCurve = new AnimationCurve(other.amplitudeAniCurve.keys);
		else
			amplitudeAniCurve = null;

		if(frequencyManualCurve)
			frequencyAniCurve = new AnimationCurve(other.frequencyAniCurve.keys);
		else
			frequencyAniCurve = null;
	}

	/// <summary>Plays the effect using the Sounder.Player audio source object pool. Uses the cached AudioClip if it's already been built, else builds it.</summary>
	/// <param name="volume">Volume to play the sound at. Between 0 and 1</param>
	/// <param name="looping"> If looping is true will repeat until Stop is called on this SounderEffect or on the AudioSource returned.</param>
	/// <returns>Returns the audio source from the object pool.</returns>
	public AudioSource Play(float volume, bool looping)
	{
		if(audioClip == null)
			MakeClip();
		audioSource = Player.PlayClip(audioClip, looping, volume);
		return audioSource;
	}
	/// <summary>Plays the effect using the Sounder.Player audio source object pool. Uses the cached AudioClip if it's already been built, else builds it.</summary>
	/// <param name="looping"> If looping is true will repeat until Stop is called on this SounderEffect or on the AudioSource returned.</param>
	/// <returns>Returns the audio source from the object pool.</returns>
	public AudioSource Play(bool looping = false)
	{
		if(audioClip == null)
			MakeClip();
		audioSource = Player.PlayClip(audioClip, looping, 1.0f);
		return audioSource;
	}
	
	/// <summary>If the sound is playing stops it.</summary>
	/// <returns>Returns the sample the audioSource was on when stopped or -1 if it was not playing.</returns>
	public int Stop()
	{
		if(audioSource == null)
			return 0;
		
		audioSource.Stop();
		return audioSource.timeSamples;
	}

	void Process(float[] samples, int offset, bool loop)
	{
		for(int iii = 0; SubEffects != null && iii < SubEffects.Count; iii++)
		{
			if(SubEffects[iii] == null)
				continue;
			SubEffects[iii].Process(samples, offset, loop);
		}

		float dur = Duration;

		SetupFilters();
			
		double samplePeriod = 1.0f / SampleRate;
			
		bool flange = flangerAmplitude > .0f && (Math.Abs(flangerOffset) > .0f || Math.Abs(flangerOffsetDelta) > .0f);
		bool phaseEchoing = phaseReset > float.Epsilon;
		bool started = delay < (offset * samplePeriod) + float.Epsilon;
		bool echoing = (echoTime > float.Epsilon) && (echoValue > float.Epsilon);
		bool lfo = LFOAmplitude > float.Epsilon;

		double time = .0f;

		float[] lfoTable = Oscillator.GetWaveTable(LFOForm);
		float[] waveTable = Oscillator.GetWaveTable(waveForm);
			
		float value = .0f;

		double phase = .0f;
		double frequency = .0f;

		double amplitudeTime = offset * samplePeriod;

		float coreDuration = CoreDuration;

		for(int iii = 0; iii < samples.Length && iii < coreDuration * SampleRate; iii++)
		{
				
			if(amplitudeTime > dur)
			{
				if(loop)
					amplitudeTime = dur;
				else
					break;
			}
			amplitudeTime += samplePeriod;

			if(!started)
			{
				if(amplitudeTime < delay)
					continue;
				amplitudeTime -= delay;
				started = true;
			}

			time += samplePeriod;
				
			value = AmplitudeCurve.Evaluate((float)amplitudeTime);
			frequency = FrequencyCurve.Evaluate((float)time);
				
			phase = time * Oscillator.tableResolution;

			uint waveIndex = (uint)(phase * frequency);

			float sample = waveTable[waveIndex % waveTable.Length];

			if(phaseEchoing)
			{
				if(time > phaseReset)
					time = .0f;
			}
				
			if(flange)
			{
				double foff = flangerOffsetDelta * time + flangerOffset;
				waveIndex += (uint)System.Math.Round(foff * waveTable.Length);
				sample += waveTable[waveIndex % waveTable.Length] * flangerAmplitude;
			}
				
			if(lfo)
			{
				uint lfoIndex = (uint)(phase * LFOFrequency);
				float temp = -lfoTable[lfoIndex % lfoTable.Length];
				temp += 1.0f;
				temp *= .5f;
				temp = 1.0f - temp * LFOAmplitude;
				sample *= temp;
			}

			samples[iii] += sample * value;
				
			if(lowFilter != null)
				lowFilter.FilterValue(ref samples[iii]);
			if(highFilter != null)
				highFilter.FilterValue(ref samples[iii]);
				
			samples[iii] = Mathf.Clamp(samples[iii], -1.0f, 1.0f);
		}

		if(echoing)
		{
			float evolume = echoValue;
			for(int eee = 1; eee < (EchoCount + 1); eee++)
			{
				int echoIndex = (int)(echoTime * eee * (float)sampleRate);
				int max = (int)(echoIndex + coreDuration * SampleRate);
					
				for(int iii = echoIndex; iii < max && iii < samples.Length; iii++)
				{
					samples[iii] += samples[iii - echoIndex] * evolume;	
				}

				evolume *= echoValue;
			}
		}
	}
	void SetupFilters()
	{
		if(lowPass == 0 && lowFilter != null)
			lowFilter = null;
		if(highPass == 0 && highFilter != null)
			highFilter = null;
		if(lowPass > 0 && lowFilter == null)
			lowFilter = new Filter();
		if(highPass > 0 && highFilter == null)
			highFilter = new Filter();
		if(lowFilter != null)
			lowFilter.RebuildIfNecessary(Filter.PassType.Low, lowPass, Q, sampleRate);
		if(highFilter != null)
			highFilter.RebuildIfNecessary(Filter.PassType.High, highPass, Q, sampleRate);
	}

	/// <summary>Creates and returns an AudioClip for the Effect</summary>
	public AudioClip MakeClip()
	{
		if(!amplitudeManualCurve)
			amplitudeAniCurve = null;
		if(!frequencyManualCurve)
			frequencyAniCurve = null;

		lowFilter = null;
		highFilter = null;

		float[] samples = new float[SampleLength];
				
#if UNITY_EDITOR
		float timeToBuild = Time.realtimeSinceStartup;
		Process(samples, 0, false);
		LastBuildTime = Time.realtimeSinceStartup - timeToBuild;
#else
		Process(samples, 0, false);
#endif
		
		AudioClip clip = AudioClip.Create("Sounder", samples.Length, 1, SampleRate, false);

		clip.SetData(samples, 0);

#if UNITY_EDITOR
		if(!Application.isPlaying)
			clip.hideFlags = HideFlags.HideAndDontSave;
#endif

		if(audioClip != null)
		{
#if UNITY_EDITOR
			if(!UnityEditor.AssetDatabase.Contains(audioClip))
				DestroyImmediate(audioClip);
#else
			Destroy(audioClip,audioClip.length); //in case it's being played
#endif
		}
		audioClip = clip;

		return audioClip;
	}

	/// <summary>Returns the AudioClip for this Effect, creating one if it hasn't already been built.</summary>
	public AudioClip GetClip()
	{
		if(audioClip == null)
			MakeClip();
		return audioClip;
	}
}
