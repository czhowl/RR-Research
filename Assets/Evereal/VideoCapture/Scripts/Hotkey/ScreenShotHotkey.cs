using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(ScreenShot))]
  public class ScreenShotHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public bool showHintUI = true;

    private ScreenShot screenShot;

    private void Awake()
    {
      screenShot = GetComponent<ScreenShot>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyDown(startCapture))
      {
        screenShot.StartCapture();
      }
    }

    void OnGUI()
    {
      if (showHintUI)
      {
        GUI.Label(new Rect(10, 10, 200, 20), "Start Capture: " + startCapture);
      }
    }
  }
}