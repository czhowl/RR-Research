namespace Evereal.VideoCapture
{
  /// <summary>
  /// Invoked when error occurred, such as muxing problems are reported through this callback.
  /// </summary>
  /// <param name="instance"><c>VideoCapture</c> instance.</param>
  /// <param name="error">Error code.</param>
  public delegate void VideoCaptureErrorEvent(VideoCapture instance, CaptureErrorCode error);

  /// <summary>
  /// Invoked when error occurred, such as muxing problems are reported through this callback.
  /// </summary>
  /// <param name="instance"><c>AudioCapture</c> instance.</param>
  /// <param name="error">Error code.</param>
  public delegate void AudioCaptureErrorEvent(AudioCapture instance, CaptureErrorCode error);

  /// <summary>
  /// Invoked when error occurred, such as muxing problems are reported through this callback.
  /// </summary>
  /// <param name="instance"><c>TextureCapture</c> instance.</param>
  /// <param name="error">Error code.</param>
  public delegate void TextureCaptureErrorEvent(TextureCapture instance, CaptureErrorCode error);

  /// <summary>
  /// Invoked when error occurred, such as muxing problems are reported through this callback.
  /// </summary>
  /// <param name="instance"><c>ScreenShot</c> instance.</param>
  /// <param name="error">Error code.</param>
  public delegate void ScreenShotErrorEvent(ScreenShot instance, CaptureErrorCode error);
}