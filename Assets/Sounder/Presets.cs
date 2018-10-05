using UnityEngine;
using System.Collections;

namespace Sounder
{
	public static class Presets 
	{
		static Oscillator.WaveForm GetRandomWaveForm()
		{
			return GetRandomWaveForm(Oscillator.WaveForm.Flow, Oscillator.WaveForm.HarmonicSaw, Oscillator.WaveForm.HarmonicSquare, Oscillator.WaveForm.HarmonicTriangle, Oscillator.WaveForm.Noise, Oscillator.WaveForm.Roll, Oscillator.WaveForm.Sawtooth, Oscillator.WaveForm.Sine, Oscillator.WaveForm.Square, Oscillator.WaveForm.Triangle);
		}
		static Oscillator.WaveForm GetRandomWaveForm(params Oscillator.WaveForm[] list)
		{
			int index = Random.Range(0, list.Length);
			return list[index];
		}
		static FrequencyData.CurveType GetRandomCurve()
		{
			return GetRandomCurve(FrequencyData.CurveType.Hook, FrequencyData.CurveType.Linear, FrequencyData.CurveType.Slope, FrequencyData.CurveType.Smooth, FrequencyData.CurveType.Square);
		}
		static FrequencyData.CurveType GetRandomCurve(params FrequencyData.CurveType[] list)
		{
			int index = Random.Range(0, list.Length);
			return list[index];
		}
		static float RandomMulti(float min, float max, int rolls)
		{
			float ret = .0f;
			for(int iii = 0; iii < rolls; iii++)
				ret += Random.Range(min, max);
			return ret;
		}
		static bool RandomChance(float percent)
		{
			return Random.Range(.0f, 1.0f) < percent;
		}
		static void Round(SounderEffect e, int decimalPlaces)
		{
			e.flangerAmplitude = (float)System.Math.Round(e.flangerAmplitude, decimalPlaces);
			e.flangerOffset = (float)System.Math.Round(e.flangerOffset, decimalPlaces);
			e.flangerOffsetDelta = (float)System.Math.Round(e.flangerOffsetDelta, decimalPlaces);
			e.LFOAmplitude = (float)System.Math.Round(e.LFOAmplitude, decimalPlaces);
			e.LFOFrequency = (float)System.Math.Round(e.LFOFrequency, decimalPlaces);
			e.Q = (float)System.Math.Round(e.Q, decimalPlaces);

			e.AmplitudeData.attackAmplitude = (float)System.Math.Round(e.AmplitudeData.attackAmplitude, decimalPlaces);
			e.AmplitudeData.attackTime = (float)System.Math.Round(e.AmplitudeData.attackTime, decimalPlaces);
			e.AmplitudeData.decayTime = (float)System.Math.Round(e.AmplitudeData.decayTime, decimalPlaces);
			e.AmplitudeData.sustainAmplitude = (float)System.Math.Round(e.AmplitudeData.sustainAmplitude, decimalPlaces);
			e.AmplitudeData.sustainTime = (float)System.Math.Round(e.AmplitudeData.sustainTime, decimalPlaces);
			e.AmplitudeData.releaseTime = (float)System.Math.Round(e.AmplitudeData.releaseTime, decimalPlaces);

			e.FrequencyData.range.min = (float)System.Math.Round(e.FrequencyData.range.min, decimalPlaces);
			e.FrequencyData.range.max = (float)System.Math.Round(e.FrequencyData.range.max, decimalPlaces);
			e.FrequencyData.frequency = (float)System.Math.Round(e.FrequencyData.frequency, decimalPlaces);
			e.FrequencyData.delta = (float)System.Math.Round(e.FrequencyData.delta, decimalPlaces);
			e.FrequencyData.deltaAccel = (float)System.Math.Round(e.FrequencyData.deltaAccel, decimalPlaces);

			for(int iii = 0; iii < e.FrequencyData.jumps.Length; iii++)
			{
				e.FrequencyData.jumps[iii].repeatTime = (float)System.Math.Round(e.FrequencyData.jumps[iii].repeatTime, decimalPlaces);
				e.FrequencyData.jumps[iii].start = (float)System.Math.Round(e.FrequencyData.jumps[iii].start, decimalPlaces);
				e.FrequencyData.jumps[iii].value = (float)System.Math.Round(e.FrequencyData.jumps[iii].value, decimalPlaces);
			}
		}
		public static SounderEffect Mutate(SounderEffect effect, float amount = .1f)
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();
			e.Copy(effect);

