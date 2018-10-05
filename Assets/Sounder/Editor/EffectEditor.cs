using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

namespace Sounder
{
	[CustomEditor(typeof(SounderEffect))]
	public class EffectEditor : Editor
	{
		[MenuItem("Assets/Create/Sounder Effect")]
		public static void CreateAsset()
		{
			string path = "Assets";
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
			{
				string tester = AssetDatabase.GetAssetPath(obj);
				if(System.IO.Directory.Exists(tester))
					path = tester;
			}

			SounderEffect e = CreateInstance<SounderEffect>();
			string fileName = path + "/New Sounder Effect.asset";
			fileName = AssetDatabase.GenerateUniqueAssetPath(fileName);
			AssetDatabase.CreateAsset(e, fileName);
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Selection.activeObject = e;
		}

		FrequencyEditor frequencyEditor;
		AmplitudeEditor amplitudeEditor;
		Texture2D waveTexture = null;
		Texture2D effectTexture = null;
		AudioSource player = null;
		SounderEffect effect { get { return target as SounderEffect; } }
		bool playOnChange = true;
		bool playLooping = false;
		bool subEffectsOpen = false;

		void GetMaxMin(AnimationCurve ac, ref float max, ref float min)
		{
			if(ac.keys.Length < 1)
				return;
			for(int iii = 0; iii < ac.keys.Length; iii++)
			{
				if(ac.keys[iii].value > max)
					max = ac.keys[iii].value;
				if(ac.keys[iii].value < min)
					min = ac.keys[iii].value;
			}
		}
		public override void OnInspectorGUI()
		{
			if(Application.isPlaying)
				return;

			MakePlayer();
			
			int sampleRate = EditorGUILayout.IntField(new GUIContent("Sample Rate", "Samples per second. Smaller values build faster larger values sound better."), effect.SampleRate);
			if(sampleRate > 50000)
				sampleRate = 50000;
			if(sampleRate > 1 && effect.SampleRate != sampleRate)
			{
				Undo.RecordObject(effect, "SounderEffect Sample Rate");
				effect.SampleRate = sampleRate;
			}
			

			Oscillator.WaveForm form = (Oscillator.WaveForm)EditorGUILayout.EnumPopup(new GUIContent("Waveform", "Waveform determines the base timbre of the sound."), effect.waveForm);
			if(form != effect.waveForm)
			{
				Undo.RecordObject(effect, "SounderEffect WaveForm");
				effect.waveForm = form;
			}

		#region WaveDraw
			Rect r = EditorGUILayout.BeginHorizontal();
			r.height = 48;

			if(waveTexture == null || GUI.changed || waveTexture.width != r.width)
			{
				if(waveTexture != null)
					DestroyImmediate(waveTexture);
				waveTexture = WaveDrawer.TextureFromSamples(Oscillator.GetWaveTable(effect.waveForm), (int)r.width, (int)r.height);
			}
			EditorGUI.DrawPreviewTexture(r, waveTexture);
			
			EditorGUILayout.LabelField(new GUIContent(), GUILayout.Width(r.width), GUILayout.Height(r.height));
			EditorGUILayout.EndHorizontal();
		#endregion

			EditorGUILayout.Separator();

			float delay = EditorGUILayout.FloatField(new GUIContent("Delay", "Delay in seconds before starting"), effect.delay);
			if(delay < .0f)
				delay = .0f;

			if(delay != effect.delay)
			{
				Undo.RecordObject(effect, "SounderEffect Delay");
				effect.delay = delay;
			}

			EditorGUILayout.Separator();

			amplitudeEditor.DrawEditor();

			EditorGUILayout.Separator();

			frequencyEditor.DrawEditor();

			string saveHash = AssetDatabase.GetAssetPath(effect).GetHashCode().ToString();

			EditorGUILayout.Separator();
			float test = EditorGUILayout.Slider(new GUIContent("LFO Amplitude", "Amount the low frequency oscillator affects the sound."), effect.LFOAmplitude, .0f, 1.0f);
			if(test != effect.LFOAmplitude)
			{
				Undo.RecordObject(effect, "SounderEffect LFO Amplitude");
				effect.LFOAmplitude = test;
			}

			if(effect.LFOAmplitude > float.Epsilon)
			{
				Oscillator.WaveForm lfoForm = (Oscillator.WaveForm)EditorGUILayout.EnumPopup(new GUIContent("LFO Waveform", "Low Frequency Oscillator."), effect.LFOForm);
				if(lfoForm != effect.LFOForm)
				{
					Undo.RecordObject(effect, "SounderEffect LFO WaveForm");
					effect.LFOForm = lfoForm;
				}

				test = effect.LFOFrequency;
				GUIHelpers.RangedSlider(ref test, 100.0f, "LFO Frequency", 
						"Frequency of the low frequency oscillator.", saveHash, false);
				if(test != effect.LFOFrequency)
				{
					Undo.RecordObject(effect, "SounderEffect LFO Frequency");
					effect.LFOFrequency = test;
				}
			}

			EditorGUILayout.Separator();
			test = EditorGUILayout.Slider(new GUIContent("Flanger Amplitude", "Resonates a copy of the sound."), effect.flangerAmplitude, .0f, 1.0f);
			if(test != effect.flangerAmplitude)
			{
				Undo.RecordObject(effect, "SounderEffect Flanger Amplitude");
				effect.flangerAmplitude = test;
			}
			if(effect.flangerAmplitude > float.Epsilon)
			{
				test = effect.flangerOffset;
				GUIHelpers.RangedSlider(ref test, 1.0f, "Flanger Offset", "Offset of the flanger, as a percent of the wavetable.", saveHash, false);
				if(test != effect.flangerOffset)
				{
					Undo.RecordObject(effect, "SounderEffect Flanger Offset");
					effect.flangerOffset = test;
				}

				test = effect.flangerOffsetDelta;
				GUIHelpers.RangedSlider(ref test, 25.0f, "Offset Delta", "Accellerate the flanger offset by this much.", saveHash, false);
				if(test != effect.flangerOffsetDelta)
				{
					Undo.RecordObject(effect, "SounderEffect Flanger Offset Delta");
					effect.flangerOffsetDelta = test;
				}
			}

			EditorGUILayout.Separator();
			test = EditorGUILayout.Slider(new GUIContent("Phase Reset", "Resets most variables to their starting values after this much time has passed."), effect.phaseReset, .0f, effect.Duration);
			if(test != effect.phaseReset)
			{
				Undo.RecordObject(effect, "SounderEffect Phase Reset");
				effect.phaseReset = test;
			}

			test = effect.echoTime;
			GUIHelpers.RangedSlider(ref test, 1.0f,"Echo Time", "Echoes the sound every echoTime seconds", saveHash, true);
			if(test != effect.echoTime)
			{
				Undo.RecordObject(effect, "SounderEffect Echo Time");
				effect.echoTime = test;
			}
			if(effect.echoTime > float.Epsilon)
			{
				test = EditorGUILayout.Slider(new GUIContent("Echo Value", "How loud the echo is as a percent of the original sound"), effect.echoValue, 0f, 1.0f);
				if(test != effect.echoValue)
				{
					Undo.RecordObject(effect, "SounderEffect Echo Value");
					effect.echoValue = test;
				}

				int itest = EditorGUILayout.IntSlider(new GUIContent("Echo Count", "How many times to echo. If less than 0 echoes until very quiet."), effect.echoCount, -1, 10);
				if(itest != effect.echoCount)
				{
					Undo.RecordObject(effect, "SounderEffect Echo Count");
					effect.echoCount = itest;
				}
			}

			EditorGUILayout.Separator();
			test = effect.highPass;
			GUIHelpers.RangedSlider(ref test, 8000.0f, "High Pass", "Frequencies lower than this are attenuated. Set to 0 to do nothing.", saveHash, true);
			if(test != effect.highPass)
			{
				Undo.RecordObject(effect, "SounderEffect High Pass");
				effect.highPass = test;
			}

			test = effect.lowPass;
			GUIHelpers.RangedSlider(ref test, 8000.0f, "Low Pass", "Frequencies higher than this are attenuated. Set to 0 to do nothing.", saveHash, true);
			if(test != effect.lowPass)
			{
				Undo.RecordObject(effect, "SounderEffect Low Pass");
				effect.lowPass = test;
			}

			test = effect.Q;
			GUIHelpers.RangedSlider(ref test, 10.0f, "Resonance", "Amount to attenuate outside frequencies.", saveHash, true);
			if(test != effect.Q)
			{
				Undo.RecordObject(effect, "SounderEffect Resonance");
				effect.Q = test;
			}
			EditorGUILayout.Separator();

			SubEffects();
			
			EditorGUILayout.Separator();

			GUILayout.BeginHorizontal();
			bool play = false;
			if(GUILayout.Button("Play"))
				play = true;
			EditorGUILayout.Separator();
			EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent("Play on Change")).x;
			playOnChange = EditorGUILayout.Toggle("Play on Change", playOnChange);
			EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent("Play Looping")).x;
			playLooping = EditorGUILayout.Toggle("Play Looping", playLooping);
			EditorGUIUtility.labelWidth = 0;

