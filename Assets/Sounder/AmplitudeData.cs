using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sounder
{
	[System.Serializable]
	public class AmplitudeData
	{
		/// <summary>Time in seconds to go from 0 to Attack Amplitude</summary>
		public float attackTime					= .05f;
		/// <summary>Amplitude at the end of the attack / beginning of the decay phase</summary>
		public float attackAmplitude			= 1.0f;
		/// <summary>Time in seconds to go from Attack Amplitude to sustain Amplitude</summary>
		public float decayTime					= .1f;
		/// <summary>Time in seconds to hold the Sustain Amplitude</summary>
		public float sustainTime				= .3f;
		/// <summary>Amplitude of the waveform during the sustain phase</summary>
		public float sustainAmplitude			= .75f;
		/// <summary>Time in seconds to go from Sustain Amplitude to 0</summary>
		public float releaseTime				= .05f;
		
		public AmplitudeData(){}
		public AmplitudeData(AmplitudeData o)
		{
			Copy(o);
		}
		public void Copy(AmplitudeData other)
		{
			attackAmplitude = other.attackAmplitude;
			attackTime = other.attackTime;
			decayTime = other.decayTime;
			sustainTime = other.sustainTime;
			sustainAmplitude = other.sustainAmplitude;
			releaseTime = other.releaseTime;
		}

		public AnimationCurve GetCurve()
		{
			List<Keyframe> keys = new List<Keyframe>();
			if(attackTime > float.Epsilon)
				keys.Add(new Keyframe(.0f, .0f));
			
			keys.Add(new Keyframe(attackTime, attackAmplitude));
			keys.Add(new Keyframe(attackTime + decayTime, sustainAmplitude));
			if(sustainTime > float.Epsilon)
				keys.Add(new Keyframe(attackTime + decayTime + sustainTime, sustainAmplitude));
			keys.Add(new Keyframe(Duration,	.0f));
			
			for(int iii = 0; iii < keys.Count; iii++)
			{
				Keyframe k = new Keyframe(keys[iii].time, keys[iii].value);

				if(iii > 0)
					k.inTangent = CalcTangent(new Vector2(keys[iii - 1].time, keys[iii - 1].value), new Vector2(k.time, k.value));
				if(iii < keys.Count - 1)
					k.outTangent = CalcTangent(new Vector2(k.time, k.value), new Vector2(keys[iii + 1].time, keys[iii + 1].value));
				keys[iii] = k;
			}
			
			return new AnimationCurve(keys.ToArray());
		}
		static float CalcTangent(Vector2 a, Vector2 b)
		{
			Vector2 c = b - a;
			return c.y / c.x;
		}
		public float Duration
		{
			get
			{
				float temp = attackTime + decayTime + sustainTime + releaseTime;
				if(temp > float.Epsilon)
					return temp;
				return .05f;
			}
		}
	}
}
