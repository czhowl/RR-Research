using UnityEngine;
using UnityEditor;

namespace Evereal.VideoCapture.Editor
{
  /// <summary>
  /// Inspector script for <c>TextureCapture</c> component.
  /// </summary>
  [CustomEditor(typeof(TextureCapture))]
  public class TextureCaptureInspector : UnityEditor.Editor
  {
    TextureCapture textureCapture;
    SerializedProperty inputTexture;

    public void OnEnable()
    {
      textureCapture = (TextureCapture)target;
      inputTexture = serializedObject.FindProperty("inputTexture");
    }

    public override void OnInspectorGUI()
    {
      // Capture Control Section
      GUILayout.Label("Capture Control", EditorStyles.boldLabel);

      textureCapture.startOnAwake = EditorGUILayout.Toggle("Start On Awake", textureCapture.startOnAwake);
      if (textureCapture.startOnAwake)
      {
        textureCapture.captureTime = EditorGUILayout.FloatField("Capture Duration (Sec)", textureCapture.captureTime);
        textureCapture.quitAfterCapture = EditorGUILayout.Toggle("Quit After Capture", textureCapture.quitAfterCapture);
      }

      // Capture Options Section
      GUILayout.Label("Capture Options", EditorStyles.boldLabel);

      textureCapture.videoCaptureType = (VideoCaptureType)EditorGUILayout.EnumPopup("Video Capture Type", textureCapture.videoCaptureType);
      if (textureCapture.videoCaptureType == VideoCaptureType.VOD)
      {
        textureCapture.saveFolder = EditorGUILayout.TextField("Save Folder", textureCapture.saveFolder);
      }
      // else if (textureCapture.videoCaptureType == VideoCaptureType.LIVE) {
      //   textureCapture.liveStreamUrl = EditorGUILayout.TextField("Live Stream Url", textureCapture.liveStreamUrl);
      // }

      serializedObject.Update();
      EditorGUILayout.PropertyField(inputTexture, new GUIContent("Render Texture"), true);
      // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
      serializedObject.ApplyModifiedProperties();

      textureCapture.captureAudio = EditorGUILayout.Toggle("Capture Audio", textureCapture.captureAudio);
      textureCapture.offlineRender = EditorGUILayout.Toggle("Offline Render", textureCapture.offlineRender);

      // Capture Options Section
      GUILayout.Label("Video Settings", EditorStyles.boldLabel);

      textureCapture.resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution Preset", textureCapture.resolutionPreset);
      if (textureCapture.resolutionPreset == ResolutionPreset.CUSTOM)
      {
        textureCapture.frameWidth = EditorGUILayout.IntField("Frame Width", textureCapture.frameWidth);
        textureCapture.frameHeight = EditorGUILayout.IntField("Frame Height", textureCapture.frameHeight);
        textureCapture.bitrate = EditorGUILayout.IntField("Bitrate (Kbps)", textureCapture.bitrate);
      }
      textureCapture.frameRate = (System.Int16)EditorGUILayout.IntField("Frame Rate", textureCapture.frameRate);

      // Capture Options Section
      GUILayout.Label("Encoder Components", EditorStyles.boldLabel);

      textureCapture.softwareEncodingOnly = EditorGUILayout.Toggle("Software Encoding Only", textureCapture.softwareEncodingOnly);

      if (GUI.changed)
      {
        EditorUtility.SetDirty(target);
      }
    }
  }
}