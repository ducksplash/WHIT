using UnityEngine;
using System.Collections;

public class SawOperator : MonoBehaviour
{
    public float BladeRPM = 250f;
    public Transform BladeTransform;
    public BladeRotationAxis BladeRotationAxis = BladeRotationAxis.y;
    
    public bool Extending = false;
    public Transform Bracket;
    public float ExtensionSpeed = 0.2f;   // units per second
    public float WaitingTime = 2f;
    public float MaximumExtension = 0.5f;

    // Fixed reference – captured once and never changed
    private float startY;
    private bool startYCaptured = false;

    private Coroutine extensionRoutine;

    void Start()
    {
        CaptureStartY();

        if (Extending)
            StartExtensionCycle();
    }

    /// <summary>
    /// Captures the exact local Y that will always be the “home” position.
    /// Safe to call multiple times – it only stores the value the first time.
    /// </summary>
    private void CaptureStartY()
    {
        if (Bracket == null || startYCaptured) return;

        startY = Bracket.localPosition.y;
        startYCaptured = true;
    }

    void LateUpdate()
    {
        if (BladeTransform != null)
        {
            
            switch (BladeRotationAxis)
            {
                case BladeRotationAxis.x:
                    BladeTransform.Rotate(BladeRPM * 6f * Time.deltaTime, 0f, 0f, Space.Self);
                    break;
                case BladeRotationAxis.y:
                    BladeTransform.Rotate(0f, BladeRPM * 6f * Time.deltaTime, 0f, Space.Self);
                    break;
                case BladeRotationAxis.z:
                    BladeTransform.Rotate(0f, 0f, BladeRPM * 6f * Time.deltaTime, Space.Self);
                    break;

            }
            
        }
    }

    public void StartExtensionCycle()
    {
        if (Bracket == null) return;

        // Guarantee we have a locked home height
        CaptureStartY();

        if (extensionRoutine != null)
            StopCoroutine(extensionRoutine);

        extensionRoutine = StartCoroutine(ExtensionCycle());
    }

    private IEnumerator ExtensionCycle()
    {
        while (true)
        {
            // Go out
            yield return LerpY(startY, startY + MaximumExtension);

            yield return new WaitForSeconds(WaitingTime);

            // Come home – always to the exact captured startY
            yield return LerpY(startY + MaximumExtension, startY);

            // Hard-enforce home position after the wait (kills any external drift)
            SetY(startY);

            yield return new WaitForSeconds(WaitingTime);
        }
    }

    private IEnumerator LerpY(float from, float to)
    {
        yield return new WaitForEndOfFrame();
        
        float distance = Mathf.Abs(to - from);
        if (distance < 0.0001f)
        {
            SetY(to);
            yield break;
        }

        float duration = distance / ExtensionSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Read current X/Z every frame so we never overwrite other axes with stale data
            Vector3 pos = Bracket.localPosition;
            pos.y = Mathf.Lerp(from, to, t);
            Bracket.localPosition = pos;

            yield return null;
        }

        // Final snap – exact value, no floating-point residue
        SetY(to);
    }

    /// <summary>
    /// Writes only the Y component, leaving X and Z untouched.
    /// </summary>
    private void SetY(float y)
    {
        Vector3 pos = Bracket.localPosition;
        pos.y = y;
        Bracket.localPosition = pos;
    }
}

public enum BladeRotationAxis
{
    x,
    y,
    z
}