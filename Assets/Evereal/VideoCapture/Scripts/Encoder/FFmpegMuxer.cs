using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Evereal.VideoCapture
{
  // This script will record target audio listener sample and encode to audio file, or mux audio into video file if required.
  [RequireComponent(typeof(AudioListener))]
  public class FFmpegMuxer : MonoBehaviour
  {
    #region Dll Import

    // [DllImport("FFmpegEncoder")]
    // private static extern IntPtr FFmpegEncoder_StartAudioCapture(int rate, string path, string ffpath);

    // [DllImport("FFmpegEncoder")]
    // private static extern void FFmpegEncoder_CaptureAudioFrame(IntPtr api, byte[] data);

    // [DllImport("FFmpegEncoder")]
    // private static extern void FFmpegEncoder_StopAudioCapture(IntPtr api);

    // [DllImport("FFmpegEncoder")]
    // private static extern void FFmpegEncoder_CleanAudioCapture(IntPtr api);

    [DllImport("FFmpegEncoder")]
    private static extern IntPtr FFmpegEncoder_StartMuxProcess(
                                           int rate,
                                           string path,
                                           string vpath,
                                           string apath,
                                           string ffpath);

    [DllImport("FFmpegEncoder")]
    private static extern void FFmpegEncoder_CleanMuxProcess(IntPtr api);

    #endregion

    #region Properties

    public static FFmpegMuxer singleton;

    // Event delegate callback for complete.
    public delegate void OnCompleteEvent(string savePath);
    // Event delegate callback for error.
    public delegate void OnErrorEvent(EncoderErrorCode error);
    // Callback for complete handling
    public event OnCompleteEvent OnComplete = delegate { };
    // Callback for error handling
    public event OnErrorEvent OnError = delegate { };

    // The captured audio path
    public string audioSavePath;

    // Is audio capture started
    public bool captureStarted { get; private set; }

    // Capture microphone settings
    public bool captureMic = false;
    // Audio source for capture microphone
    private AudioSource audioSource;

    public SampleRate sampleRate = SampleRate._44100;

    private int bufferSize;
    private int numBuffers;
    private int outputRate = 44100;
    private int headerSize = 44; // default for uncompressed wav
    private FileStream fileStream;

    // // Reference to native lib API
    // private IntPtr nativeAPI;
    // // The audio capture prepare vars
    // private IntPtr audioPointer;
    // private Byte[] audioByteBuffer;

    // Video capture instance for muxing process
    private List<IVideoCapture> videoCaptures;

    // The audio/video mux thread.
    private Thread muxingThread;

    private string ffmpegPath;

    // Log message format template
    private string LOG_FORMAT = "[FFmpegMuxer] {0}";

    #endregion

    #region Methods

    // Start capture audio session
    public bool StartCapture()
    {
      // Check if we can start capture session
      if (captureStarted)
      {
        OnError(EncoderErrorCode.CAPTURE_ALREADY_IN_PROGRESS);
        return false;
      }

      // Init audio save destination
      audioSavePath = string.Format("{0}audio_{1}_{2}.wav",
        Config.saveFolder,
        Utils.GetTimeString(),
        Utils.GetRandomString(5));

      SampleRateSettings();

      if (captureMic && Microphone.devices.Length > 0)
      {
        if (audioSource == null)
        {
          GameObject recorder = new GameObject("MicrophoneRecorder");
          recorder.transform.parent = transform;
          audioSource = recorder.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.clip = Microphone.Start("Built-in Microphone", true, 10, outputRate);
        audioSource.Play();
      }

      // StreamingAssets path is not accessible in thread
      ffmpegPath = Config.ffmpegPath;

      StartWrite();

      captureStarted = true;

      return true;
    }

    // Stop capture audio session
    public bool StopCapture()
    {
      if (!captureStarted)
      {
        Debug.LogFormat(LOG_FORMAT, "Audio capture session not start yet!");
        return false;
      }

      if (captureMic)
      {
        if (Microphone.IsRecording("Built-in Microphone"))
          Microphone.End("Built-in Microphone");
        if (audioSource != null)
          audioSource.Stop();
      }

      // write header
      WriteHeader();

      if (videoCaptures.Count > 0)
      {
        // Start merging thread when we have videos need mux.
        if (muxingThread != null)
        {
          if (muxingThread.IsAlive)
            muxingThread.Abort();
          muxingThread = null;
        }
        muxingThread = new Thread(MuxingThreadFunction);
        muxingThread.Priority = System.Threading.ThreadPriority.Lowest;
        muxingThread.IsBackground = true;
        muxingThread.Start();
      }

      captureStarted = false;

      if (videoCaptures.Count == 0)
      {
        // No video muxing required.
        OnComplete(audioSavePath);
      }

      //Debug.LogFormat(LOG_FORMAT, "Audio encode process finish!");
      return true;
    }

    public void AttachVideoCapture(IVideoCapture videoCapture)
    {
      videoCaptures.Add(videoCapture);
    }

    private int GetVideoPendingCount()
    {
      int count = 0;
      foreach (IVideoCapture videoCapture in videoCaptures)
      {
        if (videoCapture.status == CaptureStatus.PENDING)
          count++;
      }
      return count;
    }

    /// <summary>
    /// Media muxing the thread function.
    /// </summary>
    private void MuxingThreadFunction()
    {
      // Wait for all video record finish
      while (GetVideoPendingCount() < videoCaptures.Count)
      {
        Thread.Sleep(1000);
      }

      // Muxing video capture
      foreach (IVideoCapture videoCapture in videoCaptures)
      {
        if (!StartMuxProcess(videoCapture))
        {
          break;
        }
        FFmpegEncoder ffmpegEncoder = videoCapture.GetFFmpegEncoder();
        // Clean video files
        if (File.Exists(ffmpegEncoder.videoSavePath))
        {
          // clean up video with no sound
          File.Delete(ffmpegEncoder.videoSavePath);
          ffmpegEncoder.videoSavePath = "";
        }
      }

      // Clean video capture queue
      videoCaptures.Clear();

      // Clean audio file
      if (File.Exists(audioSavePath))
      {
        File.Delete(audioSavePath);
        audioSavePath = "";
      }
    }

    private void StartWrite()
    {

      // nativeAPI = FFmpegEncoder_StartAudioCapture(
      //     AudioSettings.outputSampleRate,
      //     audioSavePath,
      //     Config.ffmpegPath);
      // if (nativeAPI == IntPtr.Zero)
      // {
      //   OnError(this, EncoderErrorCode.AUDIO_FAILED_START);
      //   return false;
      // }
      // // Init temp variables
      // audioByteBuffer = new Byte[8192];
      // GCHandle audioHandle = GCHandle.Alloc(audioByteBuffer, GCHandleType.Pinned);
      // audioPointer = audioHandle.AddrOfPinnedObject();

      fileStream = new FileStream(audioSavePath, FileMode.Create);
      byte emptyByte = new byte();
      //preparing the header
      for (int i = 0; i < headerSize; i++)
      {
        fileStream.WriteByte(emptyByte);
      }
    }

    private void ConvertAndWrite(float[] dataSource)
    {

      // Marshal.Copy(data, 0, audioPointer, 2048);
      // FFmpegEncoder_CaptureAudioFrame(nativeAPI, audioByteBuffer);

      Int16[] intData = new Int16[dataSource.Length];
      // converting in 2 steps : float[] to Int16[], then Int16[] to Byte[]
      Byte[] bytesData = new Byte[dataSource.Length * 2];
      // bytesData array is twice the size of dataSource array because a float converted in Int16 is 2 bytes.
      //to convert float to Int16
      int rescaleFactor = 32767;
      for (int i = 0; i < dataSource.Length; i++)
      {
        intData[i] = (Int16)(dataSource[i] * rescaleFactor);
        Byte[] byteArr = new Byte[2];
        byteArr = BitConverter.GetBytes(intData[i]);
        byteArr.CopyTo(bytesData, i * 2);
      }

      fileStream.Write(bytesData, 0, bytesData.Length);
    }

    private void WriteHeader()
    {

      // FFmpegEncoder_StopAudioCapture(nativeAPI);
      // // Clean audio capture resources
      // FFmpegEncoder_CleanAudioCapture(nativeAPI);

      fileStream.Seek(0, SeekOrigin.Begin);

      Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
      fileStream.Write(riff, 0, 4);

      Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
      fileStream.Write(chunkSize, 0, 4);

      Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
      fileStream.Write(wave, 0, 4);

      Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
      fileStream.Write(fmt, 0, 4);

      Byte[] subChunk1 = BitConverter.GetBytes(16);
      fileStream.Write(subChunk1, 0, 4);

      UInt16 two = 2;
      UInt16 one = 1;

      Byte[] audioFormat = BitConverter.GetBytes(one);
      fileStream.Write(audioFormat, 0, 2);

      Byte[] numChannels = BitConverter.GetBytes(two);
      fileStream.Write(numChannels, 0, 2);

      Byte[] sampleRate = BitConverter.GetBytes(outputRate);
      fileStream.Write(sampleRate, 0, 4);

      Byte[] byteRate = BitConverter.GetBytes(outputRate * 4);
      // sampleRate * bytesPerSample*number of channels, here 44100*2*2

      fileStream.Write(byteRate, 0, 4);

      UInt16 four = 4;
      Byte[] blockAlign = BitConverter.GetBytes(four);
      fileStream.Write(blockAlign, 0, 2);

      UInt16 sixteen = 16;
      Byte[] bitsPerSample = BitConverter.GetBytes(sixteen);
      fileStream.Write(bitsPerSample, 0, 2);

      Byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
      fileStream.Write(dataString, 0, 4);

      Byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length - headerSize);
      fileStream.Write(subChunk2, 0, 4);

      fileStream.Close();
    }

    // Start video/audio muxing process, this is blocking function
    public bool StartMuxProcess(IVideoCapture videoCapture)
    {
      FFmpegEncoder ffmpegEncoder = videoCapture.GetFFmpegEncoder();
      string videoSavePath = string.Format("{0}capture_{1}x{2}_{3}_{4}.mp4",
        Config.saveFolder,
        ffmpegEncoder.outputFrameWidth, ffmpegEncoder.outputFrameHeight,
        Utils.GetTimeString(),
        Utils.GetRandomString(5));
      IntPtr nativeAPI = FFmpegEncoder_StartMuxProcess(
        ffmpegEncoder.bitrate,
        videoSavePath,
        ffmpegEncoder.videoSavePath,
        audioSavePath,
        ffmpegPath);
      if (nativeAPI == IntPtr.Zero)
      {
        OnError(EncoderErrorCode.MUXING_FAILED_TO_START);
        return false;
      }
      // Make sure generated the merge file
      int waitCount = 0;
      while (!File.Exists(videoSavePath))
      {
        if (waitCount++ < 100)
          Thread.Sleep(500);
        else
        {
          OnError(EncoderErrorCode.MUXING_FAILED);
          FFmpegEncoder_CleanMuxProcess(nativeAPI);
          return false;
        }
      }

      FFmpegEncoder_CleanMuxProcess(nativeAPI);
      //Debug.LogFormat(LOG_FORMAT, "Muxing process finish!");

      // Video capture callback
      videoCapture.OnAudioMuxingComplete(videoSavePath);

      return true;
    }

    private void SampleRateSettings()
    {
      if (sampleRate == SampleRate._8000)
      {
        outputRate = 8000;
      }
      else if (sampleRate == SampleRate._11025)
      {
        outputRate = 11025;
      }
      else if (sampleRate == SampleRate._12000)
      {
        outputRate = 12000;
      }
      else if (sampleRate == SampleRate._16000)
      {
        outputRate = 16000;
      }
      else if (sampleRate == SampleRate._22050)
      {
        outputRate = 22050;
      }
      else if (sampleRate == SampleRate._24000)
      {
        outputRate = 24000;
      }
      else if (sampleRate == SampleRate._32000)
      {
        outputRate = 32000;
      }
      else if (sampleRate == SampleRate._44100)
      {
        outputRate = 44100;
      }
      else if (sampleRate == SampleRate._48000)
      {
        outputRate = 48000;
      }
    }

    private void AudioErrorLog(EncoderErrorCode error)
    {
      Debug.LogWarning("AudioCapture Error Occured of type: " + error);
    }

    private void Awake()
    {
      if (singleton != null)
        return;
      singleton = this;

      captureStarted = false;

      OnError += AudioErrorLog;

      videoCaptures = new List<IVideoCapture>();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
      if (captureStarted)
      {
        // audio data is interlaced
        ConvertAndWrite(data);
      }
    }

    private void OnDestroy()
    {
      OnError -= AudioErrorLog;
    }

    #endregion
  }
}