using UnityEditor;
using UnityEngine;

namespace UnityToolbarExtender.Examples
{
	[InitializeOnLoad]
	public static class SceneViewFocuser
	{
		static bool m_enabled;

		static bool Enabled
		{
			get { return m_enabled; }
			set
			{
				m_enabled = value;
				EditorPrefs.SetBool("SceneViewFocuser", value);
			}
		}

		static SceneViewFocuser()
		{
			m_enabled = EditorPrefs.GetBool("SceneViewFocuser", false);
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			EditorApplication.pauseStateChanged += OnPauseChanged;

			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
		}

		static void OnPauseChanged(PauseState obj)
		{
			if (Enabled && obj == PauseState.Unpaused)
			{
				// Not sure why, but this must be delayed
				EditorApplication.delayCall += EditorWindow.FocusWindowIfItsOpen<SceneView>;
			}
		}

		static void OnPlayModeChanged(PlayModeStateChange obj)
		{
			if (Enabled && obj == PlayModeStateChange.EnteredPlayMode)
			{
				EditorWindow.FocusWindowIfItsOpen<SceneView>();
			}
		}

		static void OnToolbarGUI()
		{
			var tex = EditorGUIUtility.IconContent(@"UnityEditor.SceneView").image;

			GUI.changed = false;

			GUILayout.Toggle(m_enabled, new GUIContent(null, tex, "시작시 scene 화면으로 유지하기"), "Command");
			if (GUI.changed)
			{
				Enabled = !Enabled;
			}
		}
	}
}