#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartScene
{
    // -------------------------------------------------
    // Configure this path
    // -------------------------------------------------
    private const string StartScenePath = "Assets/Scenes/ETV.unity";
    // -------------------------------------------------

    private const string PreviousSceneKey =
        "PlayModeStartScene_PreviousScene";

    private const string TargetLevelKey =
        "PlayModeStartScene_TargetLevel";

    static PlayModeStartScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(
        PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
            {
                PreparePlayMode();
                break;
            }

            case PlayModeStateChange.EnteredPlayMode:
            {
                // The actual transition is handled by GameMaster.
                // Nothing needs to be loaded here.
                EditorSceneManager.playModeStartScene = null;
                break;
            }

            case PlayModeStateChange.EnteredEditMode:
            {
                Cleanup();
                break;
            }
        }
    }

    private static void PreparePlayMode()
    {
        // Remember the scene currently being edited.
        Scene currentScene = SceneManager.GetActiveScene();

        string currentPath = currentScene.path;

        if (string.IsNullOrEmpty(currentPath))
        {
            Debug.LogWarning(
                "[Editor Play Shortcut] " +
                "The current scene has no valid asset path.");

            return;
        }

        // Do not redirect ETV to itself.
        if (currentPath == StartScenePath)
        {
            EditorPrefs.DeleteKey(PreviousSceneKey);
            EditorPrefs.DeleteKey(TargetLevelKey);
        }
        else
        {
            EditorPrefs.SetString(
                PreviousSceneKey,
                currentPath);

            // Convert the scene filename into the GAMELEVEL enum.
            string sceneName =
                System.IO.Path.GetFileNameWithoutExtension(currentPath);

            if (System.Enum.TryParse(
                    sceneName,
                    out GAMELEVEL targetLevel))
            {
                EditorPrefs.SetString(
                    TargetLevelKey,
                    targetLevel.ToString());

                Debug.Log(
                    $"[Editor Play Shortcut] " +
                    $"Will bootstrap through ETV and then load: " +
                    $"{targetLevel}");
            }
            else
            {
                Debug.LogError(
                    $"[Editor Play Shortcut] " +
                    $"Could not convert scene '{sceneName}' " +
                    $"into a GAMELEVEL.");

                EditorPrefs.DeleteKey(TargetLevelKey);
            }
        }

        // Tell Unity to use ETV as the Play Mode start scene.
        SceneAsset startSceneAsset =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(
                StartScenePath);

        if (startSceneAsset != null)
        {
            EditorSceneManager.playModeStartScene =
                startSceneAsset;
        }
        else
        {
            Debug.LogError(
                $"[Editor Play Shortcut] " +
                $"Start scene not found at '{StartScenePath}'.");

            EditorSceneManager.playModeStartScene = null;
        }
    }

    private static void Cleanup()
    {
        EditorPrefs.DeleteKey(PreviousSceneKey);
        EditorPrefs.DeleteKey(TargetLevelKey);

        EditorSceneManager.playModeStartScene = null;
    }
}

#endif