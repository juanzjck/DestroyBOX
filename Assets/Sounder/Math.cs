namespace Sounder
{
	public static class Math
	{
		static public float Clamp01(float value)
		{
			if(value < .0f)
				return .0f;
			else if(value > 1.0f)
				return 1.0f;
			return value;
		}
		static public float Clamp101(float value)
		{
			if(value < -1.0f)
				return -1.0f;
			else if(value > 1.0f)
				return 1.0f;
			return value;
		}
		static public float Clamp(float value, float min, float max)
		{
			if(value < min)
				value = min;
			else if(value > max)
				value = max;
			return value;
		}
		static public float FromNeg1to1To0to1(float f)
		{
			f = f + 1.0f;
			f *= .5f;
			return f;
		}

		static public bool Approximately(float a, float b, float range = .005f)
		{
			float diff = a - b;
			return (Abs(diff) < range);
		}

		public static float Lerp(float from, float to, float percent) 
		{
			if (percent < 0.0f)
				return from;
			else if (percent >= 1.0f)
				return to;
			return (to - from) * percent + from;
		}
		public static float SmoothLerp(float from, float to, float percent)
		{
			return Lerp(from, to, SmoothStep(percent));
		}
		public static float SmoothStep(float percent)
		{
			return (percent * percent) * (3.0f - 2.0f * percent);
		}
		public static float Pow(float f, float power) 
		{
			return (float)System.Math.Pow(f, power);
		}
		public static float Abs(float f)
		{
			if(f < .0f)
				return -f;
			return f;
		}
		public static int NearestPow2(int n)
		{
			n--;
			n |= n >> 1;
			n |= n >> 2;
			n |= n >> 4;
			n |= n >> 8;
			n |= n >> 16;
			n++;
			return n;
		}
		public static int HowManyPowersOfTwo(int n)
		{
			int count = 0;
			while(n > 1)
			{
				n = n >> 1;
				count++;
			}
			return count;
		}
		public const float PI = (float)System.Math.PI;
		public const float PI2 = (float)System.Math.PI * 2;
	}
}
