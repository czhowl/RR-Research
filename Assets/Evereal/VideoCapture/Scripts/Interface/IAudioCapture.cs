namespace Evereal.VideoCapture
{
  public interface IAudioCapture
  {

    // Get if capture is started
    bool captureStarted { get; }

    // Start capture
    bool StartCapture();

    // Stop capture
    bool StopCapture();
  }
}