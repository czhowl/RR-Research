using System;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Base class for <c>FFmpegEncoder</c> and <c>GPUEncoder</c> class.
  /// </summary>
  public class EncoderBase : MonoBehaviour
  {
    /// <summary>
    /// Native encoder error status.
    /// </summary>
    private const int ERROR_VIDEO_ENCODING_CAUSE_ERRORS = 100;
    private const int ERROR_AUDIO_ENCODING_CAUSE_ERRORS = 200;
    private const int ERROR_TRANSCODING_MUXING_CAUSE_ERRORS = 300;
    private const int ERROR_RTMP_CAUSE_ERRORS = 400;
    private const int ERROR_GRAPHICS_CAPTURE_ERRORS = 500;
    private const int ERROR_CONFIGURATION_ERRORS = 600;
    private const int ERROR_SYSTEM_ERRORS = 700;
    private const int ERROR_ENCODING_CAPABILITY = 800;
    // TODO, cache software encoding error
    private const int ERROR_SOFTWARE_ENCODING_ERROR = 900;

    public enum EncoderStatus
    {
      // Common error codes
      OK = 0,
      ENCODE_IS_NOT_READY,
      NO_INPUT_FILE,
      FILE_READING_ERROR,
      OUTPUT_FILE_OPEN_FAILED,
      OUTPUT_FILE_CREATION_FAILED,
      DXGI_CREATING_FAILED,
      DEVICE_CREATING_FAILED,

      // Video/Image encoding specific error codes
      ENCODE_INIT_FAILED = ERROR_VIDEO_ENCODING_CAUSE_ERRORS,
      ENCODE_SET_CONFIG_FAILED,
      ENCODER_CREATION_FAILED,
      INVALID_TEXTURE_POINTER,
      CONTEXT_CREATION_FAILED,
      TEXTURE_CREATION_FAILED,
      TEXTURE_RESOURCES_COPY_FAILED,
      IO_BUFFER_ALLOCATION_FAILED,
      ENCODE_PICTURE_FAILED,
      ENCODE_FLUSH_FAILED,
      MULTIPLE_ENCODING_SESSION,
      INVALID_TEXTURE_RESOLUTION,

      // WIC specific error codes
      WIC_SAVE_IMAGE_FAILED,

      // Audio encoding specific error codes
      AUDIO_DEVICE_ENUMERATION_FAILED = ERROR_AUDIO_ENCODING_CAUSE_ERRORS,
      AUDIO_CLIENT_INIT_FAILED,
      WRITING_WAV_HEADER_FAILED,
      RELEASING_WAV_FAILED,

      // Transcoding and muxing specific error codes
      MF_CREATION_FAILED = ERROR_TRANSCODING_MUXING_CAUSE_ERRORS,
      MF_INIT_FAILED,
      MF_CREATING_WAV_FORMAT_FAILED,
      MF_TOPOLOGY_CREATION_FAILED,
      MF_TOPOLOGY_SET_FAILED,
      MF_TRANSFORM_NODE_SET_FAILED,
      MF_MEDIA_CREATION_FAILED,
      MF_HANDLING_MEDIA_SESSION_FAILED,

      // WAMEDIA muxing specific error codes
      WAMDEIA_MUXING_FAILED,

      // More MF error codes
      MF_STARTUP_FAILED,
      MF_TRANSFORM_CREATION_FAILED,
      MF_SOURCE_READER_CREATION_FAILED,
      MF_STREAM_SELECTION_FAILED,
      MF_MEDIA_TYPE_CREATION_FAILED,
      MF_MEDIA_TYPE_CONFIGURATION_FAILED,
      MF_MEDIA_TYPE_SET_FAILED,
      MF_MEDIA_TYPE_GET_FAILED,
      MF_CREATE_WAV_FORMAT_FROM_MEDIA_TYPE_FAILED,
      MF_TRANSFORM_OUTPUT_STREAM_INFO_FAILED,
      MF_CREATE_MEMORY_BUFFER_FAILED,
      MF_CREATE_SAMPLE_FAILED,
      MF_SAMPLE_ADD_BUFFER_FAILED,
      MF_READ_SAMPLE_FAILED,
      MF_TRANSFORM_FAILED,
      MF_BUFFER_LOCK_FAILED,

      // RTMP specific error codes
      INVALID_FLV_HEADER = ERROR_RTMP_CAUSE_ERRORS,
      INVALID_STREAM_URL,
      RTMP_CONNECTION_FAILED,
      RTMP_DISCONNECTED,
      SENDING_RTMP_PACKET_FAILED,

      // Graphics capture error codes
      GRAPHICS_DEVICE_CAPTURE_INIT_FAILED = ERROR_GRAPHICS_CAPTURE_ERRORS,
      GRAPHICS_DEVICE_CAPTURE_INVALID_TEXTURE,
      GRAPHICS_DEVICE_CAPTURE_OPEN_SHARED_RESOURCE_FAILED,
      GRAPHICS_DEVICE_CAPTURE_KEYED_MUTEX_ACQUIRE_FAILED,
      GRAPHICS_DEVICE_CAPTURE_KEYED_ACQUIRE_ACQUIRE_SYNC_FAILED,
      GRAPHICS_DEVICE_CAPTURE_KEYED_ACQUIRE_RELASE_SYNC_FAILED,

      // Configuration error codes
      MIC_NOT_CONFIGURED = ERROR_CONFIGURATION_ERRORS,
      MIC_REQUIRES_ENUMERATION,
      MIC_DEVICE_NOT_SET,
      MIC_ENUMERATION_FAILED,
      MIC_SET_FAILED,
      MIC_UNSET_FAILED,
      MIC_INDEX_INVALID,
      CAMERA_NOT_CONFIGURED,
      CAMERA_REQUIRES_ENUMERATION,
      CAMERA_DEVICE_NOT_SET,
      CAMERA_ENUMERATION_FAILED,
      CAMERA_SET_FAILED,
      CAMERA_UNSET_FAILED,
      CAMERA_INDEX_INVALID,
      LIVE_CAPTURE_SETTINGS_NOT_CONFIGURED,
      VOD_CAPTURE_SETTINGS_NOT_CONFIGURED,
      PREVIEW_CAPTURE_SETTINGS_NOT_CONFIGURED,

      // System error codes
      SYSTEM_INITIALIZE_FAILED = ERROR_SYSTEM_ERRORS,
      SYSTEM_ENCODING_TEXTURE_CREATION_FAILED,
      SYSTEM_PREVIEW_TEXTURE_CREATION_FAILED,
      SYSTEM_ENCODING_TEXTURE_FORMAT_CREATION_FAILED,
      SYSTEM_SCREENSHOT_TEXTURE_FORMAT_CREATION_FAILED,
      SYSTEM_CAPTURE_IN_PROGRESS,
      SYSTEM_CAPTURE_NOT_IN_PROGRESS,
      SYSTEM_CAPTURE_TEXTURE_NOT_RECEIVED,
      SYSTEM_CAMERA_OVERLAY_FAILED,
      SYSTEM_CAPTURE_PREVIEW_FAILED,
      SYSTEM_CAPTURE_PREVIEW_NOT_IN_PROGRESS,

      // Encoding capability error codes
      UNSUPPORTED_ENCODING_ENVIRONMENT = ERROR_ENCODING_CAPABILITY,
      UNSUPPORTED_GRAPHICS_CARD_DRIVER_VERSION,
      UNSUPPORTED_GRAPHICS_CARD,
      UNSUPPORTED_OS_VERSION,
      UNSUPPORTED_OS_PROCESSOR,
    }

    // Event delegate callback for complete.
    public delegate void OnCompleteEvent(string savePath);
    // Event delegate callback for error.
    public delegate void OnErrorEvent(EncoderErrorCode error, EncoderStatus? status);

    // The camera render content will be used for capturing video.
    [Header("Capture Cameras")]
    [Tooltip("Reference to camera that renders regular video")]
    public Camera regularCamera;
    [Tooltip("Reference to camera that renders the cubemap")]
    public Camera cubemapCamera;
    [Tooltip("Reference to camera that renders other eye for stereo capture")]
    public Camera stereoCamera;

    [Header("Video Capture Options")]
    // The type of video capture mode, regular or 360.
    public CaptureMode captureMode = CaptureMode.REGULAR;
    // The type of video capture stereo mode, left right or top bottom.
    public StereoMode stereoMode = StereoMode.NONE;
    // Stereo mode settings.
    // Average IPD of all subjects in US Army survey in meters
    public float interpupillaryDistance = 0.064f;
    // The type of video projection, used for 360 video capture.
    public ProjectionType projectionType = ProjectionType.NONE;
    // If set live streaming mode, encoded video will be push to remote streaming url instead of save to local file.
    public VideoCaptureType videoCaptureType = VideoCaptureType.VOD;
    // Audio capture settings, set false if you want to mute audio.
    public bool captureAudio = true;
    public bool captureMic = false;

    /// <summary>
    /// Encoding setting variables for video capture.
    /// </summary>
    [Header("Video Capture Settings")]
    // Resolution preset settings, set custom for other resolutions
    public ResolutionPreset resolutionPreset = ResolutionPreset.CUSTOM;
    // The width of video frame
    public Int32 frameWidth = 1280;
    // The height of video frame
    public Int32 frameHeight = 720;
    // The size of each cubemap side
    public Int32 cubemapSize = 1024;
    [Tooltip("Video bitrate in kbps")]
    public Int32 bitrate = 2000;
    public Int16 frameRate = 24;
    public Int16 antiAliasing = 1;
    // You can get test live stream key on "https://www.facebook.com/live/create".
    // ex. rtmp://rtmp-api-dev.facebook.com:80/rtmp/xxStreamKeyxx
    public string liveStreamUrl = "";
    [Tooltip("Save path for recorded video including file name. File format should be mp4 or h264")]
    // Save path for recorded video including file name (c://xxx.mp4)
    public string videoSavePath = "";
    [Tooltip("Save path for screenshot including file name. File format should be jpg")]
    // Save path for screenshot including file name (c://xxx.jpg)
    public string screenshotSavePath = "";

    ///// <summary>
    ///// Encoding setting variables for preview capture.
    ///// </summary>
    //[Header("Preview Video Settings")]
    //public ResolutionPreset previewVideoPreset = ResolutionPreset.CUSTOM;
    //public Int32 previewVideoWidth = 1280;
    //public Int32 previewVideoHeight = 720;
    //public Int32 previewVideoFramerate = 24;
    //public Int32 previewVideoBitRate = 2000000;

    // Capture is already started
    public bool captureStarted { get; protected set; }
    public bool screenshotStarted { get; protected set; }
    public bool screenshotSequence { get; protected set; }

    // The user input texture
    protected RenderTexture inputTexture = null;
    // The final output texture
    protected RenderTexture outputTexture = null;
    // The stereo video target texture
    protected RenderTexture stereoTexture = null;
    // The stereo video output texture
    protected RenderTexture stereoOutputTexture = null;
    // The equirectangular video target texture
    protected RenderTexture equirectTexture = null;
    // The stereo equirectangular video target texture
    protected RenderTexture stereoEquirectTexture = null;
    // The cubemap video target texture
    protected RenderTexture cubemapTexture = null;
    // The cubemap video render target
    protected RenderTexture cubemapRenderTarget = null;

    // The material for processing equirectangular video
    protected Material equirectMaterial;
    // The material for processing cubemap video
    protected Material cubemapMaterial;
    // The material for processing stereo video
    protected Material stereoPackMaterial;

    // Output video frame width
    public Int32 outputFrameWidth { get; protected set; }
    // Output video frame height
    public Int32 outputFrameHeight { get; protected set; }

    [Tooltip("Offset spherical coordinates (shift equirect)")]
    protected Vector2 sphereOffset = new Vector2(0, 1);
    [Tooltip("Scale spherical coordinates (flip equirect, usually just 1 or -1)")]
    protected Vector2 sphereScale = new Vector2(1, -1);

    /// <summary>
    /// Set the input render texture.
    /// </summary>
    public void SetRenderTexture(RenderTexture renderTexture) {
      inputTexture = renderTexture;
    }

    /// <summary>
    /// Settings for video capture resolution.
    /// </summary>
    protected void ResolutionPresetSettings()
    {
      if (resolutionPreset == ResolutionPreset._1280x720)
      {
        frameWidth = 1280;
        frameHeight = 720;
        bitrate = 2000;
      }
      else if (resolutionPreset == ResolutionPreset._1920x1080)
      {
        frameWidth = 1920;
        frameHeight = 1080;
        bitrate = 4000;
      }
      else if (resolutionPreset == ResolutionPreset._2048x1024)
      {
        frameWidth = 2048;
        frameHeight = 1024;
        bitrate = 4000;
      }
      else if (resolutionPreset == ResolutionPreset._2560x1280)
      {
        frameWidth = 2560;
        frameHeight = 1280;
        bitrate = 6000;
      }
      else if (resolutionPreset == ResolutionPreset._2560x1440)
      {
        frameWidth = 2560;
        frameHeight = 1440;
        bitrate = 6000;
      }
      else if (resolutionPreset == ResolutionPreset._3840x1920)
      {
        frameWidth = 3840;
        frameHeight = 1920;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._3840x2160)
      {
        frameWidth = 3840;
        frameHeight = 2160;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._4096x2048)
      {
        frameWidth = 4096;
        frameHeight = 2048;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._4096x2160)
      {
        frameWidth = 4096;
        frameHeight = 2160;
        bitrate = 10000;
      }
      else if (resolutionPreset == ResolutionPreset._7680x3840)
      {
        frameWidth = 7680;
        frameHeight = 3840;
        bitrate = 25000;
      }
      else if (resolutionPreset == ResolutionPreset._7680x4320)
      {
        frameWidth = 7680;
        frameHeight = 4320;
        bitrate = 25000;
      }
      else if (resolutionPreset == ResolutionPreset._15360x8640)
      {
        frameWidth = 15360;
        frameHeight = 8640;
        bitrate = 50000;
      }
      else if (resolutionPreset == ResolutionPreset._16384x8192)
      {
        frameWidth = 16384;
        frameHeight = 8192;
        bitrate = 50000;
      }
      else if (resolutionPreset == ResolutionPreset.CUSTOM)
      {
        if (frameWidth % 2 == 1)
        {
          frameWidth -= 1;
        }
        if (frameHeight % 2 == 1)
        {
          frameHeight -= 1;
        }
      }
    }

    /// <summary>
    /// Reset camera positions after stereo capture.
    /// </summary>
    public void ResetCameraSettings()
    {
      if (inputTexture == null)
      {
        regularCamera.transform.localPosition = Vector3.zero;
        cubemapCamera.transform.localPosition = Vector3.zero;
        stereoCamera.transform.localPosition = Vector3.zero;
        regularCamera.targetTexture = null;
        cubemapCamera.targetTexture = null;
        stereoCamera.targetTexture = null;
      }
    }

    /// <summary>
    /// Texture settings for stereo video capture.
    /// </summary>
    protected void CreateStereoTextures()
    {
      if (inputTexture == null && stereoMode != StereoMode.NONE)
      {
        // Stereo camera settings.
        if (captureMode == CaptureMode.REGULAR)
        {
          regularCamera.transform.Translate(new Vector3(-interpupillaryDistance / 2, 0, 0), Space.Self);
        }
        else if (captureMode == CaptureMode._360)
        {
          cubemapCamera.transform.Translate(new Vector3(-interpupillaryDistance / 2, 0, 0), Space.Self);
        }
        stereoCamera.transform.Translate(new Vector3(interpupillaryDistance / 2, 0, 0), Space.Self);

        // Init stereo video material.
        stereoPackMaterial = Utils.CreateMaterial("VideoCapture/StereoPack", stereoPackMaterial);
        stereoPackMaterial.DisableKeyword("STEREOPACK_TOP");
        stereoPackMaterial.DisableKeyword("STEREOPACK_BOTTOM");
        stereoPackMaterial.DisableKeyword("STEREOPACK_LEFT");
        stereoPackMaterial.DisableKeyword("STEREOPACK_RIGHT");

        // Init the temporary stereo target texture.
        stereoTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasing, stereoTexture);
        // Set stereo camera
        if (captureMode == CaptureMode.REGULAR)
          stereoCamera.targetTexture = stereoTexture;
        else
          stereoCamera.targetTexture = null;

        // Init the final stereo texture.
        stereoOutputTexture = Utils.CreateRenderTexture(outputFrameWidth, outputFrameHeight, 24, antiAliasing, stereoOutputTexture);
      }
    }

    /// <summary>
    /// Texture settings for equirectangular capture.
    /// </summary>
    protected void CreateEquirectTextures()
    {
      if (captureMode == CaptureMode._360 && projectionType == ProjectionType.EQUIRECT)
      {
        // Create material for convert cubemap to equirectangular.
        equirectMaterial = Utils.CreateMaterial("VideoCapture/CubemapToEquirect", equirectMaterial);

        // Create equirectangular render texture.
        equirectTexture = Utils.CreateRenderTexture(cubemapSize, cubemapSize, 24, antiAliasing, equirectTexture, false);
        equirectTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;

        if (stereoMode != StereoMode.NONE)
        {
          // Create stereo equirectangular render texture.
          stereoEquirectTexture = Utils.CreateRenderTexture(cubemapSize, cubemapSize, 24, antiAliasing, stereoEquirectTexture, false);
          stereoEquirectTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        }
      }
    }

    /// <summary>
    /// Texture settings for cubemap capture.
    /// </summary>
    protected void CreateCubemapTextures()
    {
      // Create cubemap render target.
      cubemapRenderTarget = Utils.CreateRenderTexture(outputFrameWidth, outputFrameWidth, 0, antiAliasing, cubemapRenderTarget);
      cubemapMaterial = Utils.CreateMaterial("VideoCapture/CubemapDisplay", cubemapMaterial);

      // Create cubemap render texture
      if (cubemapTexture == null)
      {
        cubemapTexture = new RenderTexture(cubemapSize, cubemapSize, 0);
        cubemapTexture.hideFlags = HideFlags.HideAndDontSave;
#if UNITY_5_4_OR_NEWER
        cubemapTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
#else
        cubemapTexture.isCubemap = true;
#endif
      }
    }

    /// <summary>
    /// Conversion video format to stereo.
    /// </summary>
    protected void BlitStereoTextures()
    {
      if (stereoMode != StereoMode.NONE)
      {
        // Left eye
        if (stereoMode == StereoMode.TOP_BOTTOM)
        {
          stereoPackMaterial.DisableKeyword("STEREOPACK_BOTTOM");
          stereoPackMaterial.EnableKeyword("STEREOPACK_TOP");
        }
        else if (stereoMode == StereoMode.LEFT_RIGHT)
        {
          stereoPackMaterial.DisableKeyword("STEREOPACK_RIGHT");
          stereoPackMaterial.EnableKeyword("STEREOPACK_LEFT");
        }

        Graphics.Blit(outputTexture, stereoOutputTexture, stereoPackMaterial);

        // Right eye
        if (stereoMode == StereoMode.TOP_BOTTOM)
        {
          stereoPackMaterial.EnableKeyword("STEREOPACK_BOTTOM");
          stereoPackMaterial.DisableKeyword("STEREOPACK_TOP");
        }
        else if (stereoMode == StereoMode.LEFT_RIGHT)
        {
          stereoPackMaterial.EnableKeyword("STEREOPACK_RIGHT");
          stereoPackMaterial.DisableKeyword("STEREOPACK_LEFT");
        }

        Graphics.Blit(stereoTexture, stereoOutputTexture, stereoPackMaterial);
      }
    }

    protected void RenderCubeFace(CubemapFace face, float x, float y, float w, float h)
    {
      // Texture coordinates for displaying each cube map face
      Vector3[] faceTexCoords =
      {
        // +x
        new Vector3(1, 1, 1),
        new Vector3(1, -1, 1),
        new Vector3(1, -1, -1),
        new Vector3(1, 1, -1),
        // -x
        new Vector3(-1, 1, -1),
        new Vector3(-1, -1, -1),
        new Vector3(-1, -1, 1),
        new Vector3(-1, 1, 1),

        // -y
        new Vector3(-1, -1, 1),
        new Vector3(-1, -1, -1),
        new Vector3(1, -1, -1),
        new Vector3(1, -1, 1),
        // +y flipped with -y
        new Vector3(-1, 1, -1),
        new Vector3(-1, 1, 1),
        new Vector3(1, 1, 1),
        new Vector3(1, 1, -1),

        // +z
        new Vector3(-1, 1, 1),
        new Vector3(-1, -1, 1),
        new Vector3(1, -1, 1),
        new Vector3(1, 1, 1),
        // -z
        new Vector3(1, 1, -1),
        new Vector3(1, -1, -1),
        new Vector3(-1, -1, -1),
        new Vector3(-1, 1, -1),
      };

      GL.PushMatrix();
      GL.LoadOrtho();
      GL.LoadIdentity();

      int i = (int)face;

      GL.Begin(GL.QUADS);
      GL.TexCoord(faceTexCoords[i * 4]);
      GL.Vertex3(x, y, 0);
      GL.TexCoord(faceTexCoords[i * 4 + 1]);
      GL.Vertex3(x, y + h, 0);
      GL.TexCoord(faceTexCoords[i * 4 + 2]);
      GL.Vertex3(x + w, y + h, 0);
      GL.TexCoord(faceTexCoords[i * 4 + 3]);
      GL.Vertex3(x + w, y, 0);
      GL.End();

      GL.PopMatrix();
    }
  }
}