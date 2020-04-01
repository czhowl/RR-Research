using System.Diagnostics;
using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(VideoCapture))]
  public class VideoCaptureGUI : MonoBehaviour
  {
    private VideoCapture videoCapture;

    private void Awake()
    {
      videoCapture = GetComponent<VideoCapture>();
      Application.runInBackground = true;
    }

    private void OnGUI()
    {
      if (
        videoCapture.status == CaptureStatus.READY)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture"))
        {
          videoCapture.StartCapture();
        }
      }
      else if (videoCapture.status == CaptureStatus.STARTED)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
        {
          videoCapture.StopCapture();
        }
      }
      else if (videoCapture.status == CaptureStatus.PENDING)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Muxing"))
        {
          // Waiting processing end
        }
      }
      else if (videoCapture.status == CaptureStatus.STOPPED)
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