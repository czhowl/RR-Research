namespace Evereal.VideoCapture
{
  public interface IScreenShot
  {
    // Get if capture is started
    bool captureStarted { get; }

    // Start capture
    bool StartCapture();

    // Get ffmpeg encoder instance
    FFmpegEncoder GetFFmpegEncoder();

    // Get GPU encoder instance
    GPUEncoder GetGPUEncoder();
  }
}