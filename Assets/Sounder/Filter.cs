using UnityEngine;
using System.Collections;

namespace Sounder
{
	public class Filter
	{
		int sampleRate;
		
		public enum PassType : int
		{
			Everything = 0,
			Low = 1,
			High = 2
		}

		public float Cutoff
		{
			get { return cutoff; }
			set
			{
				if(value < 10.0f)
					cutoff = 10.0f;
				cutoff = value;
				Rebuild();
			}
		}
		float cutoff = 10.0f;

		public float Resonance
		{
			get { return resonance; }
			set
			{
				if(value < .01f)
					resonance = .01f;
				resonance = value;
				Rebuild();
			}
		}
		float resonance = .05f;

		public PassType Pass
		{
			get { return pass; }
			set { pass = value; Rebuild(); }
		}
		PassType pass = PassType.Low;
		
		public Filter(PassType pass, float cutoff, float resonance, int sampleRate)
		{
			this.sampleRate = sampleRate;
			this.pass = pass;
			this.cutoff = cutoff;
			this.resonance = resonance;
			Rebuild();
		}
		public Filter(){}

		public float[] inputHistory = new float[2];
		public float[] outputHistory = new float[3];
			
		float c, a1, a2, a3, b1, b2;

		public void RebuildIfNecessary(PassType pass, float cutoff, float resonance, int sampleRate)
		{
			if(pass != this.pass || cutoff != this.cutoff || resonance != this.resonance || sampleRate != this.sampleRate)
			{
				this.pass = pass;
				this.cutoff = cutoff;
				this.resonance = resonance;
				this.sampleRate = sampleRate;
				Rebuild();
			}
		}
		void Rebuild()
		{
			switch (Pass)
			{
				case PassType.Low:
					c = 1.0f / (float)System.Math.Tan(System.Math.PI * Cutoff / sampleRate);
					a1 = 1.0f / (1.0f + Resonance * c + c * c);
					a2 = 2f * a1;
					a3 = a1;
					b1 = 2.0f * (1.0f - c * c) * a1;
					b2 = (1.0f - Resonance * c + c * c) * a1;
					break;
				case PassType.High:
					c = (float)System.Math.Tan(System.Math.PI * Cutoff / sampleRate);
					a1 = 1.0f / (1.0f + Resonance * c + c * c);
					a2 = -2f * a1;
					a3 = a1;
					b1 = 2.0f * (c * c - 1.0f) * a1;
					b2 = (1.0f - Resonance * c + c * c) * a1;
					break;
			}
		}
		public void FilterValue(ref float newInput)
		{
			newInput = a1 * newInput + a2 * inputHistory[0] + a3 * inputHistory[1] - b1 * outputHistory[0] - b2 * outputHistory[1];

			inputHistory[1] = inputHistory[0];
			inputHistory[0] = newInput;

			outputHistory[2] = outputHistory[1];
			outputHistory[1] = outputHistory[0];
			outputHistory[0] = newInput;
		}
		public float GetFilteredValue(float newInput)
		{
			FilterValue(ref newInput);
			return newInput;
		}

	}
}