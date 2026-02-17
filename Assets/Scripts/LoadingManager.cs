using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    private Coroutine _fadeCo;
    private bool _isLoading;

    // scene-load handshake
    private bool _sceneLoadedFlag;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

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
        if (_isLoading) return;
        StartCoroutine(LoadLevelSequence(levelName, onFinished));
    }

    // ---------------------------------------------------------------------
    // Master sequence
    // ---------------------------------------------------------------------
    private IEnumerator LoadLevelSequence(GAMELEVEL levelName, Action onFinished)
    {
        _isLoading = true;

        // 1) Fade to black and wait
        yield return FadeToAndWait(1f);

        // 2) Show loading UI
        ShowLoadingUI(true);
        UpdateLoadingClock();

        // 3) Load scene (controlled activation)
        yield return ChangeSceneAsync(levelName);

        // 4) Ensure fade is still black before removing loading UI
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

        // 6) Fade in and wait (this is the first frame where loading UI is definitely gone)
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

        float startAlpha = fadeCanvas.alpha;

        if (duration <= 0f || Mathf.Approximately(startAlpha, targetAlpha))
        {
            fadeCanvas.alpha = targetAlpha;
            _fadeCo = null;
            yield break;
        }

        float t = 0f;

        while (t < duration)
        {
            // ✅ Key fix: clamp dt so scene-load stalls don't "jump" the fade
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

    // ---------------------------------------------------------------------
    // Scene loading (controlled activation)
    // ---------------------------------------------------------------------
    private IEnumerator ChangeSceneAsync(GAMELEVEL levelName)
    {
        _sceneLoadedFlag = false;

        Time.timeScale = 0f;

        AsyncOperation op = SceneManager.LoadSceneAsync(levelName.ToString());
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (loadingbar != null)
                loadingbar.fillAmount = Mathf.Clamp01(op.progress / 0.9f);

            yield return null;
        }

        if (loadingbar != null) loadingbar.fillAmount = 1f;

        Transform playerTransform = Player.Instance != null
            ? Player.Instance.gameObject.GetComponentInParent<Transform>()
            : null;

        ApplySpawn(levelName, playerTransform);

        // Activate scene (this is where Unity often stalls)
        op.allowSceneActivation = true;

        while (!_sceneLoadedFlag)
            yield return null;

        GameMaster.Instance.THISLEVEL = levelName;

        Time.timeScale = 1f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _sceneLoadedFlag = true;

        if (GameMaster.Instance != null)
            GameMaster.Instance.PLAYERBUSY = false;
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

    private void ApplySpawn(GAMELEVEL levelName, Transform playerTransform)
    {
        if (playerTransform == null || GameMaster.Instance == null || Player.Instance == null)
            return;

        if (levelName == GAMELEVEL.NorasFlat)
        {
            playerTransform.position = GameMaster.Instance.SPAWNPOINTNORASFLAT;
            Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;
        }
        else if (levelName == GAMELEVEL.TawleyMeats)
        {
            playerTransform.position = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
            Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
        }
        else if (levelName == GAMELEVEL.RoarkInside)
        {
            playerTransform.position = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
            Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
        }
        else if (levelName == GAMELEVEL.RoarkOutside)
        {
            playerTransform.position = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
            Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
        }
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
