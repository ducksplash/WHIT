using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class CameraFrameSettingsCopier : EditorWindow
{
    private const string ProfileFolderName = "CameraProfiles";

    private enum DonorSource { SceneCamera, SavedProfile }

    [Serializable]
    private class CameraProfileData
    {
        public bool customRenderingSettings;
        public FrameSettings renderingPathCustomFrameSettings;
        public FrameSettingsOverrideMask renderingPathCustomFrameSettingsOverrideMask;
    }

    private Camera _donorCamera;
    private List<Camera> _supplicantCameras = new List<Camera>();

    private DonorSource _donorSource = DonorSource.SceneCamera;

    private string _profileNameToSave = "NewProfile";
    private string[] _profileFiles = Array.Empty<string>();
    private int _selectedProfileIndex = -1;

    private CameraProfileData _loadedProfile;
    private string _loadedProfileName;

    private Vector2 _scroll;

    private static string ProfileFolderPath =>
        Path.Combine(Application.persistentDataPath, ProfileFolderName);

    [MenuItem("Tools/HDRP/Camera Frame Settings Copier")]
    public static void ShowWindow()
    {
        var window = GetWindow<CameraFrameSettingsCopier>("Camera FrameSettings Copier");
        window.minSize = new Vector2(420, 480);
        window.RefreshProfileList();
    }

    private void OnEnable() => RefreshProfileList();

    private void RefreshProfileList()
    {
        if (!Directory.Exists(ProfileFolderPath))
            Directory.CreateDirectory(ProfileFolderPath);

        var files = Directory.GetFiles(ProfileFolderPath, "*.json");
        var names = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
            names[i] = Path.GetFileNameWithoutExtension(files[i]);

        _profileFiles = names;

        if (_selectedProfileIndex >= _profileFiles.Length)
            _selectedProfileIndex = _profileFiles.Length > 0 ? 0 : -1;
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Donor Source", EditorStyles.boldLabel);
        _donorSource = (DonorSource)EditorGUILayout.EnumPopup("Source", _donorSource);
        EditorGUILayout.Space();

        if (_donorSource == DonorSource.SceneCamera)
        {
            _donorCamera = (Camera)EditorGUILayout.ObjectField(
                "Donor Camera", _donorCamera, typeof(Camera), true);

            if (_donorCamera != null && _donorCamera.GetComponent<HDAdditionalCameraData>() == null)
                EditorGUILayout.HelpBox(
                    "Donor camera has no HDAdditionalCameraData component.",
                    MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField(string.IsNullOrEmpty(_loadedProfileName)
                ? "No profile loaded — use Load Profile below."
                : $"Loaded profile: {_loadedProfileName}");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Saved Profiles", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(ProfileFolderPath, EditorStyles.miniLabel);

        EditorGUILayout.BeginHorizontal();
        _selectedProfileIndex = EditorGUILayout.Popup("Profile", _selectedProfileIndex, _profileFiles);
        if (GUILayout.Button("Refresh", GUILayout.Width(70)))
            RefreshProfileList();
        EditorGUILayout.EndHorizontal();

        _profileNameToSave = EditorGUILayout.TextField("Name To Save As", _profileNameToSave);

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = _donorSource == DonorSource.SceneCamera
                      && _donorCamera != null
                      && _donorCamera.GetComponent<HDAdditionalCameraData>() != null
                      && !string.IsNullOrWhiteSpace(_profileNameToSave);
        if (GUILayout.Button("Save Profile"))
            SaveProfile();

        GUI.enabled = _selectedProfileIndex >= 0 && _selectedProfileIndex < _profileFiles.Length;
        if (GUILayout.Button("Load Profile"))
            LoadSelectedProfile();

        if (GUILayout.Button("Remove Profile"))
            RemoveSelectedProfile();

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Supplicant Cameras", EditorStyles.boldLabel);

        int removeIndex = -1;
        for (int i = 0; i < _supplicantCameras.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _supplicantCameras[i] = (Camera)EditorGUILayout.ObjectField(
                _supplicantCameras[i], typeof(Camera), true);
            if (GUILayout.Button("X", GUILayout.Width(24)))
                removeIndex = i;
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex >= 0)
            _supplicantCameras.RemoveAt(removeIndex);

        if (GUILayout.Button("Add Supplicant Camera Slot"))
            _supplicantCameras.Add(null);

        EditorGUILayout.Space();

        bool canApply = _supplicantCameras.Count > 0 &&
                         ((_donorSource == DonorSource.SceneCamera && _donorCamera != null) ||
                          (_donorSource == DonorSource.SavedProfile && _loadedProfile != null));

        GUI.enabled = canApply;
        if (GUILayout.Button("Apply To Supplicants", GUILayout.Height(32)))
            ApplyToSupplicants();
        GUI.enabled = true;

        if (!canApply)
            EditorGUILayout.HelpBox(
                "Assign a donor (scene camera or loaded profile) and at least one supplicant camera.",
                MessageType.Info);

        EditorGUILayout.EndScrollView();
    }

    // ---------- Profile file operations ----------

    private void SaveProfile()
    {
        var donorData = _donorCamera.GetComponent<HDAdditionalCameraData>();
        if (donorData == null) return;

        var profile = new CameraProfileData
        {
            customRenderingSettings = donorData.customRenderingSettings,
            renderingPathCustomFrameSettings = donorData.renderingPathCustomFrameSettings,
            renderingPathCustomFrameSettingsOverrideMask = donorData.renderingPathCustomFrameSettingsOverrideMask
        };

        string json = EditorJsonUtility.ToJson(profile, true);

        if (!Directory.Exists(ProfileFolderPath))
            Directory.CreateDirectory(ProfileFolderPath);

        string path = Path.Combine(ProfileFolderPath, _profileNameToSave + ".json");
        File.WriteAllText(path, json);

        RefreshProfileList();
        _selectedProfileIndex = Array.IndexOf(_profileFiles, _profileNameToSave);

        Debug.Log($"Saved camera profile '{_profileNameToSave}' to {path}");
    }

    private void LoadSelectedProfile()
    {
        if (_selectedProfileIndex < 0 || _selectedProfileIndex >= _profileFiles.Length) return;

        string name = _profileFiles[_selectedProfileIndex];
        string path = Path.Combine(ProfileFolderPath, name + ".json");

        if (!File.Exists(path))
        {
            EditorUtility.DisplayDialog("Load Failed", $"Profile file not found:\n{path}", "OK");
            RefreshProfileList();
            return;
        }

        string json = File.ReadAllText(path);
        var profile = new CameraProfileData();
        EditorJsonUtility.FromJsonOverwrite(json, profile);

        _loadedProfile = profile;
        _loadedProfileName = name;
        _donorSource = DonorSource.SavedProfile;

        Debug.Log($"Loaded camera profile '{name}'");
    }

    private void RemoveSelectedProfile()
    {
        if (_selectedProfileIndex < 0 || _selectedProfileIndex >= _profileFiles.Length) return;

        string name = _profileFiles[_selectedProfileIndex];
        string path = Path.Combine(ProfileFolderPath, name + ".json");

        if (EditorUtility.DisplayDialog(
                "Remove Profile", $"Delete profile '{name}'? This cannot be undone.", "Delete", "Cancel"))
        {
            if (File.Exists(path))
                File.Delete(path);

            if (_loadedProfileName == name)
            {
                _loadedProfile = null;
                _loadedProfileName = null;
            }

            RefreshProfileList();
        }
    }

    // ---------- Apply ----------

    private void ApplyToSupplicants()
    {
        CameraProfileData sourceData;

        if (_donorSource == DonorSource.SceneCamera)
        {
            if (_donorCamera == null) return;

            var donorData = _donorCamera.GetComponent<HDAdditionalCameraData>();
            if (donorData == null)
            {
                EditorUtility.DisplayDialog("Cannot Apply",
                    "Donor camera has no HDAdditionalCameraData component.", "OK");
                return;
            }

            sourceData = new CameraProfileData
            {
                customRenderingSettings = donorData.customRenderingSettings,
                renderingPathCustomFrameSettings = donorData.renderingPathCustomFrameSettings,
                renderingPathCustomFrameSettingsOverrideMask = donorData.renderingPathCustomFrameSettingsOverrideMask
            };
        }
        else
        {
            if (_loadedProfile == null)
            {
                EditorUtility.DisplayDialog("Cannot Apply",
                    "No saved profile is loaded. Use Load Profile first.", "OK");
                return;
            }
            sourceData = _loadedProfile;
        }

        int appliedCount = 0;

        foreach (var cam in _supplicantCameras)
        {
            if (cam == null) continue;

            var targetData = cam.GetComponent<HDAdditionalCameraData>();
            if (targetData == null)
            {
                Debug.LogWarning($"Skipped '{cam.name}': no HDAdditionalCameraData component found.");
                continue;
            }

            Undo.RecordObject(targetData, "Copy Camera Frame Settings");

            targetData.customRenderingSettings = sourceData.customRenderingSettings;
            targetData.renderingPathCustomFrameSettings = sourceData.renderingPathCustomFrameSettings;
            targetData.renderingPathCustomFrameSettingsOverrideMask = sourceData.renderingPathCustomFrameSettingsOverrideMask;

            EditorUtility.SetDirty(targetData);
            appliedCount++;
        }

        if (appliedCount > 0)
        {
            Debug.Log($"Applied camera frame settings to {appliedCount} supplicant camera(s).");
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}