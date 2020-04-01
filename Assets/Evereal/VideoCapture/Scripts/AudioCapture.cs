using System.IO;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// This script will record target audio listener sample and encode to audio file, or mux audio into video file if required.
  /// </summary>
  public class AudioCapture : MonoBehaviour, IAudioCapture
  {
    #region Properties

    public event AudioCaptureCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event AudioCaptureErrorEvent OnError = delegate { };

    [Tooltip("Save folder for recorded audio")]
    // Save path for recorded video including file name (c://xxx.wav)
    public string saveFolder = Config.saveFolder;

    // The audio sample rate
    public SampleRate sampleRate = SampleRate._44100;

    // Capture microphone settings
    public bool captureMicrophone = false;

    // Is audio capture started
    public bool captureStarted
    {
      get
      {
        return FFmpegMuxer.singleton.captureStarted;
      }
    }

    // Log message format template
    private string LOG_FORMAT = "[AudioCapture] {0}";

    #endregion

    #region Methods

    // Start capture audio session
    public bool StartCapture()
    {
      if (!FFmpegMuxer.singleton)
      {
        OnError(this, CaptureErrorCode.AUDIO_CAPTURE_START_FAILED);
        return false;
      }

      // Check if we can start capture session
      if (captureStarted)
      {
        OnError(this, CaptureErrorCode.AUDIO_CAPTURE_ALREADY_IN_PROGRESS);
        return false;
      }

      if (string.IsNullOrEmpty(saveFolder))
        saveFolder = Config.saveFolder;
      else
        Config.saveFolder = saveFolder;

      // init audio encoder settings
      FFmpegMuxer.singleton.sampleRate = sampleRate;
      FFmpegMuxer.singleton.captureMic = captureMicrophone;

      FFmpegMuxer.singleton.StartCapture();

      Debug.LogFormat(LOG_FORMAT, "Audio capture session started.");

      return true;
    }

    // Stop capture audio session
    public bool StopCapture()
    {
      if (!captureStarted)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture session not start yet!");
        return false;
      }

      FFmpegMuxer.singleton.StopCapture();

      OnComplete(this, FFmpegMuxer.singleton.audioSavePath);

      Debug.LogFormat(LOG_FORMAT, "Audio capture session success!");

      return true;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
      if (!FFmpegMuxer.singleton)
      {
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (!listener)
        {
          Debug.LogFormat(LOG_FORMAT, "AudioListener not found, disable audio capture!");
        }
        else
        {
          listener.gameObject.AddComponent<FFmpegMuxer>();
        }
      }
    }

    #endregion
  }
}