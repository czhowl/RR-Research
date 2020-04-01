using System;
using System.IO;
using UnityEngine;

namespace Evereal.VideoCapture
{
  /// <summary>
  /// Config settings.
  /// </summary>
  public class Config
  {
    // TODO, remove this, use pref
    public static string lastVideoFile = "";
    /// <summary>
    /// The video folder, save recorded video.
    /// </summary>
    private static string _saveFolder = "";
    public static string saveFolder
    {
      get
      {
        if (string.IsNullOrEmpty(_saveFolder))
        {
          _saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Evereal/VideoCapture/";
        }

        if (!_saveFolder.EndsWith("/") && !_saveFolder.EndsWith("\\"))
        {
          _saveFolder += "/";
        }

        if (!Directory.Exists(_saveFolder))
        {
          Directory.CreateDirectory(_saveFolder);
        }

        return _saveFolder;
      }
      set
      {
        _saveFolder = value;
      }
    }

    // The ffmpeg folder
    public static string ffmpegFolder
    {
      get
      {
        return Application.streamingAssetsPath + "/FFmpeg/";
      }
    }

    // The 32bit ffmpeg path for Windows
    public static string windowsFFmpeg32Path
    {
      get
      {
        return ffmpegFolder + "x86/ffmpeg.exe";
      }
    }

    // The 32bit ffmpeg path for Windows
    public static string windowsFFmpeg64Path
    {
      get
      {
        return ffmpegFolder + "x86_64/ffmpeg.exe";
      }
    }

    // The ffmpeg path for macOS
    public static string macOSFFmpegPath
    {
      get
      {
        return ffmpegFolder + "ffmpeg";
      }
    }

    // Get ffmpeg path
    public static string ffmpegPath
    {
      get
      {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (Utils.isWindows64Bit())
        {
          return windowsFFmpeg64Path;
        }
        return windowsFFmpeg32Path;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        return macOSFFmpegPath;
#else
        return "";
#endif
      }
    }

    // Check free trial version
    public static bool isFreeTrial()
    {
      return false;
    }
  }
}