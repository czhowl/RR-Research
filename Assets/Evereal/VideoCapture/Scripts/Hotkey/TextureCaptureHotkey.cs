using UnityEngine;

namespace Evereal.VideoCapture
{
  [RequireComponent(typeof(TextureCapture))]
  public class TextureCaptureHotkey : MonoBehaviour
  {

    [Header("Hotkeys")]
    public KeyCode startCapture = KeyCode.F1;
    public KeyCode stopCapture = KeyCode.F2;
    public bool showHintUI = true;

    private TextureCapture textureCapture;

    private void Awake()
    {
      textureCapture = GetComponent<TextureCapture>();
      Application.runInBackground = true;
    }

    void Update()
    {
      if (Input.GetKeyDown(startCapture))
      {
        textureCapture.StartCapture();
      }
      else if (Input.GetKeyDown(stopCapture))
      {
        textureCapture.StopCapture();
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