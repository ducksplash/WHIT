using Cysharp.Threading.Tasks;
using UnityEngine;

public class RectTransformChaChaSlide : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private float startX = 4000f;
    [SerializeField] private float endX = 0f;
    [SerializeField] private float duration = 1f;
    
    [Header("Ease")]
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Start()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();


        EventManager.OnSlideTickerIn += SlideIn;
        EventManager.OnSlideTickerOut += SlideOut;
    }

    private void SlideIn()
    {
        SlideInAsync();
    }


    private void SlideOut()
    {
        SlideOutAsync();
    }


    public async UniTask SlideInAsync()
    {
        if (targetRect == null) return;

        float elapsed = 0f;
        Vector2 startPosition = new Vector2(startX, targetRect.anchoredPosition.y);
        Vector2 endPosition = new Vector2(endX, targetRect.anchoredPosition.y);

        targetRect.anchoredPosition = startPosition;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Apply easing
            float t = easeCurve.Evaluate(normalizedTime);
            
            float currentX = Mathf.Lerp(startX, endX, t);
            targetRect.anchoredPosition = new Vector2(currentX, targetRect.anchoredPosition.y);
        }

        // Ensure final position
        targetRect.anchoredPosition = endPosition;
    }


    public async UniTask SlideOutAsync()
    {
        if (targetRect == null) return;

        float elapsed = 0f;
        Vector2 startPosition = new Vector2(endX, targetRect.anchoredPosition.y);
        Vector2 endPosition = new Vector2(startX, targetRect.anchoredPosition.y);

        targetRect.anchoredPosition = startPosition;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Apply easing
            float t = easeCurve.Evaluate(normalizedTime);
            
            float currentX = Mathf.Lerp(startX, endX, t);
            targetRect.anchoredPosition = new Vector2(currentX, targetRect.anchoredPosition.y);
        }

        // Ensure final position
        targetRect.anchoredPosition = endPosition;
    }


    
}