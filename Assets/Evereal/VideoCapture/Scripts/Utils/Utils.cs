using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Evereal.VideoCapture
{
  public class Utils
  {
    private static System.Random random = new System.Random();

    /// <summary>
    /// Save render texture to PNG image file.
    /// </summary>
    /// <param name="rtex">RenderTexture.</param>
    /// <param name="fileName">File name.</param>
    public static void RenderTextureToPNG(RenderTexture rtex, string fileName)
    {
      Texture2D tex = new Texture2D(rtex.width, rtex.height, TextureFormat.RGB24, false);
      RenderTexture.active = rtex;
      tex.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0, false);
      RenderTexture.active = null;
      TextureToPNG(tex, fileName);
    }

    /// <summary>
    /// Save texture to PNG image file.
    /// </summary>
    /// <param name="tex">Tex.</param>
    /// <param name="fileName">File name.</param>
    public static void TextureToPNG(Texture2D tex, string fileName)
    {
      string filePath = Config.saveFolder + fileName;
      byte[] imageBytes = tex.EncodeToPNG();
      System.IO.File.WriteAllBytes(filePath, imageBytes);
      Debug.Log("Save texture " + filePath);
    }

    /// <summary>
    /// Save render texture to JPG image file.
    /// </summary>
    /// <param name="rtex">RenderTexture.</param>
    /// <param name="fileName">File name.</param>
    public static void RenderTextureToJPG(RenderTexture rtex, string fileName)
    {
      Texture2D tex = new Texture2D(rtex.width, rtex.height, TextureFormat.RGB24, false);
      RenderTexture.active = rtex;
      tex.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0, false);
      RenderTexture.active = null;
      TextureToJPG(tex, fileName);
    }

    /// <summary>
    /// Save texture to JPG image file.
    /// </summary>
    /// <param name="tex">Tex.</param>
    /// <param name="fileName">File name.</param>
    public static void TextureToJPG(Texture2D tex, string fileName)
    {
      string filePath = Config.saveFolder + fileName;
      byte[] imageBytes = tex.EncodeToJPG();
      System.IO.File.WriteAllBytes(filePath, imageBytes);
      Debug.Log("Save texture " + filePath);
    }

    /// <summary>
    /// Create materials which will be used for equirect and cubemap generation.
    /// </summary>
    /// <param name="sName"> shader name </param>
    /// <param name="m2Create"> material </param>
    /// <returns></returns>
    public static Material CreateMaterial(string sName, Material m2Create)
    {
      if (m2Create && (m2Create.shader.name == sName))
        return m2Create;
      Shader s = Shader.Find(sName);
      return CreateMaterial(s, m2Create);
    }

    /// <summary>
    /// Create materials which will be used for equirect and cubemap generation.
    /// </summary>
    /// <param name="s"> shader code </param>
    /// <param name="m2Create"> material </param>
    /// <returns></returns>
    public static Material CreateMaterial(Shader s, Material m2Create)
    {
      if (!s)
      {
        Debug.Log("Create material missing shader!");
        return null;
      }

      if (m2Create && (m2Create.shader == s) && (s.isSupported))
        return m2Create;

      if (!s.isSupported)
      {
        return null;
      }

      m2Create = new Material(s);
      m2Create.hideFlags = HideFlags.DontSave;

      return m2Create;
    }

    /// <summary>
    /// Create RenderTexture.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="antiAliasing"></param>
    /// <param name="t2Create"></param>
    /// <param name="create"></param>
    /// <returns></returns>
    public static RenderTexture CreateRenderTexture(int width, int height, int depth, int antiAliasing, RenderTexture t2Create, bool create = true)
    {
      if (t2Create &&
        (t2Create.width == width) && (t2Create.height == height) && (t2Create.depth == depth) &&
        (t2Create.antiAliasing == antiAliasing) && (t2Create.IsCreated() == create))
        return t2Create;

      t2Create = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32);
      t2Create.antiAliasing = antiAliasing;
      t2Create.hideFlags = HideFlags.HideAndDontSave;

      // Make sure render texture is created.
      if (create)
        t2Create.Create();

      return t2Create;
    }

    public static Cubemap CreateCubemap(int cubemapSize, Cubemap c2Create)
    {
      if (c2Create && c2Create.width == cubemapSize)
        return c2Create;

      c2Create = new Cubemap(cubemapSize, TextureFormat.RGB24, false);

      return c2Create;
    }

    /// <summary>
    /// Create Texture2D.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="t2Create"></param>
    /// <returns></returns>
    public static Texture2D CreateTexture(int width, int height, Texture2D t2Create)
    {
      if (t2Create && (t2Create.width == width) && (t2Create.height == height))
        return t2Create;

      t2Create = new Texture2D(width, height, TextureFormat.RGB24, false);
      t2Create.hideFlags = HideFlags.HideAndDontSave;

      return t2Create;
    }

    /// <summary>
    /// Generate random string.
    /// </summary>
    /// <param name="length">random string length</param>
    /// <returns></returns>
    public static string GetRandomString(int length)
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GetTimeString()
    {
      return DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
    }

    public static bool isWindows64Bit()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
      string sysInfo = SystemInfo.operatingSystem;
      return sysInfo.Substring(sysInfo.Length - 5).Equals("64bit");
#else
      return false;
#endif
    }

    public static void EncodeVideo4K(string videoFile)
    {
      if (videoFile.Length == 0)
        return;
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(Config.ffmpegPath, " -i " + videoFile + " -s 3840x2160  " + videoFile.Replace(ext, "_4K" + ext));
    }

    public static void ConvertVideoGif(string videoFile)
    {
      if (videoFile.Length == 0)
        return;
      string ext = System.IO.Path.GetExtension(videoFile);
      Process.Start(Config.ffmpegPath, " -i " + Config.lastVideoFile + " -s 1920x1080 -pix_fmt rgb24  " + videoFile.Replace(ext, ".gif"));
    }

    public static void WriteLogToDisk(string msg) {
      StreamWriter writer = new StreamWriter("UnityLog.txt", true);
      writer.WriteLine(msg);
      writer.Close();
    }
  }
}