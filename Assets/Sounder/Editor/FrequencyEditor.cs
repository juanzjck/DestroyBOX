using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Sounder
{
	public class FrequencyEditor
	{
		List<bool> subFolds = new List<bool>();
		bool foldoutOpen = true;
		
		SounderEffect effect;
		FrequencyData data;
		public FrequencyEditor(SounderEffect e)
		{
			effect = e;
			data = effect.FrequencyData;
			
			if(e.FrequencyData.jumps.Length < 1)
				foldoutOpen = false;
		}

		void FrequencyJumpField(ref FrequencyJump jump, ref FrequencyData data, int index)
		{
			string saveHash = AssetDatabase.GetAssetPath(effect).GetHashCode().ToString();

			EditorGUI.indentLevel++;
			jump.start = EditorGUILayout.Slider(new GUIContent("Start Time", "Time the jumper starts. In Seconds."), jump.start, .0f, 1.0f);
			
			GUIHelpers.RangedSlider(ref jump.value, 1000.0f, "Jump " + index.ToString() + " Value", 
					"How much the frequency changes. In Hertz.", saveHash, false);
			jump.repeatTime = 
			EditorGUILayout.Slider(new GUIContent("Repeat Time", "Time to wait before each repeat. In Seconds."), jump.repeatTime, -.0f, 1.0f);
			jump.repeats = EditorGUILayout.IntSlider(new GUIContent("Jump Repeats", "How many times to repeat the jump. If negative repeats until the sound ends."), jump.repeats, -1, 25);
			EditorGUI.indentLevel--;
		}

		void FrequencyJumps(ref FrequencyData data)
		{
			foldoutOpen = EditorGUILayout.Foldout(foldoutOpen, new GUIContent("Frequency Jumpers " + data.jumps.Length, "These jump the frequency up or down."));
			if(effect.frequencyManualCurve)
				foldoutOpen = false;

			if(foldoutOpen)
			{
				EditorGUI.indentLevel++;
				int count = EditorGUILayout.IntField(new GUIContent("Jumper Count"), data.jumps.Length);

				if(count != data.jumps.Length)
				{
					if(count < 0)
						count = 0;
					FrequencyJump[] temp = new FrequencyJump[count];
					for(int iii = 0; iii < data.jumps.Length || iii < count; iii++)
					{
						if(iii < data.jumps.Length && iii < count)
							temp[iii] = data.jumps[iii];
						else if(iii < temp.Length)
							temp[iii] = new FrequencyJump();
					}
					data.jumps = temp;
				}
				while(subFolds.Count != count)
				{
					if(subFolds.Count < count)
						subFolds.Add(true);
					else 
						subFolds.RemoveAt(subFolds.Count - 1);
				}

				for(int iii = 0; iii < data.jumps.Length; iii++)
				{
					subFolds[iii] = EditorGUILayout.Foldout(subFolds[iii], new GUIContent("Jumper " + iii.ToString()));
					if(subFolds[iii])
					{
						FrequencyJumpField(ref data.jumps[iii], ref data, iii);
						
						EditorGUILayout.Separator();
					}
				}
				EditorGUI.indentLevel--;
			}
		}
		public void DrawEditor()
		{
			string saveHash = AssetDatabase.GetAssetPath(effect).GetHashCode().ToString();

			Range r = new Range(data.range.min, data.range.max);
			GUIHelpers.RangeField(ref r, "Frequency Range", "Frequencies that move outside this range will be clamped into it when creating the frequency curve.");
			if(data.range.min < .0f)
				data.range.min = .0f;
			if(data.range.max > 20000.0f)
				data.range.max = 20000.0f;
			
			if(r.min != data.range.min || r.max != data.range.max)
			{
				Undo.RecordObject(effect, "SounderEffect Frequency Range");
				data.range = r;
			}

			EditorGUI.BeginDisabledGroup(effect.frequencyManualCurve);
			
			float test = EditorGUILayout.Slider(new GUIContent("Frequency", "Starting frequency of the sound."), data.frequency, data.range.min, data.range.max);
			if(test != data.frequency)
			{
				Undo.RecordObject(effect, "SounderEffect Frequency");
				data.frequency = test;
			}
			
			test = data.delta;
			GUIHelpers.RangedSlider(ref test, 1000.0f, "Frequency Delta", "Frequency changes by this much per second.", saveHash, false);
			if(test != data.delta)
			{
				Undo.RecordObject(effect, "SounderEffect Frequency Delta");
				data.delta = test;
			}
				
			test = data.deltaAccel;
			GUIHelpers.RangedSlider(ref test, 100.0f, "Delta Acceleration", "Frequency delta accelerates by this much per second.", saveHash, false);
			if(test != data.deltaAccel)
			{
				Undo.RecordObject(effect, "SounderEffect Frequency Delta Acceleration");
				data.deltaAccel = test;
			}


			FrequencyData.CurveType ct = (FrequencyData.CurveType)EditorGUILayout.EnumPopup(new GUIContent("Frequency Curve Type", "Determines the tangents and tangent mode of the frequency curve"), 
				data.curveType);
			if(ct != data.curveType)
			{
				Undo.RecordObject(effect, "SounderEffect Curve Type");
				data.curveType = ct;
			}
			
			FrequencyJumps(ref data);
			
			EditorGUI.EndDisabledGroup();

			float amplitudeDuration = effect.AmplitudeCurve.keys[effect.AmplitudeCurve.keys.Length - 1].time;
			float frequencyDuration = amplitudeDuration;
			if(effect.FrequencyCurve.keys.Length > 0)
				frequencyDuration = effect.FrequencyCurve.keys[effect.FrequencyCurve.keys.Length -1].time;
			frequencyDuration = Mathf.Max(frequencyDuration, amplitudeDuration);
			Rect frequencyRect = new Rect(0, effect.FrequencyData.range.min, frequencyDuration, effect.FrequencyData.range.max);
			
			EditorGUI.BeginChangeCheck();

			AnimationCurve ac = new AnimationCurve(effect.FrequencyCurve.keys);
			ac = EditorGUILayout.CurveField(new GUIContent("Frequency Curve", "The above values compile to this.\nCan be edited manually."), ac, Color.cyan, frequencyRect);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(effect, "SounderEffect Frequency Curve");
				effect.FrequencyCurve = ac;

				if(!effect.frequencyManualCurve)
				{
					effect.frequencyManualCurve = true;
					return;
				}
			}

			if(effect.frequencyManualCurve)
			{
				EditorGUILayout.HelpBox("Frequency curve manualy edited. \nEditing sliders will overwrite manual curve changes.", MessageType.Warning);
				if(GUILayout.Button("Edit Frequency Sliders"))
				{
					Undo.RecordObject(effect, "SounderEffect Frequency Manual Curve Disabled");
					effect.frequencyManualCurve = false;
				}
			}
		}
	}
}