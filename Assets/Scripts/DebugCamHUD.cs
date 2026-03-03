
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugCamHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OrbitCam orbitCam;

    [Header("UI")]
    [SerializeField] private TMP_Text currentTargetText;

    [Tooltip("Parent object that contains the trigger/routine controls. Hidden when no valid NPCController target.")]
    [SerializeField] private GameObject controlsRoot;

    [Header("Trigger UI")]
    [SerializeField] private TMP_Dropdown triggerDropdown;
    [SerializeField] private Button playTriggerButton;

    [Header("Routine UI")]
    [SerializeField] private TMP_Dropdown routineDropdown;
    [SerializeField] private Button playRoutineButton;

    // runtime
    private Transform _lastTarget;
    private NPCController _npc;

    private readonly List<string> _triggerOptions = new();
    private readonly List<Routine> _routineOptions = new();

    void Awake()
    {
        if (controlsRoot != null) controlsRoot.SetActive(false);

        if (playTriggerButton != null) playTriggerButton.onClick.AddListener(OnPlayTriggerClicked);
        if (playRoutineButton != null) playRoutineButton.onClick.AddListener(OnPlayRoutineClicked);
    }

    void OnDestroy()
    {
        if (playTriggerButton != null) playTriggerButton.onClick.RemoveListener(OnPlayTriggerClicked);
        if (playRoutineButton != null) playRoutineButton.onClick.RemoveListener(OnPlayRoutineClicked);
    }

    void Update()
    {
        if (orbitCam == null) return;

        Transform t = orbitCam.GetTarget();

        if (t != _lastTarget)
        {
            _lastTarget = t;
            RefreshForTarget(t);
        }
    }

    private void RefreshForTarget(Transform target)
    {
        // Text
        if (currentTargetText != null)
        {
            currentTargetText.text = target == null
                ? "Current Target: None"
                : $"Current Target: {target.name}";
        }

        // Find NPCController if any
        _npc = null;
        if (target != null)
            _npc = target.GetComponentInParent<NPCController>();

        bool hasNpc = (_npc != null);

        if (controlsRoot != null)
            controlsRoot.SetActive(hasNpc);

        if (!hasNpc)
            return;

        RefreshTriggerDropdown();
        RefreshRoutineDropdown();
    }

    private void RefreshTriggerDropdown()
    {
        _triggerOptions.Clear();

        if (_npc.triggerNames != null)
        {
            for (int i = 0; i < _npc.triggerNames.Count; i++)
            {
                string trig = _npc.triggerNames[i];
                if (!string.IsNullOrWhiteSpace(trig))
                    _triggerOptions.Add(trig);
            }
        }

        if (triggerDropdown == null) return;

        triggerDropdown.ClearOptions();

        if (_triggerOptions.Count == 0)
        {
            triggerDropdown.AddOptions(new List<string> { "(No triggers)" });
            triggerDropdown.interactable = false;
            if (playTriggerButton != null) playTriggerButton.interactable = false;
            return;
        }

        triggerDropdown.AddOptions(_triggerOptions);
        triggerDropdown.interactable = true;
        triggerDropdown.value = Mathf.Clamp(_npc.selectedTriggerIndex, 0, _triggerOptions.Count - 1);
        triggerDropdown.RefreshShownValue();

        if (playTriggerButton != null) playTriggerButton.interactable = true;
    }

    private void RefreshRoutineDropdown()
    {
        _routineOptions.Clear();
        List<string> labels = new List<string>();

        if (_npc.routines != null)
        {
            for (int i = 0; i < _npc.routines.Count; i++)
            {
                var routineAsset = _npc.routines[i];
                if (routineAsset == null) continue;

                Routine r = routineAsset.RoutineType;

                // avoid duplicates if the list contains repeated assets/types
                if (_routineOptions.Contains(r)) continue;

                _routineOptions.Add(r);
                labels.Add(r.ToString());
            }
        }

        if (routineDropdown == null) return;

        routineDropdown.ClearOptions();

        if (_routineOptions.Count == 0)
        {
            routineDropdown.AddOptions(new List<string> { "(No routines)" });
            routineDropdown.interactable = false;
            if (playRoutineButton != null) playRoutineButton.interactable = false;
            return;
        }

        routineDropdown.AddOptions(labels);
        routineDropdown.interactable = true;

        // Try to select NPC's current selectedRoutine if it exists in list
        int idx = _routineOptions.IndexOf(_npc.selectedRoutine);
        routineDropdown.value = (idx >= 0) ? idx : 0;
        routineDropdown.RefreshShownValue();

        if (playRoutineButton != null) playRoutineButton.interactable = true;
    }

    private void OnPlayTriggerClicked()
    {
        if (_npc == null) return;
        if (_triggerOptions.Count == 0) return;
        if (triggerDropdown == null) return;

        int idx = Mathf.Clamp(triggerDropdown.value, 0, _triggerOptions.Count - 1);
        string trig = _triggerOptions[idx];

        // keep NPC’s inspector index in sync (nice for debugging)
        _npc.selectedTriggerIndex = idx;

        _npc.PlayTrigger(trig);
    }

    private void OnPlayRoutineClicked()
    {
        if (_npc == null) return;
        if (_routineOptions.Count == 0) return;
        if (routineDropdown == null) return;

        int idx = Mathf.Clamp(routineDropdown.value, 0, _routineOptions.Count - 1);
        Routine routine = _routineOptions[idx];

        // keep NPC’s selectedRoutine in sync
        _npc.selectedRoutine = routine;

        _npc.PlayRoutine(routine);
    }
}