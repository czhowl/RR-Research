using UnityEngine;
using UnityEditor;

namespace Evereal.VideoCapture.Editor
{
  /// <summary>
  /// Inspector script for <c>VideoCapture</c> component.
  /// </summary>
  [CustomEditor(typeof(VideoCapture))]
  public class VideoCaptureInspector : UnityEditor.Editor
  {
    VideoCapture videoCapture;

    public void OnEnable()
    {
      videoCapture = (VideoCapture)target;
    }

    public override void OnInspectorGUI()
    {
      // Capture Control Section
      GUILayout.Label("Capture Control", EditorStyles.boldLabel);

      videoCapture.startOnAwake = EditorGUILayout.Toggle("Start On Awake", videoCapture.startOnAwake);
      if (videoCapture.startOnAwake)
      {
        videoCapture.captureTime = EditorGUILayout.FloatField("Capture Duration (Sec)", videoCapture.captureTime);
        videoCapture.quitAfterCapture = EditorGUILayout.Toggle("Quit After Capture", videoCapture.quitAfterCapture);
      }

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      videoCapture.videoCaptureType = (VideoCaptureType)EditorGUILayout.EnumPopup("Video Capture Type", videoCapture.videoCaptureType);
      if (videoCapture.videoCaptureType == VideoCaptureType.VOD)
      {
        videoCapture.saveFolder = EditorGUILayout.TextField("Save Folder", videoCapture.saveFolder);
      }
      // else if (videoCapture.videoCaptureType == VideoCaptureType.LIVE) {
      //   videoCapture.liveStreamUrl = EditorGUILayout.TextField("Live Stream Url", videoCapture.liveStreamUrl);
      // }
      videoCapture.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", videoCapture.captureMode);
      if (videoCapture.captureMode == CaptureMode._360)
      {
        videoCapture.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", videoCapture.projectionType);
      }
      if (videoCapture.captureMode == CaptureMode._360 &&
          videoCapture.projectionType == ProjectionType.CUBEMAP)
      {
        videoCapture.stereoMode = StereoMode.NONE;
      }
      else
      {
        videoCapture.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", videoCapture.stereoMode);
      }
      if (videoCapture.stereoMode != StereoMode.NONE)
      {
        videoCapture.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", videoCapture.interpupillaryDistance);
      }
      videoCapture.captureAudio = EditorGUILayout.Toggle("Capture Audio", videoCapture.captureAudio);
      videoCapture.captureMicrophone = EditorGUILayout.Toggle("Capture Microphone", videoCapture.captureMicrophone);
      videoCapture.offlineRender = EditorGUILayout.Toggle("Offline Render", videoCapture.offlineRender);

      // Capture Options Section
      GUILayout.Label("Video Settings", EditorStyles.boldLabel);

      videoCapture.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", videoCapture.resolutionPreset);
      if (videoCapture.resolutionPreset == ResolutionPreset.CUSTOM)
      {
        videoCapture.frameWidth = EditorGUILayout.IntField("Frame Width", videoCapture.frameWidth);
        videoCapture.frameHeight = EditorGUILayout.IntField("Frame Height", videoCapture.frameHeight);
        videoCapture.bitrate = EditorGUILayout.IntField("Bitrate (Kbps)", videoCapture.bitrate);
      }
      videoCapture.frameRate = (System.Int16)EditorGUILayout.IntField("Frame Rate", videoCapture.frameRate);
      if (videoCapture.captureMode == CaptureMode._360)
      {
        videoCapture.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", videoCapture.cubemapFaceSize);
      }
      videoCapture.antiAliasingSetting = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing Settings", videoCapture.antiAliasingSetting);

      // Capture Options Section
      GUILayout.Label("Encoder Components", EditorStyles.boldLabel);

      videoCapture.softwareEncodingOnly = EditorGUILayout.Toggle("Software Encoding Only", videoCapture.softwareEncodingOnly);

      if (GUI.changed)
      {
        EditorUtility.SetDirty(target);
      }
    }
  }
}