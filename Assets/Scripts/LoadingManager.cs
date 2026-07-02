using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadingManager : MonoBehaviour
{
    [Header("Loading UI")]
    public CanvasGroup loadingpanel;
    public Image loadingbar;
    public TextMeshProUGUI loadingclock;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 3f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    
    [Header("Fade Stability")]
    [Tooltip("Caps the fade step per frame so scene-load stalls don't cause big alpha jumps.")]
    public float maxFadeStepSeconds = 1f / 30f; // 30 FPS step cap (smooth)

    public InputActionReference hotToGo;
    
    private Coroutine _fadeCo;
    private bool _isLoading;

    private bool inSecret;

    // scene-load handshake
    private bool _sceneLoadedFlag;

    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        hotToGo.action.performed += LoadSecretLevel;

        // Start black if desired
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            fadeCanvas.alpha = 1f;
            fadeCanvas.interactable = false;
        }

        ShowLoadingUI(false);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Public entry point.
    /// Fades out fully, loads scene, then fades in fully.
    /// </summary>
    public void LoadLevel(GAMELEVEL levelName, Action onFinished = null)
    {
        
        Debug.Log("LoadLevel?");
        if (_isLoading) return;
        
        Debug.Log("LoadLevel??");
        
        
        EventManager.DoLoadingSwirl();
        
        StartCoroutine(LoadLevelSequence(levelName, onFinished));
    }
    
    
    public void LoadSecretLevel(InputAction.CallbackContext callbackContext)
    {
        if (_isLoading) return;

        
        
        GAMELEVEL toGoTo = inSecret ? GAMELEVEL.NorasFlat : GAMELEVEL.SecretLevel;

        inSecret = toGoTo == GAMELEVEL.SecretLevel;
        EventManager.DebugCamEnabled(false);
        StartCoroutine(LoadLevelSequence(toGoTo));
    }


    private IEnumerator LoadLevelSequence(GAMELEVEL levelName, Action onFinished = null)
    {
        _isLoading = true;


        // 1) Fade to black and wait
        yield return FadeToAndWait(1f);

        // 2) Show loading UI
        ShowLoadingUI(true);
        UpdateLoadingClock();

        // 3) Load scene (controlled activation)
        yield return ChangeSceneAsync(levelName);
        
        
        


        EventManager.UnDoLoadingSwirl();
        
        
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            fadeCanvas.alpha = 1f;
        }

        
        // 5) Hide loading UI
        ShowLoadingUI(false);
        
        
        // ✅ Force UI to rebuild *now*, then wait until end-of-frame so it truly disappears visually
        Canvas.ForceUpdateCanvases();
        yield return null;
        yield return new WaitForEndOfFrame();

        
        yield return FadeToAndWait(0f);
        
        
        _isLoading = false;
        onFinished?.Invoke();
    }

    // ---------------------------------------------------------------------
    // Fade helpers
    // ---------------------------------------------------------------------
    public void SceneFadeIn()  => FadeTo(0f);
    public void SceneFadeOut() => FadeTo(1f);

    public void FadeTo(float targetAlpha)
    {
        if (fadeCanvas == null) return;

        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
            _fadeCo = null;
        }

        _fadeCo = StartCoroutine(FadeCanvasGroupTo(targetAlpha, fadeDuration));
    }

    private IEnumerator FadeToAndWait(float targetAlpha)
    {
        if (fadeCanvas == null) yield break;

        
        fadeCanvas.gameObject.SetActive(true);

        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
            _fadeCo = null;
        }

        _fadeCo = StartCoroutine(FadeCanvasGroupTo(targetAlpha, fadeDuration));
        yield return _fadeCo;
    }

    private IEnumerator FadeCanvasGroupTo(float targetAlpha, float duration)
    {
        if (fadeCanvas == null) yield break;


        
        while (GameMaster.Instance.DialogueManager.AwaitingFirstThoughts)
        {
            yield return null;
        }
        
        
        
        float startAlpha = fadeCanvas.alpha;

        if (duration <= 0f || Mathf.Approximately(startAlpha, targetAlpha))
        {
            fadeCanvas.alpha = targetAlpha;
            _fadeCo = null;
            yield break;
        }

        float t = 0f;

        GameMaster.Instance.PLAYERBUSY = false;
        
        while (t < duration)
        {
            
            float dt = Time.unscaledDeltaTime;
            if (maxFadeStepSeconds > 0f)
                dt = Mathf.Min(dt, maxFadeStepSeconds);

            t += dt;

            float normalized = Mathf.Clamp01(t / duration);
            float eased = fadeCurve != null ? fadeCurve.Evaluate(normalized) : normalized;

            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, eased);
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
        _fadeCo = null;

    }

    
    private IEnumerator ChangeSceneAsync(GAMELEVEL levelName)
    {
        GameMaster.Instance.THISLEVEL = levelName;
    
        _sceneLoadedFlag = false;
        Time.timeScale = 0f;

        AsyncOperation op = SceneManager.LoadSceneAsync(levelName.ToString());
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (loadingbar != null) loadingbar.fillAmount = Mathf.Clamp01(op.progress / 0.9f);
            yield return null;
        }

        if (loadingbar != null) loadingbar.fillAmount = 1f;

        op.allowSceneActivation = true;

        // Wait until scene is fully loaded
        while (!_sceneLoadedFlag) yield return null;

        Time.timeScale = 1f;

        if (GameMaster.Instance != null) GameMaster.Instance.PLAYERBUSY = false;
        
        
        if (GameMaster.Instance != null)
        {
            Debug.Log("scene loaded from LoadingManager");
            GameMaster.Instance.StartLevel(levelName);
        }
        
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _sceneLoadedFlag = true;
    }

    // ---------------------------------------------------------------------
    // UI / helpers
    // ---------------------------------------------------------------------
    private void ShowLoadingUI(bool show)
    {
        if (loadingpanel == null) return;

        loadingpanel.alpha = show ? 1f : 0f;
        loadingpanel.interactable = show;
    }

    private void UpdateLoadingClock()
    {
        if (loadingclock == null) return;

        string buildDate = "";
        buildDate += DateTime.Now.ToString("dddd");
        buildDate += ", ";
        buildDate += DateTime.Now.ToString("MMMM d");
        buildDate += MonthDay(DateTime.Now.ToString("dd"));
        buildDate += ", ";
        buildDate += DateTime.Now.ToString("yyyy");

        loadingclock.text = buildDate;
    }



    private static string MonthDay(string day)
    {
        string nuNum = "th";
        int d = int.Parse(day);

        if (d < 11 || d > 20)
        {
            char last = day[^1];
            switch (last)
            {
                case '1': nuNum = "st"; break;
                case '2': nuNum = "nd"; break;
                case '3': nuNum = "rd"; break;
            }
        }

        return nuNum;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(LoadingManager))]
