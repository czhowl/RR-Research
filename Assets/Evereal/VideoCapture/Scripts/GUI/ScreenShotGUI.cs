using System.Diagnostics;
using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(ScreenShot))]
  public class ScreenShotGUI : MonoBehaviour
  {
    private ScreenShot screenShot;

    private void Awake()
    {
      screenShot = GetComponent<ScreenShot>();

      Application.runInBackground = true;
    }

    private void OnGUI()
    {
      if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Take Screenshot"))
      {
        screenShot.StartCapture();
      }
      if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Open Image Folder"))
      {
        // Open image save directory
        Process.Start(Config.saveFolder);
      }
    }
  }
}