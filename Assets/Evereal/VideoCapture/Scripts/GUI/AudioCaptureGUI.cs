using System.Diagnostics;
using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(AudioCapture))]
  public class AudioCaptureGUI : MonoBehaviour
  {
    private AudioCapture audioCapture;

    private void Awake()
    {
      audioCapture = GetComponent<AudioCapture>();
      Application.runInBackground = true;
    }

    private void OnGUI()
    {
      if (audioCapture.captureStarted)
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
        {
          audioCapture.StopCapture();
        }
      }
      else
      {
        if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture"))
        {
          audioCapture.StartCapture();
        }
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Open Audio Folder"))
      {
        // Open video save directory
        Process.Start(Config.saveFolder);
      }
    }
  }
}