			e.AmplitudeData.attackTime			+= Random.Range(-amount, amount);
			e.AmplitudeData.attackAmplitude		+= Random.Range(-amount, amount);
			e.AmplitudeData.decayTime			+= Random.Range(-amount, amount);
			e.AmplitudeData.sustainAmplitude	+= Random.Range(-amount, amount);
			e.AmplitudeData.sustainTime			+= Random.Range(-amount, amount);
			e.AmplitudeData.releaseTime			+= Random.Range(-amount, amount);

			e.FrequencyData.frequency += Random.Range(-amount * 500.0f, amount * 500.0f);

			e.FrequencyData.delta += Random.Range(-amount * 50.0f, amount * 50.0f);
			e.FrequencyData.deltaAccel += Random.Range(-amount * 10.0f, amount * 10.0f);

			if(e.LFOAmplitude > float.Epsilon)
			{
				if(amount > .5f)
					e.LFOForm = GetRandomWaveForm();
				e.LFOAmplitude += Random.Range(-amount, amount);
				e.LFOFrequency += Random.Range(-amount * 50.0f, amount * 50.0f);
			}

			if(e.flangerAmplitude > float.Epsilon)
			{
				e.flangerAmplitude += Random.Range(-amount, amount);
				e.flangerOffset += Random.Range(-amount, amount);
				e.flangerOffsetDelta += Random.Range(-amount * 2.0f, amount * 2.0f);
			}
			
			if(e.phaseReset > .0f)
				e.phaseReset += Random.Range(-amount, amount);

			if(e.echoTime > .0f)
			{
				e.echoTime += Random.Range(-amount, amount);
				e.echoValue += Random.Range(-amount, amount);
			}

			if(e.highPass > 0)
				e.highPass += (int)Random.Range(-amount * 500.0f, amount * 500.0f);
			if(e.lowPass > 0)
				e.lowPass += (int)Random.Range(-amount * 500.0f, amount * 500.0f);

			e.Q += Random.Range(-amount, amount);
			
			return e;
		}
		public static SounderEffect Shoot()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();

			e.waveForm = GetRandomWaveForm(Oscillator.WaveForm.Sawtooth, Oscillator.WaveForm.HarmonicSaw, Oscillator.WaveForm.Square, Oscillator.WaveForm.HarmonicSquare);

			e.FrequencyData.range = new Range(25.0f, 2000.0f);
			e.FrequencyData.frequency = RandomMulti(75.0f, 1000.0f, 2);

			if(e.FrequencyData.frequency < Random.Range(750.0f, 1250.0f))
				e.FrequencyData.delta = RandomMulti(-500.0f, -100.0f, 2);
			else
				e.FrequencyData.deltaAccel = RandomMulti(-1000.0f, -500.0f, 4);
			
			
			e.FrequencyData.curveType = GetRandomCurve(FrequencyData.CurveType.Slope, FrequencyData.CurveType.Linear, FrequencyData.CurveType.Linear, FrequencyData.CurveType.Linear, FrequencyData.CurveType.Linear, FrequencyData.CurveType.Linear);

			if(RandomChance(.5f))
			{
				e.AmplitudeData.attackTime = .0f;
				e.AmplitudeData.attackAmplitude = 1.0f;
				e.AmplitudeData.decayTime = .0f;
				e.AmplitudeData.sustainAmplitude = 1.0f;
				e.AmplitudeData.sustainTime = RandomMulti(.0f, .05f, 2);
				e.AmplitudeData.releaseTime = RandomMulti(.05f, .15f, 2);
			}
			else
			{
				e.AmplitudeData.attackTime = .0f;
				e.AmplitudeData.attackAmplitude = 1.0f;
				e.AmplitudeData.decayTime = RandomMulti(.001f, .005f, 5);
				e.AmplitudeData.sustainAmplitude = .5f;
				e.AmplitudeData.sustainTime = RandomMulti(.0f, .025f, 2);
				e.AmplitudeData.releaseTime = RandomMulti(.05f, .15f, 2);
			}

			
			if(RandomChance(.25f))
			{
				e.flangerAmplitude = RandomMulti(.1f, .25f, 4);
				if(RandomChance(.5f))
					e.flangerOffset = Random.Range(-1.0f, 1.0f);
				else
					e.flangerOffsetDelta = RandomMulti(-10.0f, .0f, 4);
			}

