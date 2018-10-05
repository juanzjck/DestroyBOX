using UnityEngine;
using System.Collections;

namespace Sounder
{
	public static class WaveDrawer
	{
		static public Color back = new Color(.1f, .1f, .1f, 1.0f);
		static public Color fore = new Color(1.0f, .7f, 1.0f, 1.0f);
		public static Texture2D TextureFromSamples(float[] data, int width, int height)
		{
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		
			for(int xxx = 0; xxx < tex.width; xxx++)
			{
				for(int yyy = 0; yyy < tex.height; yyy++)
				{
					tex.SetPixel(xxx, yyy, back);
				}
			}

			int x0 = 0;
			int y0 = 0;

			for(int xxx = 0; xxx < tex.width; xxx++)
			{
				float yyy = data[Mathf.FloorToInt((float)xxx / (float)width * data.Length)];
				yyy += 1.0f;
				yyy *= .5f;
				yyy *= (height - 4);
				yyy += 2;
			
				yyy = Mathf.Clamp(yyy, 0, height);

				if(xxx != 0)
				{
					DrawLine(tex, x0, y0, xxx, Mathf.FloorToInt(yyy), fore);
				}
				x0 = xxx;
				y0 = Mathf.FloorToInt(yyy);
			}
			tex.hideFlags = HideFlags.HideAndDontSave;
			tex.Apply();
			return tex;
		}

		static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
		{
			int dy = (int)(y1-y0);
			int dx = (int)(x1-x0);
			int stepx, stepy;
 
			if (dy < 0) {dy = -dy; stepy = -1;}
			else {stepy = 1;}
			if (dx < 0) {dx = -dx; stepx = -1;}
			else {stepx = 1;}
			dy <<= 1;
			dx <<= 1;
 
			float fraction = 0;
 
			tex.SetPixel(x0, y0, col);
			if (dx > dy) 
			{
				fraction = dy - (dx >> 1);
				while (Mathf.Abs(x0 - x1) > 1) 
				{
					if (fraction >= 0) 
					{
						y0 += stepy;
						fraction -= dx;
					}
					x0 += stepx;
					fraction += dy;
					tex.SetPixel(x0, y0, col);
				}
			}
			else 
			{
				fraction = dx - (dy >> 1);
				while (Mathf.Abs(y0 - y1) > 1) 
				{
					if (fraction >= 0) 
					{
						x0 += stepx;
						fraction -= dy;
					}
					y0 += stepy;
					fraction += dx;
					tex.SetPixel(x0, y0, col);
				}
			}
		}
	}
}
