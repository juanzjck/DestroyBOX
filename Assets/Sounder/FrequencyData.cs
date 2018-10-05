using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sounder
{
	[System.Serializable]
	public class Range
	{
		public float min;
		public float max;
		public Range(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
		public float Clamp(float value)
		{
			if(value < min)
				return min;
			if(value > max)
				return max;
			return value;
		}
		public bool InRange(float value)
		{
			if(value < min)
				return false;
			if(value > max)
				return false;
			return true;
		}
	}
	[System.Serializable]
	public class FrequencyJump
	{
		/// <summary>Amount in hz the frequency is changed.</summary>
		public float value = 50.0f;
		/// <summary>Delay before starting the jumps.</summary>
		public float start = .0f;
		/// <summary>Time in seconds between repeats.</summary>
		public float repeatTime = .025f;
		/// <summary>How many times this jump repeats. If below 0 repeats until sound ends.</summary>
		public int repeats = -1;

		public FrequencyJump(){}
		public FrequencyJump(FrequencyJump o)
		{
			value = o.value;
			start = o.start;
			repeatTime = o.repeatTime;
			repeats = o.repeats;
		}
	}

	[System.Serializable]
	public class FrequencyData
	{
		public enum CurveType
		{
			Smooth,
			Linear,
			Square,
			Slope,
			Hook
		}

		/// <summary>Range frequencies are clamped to when generating the frequency curve.</summary>
		public Range range				= new Range(200, 5000);
		/// <summary>Starting frequency of sound.</summary>
		public float frequency			= 500.0f;
		/// <summary>Change in frequency per second.</summary>
		public float delta				= .0f;
		/// <summary>Change in frequencies' delta per second.</summary>
		public float deltaAccel			= .0f;
		/// <summary>Jumps in frequency.</summary>
		public FrequencyJump[] jumps	= new FrequencyJump[0];
		/// <summary>How the tangent and tangent modes are set up when generating the frequency curve.</summary>
		public CurveType curveType		= CurveType.Slope;
		

		public FrequencyData(){}
		public FrequencyData(FrequencyData o)
		{
			Copy(o);
		}
		public void Copy(FrequencyData other)
		{
			range = new Range(other.range.min, other.range.max);
			frequency = other.frequency;
			delta = other.delta;
			deltaAccel = other.deltaAccel;
			jumps = new FrequencyJump[other.jumps.Length];
			for(int iii = 0; iii < jumps.Length; iii++)
				jumps[iii] = new FrequencyJump(other.jumps[iii]);
			curveType = other.curveType;
		}
		/// <summary>Creates and returns an Animation Curve that represents the frequency over time</summary>
		/// <param name="duration">How long, in seconds, the frequency curve must be.</param>
		public AnimationCurve GetCurve(float duration)
		{
			List<Keyframe> keys = new List<Keyframe>();
			
			float timeStep = .1f * duration;

			float time = .0f;
			float curFreq = .0f;
			
			keys.Add(new Keyframe(.0f, frequency));
			keys.Add(new Keyframe(duration, frequency));

			if(jumps != null && jumps.Length > 0)
			{
				for(int iii = 0; iii < jumps.Length; iii++)
				{
					FrequencyJump jump = jumps[iii];
					if(jump.repeatTime <= .0f || Mathf.Abs(jump.value) <= .0f)
						continue;
					
					int index = 0;
					
					const float match = .0005f;
					int repeats = jump.repeats;
					float value = .0f;
					if(repeats < 0)
						repeats = Mathf.CeilToInt((duration - jump.start) / jump.repeatTime);
					for(int rrr = 1; rrr < repeats + 1; rrr++)
					{
						time = rrr * jump.repeatTime + jump.start;
						value = rrr * jump.value;
						
						while(index < keys.Count && keys[index].time < (time - match * .5f))
						{
							Keyframe k = new Keyframe(keys[index].time, (rrr - 1) * jump.value + keys[index].value);
							keys[index] = k;
							index++;
						}
						
						if(index >= keys.Count)
							break;
						
						if(Math.Approximately(keys[index].time, time, match))
						{
							Keyframe k = new Keyframe(time, value + keys[index].value);
							keys[index] = k;
						}
						else
						{
							Keyframe k = new Keyframe(time, value + keys[index].value);
							keys.Insert(index, k);
						}
						index++;
					}
					for(int jjj = index; jjj < keys.Count; jjj++)
					{
						Keyframe k = new Keyframe(keys[jjj].time, value + keys[jjj].value);
						keys[jjj] = k;
					}

				}
			}

			if(deltaAccel != .0f)
			{
				time = .0f;
				for(int iii = 0; iii < keys.Count; iii++)
				{
					if(keys[iii].time - time > timeStep)
					{
						time = time + timeStep;
						curFreq = .5f * deltaAccel * (time * time);
						curFreq += delta * time;
						Keyframe k = new Keyframe(time, keys[iii].value + curFreq);
						keys.Insert(iii, k);
					}
					else
					{
						time = keys[iii].time;
						curFreq = .5f * deltaAccel * (time * time);
						curFreq += delta * time;
						keys[iii] = new Keyframe(keys[iii].time, keys[iii].value + curFreq);
					}
				}
			}
			else if(delta != .0f)
			{
				for(int iii = 0; iii < keys.Count; iii++)
				{
					keys[iii] = new Keyframe(keys[iii].time, keys[iii].value + delta * keys[iii].time);
				}

				float diff = .0f;
				if(delta > 0)
					diff = range.max - frequency;
				else
					diff = range.min - frequency;
				float t = Mathf.Sqrt((diff * 2) / delta);

				if(t < duration && t > .0f)
				{
					for(int iii = 0; iii < keys.Count; iii++)
					{
						if(t < keys[iii].time)
						{
							keys.Insert(iii, new Keyframe(t, (delta > 0) ? range.max : range.min));
							break;
						}
					}
					
				}
			}

			for(int iii = 0; iii < keys.Count; iii++)
			{
				Keyframe k = new Keyframe(keys[iii].time, range.Clamp(keys[iii].value));

				if(curveType == CurveType.Linear)
				{
					if(iii > 0)
						k.inTangent = CalcTangent(new Vector2(keys[iii - 1].time, keys[iii - 1].value), new Vector2(k.time, k.value));
					if(iii < keys.Count - 1)
						k.outTangent = CalcTangent(new Vector2(k.time, k.value), new Vector2(keys[iii + 1].time, range.Clamp(keys[iii + 1].value)));
				}
				else if(curveType == CurveType.Square)
				{
					k.inTangent = k.outTangent = float.PositiveInfinity;
				}
				else if(curveType == CurveType.Hook)
				{
					if(iii > 0)
						k.inTangent = -CalcTangent(new Vector2(keys[iii - 1].time, keys[iii - 1].value), new Vector2(k.time, k.value));
					if(iii < keys.Count - 1)
						k.outTangent = -CalcTangent(new Vector2(k.time, k.value), new Vector2(keys[iii + 1].time, keys[iii + 1].value));
				}

				keys[iii] = k;
				
			}
			AnimationCurve ac = new AnimationCurve(keys.ToArray());

			if(curveType == CurveType.Smooth)
			{
				for(int iii = 0; iii < ac.keys.Length; iii++)
					ac.SmoothTangents(iii, 0);
			}
			
			return ac;
		}
		static float CalcTangent(Vector2 a, Vector2 b)
		{
			Vector2 c = b - a;
			return c.y / c.x;
		}
	}
}