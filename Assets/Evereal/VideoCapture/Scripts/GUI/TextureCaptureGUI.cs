using System.Diagnostics;
using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(TextureCapture))]
  public class TextureCaptureGUI : MonoBehaviour
  {
    public RenderTexture renderTexture = null;

    private TextureCapture textureCapture;

    private void Awake()
    {
      textureCapture = GetComponent<TextureCapture>();
      textureCapture.SetRenderTexture(renderTexture);

      Application.runInBackground = true;
    }

    private void OnGUI()
    {
      if (
        textureCapture.status == CaptureStatus.READY)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture"))
        {
          textureCapture.StartCapture();
        }
      }
      else if (textureCapture.status == CaptureStatus.STARTED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
        {
          textureCapture.StopCapture();
        }
      }
      else if (textureCapture.status == CaptureStatus.PENDING)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Muxing"))
        {
          // Waiting processing end
        }
      }
      else if (textureCapture.status == CaptureStatus.STOPPED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Processing"))
        {
          // Waiting processing end
        }
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Open Video Folder"))
      {
        // Open video save directory
        Process.Start(Config.saveFolder);
      }
    }
  }
}