using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

namespace Sounder
{
	static public class WavBuilder
	{
		/// <summary>Save audio clip to .wav</summary>
		/// <param name="fileName">Fill file name. Should include folder and extension.</param>
		/// <param name="clip">AudioClip to save.</param>
		static public void WriteWav(string fileName, AudioClip clip)
		{
			using(FileStream fstream = new FileStream(fileName, FileMode.Create))
			using(BinaryWriter writer = new BinaryWriter(fstream))
			{
				uint fileLength = 0;

				writer.Write("RIFF".ToCharArray());
				writer.Write(fileLength);
				writer.Write("WAVE".ToCharArray());

				uint chunkSize = 16;
				ushort formatTag = 3;
				ushort channels = 1;
				ushort bitsPerSample = 32;
		
				ushort blockAlign = (ushort)(channels * (bitsPerSample / 8));;
				uint avgBytesPerSec = (uint)clip.frequency * blockAlign;
		
				writer.Write("fmt ".ToCharArray());
				writer.Write(chunkSize);
				writer.Write(formatTag);
				writer.Write(channels);
				writer.Write((uint)clip.frequency);
				writer.Write(avgBytesPerSec);
				writer.Write(blockAlign);
				writer.Write(bitsPerSample);
		
				float[] samples = new float[clip.samples];

				clip.GetData(samples, 0);
			
				uint dataChunkSize = (uint)(samples.Length * (bitsPerSample / 8));
				writer.Write("data".ToCharArray());
				writer.Write(dataChunkSize);
				for(int iii = 0; iii < samples.Length; iii++)
					writer.Write(samples[iii]);
		
				writer.Seek(4, SeekOrigin.Begin);
				uint fileSize = (uint)writer.BaseStream.Length;
				writer.Write(fileSize - 8);

				writer.Close();
				fstream.Close();
			}
		}
	}
}