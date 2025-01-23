using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityToolbarExtender.Examples
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 14,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold,
			};	
		}
	}

	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
		public static string developScene = "Assets/Scenes/GameScene.unity";
		static SceneSwitchLeftButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
			ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
		}

		static void OnToolbarGUI()
		{
			GUILayout.FlexibleSpace();
			var style = new GUIStyle(ToolbarStyles.commandButtonStyle);
			style.hover.textColor = Color.cyan;
			style.fixedWidth = 60;
			if (GUILayout.Button(new GUIContent("개발 씬", "게임씬 이동"), style))
			{
				EditorSceneManager.OpenScene(developScene);
			}
			style.fixedWidth = 40;
			if (GUILayout.Button(new GUIContent("시작", "Start"), style))
			{
				SceneHelper.StartScene("ResourceDownloadScene");
			}
		}

		static void OnRightToolbarGUI()
        {
			var style = new GUIStyle(ToolbarStyles.commandButtonStyle);
			style.hover.textColor = Color.cyan;
			style.fixedWidth = 80;
			//style.border = new RectOffset(5, 5, 3, 3);

			if (GUILayout.Button(new GUIContent("최근 씬" , $"{SceneHelper.lastScene}"), style))
			{
				string[] guids = AssetDatabase.FindAssets("t:scene " + SceneHelper.lastScene, null);
				if (guids.Length == 0)
				{
					Debug.LogWarning("Couldn't find scene file");
				}
				else
				{
					string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);

					EditorApplication.ExitPlaymode();
					EditorSceneManager.OpenScene(scenePath);
				}
			}
		}
	}

    [InitializeOnLoad]
    static class SceneHelper
    {
        static string sceneToOpen;

        public static string lastScene
        {
            get => EditorPrefs.GetString("LastConnectScene", "");
            set => EditorPrefs.SetString("LastConnectScene", value);
        }

        public static void StartScene(string sceneName)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            var last = EditorSceneManager.GetActiveScene().name;
			lastScene = EditorSceneManager.GetActiveScene().name;

			sceneToOpen = sceneName;
			EditorApplication.update += OnUpdate;
		}

		static void OnUpdate()
		{
			if (sceneToOpen == null ||
			    EditorApplication.isPlaying || EditorApplication.isPaused ||
			    EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			EditorApplication.update -= OnUpdate;

			if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				// need to get scene via search because the path to the scene
				// file contains the package version so it'll change over time
				string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
				if (guids.Length == 0)
				{
					Debug.LogWarning("Couldn't find scene file");
				}
				else
				{
					string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
					EditorSceneManager.OpenScene(scenePath);
					EditorApplication.isPlaying = true;
				}
			}
			sceneToOpen = null;
		}
	}
}