public class LoadingManagerEditor : Editor
{
    private GAMELEVEL selectedLevel = GAMELEVEL.NorasFlat;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LoadingManager manager = (LoadingManager)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Scene Controls", EditorStyles.boldLabel);

        selectedLevel = (GAMELEVEL)EditorGUILayout.EnumPopup("Target Level", selectedLevel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Selected Level"))
            {
                manager.LoadLevel(selectedLevel);
            }

            if (GUILayout.Button("Fade In"))
            {
                manager.SceneFadeIn();
            }

            if (GUILayout.Button("Fade Out"))
            {
                manager.SceneFadeOut();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Quick Load", EditorStyles.boldLabel);


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("MainMenu"))     manager.LoadLevel(GAMELEVEL.MainMenu);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("ETV"))          manager.LoadLevel(GAMELEVEL.ETVStudio);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("NorasOldFlat"))    manager.LoadLevel(GAMELEVEL.NorasOldFlat);
            EditorGUILayout.EndHorizontal();

            // more locations here (train station, train scene)
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("NorasFlat"))    manager.LoadLevel(GAMELEVEL.NorasFlat);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("TawleyMeats"))  manager.LoadLevel(GAMELEVEL.TawleyMeats);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("RoarkOutside")) manager.LoadLevel(GAMELEVEL.RoarkOutside);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("RoarkInside"))  manager.LoadLevel(GAMELEVEL.RoarkInside);
            EditorGUILayout.EndHorizontal();

            // EditorGUILayout.BeginHorizontal();
            // if (GUILayout.Button("SecretLevel"))  manager.LoadLevel(GAMELEVEL.SecretLevel);
            // EditorGUILayout.EndHorizontal();
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Scene change and fade buttons work in Play Mode only.", MessageType.Info);
        }
    }
}

#endif