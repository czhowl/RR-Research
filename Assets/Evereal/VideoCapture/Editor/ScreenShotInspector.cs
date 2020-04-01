using UnityEngine;
using UnityEditor;

namespace Evereal.VideoCapture.Editor
{
  /// <summary>
  /// Inspector script for <c>ScreenShot</c> component.
  /// </summary>
  [CustomEditor(typeof(ScreenShot))]
  public class ScreenShotInspector : UnityEditor.Editor
  {
    ScreenShot screenshot;

    public void OnEnable()
    {
      screenshot = (ScreenShot)target;
    }

    public override void OnInspectorGUI()
    {
      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      screenshot.saveFolder = EditorGUILayout.TextField("Save Folder", screenshot.saveFolder);

      screenshot.captureMode = (CaptureMode)EditorGUILayout.EnumPopup("Capture Mode", screenshot.captureMode);
      if (screenshot.captureMode == CaptureMode._360)
      {
        screenshot.projectionType = (ProjectionType)EditorGUILayout.EnumPopup("Projection Type", screenshot.projectionType);
      }
      if (screenshot.captureMode == CaptureMode._360 &&
          screenshot.projectionType == ProjectionType.CUBEMAP)
      {
        screenshot.stereoMode = StereoMode.NONE;
      }
      else
      {
        screenshot.stereoMode = (StereoMode)EditorGUILayout.EnumPopup("Stereo Mode", screenshot.stereoMode);
      }
      if (screenshot.stereoMode != StereoMode.NONE)
      {
        screenshot.interpupillaryDistance = EditorGUILayout.FloatField("Interpupillary Distance", screenshot.interpupillaryDistance);
      }

      // Capture Options Section
      GUILayout.Label("Screenshot Settings", EditorStyles.boldLabel);

      screenshot.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", screenshot.resolutionPreset);
      if (screenshot.resolutionPreset == ResolutionPreset.CUSTOM)
      {
        screenshot.frameWidth = EditorGUILayout.IntField("Frame Width", screenshot.frameWidth);
        screenshot.frameHeight = EditorGUILayout.IntField("Frame Height", screenshot.frameHeight);
      }
      if (screenshot.captureMode == CaptureMode._360)
      {
        screenshot.cubemapFaceSize = (CubemapFaceSize)EditorGUILayout.EnumPopup("Cubemap Face Size", screenshot.cubemapFaceSize);
      }
      screenshot.antiAliasingSetting = (AntiAliasingSetting)EditorGUILayout.EnumPopup("Anti Aliasing Settings", screenshot.antiAliasingSetting);

      // Capture Options Section
      GUILayout.Label("Encoder Components", EditorStyles.boldLabel);

      screenshot.softwareEncodingOnly = EditorGUILayout.Toggle("Software Encoding Only", screenshot.softwareEncodingOnly);

      if (GUI.changed)
      {
        EditorUtility.SetDirty(target);
      }
    }
  }
}