			Round(e, 2);

			return e;
		}
		public static SounderEffect Pickup()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();

			e.waveForm = GetRandomWaveForm(Oscillator.WaveForm.Sine, Oscillator.WaveForm.HarmonicSaw, Oscillator.WaveForm.Triangle, Oscillator.WaveForm.HarmonicTriangle, Oscillator.WaveForm.Flow);

			e.AmplitudeData.attackTime = RandomMulti(.005f, .02f, 3);
			e.AmplitudeData.attackAmplitude = 1.0f;
			e.AmplitudeData.decayTime = .0f;
			e.AmplitudeData.sustainAmplitude = 1.0f;
			e.AmplitudeData.sustainTime = RandomMulti(.0f, .1f, 3);
			e.AmplitudeData.releaseTime = RandomMulti(.05f, .15f, 3);

			e.FrequencyData.range = new Range(20.0f, 5000.0f);
			e.FrequencyData.frequency = Random.Range(20.0f, 1000.0f);

			if(RandomChance(.0f))
			{
				e.FrequencyData.delta = RandomMulti(0.0f, 50.0f, 10);
				e.FrequencyData.deltaAccel = Random.Range(-5.0f, 5.0f);

				e.FrequencyData.curveType = GetRandomCurve(FrequencyData.CurveType.Square, FrequencyData.CurveType.Hook);
			}
			else
			{
				FrequencyJump[] jumps = new FrequencyJump[1];
				for(int iii = 0; iii < jumps.Length; iii++)
				{
					jumps[iii] = new FrequencyJump();
					jumps[iii].repeatTime = RandomMulti(.01f, .1f, iii + 1);
					jumps[iii].value = RandomMulti(1.0f, 50.0f, 2);
				}
				e.FrequencyData.jumps = jumps;

				e.FrequencyData.curveType = GetRandomCurve(FrequencyData.CurveType.Slope, FrequencyData.CurveType.Square, FrequencyData.CurveType.Square, FrequencyData.CurveType.Hook);
			}

			if(RandomChance(.25f))
			{
				e.echoTime = Random.Range(e.Duration * .05f, e.Duration);
				e.echoValue = RandomMulti(.1f, .2f, 3);
			}

			if(RandomChance(.7f))
				e.highPass = (int)(e.FrequencyData.frequency * Random.Range(.5f, 1.5f));

			Round(e, 2);

			return e;
		}
		public static SounderEffect Explosion()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();
			
			e.waveForm = Oscillator.WaveForm.Noise;

			e.AmplitudeData.attackTime = RandomMulti(.0f, .01f, 3);
			e.AmplitudeData.attackAmplitude = 1.0f;
			e.AmplitudeData.decayTime = RandomMulti(.01f, .06f, 3);
			e.AmplitudeData.sustainAmplitude = .5f;
			e.AmplitudeData.sustainTime = RandomMulti(.0f, .2f, 1);
			e.AmplitudeData.releaseTime = RandomMulti(.1f, .5f, 3);
			
			e.FrequencyData.range = new Range(0.0f, 1.0f);
			e.FrequencyData.frequency = RandomMulti(.01f, .04f, 2);

			if(RandomChance(.2f))
			{
				e.LFOForm = GetRandomWaveForm();
				e.LFOAmplitude = RandomMulti(.05f, .1f, 2);
				e.LFOFrequency = RandomMulti(5.0f, 50.0f, 4);
			}

			if(RandomChance(.33f))
			{
				e.highPass = Random.Range(0, 200);
				e.Q = Random.Range(.1f, 2.0f);
			}

			Round(e, 4);

			return e;
		}
		public static SounderEffect Hit()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();

			e.waveForm = GetRandomWaveForm();

			e.AmplitudeData.attackTime = .0f;
			e.AmplitudeData.attackAmplitude = 1.0f;
			e.AmplitudeData.decayTime = .0f;
			e.AmplitudeData.sustainAmplitude = 1.0f;
			e.AmplitudeData.sustainTime = .0f;
			e.AmplitudeData.releaseTime = RandomMulti(.01f, .05f, 3);

			e.FrequencyData.range = new Range(1.0f, 500.0f);
			e.FrequencyData.frequency = RandomMulti(25.0f, 125.0f, 4);

			e.FrequencyData.delta = RandomMulti(-500.0f, -250.0f, 2);

			e.FrequencyData.curveType = GetRandomCurve();

			if(RandomChance(.9f))
			{
				e.LFOForm = Oscillator.WaveForm.Noise;
				e.LFOAmplitude = RandomMulti(.10f, .25f, 4);
				e.LFOFrequency = Random.Range(.01f, .1f);
			}
			else
			{
				e.LFOForm = GetRandomWaveForm(Oscillator.WaveForm.HarmonicSaw, Oscillator.WaveForm.HarmonicSquare, Oscillator.WaveForm.HarmonicTriangle, Oscillator.WaveForm.Flow);
				e.LFOAmplitude = RandomMulti(.5f, 1.0f, 1);
				e.LFOFrequency = RandomMulti(25.0f, 50.0f, 4);
			}

			if(RandomChance(.33f))
			{
				e.echoTime = e.AmplitudeData.releaseTime + Random.Range(-.01f, .05f);
				e.echoValue = RandomMulti(.1f, .2f, 3);
				e.echoCount = Random.Range(1, 4);;
			}
			

			Round(e, 2);

			return e;
		}
		public static SounderEffect Blip()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();

			e.waveForm = GetRandomWaveForm(Oscillator.WaveForm.Flow, Oscillator.WaveForm.HarmonicSaw, Oscillator.WaveForm.HarmonicSquare, Oscillator.WaveForm.HarmonicTriangle, Oscillator.WaveForm.Roll, Oscillator.WaveForm.Sawtooth, Oscillator.WaveForm.Sine, Oscillator.WaveForm.Square, Oscillator.WaveForm.Triangle);

			e.AmplitudeData.attackTime = RandomMulti(.0f, .005f, 4);
			e.AmplitudeData.attackAmplitude = 1.0f;
			e.AmplitudeData.decayTime = .0f;
			e.AmplitudeData.sustainAmplitude = 1.0f;
			e.AmplitudeData.sustainTime = RandomMulti(.005f, .01f, 4);
			e.AmplitudeData.releaseTime = RandomMulti(.0f, .01f, 4);

			e.FrequencyData.range = new Range(1.0f, 10000.0f);
			e.FrequencyData.frequency = RandomMulti(25.0f, 1000.0f, 1);

			e.FrequencyData.delta = RandomMulti(0.0f, 20.0f, 3);
			e.FrequencyData.deltaAccel = RandomMulti(-5.0f, 5.0f, 1);

			e.FrequencyData.curveType = FrequencyData.CurveType.Linear;

			if(RandomChance(.5f))
			{
				e.FrequencyData.jumps = new FrequencyJump[1] { new FrequencyJump() };
				e.FrequencyData.jumps[0].repeatTime = e.Duration * .5f;
				e.FrequencyData.jumps[0].value = e.FrequencyData.frequency * Random.Range(.5f, 2.0f);
			}

			if(RandomChance(.5f))
			{
				e.LFOForm = GetRandomWaveForm(Oscillator.WaveForm.Sine, Oscillator.WaveForm.Roll, Oscillator.WaveForm.Flow, Oscillator.WaveForm.Triangle, Oscillator.WaveForm.HarmonicTriangle);
				e.LFOAmplitude = RandomMulti(.25f, .5f, 2);
				e.LFOFrequency = RandomMulti(25f, 50f, 4);
			}

			if(RandomChance(.5f))
			{
				if(RandomChance(.5f))
					e.highPass = Random.Range(120, e.FrequencyData.frequency);
				else
					e.lowPass = Random.Range(e.FrequencyData.frequency, 2000);

				e.Q = Random.Range(.5f, 1.0f);
			}

			Round(e, 2);

			return e;
		}
		public static SounderEffect Click()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();

			e.waveForm = GetRandomWaveForm();

			e.AmplitudeData.attackTime = .0f;
			e.AmplitudeData.attackAmplitude = 1.0f;
			e.AmplitudeData.decayTime = .0f;
			e.AmplitudeData.sustainAmplitude = 1.0f;
			e.AmplitudeData.sustainTime = .0f;
			e.AmplitudeData.releaseTime = .005f;

			if(e.waveForm == Oscillator.WaveForm.Noise)
			{
				e.FrequencyData.range = new Range(.0f, 20000.0f);
				e.FrequencyData.frequency = RandomMulti(.0025f, .01f, 2);
			}
			else
			{
				if(RandomChance(.5f))
				{
					e.FrequencyData.range = new Range(.0f, 20000.0f);
					e.FrequencyData.frequency = RandomMulti(75.0f, 150.0f, 6);
					if(RandomChance(.33f))
						e.FrequencyData.delta = RandomMulti(50000.0f, 100000.0f, 6);
				}
				else
				{
					e.FrequencyData.range = new Range(.0f, 20000.0f);
					e.FrequencyData.frequency = RandomMulti(750.0f, 1500.0f, 6);
					e.FrequencyData.delta = RandomMulti(-100000.0f, -50000.0f, 6);
				}
			}

			e.FrequencyData.curveType = FrequencyData.CurveType.Linear;
			
			if(RandomChance(.1f))
			{
				e.echoTime = .004f;
				e.echoValue = .5f;
				e.echoCount = 3;
			}

			e.lowPass = Random.Range(100.0f, 10000.0f);
			e.Q = .05f;

			Round(e, 2);

			return e;
		}
		public static SounderEffect Tone()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();

			e.waveForm = GetRandomWaveForm(Oscillator.WaveForm.Flow, Oscillator.WaveForm.HarmonicSaw, Oscillator.WaveForm.HarmonicSquare, Oscillator.WaveForm.HarmonicTriangle, Oscillator.WaveForm.Roll, Oscillator.WaveForm.Sawtooth, Oscillator.WaveForm.Sine, Oscillator.WaveForm.Square, Oscillator.WaveForm.Triangle);

			e.AmplitudeData.attackTime = .05f;
			e.AmplitudeData.attackAmplitude = 1.0f;
			e.AmplitudeData.decayTime = .0f;
			e.AmplitudeData.sustainAmplitude = 1.0f;
			e.AmplitudeData.sustainTime = RandomMulti(.15f, .5f, 4);
			e.AmplitudeData.releaseTime = .05f;

			e.FrequencyData.range = new Range(20.0f, 10000.0f);

			e.FrequencyData.frequency = Random.Range(40.0f, 1200.0f);
			
			Round(e, 2);

			return e;
		}
		public static SounderEffect Randomize()
		{
			SounderEffect e = ScriptableObject.CreateInstance<SounderEffect>();

			e.waveForm = GetRandomWaveForm();

			e.AmplitudeData.attackTime			= Random.Range(.0f, 1.0f);
			e.AmplitudeData.attackAmplitude		= Random.Range(.0f, 1.0f);
			e.AmplitudeData.decayTime			= Random.Range(.0f, 1.0f);
			e.AmplitudeData.sustainAmplitude	= Random.Range(.0f, 1.0f);
			e.AmplitudeData.sustainTime			= Random.Range(.0f, 1.0f);
			e.AmplitudeData.releaseTime			= Random.Range(.0f, 1.0f);

			e.FrequencyData.range = new Range(1.0f, 20000.0f);
			e.FrequencyData.frequency = Random.Range(.0f, 20000.0f);

			e.FrequencyData.delta = Random.Range(-100.0f, 100.0f);
			e.FrequencyData.deltaAccel = Random.Range(-10.0f, 10.0f);

			e.FrequencyData.curveType = GetRandomCurve();

			e.LFOForm = GetRandomWaveForm();
			e.LFOAmplitude = Random.Range(.0f, 1.0f);
			e.LFOFrequency = Random.Range(.0f, 100.0f);

			e.flangerAmplitude = Random.Range(.0f, 1.0f);
			e.flangerOffset = Random.Range(-1.0f, 1.0f);
			e.flangerOffsetDelta = Random.Range(-10.0f, 10.0f);

			if(RandomChance(.25f))
				e.phaseReset = Random.Range(.0f, e.Duration);
			
			if(RandomChance(.1f))
			{
				e.echoTime = RandomMulti(e.Duration * .05f, e.Duration * .2f, 6);
				e.echoValue = Random.Range(.2f, .75f);
				if(RandomChance(.2f))
					e.echoCount = -1;
				else
					e.echoCount = Random.Range(1, 10);
			}
			
			if(RandomChance(.15f))
			{
				if(RandomChance(.5f))
					e.highPass = Random.Range(0, 20000);
				else
					e.lowPass = Random.Range(0, 20000);

				e.Q = Random.Range(.0f, 5.0f);
			}
			

			Round(e, 4);

			return e;
		}
	}
}
