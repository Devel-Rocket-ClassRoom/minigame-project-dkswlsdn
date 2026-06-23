using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class StartFromTitleScene
{
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";

    static StartFromTitleScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene(TitleScenePath);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            string previousScene = EditorPrefs.GetString("PreviousScene");
            if (!string.IsNullOrEmpty(previousScene))
                EditorSceneManager.OpenScene(previousScene);
        }
    }
}
