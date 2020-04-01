namespace Evereal.VideoCapture
{
  /// <summary>
  /// Event delegate callback for video capture complete.
  /// </summary>
  /// <param name="instance"><c>VideoCapture</c> instance.</param>
  /// <param name="savePath">Video save path.</param>
  public delegate void VideoCaptureCompleteEvent(VideoCapture instance, string savePath);

  /// <summary>
  /// Event delegate callback for audio capture complete.
  /// </summary>
  /// <param name="instance"><c>AudioCapture</c> instance.</param>
  /// <param name="savePath">Audio save path.</param>
  public delegate void AudioCaptureCompleteEvent(AudioCapture instance, string savePath);

  /// <summary>
  /// Event delegate callback for render texture capture complete.
  /// </summary>
  /// <param name="instance"><c>TextureCapture</c> instance.</param>
  /// <param name="savePath">Video save path.</param>
  public delegate void TextureCaptureCompleteEvent(TextureCapture instance, string savePath);

  /// <summary>
  /// Event delegate callback for screenshot capture complete.
  /// </summary>
  /// <param name="instance"><c>ScreenShot</c> instance.</param>
  /// <param name="savePath">Screenshot save path.</param>
  public delegate void ScreenShotCompleteEvent(ScreenShot instance, string savePath);
}