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

    // Optional: tweak easing in Inspector (0..1 -> 0..1)
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _fadeCo;
    private bool _isLoading;

    private void OnEnable()
    {
        if (fadeCanvas != null) fadeCanvas.alpha = 1f;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Public helpers
    public void SceneFadeIn()  => FadeTo(0f); // 1 -> 0
    public void SceneFadeOut() => FadeTo(1f); // 0 -> 1

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

    private IEnumerator FadeCanvasGroupTo(float targetAlpha, float duration)
    {
        if (fadeCanvas == null) yield break;

        // ✅ Start from wherever we are right now (prevents snapping)
        float startAlpha = fadeCanvas.alpha;

        // If duration is 0 or already at target, snap safely
        if (duration <= 0f || Mathf.Approximately(startAlpha, targetAlpha))
        {
            fadeCanvas.alpha = targetAlpha;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // keep working during Time.timeScale=0
            float normalized = Mathf.Clamp01(t / duration);

            // ✅ consistent easing
            float eased = fadeCurve != null ? fadeCurve.Evaluate(normalized) : Mathf.SmoothStep(0f, 1f, normalized);

            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, eased);
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
        _fadeCo = null;
    }

    // ----------------------------------------------------------------
    // Your existing load logic (unchanged)
    // ----------------------------------------------------------------
    public void LoadLevel(GAMELEVEL levelName, Action onFinished = null)
    {
        if (_isLoading) return;
        StartCoroutine(ChangeSceneAsync(levelName, onFinished));
    }

    private IEnumerator ChangeSceneAsync(GAMELEVEL levelName, Action onFinished)
    {
        _isLoading = true;

        if (loadingpanel != null) loadingpanel.alpha = 1;

        Debug.Log("Loading: " + levelName);

        Transform playerTransform = Player.Instance.gameObject.GetComponentInParent<Transform>();
        ApplySpawn(levelName, playerTransform);

        Time.timeScale = 0;

        AsyncOperation op = SceneManager.LoadSceneAsync(levelName.ToString());

        if (loadingclock != null)
        {
            string buildDate = "";
            buildDate += DateTime.Now.ToString("dddd");
            buildDate += ", ";
            buildDate += DateTime.Now.ToString("MMMM d");
            buildDate += MonthDay(DateTime.Now.ToString("dd"));
            buildDate += ", ";
            buildDate += DateTime.Now.ToString("yyyy");
            loadingclock.text = buildDate;
        }

        while (!op.isDone)
        {
            if (loadingbar != null)
                loadingbar.fillAmount = Mathf.Clamp01(op.progress / 0.9f);

            yield return null;
        }

        GameMaster.Instance.THISLEVEL = levelName;

        if (loadingpanel != null) loadingpanel.alpha = 0;

        Time.timeScale = 1;

        _isLoading = false;
        onFinished?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameMaster.Instance.PLAYERBUSY = false;
    }

    private void ApplySpawn(GAMELEVEL levelName, Transform playerTransform)
    {
        if (playerTransform == null) return;

        if (levelName == GAMELEVEL.NorasFlat)
        {
            playerTransform.position = GameMaster.Instance.SPAWNPOINTNORASFLAT;
            Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;
        }

        if (levelName == GAMELEVEL.TawleyMeats)
        {
            playerTransform.position = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
            Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
        }

        if (levelName == GAMELEVEL.RoarkInside)
        {
            playerTransform.position = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
            Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
        }

        if (levelName == GAMELEVEL.RoarkOutside)
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