			if(GUILayout.Button("Save to .Wav"))
			{
				string path = "";
				try { path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(effect)); } catch{}
				
				string wavName = EditorUtility.SaveFilePanel("Save sound to wav", path, effect.name, "wav");
				WavBuilder.WriteWav(wavName, effect.GetClip());
				AssetDatabase.Refresh();
			}
			GUILayout.EndHorizontal();

			
			play = play | DrawPresets();

			if(GUILayout.Button("Mutate"))
			{
				Undo.RecordObject(effect, "SounderEffect Mutate");
				
				SounderEffect se = Presets.Mutate(effect);
				effect.Copy(se);
				DestroyImmediate(se);
				play = true;
			}

			if (Event.current.type == EventType.ValidateCommand && Event.current.commandName.CompareTo("UndoRedoPerformed") == 0)
				GUI.changed = true;

			if(GUI.changed)
			{
				EditorUtility.SetDirty(effect);
				player.Stop();
				if(player.clip != null)
					DestroyImmediate(player.clip);
				effect.MakeClip();

				if(effectTexture != null)
					DestroyImmediate(effectTexture);
				effectTexture = null;
			}

			if(play || (GUI.changed && playOnChange))
			{
				player.clip = effect.GetClip();
				player.volume = 1.0f;
				player.loop = playLooping;
				player.Play();
			}
		}
		void SubEffects()
		{
			subEffectsOpen = EditorGUILayout.Foldout(subEffectsOpen, new GUIContent("Sub Effects", "Extra effects mixed into the sound."));
			if(subEffectsOpen)
			{
				EditorGUI.indentLevel++;
				int count = EditorGUILayout.IntField(new GUIContent("Sub Effects Count", "You can add other SounderEffects here and it will mix them into the current one."), effect.SubEffects.Count);
				if(count != effect.SubEffects.Count)
					Undo.RecordObject(effect, "SounderEffect Sub Effects Count");

				while(effect.SubEffects.Count > count)
					effect.SubEffects.RemoveAt(effect.SubEffects.Count - 1);
				while(effect.SubEffects.Count < count)
					effect.SubEffects.Add(null);
				
				for(int iii = 0; iii < effect.SubEffects.Count; iii++)
				{
					SounderEffect temp = EditorGUILayout.ObjectField(new GUIContent("Sub Effect"), 
						effect.SubEffects[iii], typeof(SounderEffect), true) as SounderEffect;
					if(temp != effect.SubEffects[iii])
					{
						Undo.RecordObject(effect, "SounderEffect Sub Effect Changed");
						effect.SubEffects[iii] = temp;
					}

					if(effect.SubEffects[iii] != null && effect.SubEffects[iii].ContainsSub(effect))
					{
						EditorUtility.DisplayDialog("Sounder", "Adding that SounderEffect as a subeffect would create a recursive effect which would enter an infinite loop.\nCannot create recursive effects.", "OK");
						effect.SubEffects[iii] = null;
					}
				}
				EditorGUI.indentLevel--;
			}
		}
		bool DrawPresets()
		{
			EditorGUILayout.Separator();
			SounderEffect preset = null;
			EditorGUILayout.LabelField("Presets");
			
			string[] presets = new string[] { "Shoot", "Pickup", "Hit", "Explosion", "Blip", "Click", "Tone", "Random" };

			int sel = GUILayout.SelectionGrid(-1, presets, 3);

			if(sel == 0)
				preset = Presets.Shoot();
			else if(sel == 1)
				preset = Presets.Pickup();
			else if(sel == 2)
				preset = Presets.Hit();
			else if(sel == 3)
				preset = Presets.Explosion();
			else if(sel == 4)
				preset = Presets.Blip();
			else if(sel == 5)
				preset = Presets.Click();
			else if(sel == 6)
				preset = Presets.Tone();
			else if(sel == 7)
				preset = Presets.Randomize();
			
			if(preset != null)
			{
				Undo.RecordObject(effect, "SounderEffect Preset " + presets[sel]);
				effect.Copy(preset);
				DestroyImmediate(preset);
				GUI.changed = true;

				return true;
			}
			return false;
		}

		void MakePlayer()
		{
			if(player != null)
				return;
			GameObject go = new GameObject("Player");
			go.hideFlags = HideFlags.HideAndDontSave;
			player = go.AddComponent<AudioSource>();
			player.hideFlags = HideFlags.HideAndDontSave;
			
		}

		void OnEnable()
		{
			if(Application.isPlaying)
				return;

			frequencyEditor = new FrequencyEditor(effect);
			amplitudeEditor = new AmplitudeEditor(effect);
			
			MakePlayer();
			player.clip = effect.GetClip();
			player.volume = 1.0f;
			player.Play();
		}
		void OnDisable()
		{
			if(effectTexture != null)
				DestroyImmediate(effectTexture);
			if(waveTexture != null)
				DestroyImmediate(waveTexture);

			if(player == null)
				return;
			if(player.clip != null)
				DestroyImmediate(player.clip);
			DestroyImmediate(player.gameObject);
		}
		void OnDestroy()
		{
			OnDisable();
		}
		public override bool HasPreviewGUI()
		{
			if(Application.isPlaying)
				return false;

			return true;
		}
		public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
		{
			if(Application.isPlaying)
				return;

			if(GUI.Button(r, ""))
			{
				player.clip = effect.GetClip();
				player.volume = 1.0f;
				player.Play();
			}

			string duration = "Duration: " + effect.Duration.ToString("F4");
			string buildTime = "Build Time: " + effect.LastBuildTime.ToString("F4");
			Vector2 duraSize = GUI.skin.label.CalcSize(new GUIContent(duration));
			Vector2 buildSize = GUI.skin.label.CalcSize(new GUIContent(buildTime));
			duraSize.x += 4;
			buildSize.x += 4;
			
			Rect duraRect = new Rect(r.xMin, r.yMin, duraSize.x, duraSize.y);
			Rect buildRect = new Rect(r.width - buildSize.x, r.yMin, buildSize.x, buildSize.y);
			
			float offy = Mathf.Max(duraSize.y, buildSize.y);
			if(duraSize.x + buildSize.x < r.width)
			{
				r.height -= offy;
				r.y += offy;
			}

			float[] samples = new float[effect.GetClip().samples];
			effect.GetClip().GetData(samples, 0);
			
			if(effectTexture == null || GUI.changed || effectTexture.width != r.width)
			{
				if(effectTexture != null)
					DestroyImmediate(effectTexture);
				effectTexture = WaveDrawer.TextureFromSamples(samples, (int)r.width, (int)r.height);
			}
			EditorGUI.DrawPreviewTexture(r, effectTexture);
			
			if(duraSize.x + buildSize.x > r.width)
				return;

			GUIStyle s = new GUIStyle();
			s.normal.textColor = WaveDrawer.fore;
			s.alignment = TextAnchor.UpperCenter;

			EditorGUI.DrawRect(new Rect(r.xMin, r.yMin - offy, r.width, offy), WaveDrawer.back);

			EditorGUI.LabelField(duraRect, duration, s);
			EditorGUI.LabelField(buildRect, buildTime, s);
		}
	}
}