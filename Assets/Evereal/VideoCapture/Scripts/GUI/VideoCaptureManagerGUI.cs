using System.Diagnostics;
using UnityEngine;

namespace Evereal.VideoCapture {

	[RequireComponent(typeof(VideoCaptureManager))]
	public class VideoCaptureManagerGUI : MonoBehaviour {

		private VideoCaptureManager videoCaptureManager;

		private void Awake()
    {
      videoCaptureManager = GetComponent<VideoCaptureManager>();
      Application.runInBackground = true;
    }

		private void OnGUI()
    {
			if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 60, 150, 50), "Open Video Folder"))
      {
        // Open video save directory
        Process.Start(Config.saveFolder);
      }
			bool stopped = false;
			bool pending = false;
			// check if still processing
			foreach (VideoCapture videoCapture in videoCaptureManager.videoCaptures) {
				if (videoCapture.status == CaptureStatus.STOPPED) {
					stopped = true;
					break;
				}
				if (videoCapture.status == CaptureStatus.PENDING) {
					pending = true;
					break;
				}
			}
			if (stopped) {
				if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Processing")) {
          // Waiting processing end
        }
				return;
			}
			if (pending) {
				if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Muxing")) {
          // Waiting processing end
        }
				return;
			}
			if (videoCaptureManager.captureStarted) {
  			if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture")) {
          videoCaptureManager.StopCapture();
        }
			}
			else {
				if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Start Capture")) {
          videoCaptureManager.StartCapture();
        }
			}
    }
	}
}