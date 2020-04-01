using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(AudioCapture))]
  public class AudioCaptureHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public KeyCode stopCapture = KeyCode.F2;
    public bool showHintUI = true;

    private AudioCapture audioCapture;

    private void Awake()
    {
      audioCapture = GetComponent<AudioCapture>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyDown(startCapture))
      {
        audioCapture.StartCapture();
      }
      else if (Input.GetKeyDown(stopCapture))
      {
        audioCapture.StopCapture();
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