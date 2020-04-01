namespace Evereal.VideoCapture {
	public interface IVideoCapture  {

		// Get or set the current status
		CaptureStatus status { get; }

		// Start capture
		bool StartCapture();

		// Stop capture
		bool StopCapture();

    // When audio muxing complete
    void OnAudioMuxingComplete(string path);

		// Get ffmpeg encoder instance
		FFmpegEncoder GetFFmpegEncoder();

		// Get GPU encoder instance
		GPUEncoder GetGPUEncoder();
	}
}