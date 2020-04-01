using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>VideoCapture</c> component, manage and record gameplay video from specific camera.
  /// Work with ffmpeg encoder or GPU encoder component to generate gameplay videos.
  /// </summary>
  public class VideoCapture : MonoBehaviour, IVideoCapture
  {

    #region Properties

    [Header("Capture Controls")]

    // Start capture on awake if set to true.
    public bool startOnAwake = false;
    // Quit process after capture finish.
    public bool quitAfterCapture = false;
    // Get or set the current status.
    public CaptureStatus status { get; protected set; }
    // The capture duration if start capture on awake.
    public float captureTime = 30f;
    // Callback for complete handling.
    public event VideoCaptureCompleteEvent OnComplete = delegate { };
    // Callback for error handling.
    public event VideoCaptureErrorEvent OnError = delegate { };

    [Header("Capture Options")]

    // If set live streaming mode, encoded video will be push to remote streaming url instead of save to local file.
    public VideoCaptureType videoCaptureType = VideoCaptureType.VOD;
    [Tooltip("Save folder for recorded video")]
    // Save path for recorded video including file name (c://xxx.mp4)
    public string saveFolder = Config.saveFolder;
    // You can get test live stream key on "https://www.facebook.com/live/create".
    // ex. rtmp://rtmp-api-dev.facebook.com:80/rtmp/xxStreamKeyxx
    public string liveStreamUrl = "";
    // The type of video capture mode, regular or 360.
    public CaptureMode captureMode = CaptureMode.REGULAR;
    // The type of video projection, used for 360 video capture.
    public ProjectionType projectionType = ProjectionType.NONE;
    // The type of video capture stereo mode, left right or top bottom.
    public StereoMode stereoMode = StereoMode.NONE;
    // Stereo mode settings.
    // Average IPD of all subjects in US Army survey in meters
    public float interpupillaryDistance = 0.0635f;
    // Audio capture settings, set false if you want to mute audio.
    public bool captureAudio = true;
    // Capture microphone settings
    public bool captureMicrophone = false;
    // Setup Time.maximumDeltaTime to avoiding nasty stuttering.
    // https://docs.unity3d.com/ScriptReference/Time-maximumDeltaTime.html
    public bool offlineRender = false;
    // The original maximum delta time
    private float originalMaximumDeltaTime;

    /// <summary>
    /// Encoding setting variables for video capture.
    /// </summary>
    [Header("Video Settings")]
    // Resolution preset settings, set custom for other resolutions
    public ResolutionPreset resolutionPreset = ResolutionPreset.CUSTOM;
    public CubemapFaceSize cubemapFaceSize = CubemapFaceSize._1024;
    private Int32 cubemapSize = 1024;
    public Int32 frameWidth = 1280;
    public Int32 frameHeight = 720;
    [Tooltip("Video bitrate in kbps")]
    public Int32 bitrate = 2000;
    public Int16 frameRate = 24;
    public AntiAliasingSetting antiAliasingSetting = AntiAliasingSetting._1;
    private Int16 antiAliasing = 1;

    /// <summary>
    /// Encoder components for video encoding.
    /// </summary>
    [Header("Encoder Components")]
    // Only use software encoder for video encoding
    public bool softwareEncodingOnly = false;
    // FFmpeg Encoder
    public FFmpegEncoder ffmpegEncoder;
    // GPU Encoder
    public GPUEncoder gpuEncoder;

    // The garbage collection thread.
    private Thread garbageCollectionThread;
    public static bool garbageThreadRunning = false;

    // Use hardware encoding
    private bool hardwareEncoding = false;

    // Log message format template
    private string LOG_FORMAT = "[VideoCapture] {0}";

    #endregion

    #region Video Capture

    /// <summary>
    /// Initialize the attributes of the capture session and start capture.
    /// </summary>
    public bool StartCapture()
    {
      if (status != CaptureStatus.READY)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Previous video capture session not finish yet!");
        OnError(this, CaptureErrorCode.VIDEO_CAPTURE_ALREADY_IN_PROGRESS);
        return false;
      }

      if (!File.Exists(Config.ffmpegPath))
      {
        Debug.LogErrorFormat(LOG_FORMAT,
          "FFmpeg not found, please follow document and add ffmpeg executable before start capture!");
        OnError(this, CaptureErrorCode.FFMPEG_NOT_FOUND);
        return false;
      }

      if (string.IsNullOrEmpty(saveFolder))
        saveFolder = Config.saveFolder;
      else
        Config.saveFolder = saveFolder;

      if (captureMode == CaptureMode._360)
      {
        if (projectionType == ProjectionType.NONE)
        {
          Debug.LogFormat(LOG_FORMAT,
            "Projection type should be set for 360 capture, set type to equirect for generating texture properly");
          projectionType = ProjectionType.EQUIRECT;
        }
        if (projectionType == ProjectionType.CUBEMAP)
        {
          if (stereoMode != StereoMode.NONE)
          {
            Debug.LogFormat(LOG_FORMAT,
              "Stereo settings not support for cubemap capture, reset to mono video capture.");
            stereoMode = StereoMode.NONE;
          }
        }
        CubemapSizeSettings();
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      if (frameRate < 18)
      {
        frameRate = 18;
        Debug.LogFormat(LOG_FORMAT, "Minimum frame rate is 18, set frame rate to 18.");
      }

      if (frameRate > 120)
      {
        frameRate = 120;
        Debug.LogFormat(LOG_FORMAT, "Maximum frame rate is 120, set frame rate to 120.");
      }

      AntiAliasingSettings();

      if (captureAudio && offlineRender)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture not supported in offline render mode, disable audio capture!");
        captureAudio = false;
      }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (Config.isFreeTrial())
      {
        Debug.LogFormat(LOG_FORMAT, "GPU encoding is not supported in free trial version, fall back to software encoding.");
        hardwareEncoding = false;
      }
      else if (!softwareEncodingOnly &&
        gpuEncoder.instantiated &&
        gpuEncoder.IsSupported())
      {
        hardwareEncoding = true;
      }
      else
      {
        Debug.LogFormat(LOG_FORMAT, "GPU encoding is not supported in this device, fall back to software encoding.");
      }
#endif

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
      Debug.LogFormat(LOG_FORMAT, "GPU encoding is not supported on macOS system, fall back to software encoding.");
      hardwareEncoding = false;
#endif

      // Init ffmpeg audio capture
      if (!hardwareEncoding && captureAudio && !FFmpegMuxer.singleton)
      {
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (!listener)
        {
          Debug.LogFormat(LOG_FORMAT, "AudioListener not found, disable audio capture!");
          captureAudio = false;
        }
        else
        {
          listener.gameObject.AddComponent<FFmpegMuxer>();
        }
      }

      if (hardwareEncoding)
      {
        // init GPU encoding settings
        GPUEncoderSettings();

        if (!gpuEncoder.StartCapture())
        {
          OnError(this, CaptureErrorCode.VIDEO_CAPTURE_START_FAILED);
          return false;
        }
      }
      else
      {
        // init ffmpeg encoding settings
        FFmpegEncoderSettings();

        if (!ffmpegEncoder.StartCapture())
        {
          OnError(this, CaptureErrorCode.VIDEO_CAPTURE_START_FAILED);
          return false;
        }

        if (captureAudio)
        {
          // start ffmpeg audio encoding
          if (!FFmpegMuxer.singleton.captureStarted)
          {
            FFmpegMuxer.singleton.StartCapture();
          }
          FFmpegMuxer.singleton.AttachVideoCapture(this);
        }
      }

      // Update current status.
      status = CaptureStatus.STARTED;

      // Start garbage collect thread.
      if (!garbageThreadRunning)
      {
        garbageThreadRunning = true;

        if (garbageCollectionThread != null &&
          garbageCollectionThread.IsAlive)
        {
          garbageCollectionThread.Abort();
          garbageCollectionThread = null;
        }


        garbageCollectionThread = new Thread(GarbageCollectionProcess);
        garbageCollectionThread.Priority = System.Threading.ThreadPriority.Lowest;
        garbageCollectionThread.IsBackground = true;
        garbageCollectionThread.Start();
      }

      if (offlineRender)
      {
        // Backup maximumDeltaTime states.
        originalMaximumDeltaTime = Time.maximumDeltaTime;
        Time.maximumDeltaTime = Time.fixedDeltaTime;
      }

      Debug.LogFormat(LOG_FORMAT, "Video capture session started.");
      return true;
    }

    /// <summary>
    /// Stop capturing and produce the finalized video. Note that the video file may not be completely written when this method returns. In order to know when the video file is complete, register <c>OnComplete</c> delegate.
    /// </summary>
    public bool StopCapture()
    {
      if (status != CaptureStatus.STARTED)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Video capture session not start yet!");
        return false;
      }

      if (offlineRender)
      {
        // Restore maximumDeltaTime states.
        Time.maximumDeltaTime = originalMaximumDeltaTime;
      }

      // pending for video encoding process
      status = CaptureStatus.STOPPED;

      if (hardwareEncoding && gpuEncoder.captureStarted)
      {
        gpuEncoder.StopCapture();
      }

      if (!hardwareEncoding && ffmpegEncoder.captureStarted)
      {
        ffmpegEncoder.StopCapture();

        if (captureAudio && FFmpegMuxer.singleton && FFmpegMuxer.singleton.captureStarted)
        {
          FFmpegMuxer.singleton.StopCapture();
        }

        Debug.LogFormat(LOG_FORMAT, "Video capture session stopped, generating video...");
      }

      return true;
    }

    private void GPUEncoderSettings()
    {
      gpuEncoder.videoCaptureType = videoCaptureType;
      gpuEncoder.captureMode = captureMode;
      gpuEncoder.resolutionPreset = resolutionPreset;
      gpuEncoder.frameWidth = frameWidth;
      gpuEncoder.frameHeight = frameHeight;
      gpuEncoder.cubemapSize = cubemapSize;
      gpuEncoder.bitrate = bitrate;
      gpuEncoder.frameRate = frameRate;
      gpuEncoder.projectionType = projectionType;
      gpuEncoder.liveStreamUrl = liveStreamUrl;
      gpuEncoder.stereoMode = stereoMode;
      gpuEncoder.interpupillaryDistance = interpupillaryDistance;
      gpuEncoder.captureAudio = captureAudio;
      gpuEncoder.captureMic = captureMicrophone;
      gpuEncoder.antiAliasing = antiAliasing;
    }

    private void FFmpegEncoderSettings()
    {
      ffmpegEncoder.videoCaptureType = videoCaptureType;
      ffmpegEncoder.captureMode = captureMode;
      ffmpegEncoder.resolutionPreset = resolutionPreset;
      ffmpegEncoder.frameWidth = frameWidth;
      ffmpegEncoder.frameHeight = frameHeight;
      ffmpegEncoder.cubemapSize = cubemapSize;
      ffmpegEncoder.bitrate = bitrate;
      ffmpegEncoder.frameRate = frameRate;
      ffmpegEncoder.projectionType = projectionType;
      ffmpegEncoder.liveStreamUrl = liveStreamUrl;
      ffmpegEncoder.stereoMode = stereoMode;
      ffmpegEncoder.interpupillaryDistance = interpupillaryDistance;
      ffmpegEncoder.captureAudio = captureAudio;
      ffmpegEncoder.captureMic = captureMicrophone;
      ffmpegEncoder.antiAliasing = antiAliasing;
    }

    private void CubemapSizeSettings()
    {
      if (cubemapFaceSize == CubemapFaceSize._512)
      {
        cubemapSize = 512;
      }
      else if (cubemapFaceSize == CubemapFaceSize._1024)
      {
        cubemapSize = 1024;
      }
      else if (cubemapFaceSize == CubemapFaceSize._2048)
      {
        cubemapSize = 2048;
      }
    }

    private void AntiAliasingSettings()
    {
      if (antiAliasingSetting == AntiAliasingSetting._1)
      {
        antiAliasing = 1;
      }
      else if (antiAliasingSetting == AntiAliasingSetting._2)
      {
        antiAliasing = 2;
      }
      else if (antiAliasingSetting == AntiAliasingSetting._4)
      {
        antiAliasing = 4;
      }
      else if (antiAliasingSetting == AntiAliasingSetting._8)
      {
        antiAliasing = 8;
      }
    }

    public FFmpegEncoder GetFFmpegEncoder()
    {
      return ffmpegEncoder;
    }

    public GPUEncoder GetGPUEncoder()
    {
      return gpuEncoder;
    }

    /// <summary>
    /// Handle callbacks for the video encoder complete.
    /// </summary>
    /// <param name="savePath">Video save path.</param>
    private void OnEncoderComplete(string savePath)
    {
      if (hardwareEncoding || !captureAudio) // No audio capture required, done!
      {
        status = CaptureStatus.READY;

        OnComplete(this, savePath);

        Debug.LogFormat(LOG_FORMAT, "Video capture session success!");
      }
      else
      {
        // Pending for ffmpeg audio capture and muxing
        status = CaptureStatus.PENDING;
      }
    }

    /// <summary>
    /// Handle audio process complete when capture audio.
    /// </summary>
    /// <param name="savePath">Final muxing video path.</param>
    public void OnAudioMuxingComplete(string savePath)
    {
      status = CaptureStatus.READY;

      OnComplete(this, savePath);

      Debug.LogFormat(LOG_FORMAT, "Video generated success!");
    }

    /// <summary>
    /// Garbage collection thread function.
    /// </summary>
    void GarbageCollectionProcess()
    {
      while (status != CaptureStatus.READY)
      {
        // TODO, adjust gc interval dynamic.
        Thread.Sleep(1000);
        System.GC.Collect();
      }

      garbageThreadRunning = false;
    }

