using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class AudioSlave : MonoBehaviour
{
    [Header("Libraries")]
    public List<BGM> BGMList = new List<BGM>();
    public List<SFX> SFXList = new List<SFX>();

    [Header("Mixers (must contain matching Output Groups)")]
    public AudioMixer BGMMixer;
    public AudioMixer SFXMixer;

    [Tooltip("Name of the AudioMixerGroup inside BGMMixer to output to (e.g. 'Master' or 'BGM').")]
    public string BGMGroupName = "Master";

    [Tooltip("Name of the AudioMixerGroup inside SFXMixer to output to (e.g. 'Master' or 'SFX').")]
    public string SFXGroupName = "Master";

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    private void Awake()
    {
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _sfxSource = gameObject.AddComponent<AudioSource>();

        // BGM defaults
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.spatialBlend = 0f; 

        // SFX defaults
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f;

        // Route to mixer groups (if configured)
        if (BGMMixer != null)
        {
            var groups = BGMMixer.FindMatchingGroups(BGMGroupName);
            if (groups != null && groups.Length > 0) _bgmSource.outputAudioMixerGroup = groups[0];
            else Debug.LogWarning($"AudioSlave: No BGM mixer group found named '{BGMGroupName}' in BGMMixer.");
        }

        if (SFXMixer != null)
        {
            var groups = SFXMixer.FindMatchingGroups(SFXGroupName);
            if (groups != null && groups.Length > 0) _sfxSource.outputAudioMixerGroup = groups[0];
            else Debug.LogWarning($"AudioSlave: No SFX mixer group found named '{SFXGroupName}' in SFXMixer.");
        }
    }

    public void PlayBGM(BGMResource resource)
    {
        BGM bgm = BGMList.Find(x => x != null && x.AudioResource == resource);
        if (bgm == null)
        {
            Debug.LogWarning($"AudioSlave.PlayBGM: No BGM entry found for '{resource}'.");
            return;
        }

        if (bgm.AudioClip == null)
        {
            Debug.LogWarning($"AudioSlave.PlayBGM: BGM '{resource}' has no AudioClip assigned.");
            return;
        }

        // If same track already playing, do nothing (but still apply volume if you want)
        if (_bgmSource.isPlaying && _bgmSource.clip == bgm.AudioClip)
            return;

        _bgmSource.clip = bgm.AudioClip; 
        _bgmSource.volume = Mathf.Clamp01(bgm.BGMVolume);
        _bgmSource.Play();
    }


    public void PlaySFX(SFXResource resource)
    {
        if (_sfxSource == null)
        {
            Debug.LogError("AudioSlave.PlaySFX: _sfxSource is null (Awake may not have run?).");
            return;
        }

        SFX sfx = SFXList.Find(x => x != null && x.AudioResource == resource);
        if (sfx == null)
        {
            Debug.LogWarning($"AudioSlave.PlaySFX: No SFX entry found for '{resource}'.");
            return;
        }

        if (sfx.AudioClip == null)
        {
            Debug.LogWarning($"AudioSlave.PlaySFX: SFX '{resource}' has no AudioClip assigned.");
            return;
        }

        // Sanity (prevents silent "why can't I hear it?")
        _sfxSource.mute = false;
        _sfxSource.bypassEffects = false;
        _sfxSource.bypassListenerEffects = false;
        _sfxSource.bypassReverbZones = false;

        float vol = Mathf.Clamp01(sfx.SFXVolume);
        if (vol <= 0f) Debug.LogWarning($"AudioSlave.PlaySFX: SFX '{resource}' volume is 0. It will be silent.");

        // Use OneShot for SFX (more reliable than swapping clip + Play)
        _sfxSource.PlayOneShot(sfx.AudioClip, vol);

        // If you still hear nothing, this helps pinpoint mixer/group issues
        if (_sfxSource.outputAudioMixerGroup == null && SFXMixer != null)
        {
            Debug.LogWarning($"AudioSlave.PlaySFX: outputAudioMixerGroup is NULL. " + $"Check SFXGroupName '{SFXGroupName}' exists in SFXMixer and is assigned in Awake.");
        }
    }


    
    public void StopBGM()
    {
        if (_bgmSource != null) _bgmSource.Stop();
    }

    public void StopSFX()
    {
        if (_sfxSource != null) _sfxSource.Stop();
    }

}


#if UNITY_EDITOR


[CustomEditor(typeof(AudioSlave))]
public class AudioSlaveEditor : Editor
{
    private BGMResource _selectedBGM = BGMResource.SongOne;
    private SFXResource _selectedSFX = SFXResource.TypeWriter0; 

    private bool _showBgm = true;
    private bool _showSfx = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Audio Debug Controls", EditorStyles.boldLabel);

        var audioSlave = (AudioSlave)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to hear audio. These buttons call your AudioSlave methods, " +
                "but AudioSources won't audition reliably outside Play Mode.",
                MessageType.Info);
        }

        // -------------------------
        // BGM SECTION
        // -------------------------
        EditorGUILayout.Space(8);
        _showBgm = EditorGUILayout.BeginFoldoutHeaderGroup(_showBgm, "BGM");
        if (_showBgm)
        {
            EditorGUI.indentLevel++;

            _selectedBGM = (BGMResource)EditorGUILayout.EnumPopup("Track", _selectedBGM);

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Play BGM"))
                {
                    audioSlave.PlayBGM(_selectedBGM);
                }

                if (GUILayout.Button("Stop BGM"))
                {
                    audioSlave.StopBGM();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // -------------------------
        // SFX SECTION
        // -------------------------
        EditorGUILayout.Space(8);
        _showSfx = EditorGUILayout.BeginFoldoutHeaderGroup(_showSfx, "SFX");
        if (_showSfx)
        {
            EditorGUI.indentLevel++;

            _selectedSFX = (SFXResource)EditorGUILayout.EnumPopup("SFX", _selectedSFX);

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Play SFX"))
                {
                    audioSlave.PlaySFX(_selectedSFX);
                }

                if (GUILayout.Button("Stop SFX"))
                {
                    audioSlave.StopSFX();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(8);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
