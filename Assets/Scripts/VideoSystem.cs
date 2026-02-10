using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoSystem : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public Slider scrubSlider;

    [Header("State (Read Only)")]
    [SerializeField] private PCVideo currentVideo;

    private bool isPrepared;
    private bool isScrubbing;
    private bool wasPlayingBeforeScrub;
    public InputActionReference goBack;

    public Image videoImage;

    private Coroutine playAfterDelayCo;
    
    private const string ResourcesVideoFolder = "VIDEOS";

    private void Awake()
    {
        if (videoPlayer == null) videoPlayer = GetComponentInChildren<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.source = VideoSource.VideoClip;

            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.loopPointReached += OnLoopPointReached;
        }

        if (scrubSlider != null)
        {
            scrubSlider.minValue = 0f;
            scrubSlider.maxValue = 1f;
            scrubSlider.SetValueWithoutNotify(0f);

            // Value changed => seek (but only if scrubbing)
            scrubSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            scrubSlider.onValueChanged.AddListener(OnSliderValueChanged);

            // Add drag callbacks to the slider GameObject
            EnsureDragRelay(scrubSlider.gameObject);
        }
        
        videoImage.color = Color.black;
    }

    private void OnDestroy()
    {
        if (scrubSlider != null) scrubSlider.onValueChanged.RemoveListener(OnSliderValueChanged);

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.loopPointReached -= OnLoopPointReached;
        }
        
        
    }

    private void Update()
    {
        if (videoPlayer == null || scrubSlider == null) return;
        if (!isPrepared || !videoPlayer.isPrepared) return;
        if (isScrubbing) return;

        double length = videoPlayer.length;
        if (length > 0.0001)
        {
            float normalized = (float)(videoPlayer.time / length);
            // IMPORTANT: don't notify, or you'll fight with user + call seek
            scrubSlider.SetValueWithoutNotify(normalized);
        }
    }


    public void LoadVideo(PCVideo video)
    {
        currentVideo = video;
        isPrepared = false;

        videoImage.color = Color.black;

        goBack.action.performed += InputCloseVideoPlayer;

        
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(true);
        TerminalEventManager.OnVideoControlCommand += ControlVideo;
        
        
        if (videoPlayer == null)
        {
            Debug.LogError("VideoSystem: No VideoPlayer reference assigned.");
            return;
        }

        string resourcePath = $"{ResourcesVideoFolder}/{video}";
        VideoClip clip = Resources.Load<VideoClip>(resourcePath);

        if (clip == null)
        {
            Debug.LogError(
                $"VideoSystem: Could not load VideoClip from Resources at '{resourcePath}'.\n" +
                $"Expected file: Assets/Resources/{resourcePath}.mp4"
            );
            return;
        }

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        if (scrubSlider != null) scrubSlider.SetValueWithoutNotify(0f);

        if (playAfterDelayCo != null)
        {
            StopCoroutine(playAfterDelayCo);
            playAfterDelayCo = null;
        }

        playAfterDelayCo = StartCoroutine(PlayAfterDelay());
    }


    private void ControlVideo(VidControl vidControl)
    {
        switch (vidControl)
        {

            case VidControl.Play: 
                PlayVideo();
                break;
            case VidControl.Pause: 
                PauseVideo();
                break;
            case VidControl.Stop: 
                StopVideo();
                break;
        }
    }
    

    private IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        PlayVideo();
    }
    
    
    public void PlayVideo()
    {
        if (videoPlayer == null) return;

        if (!videoPlayer.isPrepared)
        {
            videoPlayer.prepareCompleted -= AutoPlayAfterPrepare;
            videoPlayer.prepareCompleted += AutoPlayAfterPrepare;
            videoPlayer.Prepare();
            return;
        }

        videoPlayer.Play();
        videoImage.color = Color.white;
    }

    public void PauseVideo()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared) return;

        if (videoPlayer.isPaused)
        {
            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Pause();
        }
    }

    public void StopVideo()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        if (scrubSlider != null) scrubSlider.SetValueWithoutNotify(0f);
    }

    public void ClosePlayer()
    {
        Debug.Log("VideoSystem: ClosePlayer called (stub).");
    }

    // Called by slider onValueChanged
    private void OnSliderValueChanged(float normalizedValue)
    {
        if (!isScrubbing) return; // <- key: clicks/drags will set isScrubbing via begin drag
        SeekToNormalized(normalizedValue);
    }

    private void SeekToNormalized(float normalizedValue)
    {
        if (videoPlayer == null || !videoPlayer.isPrepared) return;

        normalizedValue = Mathf.Clamp01(normalizedValue);

        double length = videoPlayer.length;
        if (length <= 0.0001) return;

        videoPlayer.time = normalizedValue * length;

        // For immediate visual feedback while paused:
        videoPlayer.Pause();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        isPrepared = true;

        if (scrubSlider != null)
            scrubSlider.SetValueWithoutNotify(0f);

        ShowFirstFrame(); 
    }

    private void OnLoopPointReached(VideoPlayer vp)
    {
        if (scrubSlider != null)
            scrubSlider.SetValueWithoutNotify(1f);
    }

    private void AutoPlayAfterPrepare(VideoPlayer vp)
    {
        vp.prepareCompleted -= AutoPlayAfterPrepare;
        vp.Play();
    }

    // --- Drag relay helper (hooks BeginDrag/EndDrag on the slider) ---

    private void EnsureDragRelay(GameObject sliderGO)
    {
        var relay = sliderGO.GetComponent<SliderDragRelay>();
        if (relay == null) relay = sliderGO.AddComponent<SliderDragRelay>();
        relay.Init(this);
    }

    internal void BeginScrub()
    {
        if (isScrubbing) return;

        isScrubbing = true;

        if (videoPlayer != null && videoPlayer.isPrepared)
        {
            wasPlayingBeforeScrub = videoPlayer.isPlaying;
            videoPlayer.Pause();
        }
    }

    internal void EndScrub()
    {
        if (!isScrubbing) return;

        isScrubbing = false;

        if (videoPlayer != null && videoPlayer.isPrepared && wasPlayingBeforeScrub)
            videoPlayer.Play();
    }

    

    public void UI_Play() => PlayVideo();
    public void UI_Pause() => PauseVideo();
    public void UI_Stop() => StopVideo();

    // Nested helper component to catch drag events on the Slider
    private class SliderDragRelay : MonoBehaviour,
        IPointerDownHandler,
        IBeginDragHandler,
        IEndDragHandler
    {
        private VideoSystem owner;

        public void Init(VideoSystem o) => owner = o;

        // CLICK (jump to position, resume only if playing)
        public void OnPointerDown(PointerEventData eventData)
        {
            if (owner == null) return;
            if (owner.videoPlayer == null || !owner.videoPlayer.isPrepared) return;

            bool wasPlaying = owner.videoPlayer.isPlaying;

            owner.SeekToNormalized(owner.scrubSlider.value);

            if (wasPlaying)
                owner.videoPlayer.Play();
            else
                owner.videoPlayer.Pause();
        }

        // DRAG START (enter scrub mode)
        public void OnBeginDrag(PointerEventData eventData)
        {
            owner?.BeginScrub();
        }

        // DRAG END (exit scrub mode)
        public void OnEndDrag(PointerEventData eventData)
        {
            owner?.EndScrub();
        }
    }


    private void ShowFirstFrame()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared) return;

        // Ensure we’re at the start
        videoPlayer.time = 0;

        // Force a render: play then pause on the next frame
        videoPlayer.Play();
        StartCoroutine(PauseNextFrame());
    }

    private System.Collections.IEnumerator PauseNextFrame()
    {
        // Wait one frame so the VideoPlayer outputs a frame to the target
        yield return null;

        if (videoPlayer == null) yield break;

        videoPlayer.Pause();
    }

    public void CloseVideoPlayer()
    {
        TerminalEventManager.OnVideoControlCommand -= ControlVideo;

        //GameMaster.Instance.TerminalEventManager.BackButtonOverride(false);
        GameMaster.Instance.TerminalEventManager.VideoPlayerClosed();
    }

    private void InputCloseVideoPlayer(InputAction.CallbackContext callbackContext)
    {
        CloseVideoPlayer();
    }

    

}

public enum PCVideo
{
    tigger,
    luna,
    loki,
    rocket
}

public enum VidControl
{
    Play,
    Pause,
    Stop
}