#endregion

#region Unity Lifecycle

    private void Awake()
    {
      if (ffmpegEncoder == null)
      {
        ffmpegEncoder = GetComponentInChildren<FFmpegEncoder>(true);
        if (ffmpegEncoder == null)
        {
          Debug.LogErrorFormat(LOG_FORMAT,
           "Component FFmpegEncoder not found, please use prefab or follow the document to set up video capture.");
          return;
        }
      }

      if (ffmpegEncoder != null)
        ffmpegEncoder.OnComplete += OnEncoderComplete;

      if (gpuEncoder == null)
      {
        gpuEncoder = GetComponentInChildren<GPUEncoder>(true);
        if (gpuEncoder == null)
        {
          Debug.LogErrorFormat(LOG_FORMAT,
           "Component hardware encoder not found, please use prefab or follow the document to set up video capture.");
        }
      }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (gpuEncoder != null)
      {
        gpuEncoder.gameObject.SetActive(true);
        gpuEncoder.OnComplete += OnEncoderComplete;
      }
#endif

      status = CaptureStatus.READY;

      if (startOnAwake)
      {
        StartCapture();
      }
    }

    private void Update()
    {
      if (startOnAwake)
      {
        if (Time.time >= captureTime && status == CaptureStatus.STARTED)
        {
          StopCapture();
        }
        if (quitAfterCapture && status == CaptureStatus.READY)
        {
#if UNITY_EDITOR
          UnityEditor.EditorApplication.isPlaying = false;
#else
          Application.Quit();
#endif
        }
      }
    }

    private void OnDestroy()
    {
      // Check if still processing on destroy
      if (status == CaptureStatus.STARTED)
      {
        StopCapture();
      }

      if (ffmpegEncoder != null)
        ffmpegEncoder.OnComplete -= OnEncoderComplete;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (gpuEncoder != null)
        gpuEncoder.OnComplete -= OnEncoderComplete;
#endif
    }

    private void OnApplicationQuit()
    {
      // Check if still processing on application quit
      if (status == CaptureStatus.STARTED)
      {
        StopCapture();
      }
    }

#endregion

  }
}
