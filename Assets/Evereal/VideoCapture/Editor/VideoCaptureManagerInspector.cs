using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Evereal.VideoCapture.Editor
{
	/// <summary>
  /// Inspector script for <c>VideoCaptureManager</c> component.
  /// </summary>
	[CustomEditor(typeof(VideoCaptureManager))]
  public class VideoCaptureManagerInspector : UnityEditor.Editor
  {
		VideoCaptureManager manager;
		SerializedProperty videoCaptures;

		public void OnEnable() {
			manager = (VideoCaptureManager)target;
			videoCaptures = serializedObject.FindProperty("videoCaptures");
    }

		public override void OnInspectorGUI() {

			// Capture Control Section
      GUILayout.Label("Capture Control", EditorStyles.boldLabel);

			manager.startOnAwake = EditorGUILayout.Toggle("Start On Awake", manager.startOnAwake);

			if (manager.startOnAwake)
      {
        manager.captureTime = EditorGUILayout.FloatField("Capture Duration (Sec)", manager.captureTime);
        manager.quitAfterCapture = EditorGUILayout.Toggle("Quit After Capture", manager.quitAfterCapture);
      }

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      manager.saveFolder = EditorGUILayout.TextField("Save Folder", manager.saveFolder);
      manager.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", manager.captureMode);
      if (manager.captureMode == CaptureMode._360) {
        manager.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", manager.projectionType);
      }
      if (manager.captureMode == CaptureMode._360 &&
          manager.projectionType == ProjectionType.CUBEMAP) {
        manager.stereoMode = StereoMode.NONE;
      }
      else {
        manager.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", manager.stereoMode);
      }
      if (manager.stereoMode != StereoMode.NONE) {
        manager.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", manager.interpupillaryDistance);
      }
      manager.captureAudio = EditorGUILayout.Toggle("Capture Audio", manager.captureAudio);
      manager.offlineRender = EditorGUILayout.Toggle("Offline Render", manager.offlineRender);

      // Capture Options Section
      GUILayout.Label("Video Settings", EditorStyles.boldLabel);

      manager.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", manager.resolutionPreset);
      if (manager.resolutionPreset == ResolutionPreset.CUSTOM) {
        manager.frameWidth = EditorGUILayout.IntField("Frame Width", manager.frameWidth);
        manager.frameHeight = EditorGUILayout.IntField("Frame Height", manager.frameHeight);
        manager.bitrate = EditorGUILayout.IntField("Bitrate (Kbps)", manager.bitrate);
      }
      manager.frameRate = (System.Int16)EditorGUILayout.IntField("Frame Rate", manager.frameRate);
      if (manager.captureMode == CaptureMode._360) {
        manager.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", manager.cubemapFaceSize);
      }
      manager.antiAliasingSetting = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing Settings", manager.antiAliasingSetting);

      // Capture Options Section
      GUILayout.Label("Encoder Components", EditorStyles.boldLabel);

      manager.softwareEncodingOnly = EditorGUILayout.Toggle("Software Encoding Only", manager.softwareEncodingOnly);

			serializedObject.Update();
			EditorGUILayout.PropertyField(videoCaptures, new GUIContent("Video Captures"), true);
			// Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
      serializedObject.ApplyModifiedProperties();

      if (GUI.changed)
      {
        EditorUtility.SetDirty(target);
      }
		}
  }
}