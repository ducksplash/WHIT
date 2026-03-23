using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Attach to any TextMeshProUGUI object.
/// Call PlayText() from DialogueManager instead of setting .text directly.
/// Each character slides in from the right and fades up to its final position.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPTypewriter : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds between each character appearing.")]
    public float charDelay = 0.04f;

    [Tooltip("How long each character takes to slide to its final position.")]
    public float charSlideDuration = 0.12f;

    [Header("Slide")]
    [Tooltip("How far right (in TMP local units) each character starts from.")]
    public float slideOffsetX = 18f;

    [Tooltip("Optional upward starting offset.")]
    public float slideOffsetY = 0f;

    [Header("Easing")]
    [Tooltip("If true, uses SmoothStep. If false, uses linear.")]
    public bool smoothStep = true;

    // ── internal ──────────────────────────────────────────────────────────
    private TextMeshProUGUI _tmp;
    private Coroutine       _animCoroutine;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Set text immediately with no animation.
    /// </summary>
    public void SetImmediate(string text)
    {
        StopAnimation();
        _tmp.text  = text;
        _tmp.alpha = 1f;
    }

    /// <summary>
    /// Clear the text field.
    /// </summary>
    public void Clear()
    {
        StopAnimation();
        _tmp.text = "";
    }

    /// <summary>
    /// Animate text in. Returns a Task that completes when all characters
    /// have finished animating (or the token is cancelled).
    /// Drop-in replacement for setting .text directly.
    /// </summary>
    public Task PlayText(string text, CancellationToken token = default)
    {
        StopAnimation();

        var tcs = new TaskCompletionSource<bool>();
        _animCoroutine = StartCoroutine(AnimateCoroutine(text, 0, token, tcs));
        return tcs.Task;
    }

    /// <summary>
    /// Sets <paramref name="prefix"/> instantly at full alpha, then animates
    /// <paramref name="body"/> character by character.
    /// Use this for "ContactName: message" strings so the name is immediately readable.
    /// </summary>
    public Task PlayTextWithPrefix(string prefix, string body, CancellationToken token = default)
    {
        StopAnimation();

        var tcs = new TaskCompletionSource<bool>();
        _animCoroutine = StartCoroutine(AnimateCoroutine(prefix + body, prefix.Length, token, tcs));
        return tcs.Task;
    }

    /// <summary>
    /// Stop any running animation immediately and leave text as-is.
    /// </summary>
    public void StopAnimation()
    {
        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }
    }

    // ── Core coroutine ────────────────────────────────────────────────────
    private System.Collections.IEnumerator AnimateCoroutine(
        string text,
        int    instantUpTo,         // TMP character index below which chars are revealed instantly
        CancellationToken token,
        TaskCompletionSource<bool> tcs)
    {
        _tmp.text = text;
        _tmp.alpha = 1f;
        _tmp.ForceMeshUpdate();

        TMP_TextInfo info       = _tmp.textInfo;
        int          totalChars = info.characterCount;

        // ── Pass 1: hide every visible character that should animate ─────────
        for (int i = 0; i < totalChars; i++)
        {
            if (!info.characterInfo[i].isVisible) continue;

            int      matIdx = info.characterInfo[i].materialReferenceIndex;
            int      vtxIdx = info.characterInfo[i].vertexIndex;
            Color32[] cols  = info.meshInfo[matIdx].colors32;

            // Characters inside the instant prefix stay fully visible
            byte alpha = (i < instantUpTo) ? (byte)255 : (byte)0;
            for (int v = 0; v < 4; v++)
                cols[vtxIdx + v].a = alpha;
        }
        _tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // ── Pass 2: animate each character in sequence ──────────────────────
        for (int i = 0; i < totalChars; i++)
        {
            // Skip prefix characters — already fully visible
            if (i < instantUpTo) continue;

            if (token.IsCancellationRequested)
            {
                RevealAll();
                tcs.SetResult(false);
                yield break;
            }

            TMP_CharacterInfo charInfo = info.characterInfo[i];

            // Spaces / invisible glyphs: just wait the delay, no slide
            if (!charInfo.isVisible)
            {
                if (charDelay > 0f) yield return new WaitForSeconds(charDelay);
                continue;
            }

            int matIdx = charInfo.materialReferenceIndex;
            int vtxIdx = charInfo.vertexIndex;

            // Snapshot baked vertex positions
            Vector3[] meshVerts = info.meshInfo[matIdx].vertices;
            var origin = new Vector3[4];
            for (int v = 0; v < 4; v++)
                origin[v] = meshVerts[vtxIdx + v];

            // ── Slide + fade animation for this one character ───────────────
            float elapsed = 0f;
            while (elapsed < charSlideDuration)
            {
                if (token.IsCancellationRequested)
                {
                    RevealAll();
                    tcs.SetResult(false);
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / charSlideDuration);
                float s = smoothStep ? Mathf.SmoothStep(0f, 1f, t) : t;

                float   ox    = Mathf.Lerp(slideOffsetX, 0f, s);
                float   oy    = Mathf.Lerp(slideOffsetY, 0f, s);
                byte    alpha = (byte)(s * 255f);

                meshVerts = _tmp.textInfo.meshInfo[matIdx].vertices;
                Color32[] cols = _tmp.textInfo.meshInfo[matIdx].colors32;

                for (int v = 0; v < 4; v++)
                {
                    meshVerts[vtxIdx + v] = origin[v] + new Vector3(ox, oy, 0f);
                    cols[vtxIdx + v].a    = alpha;
                }

                _tmp.UpdateVertexData(
                    TMP_VertexDataUpdateFlags.Vertices |
                    TMP_VertexDataUpdateFlags.Colors32);

                yield return null;
            }

            // Settle exactly at final position / full alpha
            meshVerts = _tmp.textInfo.meshInfo[matIdx].vertices;
            Color32[] finalCols = _tmp.textInfo.meshInfo[matIdx].colors32;

            for (int v = 0; v < 4; v++)
            {
                meshVerts[vtxIdx + v]   = origin[v];
                finalCols[vtxIdx + v].a = 255;
            }

            _tmp.UpdateVertexData(
                TMP_VertexDataUpdateFlags.Vertices |
                TMP_VertexDataUpdateFlags.Colors32);

            if (charDelay > 0f)
                yield return new WaitForSeconds(charDelay);
        }

        _animCoroutine = null;
        tcs.SetResult(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Instantly snap all characters to full alpha / correct position.
    /// Called when a cancellation comes in mid-animation so text isn't left invisible.
    /// </summary>
    private void RevealAll()
    {
        _tmp.ForceMeshUpdate();
        TMP_TextInfo info = _tmp.textInfo;

        for (int i = 0; i < info.characterCount; i++)
        {
            if (!info.characterInfo[i].isVisible) continue;

            int       matIdx = info.characterInfo[i].materialReferenceIndex;
            int       vtxIdx = info.characterInfo[i].vertexIndex;
            Color32[] cols   = info.meshInfo[matIdx].colors32;

            for (int v = 0; v < 4; v++)
                cols[vtxIdx + v].a = 255;
        }

        _tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        _animCoroutine = null;
    }
}
