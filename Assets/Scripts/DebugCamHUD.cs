using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugCamHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DebugCamera orbitCam;

    [Header("UI")]
    [SerializeField] private TMP_Text currentTargetText;

    [Tooltip("Optional: shows NPC FSM state (Patrolling/Approaching/etc).")]
    [SerializeField] private TMP_Text currentStateText;

    [Tooltip("Parent object that contains the controls. Hidden when no valid NPCController target.")]
    [SerializeField] private GameObject controlsRoot;

    // ---------------------------------------------------------
    // Trigger UI
    // ---------------------------------------------------------
    [Header("Trigger UI")]
    [SerializeField] private TMP_Dropdown triggerDropdown;
    [SerializeField] private Button playTriggerButton;

    // ---------------------------------------------------------
    // FSM UI
    // Mirrors NPCController Inspector panel:
    // - Force Patrol
    // - Clear Target
    // ---------------------------------------------------------
    [Header("FSM UI")]
    [SerializeField] private Button forcePatrolButton;
    [SerializeField] private Button clearTargetButton;

    // ---------------------------------------------------------
    // Target Testing (NPC Enum) UI
    // Mirrors NPCController Inspector panel:
    // - Pick NPC enum
    // - Approach / Attack / Talk
    // ---------------------------------------------------------
    [Header("Target Testing UI (NPC Enum)")]
    [SerializeField] private TMP_Dropdown targetNpcDropdown;
    [SerializeField] private Button approachButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button talkButton;

    // runtime
    private Transform _lastTarget;
    private NPCController _npc;

    private readonly List<string> _triggerOptions = new();

    // NPC enum dropdown cache
    private readonly List<NPC> _npcEnumOptions = new();
    private readonly List<string> _npcEnumLabels = new();
    private bool _npcEnumBuilt = false;

    void Awake()
    {
        if (controlsRoot != null) controlsRoot.SetActive(false);

        // Trigger
        if (playTriggerButton != null) playTriggerButton.onClick.AddListener(OnPlayTriggerClicked);

        // FSM
        if (forcePatrolButton != null) forcePatrolButton.onClick.AddListener(OnForcePatrolClicked);
        if (clearTargetButton != null) clearTargetButton.onClick.AddListener(OnClearTargetClicked);

        // Target testing
        if (approachButton != null) approachButton.onClick.AddListener(OnApproachClicked);
        if (attackButton != null) attackButton.onClick.AddListener(OnAttackClicked);
        if (talkButton != null) talkButton.onClick.AddListener(OnTalkClicked);

        if (targetNpcDropdown != null)
            targetNpcDropdown.onValueChanged.AddListener(OnTargetNpcDropdownChanged);

        BuildNpcEnumDropdownIfNeeded();
    }

    void OnDestroy()
    {
        if (playTriggerButton != null) playTriggerButton.onClick.RemoveListener(OnPlayTriggerClicked);

        if (forcePatrolButton != null) forcePatrolButton.onClick.RemoveListener(OnForcePatrolClicked);
        if (clearTargetButton != null) clearTargetButton.onClick.RemoveListener(OnClearTargetClicked);

        if (approachButton != null) approachButton.onClick.RemoveListener(OnApproachClicked);
        if (attackButton != null) attackButton.onClick.RemoveListener(OnAttackClicked);
        if (talkButton != null) talkButton.onClick.RemoveListener(OnTalkClicked);

        if (targetNpcDropdown != null)
            targetNpcDropdown.onValueChanged.RemoveListener(OnTargetNpcDropdownChanged);
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

        // Keep state text live (nice for debugging)
        if (_npc != null && currentStateText != null)
            currentStateText.text = $"State: {_npc.GetCurrentState()}";
    }

    private void RefreshForTarget(Transform target)
    {
        _npc = null;
        if (target != null) _npc = target.GetComponentInParent<NPCController>();

        // Text
        if (currentTargetText != null)
        {
            if (_npc == null)
                currentTargetText.text = "Current Target: None";
            else
                currentTargetText.text = $"Current Target: {_npc.thisNPC}";
        }

        if (currentStateText != null)
            currentStateText.text = (_npc == null) ? "State: -" : $"State: {_npc.GetCurrentState()}";

        bool hasNpc = (_npc != null);

        if (controlsRoot != null)
            controlsRoot.SetActive(hasNpc);

        if (!hasNpc)
        {
            SetAllControlsInteractable(false);
            return;
        }

        SetAllControlsInteractable(true);

        RefreshTriggerDropdown();
        RefreshTargetNpcDropdownSelectionFromNpc();
    }

    private void SetAllControlsInteractable(bool on)
    {
        if (playTriggerButton != null) playTriggerButton.interactable = on;

        if (forcePatrolButton != null) forcePatrolButton.interactable = on;
        if (clearTargetButton != null) clearTargetButton.interactable = on;

        if (approachButton != null) approachButton.interactable = on;
        if (attackButton != null) attackButton.interactable = on;
        if (talkButton != null) talkButton.interactable = on;

        if (triggerDropdown != null) triggerDropdown.interactable = on;
        if (targetNpcDropdown != null) targetNpcDropdown.interactable = on;
    }

    // =========================================================
    // Trigger UI
    // =========================================================

    private void RefreshTriggerDropdown()
    {
        _triggerOptions.Clear();

        if (_npc != null && _npc.triggerNames != null)
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

        int idx = (_npc != null) ? Mathf.Clamp(_npc.selectedTriggerIndex, 0, _triggerOptions.Count - 1) : 0;
        triggerDropdown.value = idx;
        triggerDropdown.RefreshShownValue();

        if (playTriggerButton != null) playTriggerButton.interactable = true;
    }

    private void OnPlayTriggerClicked()
    {
        if (_npc == null) return;
        if (_triggerOptions.Count == 0) return;
        if (triggerDropdown == null) return;

        int idx = Mathf.Clamp(triggerDropdown.value, 0, _triggerOptions.Count - 1);
        string trig = _triggerOptions[idx];

        _npc.selectedTriggerIndex = idx;
        _npc.PlayTrigger(trig);
    }

    // =========================================================
    // FSM UI (Force Patrol / Clear Target)
    // =========================================================

    private void OnForcePatrolClicked()
    {
        if (_npc == null) return;
        _npc.ForcePatrol();
        RefreshForTarget(_lastTarget);
    }

    private void OnClearTargetClicked()
    {
        if (_npc == null) return;
        _npc.ClearTarget();
        RefreshForTarget(_lastTarget);
    }

    // =========================================================
    // Target Testing UI (NPC Enum)
    // =========================================================

    private void BuildNpcEnumDropdownIfNeeded()
    {
        if (_npcEnumBuilt) return;
        _npcEnumBuilt = true;

        _npcEnumOptions.Clear();
        _npcEnumLabels.Clear();

        Array vals = Enum.GetValues(typeof(NPC));
        for (int i = 0; i < vals.Length; i++)
        {
            NPC v = (NPC)vals.GetValue(i);
            _npcEnumOptions.Add(v);
            _npcEnumLabels.Add(v.ToString());
        }

        if (targetNpcDropdown == null) return;

        targetNpcDropdown.ClearOptions();
        if (_npcEnumLabels.Count == 0)
        {
            targetNpcDropdown.AddOptions(new List<string> { "(No NPC enum values)" });
            targetNpcDropdown.interactable = false;
            return;
        }

        targetNpcDropdown.AddOptions(_npcEnumLabels);
        targetNpcDropdown.interactable = true;
        targetNpcDropdown.value = 0;
        targetNpcDropdown.RefreshShownValue();
    }

    private void RefreshTargetNpcDropdownSelectionFromNpc()
    {
        if (_npc == null) return;
        if (targetNpcDropdown == null) return;
        if (_npcEnumOptions.Count == 0) return;

        int idx = _npcEnumOptions.IndexOf(_npc.debugTargetNPC);
        if (idx < 0) idx = 0;

        targetNpcDropdown.SetValueWithoutNotify(idx);
        targetNpcDropdown.RefreshShownValue();
    }

    private void OnTargetNpcDropdownChanged(int idx)
    {
        if (_npc == null) return;
        if (_npcEnumOptions.Count == 0) return;

        idx = Mathf.Clamp(idx, 0, _npcEnumOptions.Count - 1);
        _npc.debugTargetNPC = _npcEnumOptions[idx];
    }

    private void OnApproachClicked()
    {
        if (_npc == null) return;
        SyncNpcDebugTargetFromDropdown();
        _npc.DebugApproachTargetNPC();
        RefreshForTarget(_lastTarget);
    }

    private void OnAttackClicked()
    {
        if (_npc == null) return;
        SyncNpcDebugTargetFromDropdown();
        _npc.DebugAttackTargetNPC();
        RefreshForTarget(_lastTarget);
    }

    private void OnTalkClicked()
    {
        if (_npc == null) return;
        SyncNpcDebugTargetFromDropdown();
        _npc.DebugTalkTargetNPC();
        RefreshForTarget(_lastTarget);
    }

    private void SyncNpcDebugTargetFromDropdown()
    {
        if (_npc == null) return;
        if (targetNpcDropdown == null) return;
        if (_npcEnumOptions.Count == 0) return;

        int idx = Mathf.Clamp(targetNpcDropdown.value, 0, _npcEnumOptions.Count - 1);
        _npc.debugTargetNPC = _npcEnumOptions[idx];
    }
}