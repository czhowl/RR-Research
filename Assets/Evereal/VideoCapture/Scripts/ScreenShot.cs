using System;
using System.Threading;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>ScreenShot</c> component, manage and record game screenshot from specific camera.
  /// Work with software encoder or GPU encoder component to generate screenshot.
  /// </summary>
  public class ScreenShot : MonoBehaviour, IScreenShot
  {
    #region Properties

    private bool _captureStarted;

    public bool captureStarted
    {
      get
      {
        return _captureStarted;
      }
    }

    // Callback for complete handling.
    public event ScreenShotCompleteEvent OnComplete = delegate { };
    // Callback for error handling.
    public event ScreenShotErrorEvent OnError = delegate { };

    [Header("Capture Options")]

    // Save path for recorded video including file name (c://xxx.jpg)
    public string saveFolder = Config.saveFolder;
    // The type of video capture mode, regular or 360.
    public CaptureMode captureMode = CaptureMode.REGULAR;
    // The type of video projection, used for 360 video capture.
    public ProjectionType projectionType = ProjectionType.NONE;
    // The type of video capture stereo mode, left right or top bottom.
    public StereoMode stereoMode = StereoMode.NONE;
    // Stereo mode settings.
    // Average IPD of all subjects in US Army survey in meters
    public float interpupillaryDistance = 0.0635f;

    /// <summary>
    /// Encoding setting variables for image capture.
    /// </summary>
    [Header("Screenshot Settings")]
    // Resolution preset settings, set custom for other resolutions
    public ResolutionPreset resolutionPreset = ResolutionPreset.CUSTOM;
    public CubemapFaceSize cubemapFaceSize = CubemapFaceSize._1024;
    private Int32 cubemapSize = 1024;
    public Int32 frameWidth = 1280;
    public Int32 frameHeight = 720;
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

    // Use hardware encoding
    private bool hardwareEncoding = false;

    // The garbage collection thread.
    private Thread garbageCollectionThread;
    public static bool garbageThreadRunning = false;

    // Log message format template
    private string LOG_FORMAT = "[ScreenShot] {0}";

    #endregion

    #region Video Capture

    /// <summary>
    /// Initialize the attributes of the capture session and start capture.
    /// </summary>
    public bool StartCapture()
    {
      if (_captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Previous screenshot session not finish yet!");
        OnError(this, CaptureErrorCode.SCREENSHOT_ALREADY_IN_PROGRESS);
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

      AntiAliasingSettings();

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

      if (hardwareEncoding)
      {
        // init hardware encoding settings
        GPUEncoderSettings();

        if (!gpuEncoder.StartScreenShot())
        {
          OnError(this, CaptureErrorCode.SCREENSHOT_START_FAILED);
          return false;
        }
      }
      else
      {
        // init ffmpeg encoding settings
        FFmpegEncoderSettings();

        if (!ffmpegEncoder.StartScreenShot())
        {
          OnError(this, CaptureErrorCode.SCREENSHOT_START_FAILED);
          return false;
        }
      }

      _captureStarted = true;

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

      Debug.LogFormat(LOG_FORMAT, "Screen shot session started.");
      return true;
    }

    private void GPUEncoderSettings()
    {
      gpuEncoder.captureMode = captureMode;
      gpuEncoder.resolutionPreset = resolutionPreset;
      gpuEncoder.frameWidth = frameWidth;
      gpuEncoder.frameHeight = frameHeight;
      gpuEncoder.cubemapSize = cubemapSize;
      gpuEncoder.projectionType = projectionType;
      gpuEncoder.stereoMode = stereoMode;
      gpuEncoder.interpupillaryDistance = interpupillaryDistance;
      gpuEncoder.antiAliasing = antiAliasing;
    }

    private void FFmpegEncoderSettings()
    {
      ffmpegEncoder.captureMode = captureMode;
      ffmpegEncoder.resolutionPreset = resolutionPreset;
      ffmpegEncoder.frameWidth = frameWidth;
      ffmpegEncoder.frameHeight = frameHeight;
      ffmpegEncoder.cubemapSize = cubemapSize;
      ffmpegEncoder.projectionType = projectionType;
      ffmpegEncoder.stereoMode = stereoMode;
      ffmpegEncoder.interpupillaryDistance = interpupillaryDistance;
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
    /// <param name="savePath">Image save path.</param>
    private void OnEncoderComplete(string savePath)
    {
      OnComplete(this, savePath);

      _captureStarted = false;

      Debug.LogFormat(LOG_FORMAT, "Screen shot session success!");
    }

    /// <summary>
    /// Garbage collection thread function.
    /// </summary>
    void GarbageCollectionProcess()
    {
      Thread.Sleep(1000);
      System.GC.Collect();

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
           "Component software Encoder not found, please use prefab or follow the document to set up video capture.");
        }
      }

      if (gpuEncoder == null)
      {
        gpuEncoder = GetComponentInChildren<GPUEncoder>(true);
        if (gpuEncoder == null)
        {
          Debug.LogErrorFormat(LOG_FORMAT,
           "Component GPU encoder not found, please use prefab or follow the document to set up video capture.");
        }
      }

      if (ffmpegEncoder != null)
        ffmpegEncoder.OnComplete += OnEncoderComplete;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (gpuEncoder != null)
      {
        gpuEncoder.gameObject.SetActive(true);
        gpuEncoder.OnComplete += OnEncoderComplete;
      }
#endif
    }

    private void OnDestroy()
    {
      if (ffmpegEncoder != null)
        ffmpegEncoder.OnComplete -= OnEncoderComplete;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      if (gpuEncoder != null)
        gpuEncoder.OnComplete -= OnEncoderComplete;
#endif
    }

    #endregion
  }
}