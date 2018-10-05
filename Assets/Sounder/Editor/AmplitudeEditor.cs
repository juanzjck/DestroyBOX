using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Sounder
{
	public class AmplitudeEditor 
	{
		SounderEffect effect;
		public AmplitudeEditor(SounderEffect e) { effect = e; }

		public void DrawEditor()
		{
			AmplitudeData data = effect.AmplitudeData;

			EditorGUI.BeginDisabledGroup(effect.amplitudeManualCurve);

			float test = EditorGUILayout.Slider(new GUIContent("Attack Amplitude", "The amplitude of the sound at the peak of the attack phase."), data.attackAmplitude, .0f, 1.0f);
			if(test != data.attackAmplitude)
			{
				Undo.RecordObject(effect, "SounderEffect Attack Amplitude");
				data.attackAmplitude = test;
			}

			test = EditorGUILayout.Slider(new GUIContent("Attack Time", "The time it takes for the amplitude to reach the value of Attack Amplitude."), data.attackTime, .0f, 2.0f);
			if(test != data.attackTime)
			{
				Undo.RecordObject(effect, "SounderEffect Attack Time");
				data.attackTime = test;
			}

			test = EditorGUILayout.Slider(new GUIContent("Decay Time", "The time it takes for the amplitude to go from the Attack Amplitude to the Sustain Amplitude."), data.decayTime, .0f, 2.0f);
			if(test != data.decayTime)
			{
				Undo.RecordObject(effect, "SounderEffect DecayTime");
				data.decayTime = test;
			}

			test = EditorGUILayout.Slider(new GUIContent("Sustain Amplitude", "The amplitude of the sound during its sustain phase."), data.sustainAmplitude, .0f, 1.0f);
			if(test != data.sustainAmplitude)
			{
				Undo.RecordObject(effect, "SounderEffect Sustain Amplitude");
				data.sustainAmplitude = test;
			}

			test = EditorGUILayout.Slider(new GUIContent("Sustain Time", "The duration of the sustain phase."), data.sustainTime, .0f, 2.0f);
			if(test != data.sustainTime)
			{
				Undo.RecordObject(effect, "SounderEffect Sustain Time");
				data.sustainTime = test;
			}

			test = EditorGUILayout.Slider(new GUIContent("Release Time", "The time it takes to go from the Sustain Amplitude to silence."), data.releaseTime, .0f, 2.0f);
			if(test != data.releaseTime)
			{
				Undo.RecordObject(effect, "SounderEffect Realease Time");
				data.releaseTime = test;
			}
			
			EditorGUI.EndDisabledGroup();

			float duration = 1.0f;
			if(effect.AmplitudeCurve.keys.Length > 0)
				duration = effect.AmplitudeCurve.keys[effect.AmplitudeCurve.keys.Length -1].time;

			Rect amplitudeRect = new Rect(.0f, .0f, effect.amplitudeManualCurve ? duration + .25f : duration, 1.0f);

			EditorGUI.BeginChangeCheck();
			AnimationCurve ac = new AnimationCurve(effect.AmplitudeCurve.keys);
			ac = EditorGUILayout.CurveField(new GUIContent("Amplitude Envelope", "Amplitude envelope. \nSound effect ends on the last key of this curve.\nIf manually edited ignores ADSR values."), ac, Color.yellow, amplitudeRect);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(effect, "SounderEffect Amplitude Envelope");
				effect.AmplitudeCurve = ac;

				if(!effect.amplitudeManualCurve)
				{
					effect.amplitudeManualCurve = true;
					return;
				}
			}

			if(effect.amplitudeManualCurve)
			{
				EditorGUILayout.HelpBox("Amplitude curve edited. Editing sliders will overwrite manual curve changes.", MessageType.Warning);
				if(GUILayout.Button("Edit Amplitude Sliders"))
				{
					Undo.RecordObject(effect, "SounderEffect Amplitude Manual Curve Disabled");
					effect.amplitudeManualCurve = false;
				}
			}

		}
	}
}