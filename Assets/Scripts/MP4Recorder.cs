using UnityEngine;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder.Encoder;
#endif
using UnityEngine.InputSystem;
public class MP4Recorder : MonoBehaviour
{
    
#if UNITY_EDITOR
    [Header("Recording")]
    public bool recordOnStart = false;

    [Header("Resolution")]
    public int width = 1920;
    public int height = 1080;

    [Header("Framerate")]
    public int frameRate = 60;

    public InputActionReference toggleRecording;
    
    [Header("Output")]
    public string outputFolder = "Recordings";
    public string fileName = "Recording";

    private RecorderController _controller;

    [SerializeField] private CanvasGroup recCanvas;
    
    private bool isRecording;

    private Coroutine recReminderCo;
    
    
    private void Start()
    {
        recCanvas.alpha = 0;
        
        if (recordOnStart)
        {
            ToggleRecording();
        }

        toggleRecording.action.performed += ToggleRecording;

    }


    private void ToggleRecording(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        isRecording = !isRecording;

        if (isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    private IEnumerator RecReminder()
    {
        while (isRecording)
        {
            recCanvas.alpha = 1;
            yield return new WaitForSeconds(1f);
            recCanvas.alpha = 0;
            yield return new WaitForSeconds(1f);
            recCanvas.alpha = 1;
            yield return new WaitForSeconds(1f);
            recCanvas.alpha = 0;
        }

        yield return new WaitForEndOfFrame();
    }
    

    public void StartRecording()
    {
        if (recReminderCo != null)
        {
            StopCoroutine(recReminderCo);
            recReminderCo = null;
        }

        recReminderCo = StartCoroutine(RecReminder());
        
        if (_controller != null && _controller.IsRecording()) return;

        var controllerSettings =
            ScriptableObject.CreateInstance<RecorderControllerSettings>();

        _controller = new RecorderController(controllerSettings);

        var movieRecorder =
            ScriptableObject.CreateInstance<MovieRecorderSettings>();

        movieRecorder.name = "MP4 Recorder";
        movieRecorder.Enabled = true;

        // NEW ENCODER API
        movieRecorder.EncoderSettings =
            new CoreEncoderSettings
            {
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High
            };

        movieRecorder.ImageInputSettings =
            new GameViewInputSettings
            {
                OutputWidth = width,
                OutputHeight = height
            };

        movieRecorder.AudioInputSettings.PreserveAudio = true;
        
        string timestamp = System.DateTime.Now.ToString("dd MMMM yyyy HH-mm-ss");

        string finalFileName = $"{fileName}_{timestamp}";

        movieRecorder.OutputFile = Path.Combine(Application.persistentDataPath, finalFileName);
        
        controllerSettings.AddRecorderSettings(movieRecorder);

        controllerSettings.SetRecordModeToManual();

        controllerSettings.FrameRate = frameRate;
        controllerSettings.CapFrameRate = true;

        _controller.PrepareRecording();
        _controller.StartRecording();

        Debug.Log("MP4 Recording Started");
    }

    public void StopRecording()
    {
        if (_controller == null)
            return;

        if (_controller.IsRecording())
        {
            _controller.StopRecording();

            Debug.Log("MP4 Recording Stopped");
        }
    }
    
#endif
}