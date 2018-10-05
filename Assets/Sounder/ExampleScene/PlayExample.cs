using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayExample : MonoBehaviour 
{
	public UnityEngine.Audio.AudioMixerGroup mixer;

	public SounderEffect playEffect;
	public SounderEffect loopEffect;
	public SounderEffect changeEffect;
	public SounderEffect localPlayEffect;

	SounderEffect createdByScript = null;

	void Awake()
	{
		Sounder.Player.Mixer = mixer;
	}

	void Update() 
	{
		if(Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			Sounder.Player.Volume -= .1f;
		}
		if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus))
		{
			Sounder.Player.Volume += .1f;
		}


		if(Input.GetKeyDown(KeyCode.Q))
		{
			if(playEffect != null)
				playEffect.Play();	
			//Playing a SounderEffect is as simple as calling Play();
		}
		if(Input.GetKeyDown(KeyCode.W))
		{
			if(playEffect != null)
				Sounder.Presets.Mutate(playEffect, .2f).Play(); 
			//You don't need to hold on to a reference of a sounder effect(unless you're playing it looping)
			//Mutate creates a whole new effect. Calling play on it builds the audio clip and feeds it to the audio pool in Sounder.Player
		}
		if(Input.GetKeyDown(KeyCode.E))
		{
			if(localPlayEffect != null)
			{
				GetComponent<AudioSource>().clip = localPlayEffect.GetClip();
				GetComponent<AudioSource>().Play();
			//You can get the clip from a SounderEffect and play it using any AudioSource or do further processing on it
			}
		}
		
		if(Input.GetKeyDown(KeyCode.A))
		{
			if(loopEffect != null)
				loopEffect.Play(true);
			//If you play a SounderEffect looping it will continue to loop until you tell the SounderEffect or the audioSource to Stop();
		}
		if(Input.GetKeyUp(KeyCode.A))
		{
			if(loopEffect != null)
				loopEffect.Stop();
		}

		if(Input.GetKeyDown(KeyCode.Z))
		{
			if(createdByScript == null)
			{
				createdByScript = ScriptableObject.CreateInstance<SounderEffect>();
				createdByScript.AmplitudeData.attackTime = .05f;
				createdByScript.AmplitudeData.decayTime = .25f;
				createdByScript.AmplitudeData.sustainTime = .4f;
				createdByScript.AmplitudeData.releaseTime = .3f;
				createdByScript.AmplitudeData.sustainAmplitude = .5f;

				createdByScript.FrequencyData.frequency = 440.0f;
				createdByScript.FrequencyData.deltaAccel = -660.0f;
			}
			createdByScript.Play();
			//You can create or modify SounderEffects in script
		}

		if(changeEffect == null)
			return;
		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			changeEffect.FrequencyData.frequency = 100.0f;
			changeEffect.MakeClip();
			//MakeClipt forces Sounder to remake the clip even if it already exists. 
			//Without this calling play would use a cached version of the sound if it exists
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			changeEffect.FrequencyData.frequency = 200.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			changeEffect.FrequencyData.frequency = 300.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			changeEffect.FrequencyData.frequency = 400.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha5))
		{
			changeEffect.FrequencyData.frequency = 500.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha6))
		{
			changeEffect.FrequencyData.frequency = 600.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha7))
		{
			changeEffect.FrequencyData.frequency = 700.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha8))
		{
			changeEffect.FrequencyData.frequency = 800.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha9))
		{
			changeEffect.FrequencyData.frequency = 900.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
		if(Input.GetKeyDown(KeyCode.Alpha0))
		{
			changeEffect.FrequencyData.frequency = 1000.0f;
			changeEffect.MakeClip();
			changeEffect.Play();
		}
	}
	void OnGUI()
	{
		GUILayout.Label("Push Q to play a sound or W to play a randomly modified version of that sound.");
		GUILayout.Label("Push E to play a sound through the audio source on this object");
		GUILayout.Label("Hold A to play a looping sound");
		GUILayout.Label("Push Z to play a sound created from script");
		GUILayout.Label("Push Alpha1 through Alpha0 to play a sound modified depending on the key");
		GUILayout.Label("Push + or - to increase or decrease the volume. Volume: " + Sounder.Player.Volume.ToString("P"));
	}
}
