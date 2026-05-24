using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerStatus : MonoBehaviour
{
    // ── Storage keys ───────────────────────────────────────────────────────
    private const string KEY_PUNTS          = "player_punts";
    private const string KEY_DEATHS         = "player_deaths";
    private const string KEY_PHOTOS_TAKEN   = "player_photos_taken";

    // ── Backing fields ─────────────────────────────────────────────────────
    private float _punts;
    private int   _numberOfDeaths;
    private int   _photosTaken;

    // ── Public properties — read from memory, write through to StoredPrefs ─
    public float Punts
    {
        get => _punts;
        set
        {
            _punts = value;
            if (StoredPrefs.Instance != null)
            {
                StoredPrefs.Instance.SetFloat(KEY_PUNTS, _punts);
                StoredPrefs.Instance.Save();
            }
        }
    }

    [Header("Stats")]
    public int NumberOfDeaths
    {
        get => _numberOfDeaths;
        set
        {
            _numberOfDeaths = value;
            if (StoredPrefs.Instance != null)
            {
                StoredPrefs.Instance.SetInt(KEY_DEATHS, _numberOfDeaths);
                StoredPrefs.Instance.Save();
            }
        }
    }

    public int PhotosTaken
    {
        get => _photosTaken;
        set
        {
            _photosTaken = value;
            if (StoredPrefs.Instance != null)
            {
                StoredPrefs.Instance.SetInt(KEY_PHOTOS_TAKEN, _photosTaken);
                StoredPrefs.Instance.Save();
            }
        }
    }


    private void Start()
    {
        // StoredPrefs may still be loading from disk — WhenLoaded guarantees
        // the callback fires only once data is fully deserialised.
        StoredPrefs.WhenLoaded(LoadFromPrefs);
    }

    // ── Load ───────────────────────────────────────────────────────────────
    private void LoadFromPrefs()
    {
        _punts         = StoredPrefs.Instance.GetFloat(KEY_PUNTS,        0f);
        _numberOfDeaths = StoredPrefs.Instance.GetInt(KEY_DEATHS,         0);
        _photosTaken    = StoredPrefs.Instance.GetInt(KEY_PHOTOS_TAKEN,   0);

        Debug.Log($"[PlayerStatus] Loaded — Punts:{_punts} Deaths:{_numberOfDeaths} Photos:{_photosTaken}");
    }

    // ── Convenience helpers ────────────────────────────────────────────────
    public void AddPunt(float amount = 1f)   => Punts          += amount;
    public void AddDeath()                   => NumberOfDeaths += 1;
    public void AddPhoto()                   => PhotosTaken    += 1;

    public void ResetAll()
    {
        Punts          = 0f;
        NumberOfDeaths = 0;
        PhotosTaken    = 0;
    }
}

// ── Editor ─────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
[CustomEditor(typeof(PlayerStatus))]
public class PlayerStatusEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerStatus ps = (PlayerStatus)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Live Values", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.FloatField("Punts",           ps.Punts);
            EditorGUILayout.IntField ("Deaths",           ps.NumberOfDeaths);
            EditorGUILayout.IntField ("Photos Taken",     ps.PhotosTaken);
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Punt"))  ps.AddPunt();
            if (GUILayout.Button("+ Death")) ps.AddDeath();
            if (GUILayout.Button("+ Photo")) ps.AddPhoto();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Reset All Stats", GUILayout.Height(32)))
            {
                if (EditorUtility.DisplayDialog("Reset All Stats",
                    "This will zero all player stats and save immediately. Are you sure?", "Reset", "Cancel"))
                    ps.ResetAll();
            }
            GUI.backgroundColor = Color.white;
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Controls are available in Play Mode only.", MessageType.None);

        if (Application.isPlaying)
            Repaint(); // keep live values refreshing while running
    }
}
#endif