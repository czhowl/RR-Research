using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// <c>FFmpegEncoder</c> will capture the camera's render texture and encode it to video files by ffmpeg encoder.
  /// </summary>
  public class FFmpegEncoder : EncoderBase
  {
    #region Dll Import

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartVodCapture(int width,
                                                               int height,
                                                               int rate,
                                                               ProjectionType proj,
                                                               StereoMode sm,
                                                               string path,
                                                               string ffpath);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CaptureVodFrames(IntPtr api, byte[] data, int count);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_StopVodCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanVodCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartLiveCapture(int width,
                                                                int height,
                                                                int rate,
                                                                ProjectionType proj,
                                                                StereoMode sm,
                                                                string streamUrl,
                                                                string ffpath);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CaptureLiveFrames(IntPtr api, byte[] data, int count);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_StopLiveCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    static extern void FFmpegEncoder_CleanLiveCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartScreenshot(int width,
                                                               int height,
                                                               ProjectionType proj,
                                                               StereoMode sm,
                                                               string path,
                                                               string ffpath);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CaptureScreenshot(IntPtr api, byte[] data);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_StopScreenshot(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanScreenshot(IntPtr api);

    #endregion

    #region Properties

    // The delta time of each frame
    private float deltaFrameTime;
    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // The texture holding the video frame data.
    private Texture2D texture2D = null;
    // Whether or not there is a frame capturing now.
    private bool isCapturingFrame;
    // Whether or not there is a screenshot capturing now.
    private bool isCapturingScreenshot;
    // The time spent during capturing.
    private float capturingTime;
    // Frame statistics info.
    private int capturedFrameCount;
    // Reference to native encoder API
    private IntPtr nativeAPI;

    /// <summary>
    /// Frame data will be sent to frame encode queue.
    /// </summary>
    private struct FrameData
    {
      // The RGB pixels will be encoded.
      public byte[] pixels;
      // How many this frame will be counted.
      public int count;
      // Constructor.
      public FrameData(byte[] p, int c)
      {
        pixels = p;
        count = c;
      }
    }
    // The frame encode queue.
    private Queue<FrameData> frameQueue;

#if UNITY_2018_3_OR_NEWER
    private bool supportsAsyncGPUReadback;
    private bool isAsyncGPUReading;
    // The async frame request queue.
    private Queue<AsyncGPUReadbackRequest> requestQueue;
#endif

    // The frame encode thread.
    private Thread encodeThread;

    // Log message format template
    private string LOG_FORMAT = "[FFmpegEncoder] {0}";

    #endregion

    #region Methods

    // Start capture video
    public bool StartCapture()
    {
      // Check if we can start capture session
      if (captureStarted)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS, null);
        return false;
      }
      // if (videoCaptureType == VideoCaptureType.LIVE)
      // {
      //   if (string.IsNullOrEmpty(liveStreamUrl))
      //   {
      //     OnError(EncoderErrorCode.INVALID_STREAM_URI, null);
      //     return false;
      //   }
      // }

      if (captureMode != CaptureMode.REGULAR && inputTexture != null)
      {
        Debug.LogFormat(LOG_FORMAT,
          "CaptureMode should be set for regular for user input render texture");
        captureMode = CaptureMode.REGULAR;
      }

      if (captureMode == CaptureMode._360 && projectionType == ProjectionType.NONE)
      {
        Debug.LogFormat(LOG_FORMAT,
          "ProjectionType should be set for 360 capture, set type to equirect for generating texture properly");
        projectionType = ProjectionType.EQUIRECT;
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      // Resolution preset settings
      ResolutionPresetSettings();

      // Create texture for encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      if (videoCaptureType == VideoCaptureType.VOD)
      {
        videoSavePath = string.Format("{0}video_{1}x{2}_{3}_{4}.mp4",
          Config.saveFolder,
          outputFrameWidth, outputFrameHeight,
          Utils.GetTimeString(),
          Utils.GetRandomString(5));
      }

      // Reset tempory variables.
      capturingTime = 0f;
      capturedFrameCount = 0;
      frameQueue = new Queue<FrameData>();
#if UNITY_2018_3_OR_NEWER
      if (supportsAsyncGPUReadback)
        requestQueue = new Queue<AsyncGPUReadbackRequest>();
#endif
      // Pass projection, stereo metadata into native plugin
      if (videoCaptureType == VideoCaptureType.VOD)
      {
        nativeAPI = FFmpegEncoder_StartVodCapture(
          outputFrameWidth,
          outputFrameHeight,
          frameRate,
          projectionType,
          stereoMode,
          videoSavePath,
          Config.ffmpegPath);

        if (nativeAPI == IntPtr.Zero)
        {
          OnError(EncoderErrorCode.VOD_FAILED_TO_START, null);
          return false;
        }
      }
      // else if (videoCaptureType == VideoCaptureType.LIVE)
      // {
      //   nativeAPI = FFmpegEncoder_StartLiveCapture(
      //     outputFrameWidth,
      //     outputFrameHeight,
      //     frameRate,
      //     projectionType,
      //     stereoMode,
      //     liveStreamUrl,
      //     Config.ffmpegPath);

      //   if (nativeAPI == IntPtr.Zero)
      //   {
      //     OnError(EncoderErrorCode.LIVE_FAILED_TO_START, null);
      //     return false;
      //   }
      // }

      // Update current status.
      captureStarted = true;

      if (encodeThread != null && encodeThread.IsAlive)
      {
        encodeThread.Abort();
        encodeThread = null;
      }

      // Start encoding thread.
      encodeThread = new Thread(FrameEncodeProcess);
      encodeThread.Priority = System.Threading.ThreadPriority.Lowest;
      encodeThread.IsBackground = true;
      encodeThread.Start();

      //Debug.LogFormat(LOG_FORMAT, "FFmpegEncoder Started");

      return true;
    }
    /// <summary>
    /// Stop capture video.
    /// </summary>
    public bool StopCapture()
    {
      if (!captureStarted)
      {
        Debug.LogWarningFormat(LOG_FORMAT, "Video capture session not start yet!");
        return false;
      }

      // Reset camera settings.
      ResetCameraSettings();

      // Update current status.
      captureStarted = false;

      //Debug.LogFormat(LOG_FORMAT, "FFmpegEncoder Stopped");
      return true;
    }

    /// <summary>
    /// Configuration for Screenshot
    /// </summary>
    public bool StartScreenShot()
    {
      // Check if we can start capture session
      if (screenshotStarted)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS, null);
        return false;
      }

      if (captureMode != CaptureMode.REGULAR && inputTexture != null)
      {
        Debug.LogFormat(LOG_FORMAT,
          "CaptureMode should be set for regular for user input render texture");
        captureMode = CaptureMode.REGULAR;
      }

      if (captureMode == CaptureMode._360 && projectionType == ProjectionType.NONE)
      {
        Debug.LogFormat(LOG_FORMAT,
          "ProjectionType should be set for 360 capture, set type to equirect for generating texture properly");
        projectionType = ProjectionType.EQUIRECT;
      }
      else if (captureMode == CaptureMode.REGULAR)
      {
        // Non 360 capture doesn't have projection type
        projectionType = ProjectionType.NONE;
      }

      // Resolution preset settings
      ResolutionPresetSettings();

      // Create texture for encoding
      CreateRenderTextures();

      // Create textures for stereo
      CreateStereoTextures();

      // If we haven't set the save path, we want to use project folder and timestamped file name by default
      screenshotSavePath = string.Format("{0}screenshot_{1}x{2}_{3}_{4}.jpg",
        Config.saveFolder,
        outputFrameWidth, outputFrameHeight,
        Utils.GetTimeString(),
        Utils.GetRandomString(5));

      // Reset tempory variables.
      capturingTime = 0f;
      capturedFrameCount = 0;
      frameQueue = new Queue<FrameData>();

      // Pass projection, stereo info into native plugin
      nativeAPI = FFmpegEncoder_StartScreenshot(
        outputFrameWidth,
        outputFrameHeight,
        projectionType,
        stereoMode,
        screenshotSavePath,
        Config.ffmpegPath);
      if (nativeAPI == IntPtr.Zero)
      {
        OnError(EncoderErrorCode.SCREENSHOT_FAILED_TO_START, null);
        return false;
      }

      // Update current status.
      screenshotStarted = true;
      screenshotSequence = false;

      if (encodeThread != null && encodeThread.IsAlive)
      {
        encodeThread.Abort();
        encodeThread = null;
      }

      // Start encoding thread.
      encodeThread = new Thread(ScreenshotEncodeProcess);
      encodeThread.Priority = System.Threading.ThreadPriority.Lowest;
      encodeThread.IsBackground = true;
      encodeThread.Start();

      //Debug.LogFormat(LOG_FORMAT, "FFmpegEncoder Started");

      return true;
    }

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called before any Start functions and also just after a prefab is instantiated.
    /// </summary>
    private void Awake()
    {
      //regularCamera = GetComponent<Camera>();

      deltaFrameTime = 1f / frameRate;

      captureStarted = false;
      screenshotStarted = false;

#if UNITY_2018_3_OR_NEWER
      supportsAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback;
      // Comment this line below to experiment with Async GPU Readback feature
      supportsAsyncGPUReadback = false;
#endif

      OnError += EncoderErrorLog;
    }

    /// <summary>
    /// Called once per frame, after Update has finished.
    /// </summary>
    private void LateUpdate()
    {
      // Capture not started yet
      if (!captureStarted && !screenshotStarted)
        return;

      capturingTime += Time.deltaTime;
      if (!isCapturingFrame)
      {
        int totalRequiredFrameCount = 1;
        if (captureStarted || (screenshotStarted && screenshotSequence))
          totalRequiredFrameCount = (int)(capturingTime / deltaFrameTime);
        // Skip frames if we already got enough
        if (capturedFrameCount < totalRequiredFrameCount)
        {
          // Capture panorama mono video.
          if (captureMode == CaptureMode._360)
          {
            StartCoroutine(CaptureCubemapFrameAsync());
          }
          // Capture normal stereo video.
          else if (captureMode == CaptureMode.REGULAR)
          {
            StartCoroutine(CaptureFrameAsync());
          }
        }
      }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
#if UNITY_2018_3_OR_NEWER
      // Capture not started yet
      if (!captureStarted)
        return;

      if (supportsAsyncGPUReadback && !isAsyncGPUReading)
      {
        StartCoroutine(GPUFrameReadbackAsync());
      }
#endif
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void OnDestroy()
    {
      OnError -= EncoderErrorLog;

      if (outputTexture != null)
      {
        RenderTexture.Destroy(outputTexture);
        outputTexture = null;
      }
      if (stereoTexture != null)
      {
        RenderTexture.Destroy(stereoTexture);
        stereoTexture = null;
      }
      if (stereoOutputTexture != null)
      {
        RenderTexture.Destroy(stereoOutputTexture);
        stereoOutputTexture = null;
      }
      if (equirectTexture != null)
      {
        RenderTexture.Destroy(equirectTexture);
        equirectTexture = null;
      }
      if (stereoEquirectTexture != null)
      {
        RenderTexture.Destroy(stereoEquirectTexture);
        stereoEquirectTexture = null;
      }
    }

#endregion // Unity Lifecycle

#region Video Encoding
    /// <summary>
    /// Capture frame coroutine implementation.
    /// </summary>
    private IEnumerator CaptureFrameAsync()
    {
      isCapturingFrame = true;

      yield return new WaitForEndOfFrame();

#if UNITY_2018_3_OR_NEWER
      if (supportsAsyncGPUReadback)
      {
        // use async gpu readback if possibile
        requestQueue.Enqueue(AsyncGPUReadback.Request(outputTexture));
      }
      else
      {
        CopyFrameTexture();
        yield return null;
        EnqueueFrameTexture();
      }
#else
      CopyFrameTexture();
      yield return null;
      EnqueueFrameTexture();
#endif

      if (screenshotStarted && !screenshotSequence)
        screenshotStarted = false;
      isCapturingFrame = false;
    }
    /// <summary>
    /// Capture cubemap frame coroutine implementation.
    /// </summary>
    private IEnumerator CaptureCubemapFrameAsync()
    {
      isCapturingFrame = true;
      yield return new WaitForEndOfFrame();
      // Blit cubemap texture.
      BlitCubemapFrame();
      yield return null;
      // Send for encoding.
      EnqueueFrameTexture();
      if (screenshotStarted && !screenshotSequence)
        screenshotStarted = false;
      isCapturingFrame = false;
    }

    /// <summary>
    /// Blit cubemap frame implementation.
    /// </summary>
    private void BlitCubemapFrame()
    {
      if (projectionType == ProjectionType.CUBEMAP)
      {
        cubemapCamera.RenderToCubemap(cubemapTexture);

        cubemapMaterial.SetTexture("_CubeTex", cubemapTexture);
        cubemapMaterial.SetVector("_SphereScale", sphereScale);
        cubemapMaterial.SetVector("_SphereOffset", sphereOffset);
        cubemapMaterial.SetMatrix("_CubeTransform", Matrix4x4.identity);
        cubemapMaterial.SetPass(0);

        Graphics.SetRenderTarget(cubemapRenderTarget);

        float s = 1.0f / 3.0f;
        RenderCubeFace(CubemapFace.PositiveX, 0.0f, 0.5f, s, 0.5f);
        RenderCubeFace(CubemapFace.NegativeX, s, 0.5f, s, 0.5f);
        RenderCubeFace(CubemapFace.PositiveY, s * 2.0f, 0.5f, s, 0.5f);

        RenderCubeFace(CubemapFace.NegativeY, 0.0f, 0.0f, s, 0.5f);
        RenderCubeFace(CubemapFace.PositiveZ, s, 0.0f, s, 0.5f);
        RenderCubeFace(CubemapFace.NegativeZ, s * 2.0f, 0.0f, s, 0.5f);

        Graphics.SetRenderTarget(null);
        Graphics.Blit(cubemapRenderTarget, outputTexture);

        CopyFrameTexture();
      }
      else if (projectionType == ProjectionType.EQUIRECT)
      {
        cubemapCamera.RenderToCubemap(equirectTexture);
        cubemapCamera.Render();
        // Convert to equirectangular projection.
        Graphics.Blit(equirectTexture, outputTexture, equirectMaterial);
        // From frameRenderTexture to frameTexture.
        if (stereoMode != StereoMode.NONE)
        {
          stereoCamera.RenderToCubemap(stereoEquirectTexture);
          stereoCamera.Render();
          // Convert to equirectangular projection.
          Graphics.Blit(stereoEquirectTexture, stereoTexture, equirectMaterial);
        }
        CopyFrameTexture();
      }
    }

#if UNITY_2018_3_OR_NEWER
    /// <summary>
    /// Async gpu frame texture readback coroutine implementation.
    /// </summary>
    private IEnumerator GPUFrameReadbackAsync()
    {
      isAsyncGPUReading = true;

      yield return new WaitForEndOfFrame();

      while (requestQueue.Count > 0)
      {
        var request = requestQueue.Peek();

        if (request.hasError)
        {
          requestQueue.Dequeue();
        }
        else if (request.done)
        {
          var buffer = request.GetData<Color32>();
          texture2D.SetPixels32(buffer.ToArray());
          texture2D.Apply();

          EnqueueFrameTexture();

          requestQueue.Dequeue();

          yield return null;
        }
        else
        {
          break;
        }
      }

      isAsyncGPUReading = false;
    }
#endif

    /// <summary>
    /// Copy the frame texture from GPU to CPU.
    /// </summary>
    void CopyFrameTexture()
    {
      if (inputTexture != null)
      {
        // Bind user input texture.
        RenderTexture.active = inputTexture;
      }
      else if (stereoMode == StereoMode.NONE)
      {
        // Bind camera render texture.
        RenderTexture.active = outputTexture;
      }
      else
      {
        // Stereo cubemap capture not support.
        if (captureMode == CaptureMode._360 && projectionType == ProjectionType.CUBEMAP)
          return;

        BlitStereoTextures();

        RenderTexture.active = stereoOutputTexture;
      }
      // TODO, remove expensive step of copying pixel data from GPU to CPU.
      texture2D.ReadPixels(new Rect(0, 0, outputFrameWidth, outputFrameHeight), 0, 0, false);

      texture2D.Apply();
      // Restore RenderTexture states.
      RenderTexture.active = null;
    }

    /// <summary>
    /// Send the captured frame texture to encode queue.
    /// </summary>
    void EnqueueFrameTexture()
    {
      int totalRequiredFrameCount = (int)(capturingTime / deltaFrameTime);
      int requiredFrameCount = totalRequiredFrameCount - capturedFrameCount;
      lock (this)
      {
        frameQueue.Enqueue(new FrameData(texture2D.GetRawTextureData(), requiredFrameCount));
      }
      capturedFrameCount = totalRequiredFrameCount;
    }

    // Frame encoding process in thread
    private void FrameEncodeProcess()
    {
      //Debug.LogFormat(LOG_FORMAT, "Encoding thread started!");
      while (captureStarted || frameQueue.Count > 0)
      {
        if (frameQueue.Count > 0)
        {
          FrameData frame;
          lock (this)
          {
            frame = frameQueue.Dequeue();
          }
          if (videoCaptureType == VideoCaptureType.VOD)
          {
            FFmpegEncoder_CaptureVodFrames(nativeAPI, frame.pixels, frame.count);
          }
          // else if (videoCaptureType == VideoCaptureType.LIVE)
          // {
          //   FFmpegEncoder_CaptureLiveFrames(nativeAPI, frame.pixels, frame.count);
          // }
        }
        else
        {
          // Wait 1 second for captured frame
          Thread.Sleep(1000);
        }
      }
      // Notify native encoding process finish
      if (videoCaptureType == VideoCaptureType.VOD)
      {
        FFmpegEncoder_StopVodCapture(nativeAPI);
        FFmpegEncoder_CleanVodCapture(nativeAPI);
      }
      // else if (videoCaptureType == VideoCaptureType.LIVE)
      // {
      //   FFmpegEncoder_StopLiveCapture(nativeAPI);
      //   FFmpegEncoder_CleanLiveCapture(nativeAPI);
      // }

      // Notify caller video capture complete
      OnComplete(videoSavePath);
      //Debug.LogFormat(LOG_FORMAT, "Video encode process finish!");
    }

    private void ScreenshotEncodeProcess()
    {
      //Debug.LogFormat(LOG_FORMAT, "Encoding thread started!");
      while (screenshotStarted || frameQueue.Count > 0)
      {
        if (frameQueue.Count > 0)
        {
          FrameData frame;
          lock (this)
          {
            frame = frameQueue.Dequeue();
          }
          FFmpegEncoder_CaptureScreenshot(nativeAPI, frame.pixels);
        }
        else
        {
          // Wait 1 second for captured frame
          Thread.Sleep(1000);
        }
      }
      FFmpegEncoder_StopScreenshot(nativeAPI);
      FFmpegEncoder_CleanScreenshot(nativeAPI);

      OnComplete(screenshotSavePath);
      //Debug.LogFormat(LOG_FORMAT, "Video encode process finish!");
    }

    private void EncoderErrorLog(EncoderErrorCode error, EncoderStatus? encoderStatus)
    {
      Debug.LogWarningFormat(LOG_FORMAT,
        "FFmpeg Encoder Error Occured of type: " + error + " [Error code: " + encoderStatus + " ] \n" +
        "Please check EncoderLog.txt log file for more information");
    }

    /// <summary>
    /// Create the RenderTexture for encoding texture
    /// </summary>
    private void CreateRenderTextures()
    {
      outputFrameWidth = inputTexture == null ? frameWidth : inputTexture.width;
      outputFrameHeight = inputTexture == null ? frameHeight : inputTexture.height;

      if (outputTexture != null &&
        (outputFrameWidth != outputTexture.width ||
        outputFrameHeight != outputTexture.height))
      {
        if (outputTexture)
        {
          Destroy(outputTexture);
          outputTexture = null;
        }

        if (equirectTexture)
        {
          Destroy(equirectTexture);
          equirectTexture = null;
        }

        if (stereoEquirectTexture)
        {
          Destroy(stereoEquirectTexture);
          stereoEquirectTexture = null;
        }

        if (stereoTexture)
        {
          Destroy(stereoTexture);
          stereoTexture = null;
        }

        if (stereoOutputTexture)
        {
          Destroy(stereoOutputTexture);
          stereoOutputTexture = null;
        }

        if (texture2D)
        {
          Destroy(texture2D);
          texture2D = null;
        }
      }

      // Pixels stored in frameRenderTexture(RenderTexture) always read by frameTexture2D.
      texture2D = Utils.CreateTexture(outputFrameWidth, outputFrameHeight, texture2D);

      // Capture from user input texture
      if (inputTexture != null)
        return;

      // Create a RenderTexture with desired frame size for dedicated camera capture to store pixels in GPU.
      outputTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasing, outputTexture);
      // For capturing normal 2D video, use frameTexture(Texture2D) for intermediate cpu saving, frameRenderTexture(RenderTexture) store the pixels read by frameTexture.
      if (captureMode == CaptureMode.REGULAR)
      {
        regularCamera.targetTexture = outputTexture;
      }
      // For capture panorama video:
      // EQUIRECTANGULAR: use cubemapTexture(RenderTexture) for intermediate cpu saving.
      // CUBEMAP: use texture2D for intermediate cpu saving.
      else if (captureMode == CaptureMode._360)
      {
        cubemapCamera.targetTexture = outputTexture;
        if (projectionType == ProjectionType.CUBEMAP)
        {
          CreateCubemapTextures();
        }
        else
        {
          // Create equirectangular textures and materials.
          CreateEquirectTextures();
        }
      }
    }

    protected new void CreateStereoTextures()
    {
      base.CreateStereoTextures();

      if (inputTexture == null && stereoMode != StereoMode.NONE)
      {
        stereoCamera.targetTexture = stereoTexture;
      }
    }

#endregion
  }
}