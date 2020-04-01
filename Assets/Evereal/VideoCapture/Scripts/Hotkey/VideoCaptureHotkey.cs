using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(VideoCapture))]
  public class VideoCaptureHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public KeyCode stopCapture = KeyCode.F2;
    public bool showHintUI = true;

    private VideoCapture videoCapture;

    private void Awake()
    {
      videoCapture = GetComponent<VideoCapture>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyDown(startCapture))
      {
        videoCapture.StartCapture();
      }
      else if (Input.GetKeyDown(stopCapture))
      {
        videoCapture.StopCapture();
      }
    }

    void OnGUI()
    {
      if (showHintUI)
      {
        GUI.Label(new Rect(10, 10, 200, 20), "Start Capture: " + startCapture);
        GUI.Label(new Rect(10, 30, 200, 20), "Stop Capture: " + stopCapture);
      }
    }
  }
}