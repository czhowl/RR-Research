using System.IO;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace Evereal.VideoCapture.Editor
{
  public class VideoCaptureMenu : MonoBehaviour
  {
    private const string WINDOWS_FFMPEG_32_DOWNLOAD_URL = "https://evereal.s3-us-west-1.amazonaws.com/ffmpeg/4.2/x86/ffmpeg.exe";
    private const string WINDOWS_FFMPEG_64_DOWNLOAD_URL = "https://evereal.s3-us-west-1.amazonaws.com/ffmpeg/4.2/x86_64/ffmpeg.exe";
    private const string OSX_FFMPEG_DOWNLOAD_URL = "https://evereal.s3-us-west-1.amazonaws.com/ffmpeg/4.2/ffmpeg";

    private static Thread downloadFFmpegThread;

    [MenuItem("Evereal/VideoCapture/GameObject/VideoCapture", false, 10)]
    private static void CreateVideoCaptureObject(MenuCommand menuCommand)
    {
      GameObject videoCapturePrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/VideoCapture")) as GameObject;
      videoCapturePrefab.name = "VideoCapture";
      //PrefabUtility.DisconnectPrefabInstance(videoCapturePrefab);
      GameObjectUtility.SetParentAndAlign(videoCapturePrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(videoCapturePrefab, "Create " + videoCapturePrefab.name);
      Selection.activeObject = videoCapturePrefab;
    }

    [MenuItem("Evereal/VideoCapture/GameObject/AudioCapture", false, 10)]
    private static void CreateAudioCaptureObject(MenuCommand menuCommand)
    {
      GameObject audioCapturePrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/AudioCapture")) as GameObject;
      audioCapturePrefab.name = "AudioCapture";
      //PrefabUtility.DisconnectPrefabInstance(audioCapturePrefab);
      GameObjectUtility.SetParentAndAlign(audioCapturePrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(audioCapturePrefab, "Create " + audioCapturePrefab.name);
      Selection.activeObject = audioCapturePrefab;
    }

    [MenuItem("Evereal/VideoCapture/GameObject/TextureCapture", false, 10)]
    private static void CreateTextureCaptureObject(MenuCommand menuCommand)
    {
      GameObject textureCapturePrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/TextureCapture")) as GameObject;
      textureCapturePrefab.name = "TextureCapture";
      //PrefabUtility.DisconnectPrefabInstance(textureCapturePrefab);
      GameObjectUtility.SetParentAndAlign(textureCapturePrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(textureCapturePrefab, "Create " + textureCapturePrefab.name);
      Selection.activeObject = textureCapturePrefab;
    }

    [MenuItem("Evereal/VideoCapture/GameObject/ScreenShot", false, 10)]
    private static void CreateScreenShotObject(MenuCommand menuCommand)
    {
      GameObject screenshotPrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Prefabs/ScreenShot")) as GameObject;
      screenshotPrefab.name = "ScreenShot";
      //PrefabUtility.DisconnectPrefabInstance(screenshotPrefab);
      GameObjectUtility.SetParentAndAlign(screenshotPrefab, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(screenshotPrefab, "Create " + screenshotPrefab.name);
      Selection.activeObject = screenshotPrefab;
    }

    [MenuItem("Evereal/VideoCapture/FFmpeg/Download Windows Build (32 bit)")]
    private static void DownloadFFmpeg32ForWindows()
    {
      string ffmpeg32Folder = Config.ffmpegFolder + "x86/";
      if (!Directory.Exists(ffmpeg32Folder))
      {
        Directory.CreateDirectory(ffmpeg32Folder);
      }

      if (downloadFFmpegThread != null)
      {
        if (downloadFFmpegThread.IsAlive)
          downloadFFmpegThread.Abort();
        downloadFFmpegThread = null;
      }
      string windowsFFmpegPath = Config.windowsFFmpeg32Path;
      downloadFFmpegThread = new Thread(
        () => DownloadFFmpegThreadFunction(WINDOWS_FFMPEG_32_DOWNLOAD_URL, windowsFFmpegPath));
      downloadFFmpegThread.Priority = System.Threading.ThreadPriority.Lowest;
      downloadFFmpegThread.IsBackground = true;
      downloadFFmpegThread.Start();
    }

    [MenuItem("Evereal/VideoCapture/FFmpeg/Download Windows Build (64 bit)")]
    private static void DownloadFFmpeg64ForWindows()
    {
      string ffmpeg64Folder = Config.ffmpegFolder + "x86_64/";
      if (!Directory.Exists(ffmpeg64Folder))
      {
        Directory.CreateDirectory(ffmpeg64Folder);
      }

      if (downloadFFmpegThread != null)
      {
        if (downloadFFmpegThread.IsAlive)
          downloadFFmpegThread.Abort();
        downloadFFmpegThread = null;
      }
      string windowsFFmpegPath = Config.windowsFFmpeg64Path;
      downloadFFmpegThread = new Thread(
        () => DownloadFFmpegThreadFunction(WINDOWS_FFMPEG_64_DOWNLOAD_URL, windowsFFmpegPath));
      downloadFFmpegThread.Priority = System.Threading.ThreadPriority.Lowest;
      downloadFFmpegThread.IsBackground = true;
      downloadFFmpegThread.Start();
    }

    [MenuItem("Evereal/VideoCapture/FFmpeg/Download macOS Build")]
    private static void DownloadFFmpegForOSX()
    {
      if (!Directory.Exists(Config.ffmpegFolder))
      {
        Directory.CreateDirectory(Config.ffmpegFolder);
      }

      if (downloadFFmpegThread != null)
      {
        if (downloadFFmpegThread.IsAlive)
          downloadFFmpegThread.Abort();
        downloadFFmpegThread = null;
      }
      string macOSFFmpegPath = Config.macOSFFmpegPath;
      downloadFFmpegThread = new Thread(
        () => DownloadFFmpegThreadFunction(OSX_FFMPEG_DOWNLOAD_URL, macOSFFmpegPath));
      downloadFFmpegThread.Priority = System.Threading.ThreadPriority.Lowest;
      downloadFFmpegThread.IsBackground = true;
      downloadFFmpegThread.Start();
    }

    [MenuItem("Evereal/VideoCapture/FFmpeg/Grant macOS Build Permission")]
    private static void GrantFFmpegPermissionForOSX()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
      CommandProcess.Run("chmod", "a+x " + "\"" + Config.macOSFFmpegPath + "\"");
      UnityEngine.Debug.Log("Grant permission for: " + Config.macOSFFmpegPath);
#endif
    }

    private static void DownloadFFmpegThreadFunction(string downloadUrl, string savePath)
    {
      UnityEngine.Debug.Log("Download FFmpeg in the background, please wait a few minutes until complete...");
      CommandProcess.Run("curl", downloadUrl + " --output " + "\"" + savePath + "\"");
      GrantFFmpegPermissionForOSX();
      UnityEngine.Debug.Log("Download FFmpeg complete!");
    }
  }
}