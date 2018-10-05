using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sounder
{
	public class Player
	{
		static float volume = 1.0f;
		/// <summary>Volume all sounds played using Sounder.Player are modified by</summary>
		public static float Volume { get { return volume; } set { volume = Mathf.Clamp01(value); } }
		/// <summary>Returns the list of audio sources</summary>
		public static List<AudioSource> SourcePool { get { return sources; } }
		/// <summary>Audio Mixer used for all SounderEffects, if Null none is used</summary>
		public static UnityEngine.Audio.AudioMixerGroup Mixer
		{
			get { return mixer; }
			set
			{
				mixer = value;
				for(int iii = 0; iii < sources.Count; iii++)
				{
					sources[iii].outputAudioMixerGroup = mixer;
				}
			}
		}
		
		static UnityEngine.Audio.AudioMixerGroup mixer;
		
		static int index = 0;

		static List<AudioSource> sources = new List<AudioSource>();

		static GameObject Sounder;
		
		static public void Setup()
		{
			if(Sounder != null)
				return;
			Sounder = new GameObject("Sounder");
			Object.DontDestroyOnLoad(Sounder);

			SetSourcePoolSize(16);
			
			Oscillator.RebuildWaveTables(AudioSettings.outputSampleRate);
		}
		/// <summary>Resizes the pool of audio sources</summary>
		/// <param name="poolSize"></param>
		static public void SetSourcePoolSize(int poolSize)
		{
			Setup();
			if(poolSize < 1)
				return;
			while(sources.Count < poolSize)
			{
				GameObject go = new GameObject("Player");
				Object.DontDestroyOnLoad(go);
				go.transform.parent = Sounder.transform;
				AudioSource a = go.AddComponent<AudioSource>();
				a.outputAudioMixerGroup = mixer;
				sources.Add(a);
			}
			while(sources.Count > poolSize)
			{
				GameObject.Destroy(sources[sources.Count - 1]);
				sources.RemoveAt(sources.Count - 1);
			}
		}
		/// <summary>Plays the clip using one of the audio sources in the pool</summary>
		/// <param name="clip">The audio clip to play</param>
		/// <param name="volume">The volume to play it at. Will be multiplied by Player.Volume</param>
		/// <param name="loop">If true loops the sound until manually stopped</param>
		/// <returns>Returns the AudioSource used to play. Plays nothing and returns null if the whole pool is busy</returns>
		static public AudioSource PlayClip(AudioClip clip, bool loop = false, float volume = 1.0f)
		{
			Setup();
			
			volume *= Volume;

			for(int iii = 0; iii < sources.Count; iii++)
			{
				index++;
				if(index >= sources.Count)
					index = 0;

				if(!sources[index].isPlaying)
				{
					sources[index].volume = volume;
					sources[index].clip = clip;
					sources[index].loop = loop;
					sources[index].Play();
					return sources[index];
				}
			}
			return null;
		}
	}
}