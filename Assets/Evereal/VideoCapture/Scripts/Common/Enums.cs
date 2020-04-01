namespace Evereal.VideoCapture
{
  /// <summary>
  ///                    READY
  ///                      |
  ///                      | StartCapture()
  ///    StartCapture()    v
  ///  ---------------> STARTED
  ///  |                   |
  ///  |                   | StopCapture()
  ///  |                   V
  ///  |                STOPPED
  ///  |                   |
  ///  |                   | Encoding?
  ///  |                   v
  ///  |                PENDING
  ///  |                   |
  ///  |                   | Muxing?
  ///  |                   v
  ///  ----------------- READY
  /// </summary>
  public enum CaptureStatus
  {
    READY,
    STARTED,
    STOPPED,
    PENDING,
  }

  /// <summary>
  /// Indicates the error of video capture component.
  /// </summary>
  public enum CaptureErrorCode
  {
    // The ffmpeg executable file is not found, this plugin is depend on ffmpeg to encode videos
    FFMPEG_NOT_FOUND,
    // The audio/video mux process failed
    VIDEO_AUDIO_MUX_START_FAILED,
    // Video capture session already started
    VIDEO_CAPTURE_ALREADY_IN_PROGRESS,
    // Audio capture session already started
    AUDIO_CAPTURE_ALREADY_IN_PROGRESS,
    // Video capture session start failed
    VIDEO_CAPTURE_START_FAILED,
    // Audio capture session start failed
    AUDIO_CAPTURE_START_FAILED,
    // Input render texture not found
    INPUT_TEXTURE_NOT_FOUND,
    // Image capture session start failed
    SCREENSHOT_START_FAILED,
    // Screenshot session already started
    SCREENSHOT_ALREADY_IN_PROGRESS,
  }

  /// <summary>
  /// Indicates the error of encoder component.
  /// </summary>
  public enum EncoderErrorCode
  {
    UNSUPPORTED_SPEC,
    VOD_FAILED_TO_START,
    LIVE_FAILED_TO_START,
    PREVIEW_FAILED_TO_START,
    SCREENSHOT_FAILED_TO_START,
    INVALID_STREAM_URI,
    TEXTURE_ENCODE_FAILED,
    SCREENSHOT_FAILED,
    CAPTURE_ALREADY_IN_PROGRESS,
    MUXING_FAILED_TO_START,
    MUXING_FAILED,
  }

  public enum VRDeviceType
  {
    UNKNOWN,
    OCULUS_RIFT,
    HTC_VIVE,
  }

  public enum StereoMode
  {
    NONE,
    TOP_BOTTOM,
    LEFT_RIGHT
  }

  public enum ProjectionType
  {
    NONE,
    // <summary>
    // Equirectangular format.
    // https://en.wikipedia.org/wiki/Equirectangular_projection
    // </summary>
    EQUIRECT,
    // <summary>
    // Cubemap format.
    // https://docs.unity3d.com/Manual/class-Cubemap.html
    // </summary>
    // Cubemap video format layout:
    // +------------------+------------------+------------------+
    // |                  |                  |                  |
    // |                  |                  |                  |
    // |    +X (Right)    |    -X (Left)     |     +Y (Top)     |
    // |                  |                  |                  |
    // |                  |                  |                  |
    // +------------------+------------------+------------------+
    // |                  |                  |                  |
    // |                  |                  |                  |
    // |   +Y (Bottom)    |   +Z (Fromt)     |    -Z (Back)     |
    // |                  |                  |                  |
    // |                  |                  |                  |
    // +------------------+------------------+------------------+
    //
    CUBEMAP
  }

  public enum GraphicsCard
  {
    NVIDIA,
    AMD,
    UNSUPPORTED_DEVICE
  }

  public enum CaptureMode
  {
    REGULAR,
    _360,
  }

  public enum CaptureType
  {
    VOD,
    LIVE,
    //PREVIEW,
    SCREENSHOT
  }

  public enum VideoCaptureType
  {
    VOD,
    // LIVE,
  }

  public enum ResolutionPreset
  {
    CUSTOM,
    // 480p (720 x 480) Standard Definition (SD) (resolution of DVD video)
    _720x480,
    // 720p (1280 x 720) High Definition (HD)
    _1280x720,
    // 1080p (1920 x 1080) Full High Definition (FHD)
    _1920x1080,
    // 2K (2048 x 1024, 2:1)
    _2048x1024,
    // 2K (2048 x 1280)
    _2560x1280,
    // 2K (2560 x 1440)
    _2560x1440,
    // 4K (3840 x 1920)
    _3840x1920,
    // 4K (3840 x 2160) Quad Full High Definition (QFHD) (also known as UHDTV/UHD-1, resolution of Ultra High Definition TV)
    _3840x2160,
    // 4K (4096 x 2048, 2:1)
    _4096x2048,
    // 4K (4096 x 2160) Ultra High Definition (UHD)
    _4096x2160,
    // 8K (7680 x 3840, 2:1)
    _7680x3840,
    // 8K (7680 x 4320) Ultra High Definition (UHD), only supported in software encoder
    _7680x4320,
     // 16K (15360 × 8640) Ultra High Definition (UHD), only supported in software encoder
    _15360x8640,
    // 16K (16384 × 8192) Ultra High Definition (UHD), only supported in software encoder
    _16384x8192,
  }

  public enum ImageResolutionPreset
  {
    CUSTOM,
    // 480p (720 x 480) Standard Definition (SD) (resolution of DVD video)
    _720x480,
    // 720p (1280 x 720) High Definition (HD)
    _1280x720,
    // 1080p (1920 x 1080) Full High Definition (FHD)
    _1920x1080,
    // 2K (2048 x 1024, 2:1)
    _2048x1024,
    // 2K (2048 x 1280)
    _2560x1280,
    // 2K (2560 x 1440)
    _2560x1440,
    // 4K (3840 x 1920)
    _3840x1920,
    // 4K (3840 x 2160) Quad Full High Definition (QFHD) (also known as UHDTV/UHD-1, resolution of Ultra High Definition TV)
    _3840x2160,
    // 4K (4096 x 2048, 2:1)
    _4096x2048,
    // 4K (4096 x 2160) Ultra High Definition (UHD)
    _4096x2160,
    // 8K (7680 x 3840, 2:1)
    _7680x3840,
    // 8K (7680 x 4320) Ultra High Definition (UHD), only supported in software encoder
    _7680x4320,
    // 16K (15360 × 8640) Ultra High Definition (UHD), only supported in software encoder
    _15360x8640,
    // 16K (16384 × 8192) Ultra High Definition (UHD), only supported in software encoder
    _16384x8192,
  }

  public enum CubemapFaceSize {
    _512,
    _1024,
    _2048,
  }

  public enum AntiAliasingSetting {
    _1,
    _2,
    _4,
    _8,
  }

  public enum SampleRate
  {
    _8000,
    _11025,
    _12000,
    _16000,
    _22050,
    _24000,
    _32000,
    _44100,
    _48000,
  }
}