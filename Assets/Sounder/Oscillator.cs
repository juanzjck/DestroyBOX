using System.Collections.Generic;
using UnityEngine;

namespace Sounder
{
	public static class Oscillator
	{
		public enum WaveForm : int
		{
			Sine				= 0,
			Square				= 1,
			HarmonicSquare		= 2,
			Sawtooth			= 3,
			HarmonicSaw			= 4,
			Triangle			= 5,
			HarmonicTriangle	= 6,
			Flow				= 7,
			Roll				= 8,
			
			Noise				= 256
		}
		static Dictionary<WaveForm, float[]> waveTables = new Dictionary<WaveForm,float[]>();
		public static int tableResolution = 0;
		public static float tablePeriod = 1.0f;

		static Oscillator()
		{
			waveTables = BuildWaveTables(48000);
		}
		public static void RebuildWaveTables(int resolution)
		{
			waveTables = BuildWaveTables(resolution);
		}
		static Dictionary<WaveForm, float[]> BuildWaveTables(int resolution)
		{
			if(resolution == tableResolution)
				return waveTables;
			tableResolution = resolution;
			tablePeriod = 1.0f / (float)tableResolution;

			Dictionary<WaveForm, float[]> ret = new Dictionary<WaveForm,float[]>();
			
			ret[WaveForm.Sine]				= BuildSine(resolution);
			ret[WaveForm.Square]			= BuildSquare(resolution);
			ret[WaveForm.HarmonicSquare]	= BuildHarmonic(resolution, 2, (a, b) => { return 1.0f / a; });
			ret[WaveForm.Sawtooth]			= BuildSawtooth(resolution);
			ret[WaveForm.HarmonicSaw]		= BuildHarmonic(resolution, 1, (a, b) => { return 1.0f / a; });
			ret[WaveForm.Triangle]			= BuildTriangle(resolution);
			ret[WaveForm.HarmonicTriangle]	= BuildHarmonic(resolution, 2, (a, b) => { return 1.0f / (a * a) * -System.Math.Sign(b); }, 3);
			ret[WaveForm.Flow]				= BuildFlow(resolution);
			ret[WaveForm.Roll]				= BuildHarmonic(resolution, 1, (a, b) => { return 1.0f / (a * a); });
			ret[WaveForm.Noise]				= BuildNoise(resolution);

			return ret;
		}
		static void ClampWaveTable(ref float[] table)
		{
			float min = table[0];
			float max = table[0];
			for (int iii = 0; iii < table.Length; iii++)
			{
				if(table[iii] < min)
					min = table[iii];
				if(table[iii] > max)
					max = table[iii];
			}

			float range = max - min;
			if(range < float.Epsilon)
				return;

			float mult = 2.0f / range;
			for (int iii = 0; iii < table.Length; iii++)
				table[iii] *= mult;
		}
		static float[] BuildSine(int resolution)
		{
			float[] temp = new float[resolution];
			for (int iii = 0; iii < temp.Length; iii++)
				temp[iii] = (float)System.Math.Sin(((float)iii / temp.Length) * Math.PI2);
			
			return temp;
		}
		static float[] BuildSquare(int resolution)
		{
			float[] temp = new float[resolution];
			for (int iii = 0; iii < temp.Length; iii++)
				temp[iii] = (float)System.Math.Sign(System.Math.Sin(((float)iii / temp.Length) * Math.PI2));
			
			return temp;
		}
		static float[] BuildTriangle(int resolution)
		{
			float[] temp = new float[resolution];
			for (int iii = 0; iii < temp.Length; iii++)
			{
				int negOnePoint = temp.Length / 4;
				int onePoint = negOnePoint * 3;
				if(iii <= negOnePoint)
					temp[iii] = Math.Lerp(.0f, 1.0f, (float)iii / (float)negOnePoint);
				else if(iii <= onePoint)
					temp[iii] = Math.Lerp(1.0f, -1.0f, (float)(iii - negOnePoint) / (float)(onePoint - negOnePoint));
				else
					temp[iii] = Math.Lerp(-1.0f, .0f, (float)(iii - onePoint) / (float)(temp.Length - onePoint));
			}
			
			return temp;
		}

		static float[] BuildHarmonic(int resolution, float harmonicOffset, System.Func<float, float, float> harmonicMultiplier, int harmonies = 11)
		{
			float[] temp = new float[resolution];
			float multiplier = -1.0f;
			for(float jjj = 1; jjj < (harmonies * harmonicOffset) + 1; jjj += harmonicOffset)
			{
				multiplier = harmonicMultiplier(jjj, multiplier);

				for (int iii = 0; iii < temp.Length; iii++)
					temp[iii] += (float)System.Math.Sin(((float)(iii * jjj) / temp.Length) * Math.PI2) * multiplier;
			}

			ClampWaveTable(ref temp);
			return temp;
		}
		static float[] BuildFlow(int resolution)
		{
			float[] temp = new float[resolution];
			for (int iii = 0; iii < temp.Length; iii++)
			{
				int negOnePoint = temp.Length / 4;
				int onePoint = negOnePoint * 3;
				if(iii <= negOnePoint)
					temp[iii] = Math.SmoothLerp(.0f, 1.0f, (float)iii / (float)negOnePoint);
				else if(iii <= onePoint)
					temp[iii] = Math.SmoothLerp(1.0f, -1.0f, (float)(iii - negOnePoint) / (float)(onePoint - negOnePoint));
				else
					temp[iii] = Math.SmoothLerp(-1.0f, .0f, (float)(iii - onePoint) / (float)(temp.Length - onePoint));
			}
			
			return temp;
		}

		static float[] BuildSawtooth(int resolution)
		{
			float[] temp = new float[resolution];
			for (int iii = 0; iii < temp.Length; iii++)
			{
				float bias = 21.0f/20.0f;

				float samplesPerWave = resolution;
				float percent = (float)(iii) / samplesPerWave;
				percent -= (float)System.Math.Floor(percent);
				percent = percent * bias;
				if(percent > 1.0f)
				{
					percent = System.Math.Abs((percent - 1.0f) / -1.0f/20.0f);
				}
				percent *= 2.0f;
				percent -= 1.0f;

				temp[iii] = -percent;
			}
				
			return temp;
		}
		static float[] BuildNoise(int resolution)
		{
			float[] temp = new float[resolution];
			System.Random rand = new System.Random();
			for (int iii = 0; iii < temp.Length; iii++)
				temp[iii] = (float)rand.NextDouble() * 2.0f - 1.0f;
			
			return temp;
		}
		public static float[] GetWaveTable(WaveForm form)
		{
			return waveTables[form];
		}
		/// <summary>Gets a copy of the wavetable at resolution </summary>
		public static float[] GetWaveTableCopy(WaveForm form, int resolution = -1)
		{
			if(resolution < 1)
				resolution = waveTables[form].Length;
			float[] ret = new float[resolution];

			if(waveTables[form].Length == ret.Length)
			{
				for(int iii = 0; iii < ret.Length; iii++)
					ret[iii] = waveTables[form][iii];
			}
			else
			{
				for(int iii = 0; iii < ret.Length; iii++)
				{
					float percent = (float)iii / (float)ret.Length;
					int index = (int)(percent * waveTables[form].Length);
					ret[iii] = waveTables[form][index];
				}
			}
			
			return ret;
		}
	}
}
