using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Sounder
{
	public static class GUIHelpers
	{
		public class LabelGroup : IDisposable
		{
			int startIndent;

			public LabelGroup(GUIContent label)
			{
				EditorGUILayout.BeginHorizontal();

				float width = GUI.skin.label.CalcSize(label).x;
		
				if(width < EditorGUIUtility.labelWidth - 9 * EditorGUI.indentLevel)
					width =EditorGUIUtility.labelWidth - 9 * EditorGUI.indentLevel;

				EditorGUILayout.LabelField(label, GUILayout.Width(width));

				startIndent = EditorGUI.indentLevel;
			}

			public void Dispose()
			{
				EditorGUI.indentLevel = startIndent;
				EditorGUILayout.EndHorizontal();
			}
		}

		static public void RangeField(ref Range r, string text, string tooltip)
		{
			Rect rect = EditorGUILayout.GetControlRect();

			int startIndent = EditorGUI.indentLevel;
			

			Rect label = rect;
			label.width = GUI.skin.label.CalcSize( new GUIContent(text) ).x;
			if( label.width < EditorGUIUtility.labelWidth)
				label.width = EditorGUIUtility.labelWidth;

			EditorGUI.LabelField(label, new GUIContent(text, tooltip));

			EditorGUI.indentLevel = 0;

			GUIStyle st = new GUIStyle(EditorStyles.numberField);
			st.stretchWidth = false;

			Rect min = rect;
			min.x = label.xMax;
			min.width = (rect.width - min.x) * .5f;
			
			float test, testm;
			st.CalcMinMaxWidth(new GUIContent("Min"), out test, out testm);

			EditorGUIUtility.fieldWidth = 32;
			EditorGUIUtility.labelWidth = test + 3;
									
			r.min = EditorGUI.FloatField(min, "Min", r.min, st);

			Rect max = min;
			max.x = min.xMax + 12;
			max.xMax = rect.xMax;
			
			r.max = EditorGUI.FloatField(max, "Max", r.max);
			
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
			EditorGUI.indentLevel = startIndent;
		}


		class RangedFloat
		{
			public Range range;
			public bool open = false;
			public RangedFloat(float start, float range, bool onlyPositive)
			{
				this.range = new Range(-range, range);
				if(!this.range.InRange(start))
					this.range = new Range(start - range, start + range);
				if(onlyPositive && this.range.min < .0f)
					this.range.min = .0f;
			}
		}
		static public void RangedSlider(ref float value, float range, string text, string tooltip, string saveHash, bool onlyPositive)
		{
			saveHash += text;

			Rect rect = EditorGUILayout.GetControlRect();
			rect.height = GUI.skin.horizontalSlider.CalcSize(new GUIContent(text)).y;
			
			RangedFloat r = null;

			if(EditorPrefs.HasKey(saveHash))
			{
				r = new RangedFloat(value, range, onlyPositive);
				r.range.min = EditorPrefs.GetFloat(saveHash + "min");
				r.range.max = EditorPrefs.GetFloat(saveHash + "max");
				r.open = EditorPrefs.GetBool(saveHash);
			}

			if(r == null || !r.range.InRange(value))
			{
				r = new RangedFloat(value, range, onlyPositive);
				r.open = EditorPrefs.GetBool(saveHash, false);
			}
			
			Rect fold = rect;

			fold.width = 32;
			r.open = EditorGUI.Foldout(fold, r.open, "");

			value = EditorGUI.Slider(rect, new GUIContent(text, tooltip), value, r.range.min, r.range.max);
			
			if(r.open)
			{
				EditorGUI.indentLevel++;
				RangeField(ref r.range, "Slider Range", "Customize this slider's range");
				EditorGUI.indentLevel--;

				value = r.range.Clamp(value);
			}
			
			if(onlyPositive && r.range.min < 0)
			{
				r.range.min = 0;
			}

			EditorPrefs.SetFloat(saveHash + "min", r.range.min);
			EditorPrefs.SetFloat(saveHash + "max", r.range.max);
			EditorPrefs.SetBool(saveHash , r.open);
		}
	}
}