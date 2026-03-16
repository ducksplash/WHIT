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
    [SerializeField] private Button respawnButton;
    [Header("Camera UI")]
    [SerializeField] private Button panHorizontalButton;
    [SerializeField] private Button panVerticalButton;
    [SerializeField] private Button stopPanButton;
    [SerializeField] private Toggle followToggle;
    [SerializeField] private Button resetZoomButton;
    [SerializeField] private Slider orbitDistanceSlider;
    [SerializeField] private Slider followDistanceSlider;
    [Header("Spawn UI")]
    [SerializeField] private TMP_Dropdown spawnNpcDropdown;
    [SerializeField] private Button deleteNpcButton;
    
    [Header("Trigger UI")]
    [SerializeField] private TMP_Dropdown triggerDropdown;
    [SerializeField] private Button playTriggerButton;

    
    [Header("FSM UI")]
    [SerializeField] private Button forcePatrolButton;
    [SerializeField] private Button clearTargetButton;

    [SerializeField] private NPCManager npcManager;
    
    
    [Header("Target Testing UI (NPC Enum)")]
    [SerializeField] private TMP_Dropdown targetNpcDropdown;
    [SerializeField] private Button approachButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button talkButton;
    [SerializeField] private Button toggleOneButton;
    [SerializeField] private Button toggleTwoButton;
    [SerializeField] private Button sitButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Button lieButton;
    [SerializeField] private Button wakeButton;
    [SerializeField] private Button xButton;

    // runtime
    private Transform _lastTarget;
    private NPCController _npc;

    private readonly List<string> _triggerOptions = new();

    // NPC enum dropdown cache
    private readonly List<NPC> _npcEnumOptions = new();
    private readonly List<string> _npcEnumLabels = new();
    private bool _npcEnumBuilt = false;
    
    private readonly List<NPC> _spawnNpcEnumOptions = new();
    private readonly List<string> _spawnNpcEnumLabels = new();
    private bool _spawnNpcEnumBuilt = false;
    
    private void Awake()
    {
        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();

        BuildNpcEnumDropdownIfNeeded();
        BuildSpawnNpcDropdownIfNeeded();
    }
    
    
    void OnEnable()
    {
        if (controlsRoot != null) controlsRoot.SetActive(false);
        if (respawnButton != null) respawnButton.onClick.AddListener(OnRespawnClicked);
        // Trigger
        if (playTriggerButton != null) playTriggerButton.onClick.AddListener(OnPlayTriggerClicked);

        if (spawnNpcDropdown != null) spawnNpcDropdown.onValueChanged.AddListener(OnSpawnNpcDropdownChanged);
        if (deleteNpcButton != null) deleteNpcButton.onClick.AddListener(OnDeleteNpcClicked);
        // FSM
        if (forcePatrolButton != null) forcePatrolButton.onClick.AddListener(OnForcePatrolClicked);
        if (clearTargetButton != null) clearTargetButton.onClick.AddListener(OnClearTargetClicked);

        // Target testing
        if (approachButton != null) approachButton.onClick.AddListener(OnApproachClicked);
        if (attackButton != null) attackButton.onClick.AddListener(OnAttackClicked);
        if (talkButton != null) talkButton.onClick.AddListener(OnTalkClicked);

        if (targetNpcDropdown != null) targetNpcDropdown.onValueChanged.AddListener(OnTargetNpcDropdownChanged);

        if (sitButton != null) sitButton.onClick.AddListener(OnSitClicked);
        if (standButton != null) standButton.onClick.AddListener(OnStandClicked);
        if (lieButton != null) lieButton.onClick.AddListener(OnLieClicked);
        if (wakeButton != null) wakeButton.onClick.AddListener(OnWakeClicked);
        if (xButton != null) xButton.onClick.AddListener(OnXClicked);

        if (toggleOneButton != null) toggleOneButton.onClick.AddListener(OnToggleOutfitOneClicked);
        if (toggleTwoButton != null) toggleTwoButton.onClick.AddListener(OnToggleOutfitTwoClicked);

        // Camera UI
        if (panHorizontalButton != null) panHorizontalButton.onClick.AddListener(OnPanHorizontalClicked);
        if (panVerticalButton != null) panVerticalButton.onClick.AddListener(OnPanVerticalClicked);
        if (stopPanButton != null) stopPanButton.onClick.AddListener(OnStopPanClicked);
        if (resetZoomButton != null) resetZoomButton.onClick.AddListener(OnResetZoomClicked);

        if (followToggle != null) followToggle.onValueChanged.AddListener(OnFollowToggleChanged);
        if (orbitDistanceSlider != null) orbitDistanceSlider.onValueChanged.AddListener(OnOrbitDistanceSliderChanged);
        if (followDistanceSlider != null) followDistanceSlider.onValueChanged.AddListener(OnFollowDistanceSliderChanged);
        
        RefreshSpawnUI();
    }

    void OnDisable()
    {
        OnClearTargetClicked();

        if (playTriggerButton != null) playTriggerButton.onClick.RemoveListener(OnPlayTriggerClicked);
        if (respawnButton != null) respawnButton.onClick.RemoveListener(OnRespawnClicked);
        if (sitButton != null) sitButton.onClick.RemoveListener(OnSitClicked);
        if (standButton != null) standButton.onClick.RemoveListener(OnStandClicked);
        if (lieButton != null) lieButton.onClick.RemoveListener(OnLieClicked);
        if (wakeButton != null) wakeButton.onClick.RemoveListener(OnWakeClicked);

        if (forcePatrolButton != null) forcePatrolButton.onClick.RemoveListener(OnForcePatrolClicked);
        if (clearTargetButton != null) clearTargetButton.onClick.RemoveListener(OnClearTargetClicked);

        if (approachButton != null) approachButton.onClick.RemoveListener(OnApproachClicked);
        if (attackButton != null) attackButton.onClick.RemoveListener(OnAttackClicked);
        if (talkButton != null) talkButton.onClick.RemoveListener(OnTalkClicked);
        if (xButton != null) xButton.onClick.RemoveListener(OnXClicked);

        if (toggleOneButton != null) toggleOneButton.onClick.RemoveListener(OnToggleOutfitOneClicked);
        if (toggleTwoButton != null) toggleTwoButton.onClick.RemoveListener(OnToggleOutfitTwoClicked);

        if (targetNpcDropdown != null) targetNpcDropdown.onValueChanged.RemoveListener(OnTargetNpcDropdownChanged);

        if (spawnNpcDropdown != null) spawnNpcDropdown.onValueChanged.RemoveListener(OnSpawnNpcDropdownChanged);
        if (deleteNpcButton != null) deleteNpcButton.onClick.RemoveListener(OnDeleteNpcClicked);
        
        if (panHorizontalButton != null) panHorizontalButton.onClick.RemoveListener(OnPanHorizontalClicked);
        if (panVerticalButton != null) panVerticalButton.onClick.RemoveListener(OnPanVerticalClicked);
        if (stopPanButton != null) stopPanButton.onClick.RemoveListener(OnStopPanClicked);
        if (resetZoomButton != null) resetZoomButton.onClick.RemoveListener(OnResetZoomClicked);

        if (followToggle != null) followToggle.onValueChanged.RemoveListener(OnFollowToggleChanged);
        if (orbitDistanceSlider != null) orbitDistanceSlider.onValueChanged.RemoveListener(OnOrbitDistanceSliderChanged);
        if (followDistanceSlider != null) followDistanceSlider.onValueChanged.RemoveListener(OnFollowDistanceSliderChanged);
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

        if (_npc != null && currentStateText != null)
            currentStateText.text = $"State: {_npc.GetCurrentState()}";
    }
    private void BuildSpawnNpcDropdownIfNeeded()
    {
        if (_spawnNpcEnumBuilt) return;
        _spawnNpcEnumBuilt = true;

        _spawnNpcEnumOptions.Clear();
        _spawnNpcEnumLabels.Clear();

        if (npcManager != null)
        {
            List<NPC> spawnable = npcManager.GetSpawnableNPCs();
            for (int i = 0; i < spawnable.Count; i++)
            {
                NPC v = spawnable[i];
                _spawnNpcEnumOptions.Add(v);
                _spawnNpcEnumLabels.Add(v.ToString());
            }
        }

        if (spawnNpcDropdown == null) return;

        spawnNpcDropdown.ClearOptions();

        if (_spawnNpcEnumLabels.Count == 0)
        {
            spawnNpcDropdown.AddOptions(new List<string> { "(No spawnable NPC prefabs)" });
            spawnNpcDropdown.interactable = false;
            return;
        }

        spawnNpcDropdown.AddOptions(_spawnNpcEnumLabels);
        spawnNpcDropdown.interactable = true;
        spawnNpcDropdown.value = 0;
        spawnNpcDropdown.RefreshShownValue();

        if (orbitCam != null && _spawnNpcEnumOptions.Count > 0)
            orbitCam.UI_SetSpawnNpc(_spawnNpcEnumOptions[0]);
    }
    
    private void OnSpawnNpcDropdownChanged(int idx)
    {
        if (orbitCam == null) return;
        if (_spawnNpcEnumOptions.Count == 0) return;

        idx = Mathf.Clamp(idx, 0, _spawnNpcEnumOptions.Count - 1);
        orbitCam.UI_SetSpawnNpc(_spawnNpcEnumOptions[idx]);
    }
    

    private void OnDeleteNpcClicked()
    {
        if (orbitCam == null) return;

        orbitCam.UI_DeleteSelectedNpc();

        _lastTarget = null;
        RefreshForTarget(null);
        RefreshSpawnUI();
    }
    
    private void RefreshSpawnUI()
    {
        if (orbitCam == null) return;
        
        if (deleteNpcButton != null)
            deleteNpcButton.interactable = (_npc != null);
    }
    
    private void RefreshForTarget(Transform target)
    {
        _npc = null;
        if (target != null) _npc = target.GetComponentInParent<NPCController>();

        if (currentTargetText != null)
        {
            if (_npc == null)
                currentTargetText.text = "Current Target: None";
            else
                currentTargetText.text = $"Current Target: {_npc.GetComponent<MeNPC>().ThisNPCName}";
        }

        if (currentStateText != null)
            currentStateText.text = (_npc == null) ? "State: -" : $"State: {_npc.GetCurrentState()}";

        bool hasNpc = (_npc != null);

        if (controlsRoot != null)
            controlsRoot.SetActive(hasNpc);

        if (!hasNpc)
        {
            SetAllControlsInteractable(false);
            RefreshCameraUI();
            return;
        }

        SetAllControlsInteractable(true);

        RefreshTriggerDropdown();
        RefreshTargetNpcDropdownSelectionFromNpc();
        RefreshSpawnUI();
    }

    private void SetAllControlsInteractable(bool on)
    {
        if (playTriggerButton != null) playTriggerButton.interactable = on;

        if (forcePatrolButton != null) forcePatrolButton.interactable = on;
        if (clearTargetButton != null) clearTargetButton.interactable = on;

        if (approachButton != null) approachButton.interactable = on;
        if (attackButton != null) attackButton.interactable = on;
        if (talkButton != null) talkButton.interactable = on;

        if (sitButton != null) sitButton.interactable = on;
        if (standButton != null) standButton.interactable = on;
        if (lieButton != null) lieButton.interactable = on;
        if (wakeButton != null) wakeButton.interactable = on;

        if (toggleOneButton != null) toggleOneButton.interactable = on;
        if (toggleTwoButton != null) toggleTwoButton.interactable = on;

        if (triggerDropdown != null) triggerDropdown.interactable = on;
        if (targetNpcDropdown != null) targetNpcDropdown.interactable = on;

        RefreshCameraUI();
    }

    private void OnPanHorizontalClicked()
    {
        if (orbitCam == null) return;
        orbitCam.UI_PlayPanHorizontal();
        RefreshCameraUI();
    }

    private void OnPanVerticalClicked()
    {
        if (orbitCam == null) return;
        orbitCam.UI_PlayPanVertical();
        RefreshCameraUI();
    }

    private void OnStopPanClicked()
    {
        if (orbitCam == null) return;
        orbitCam.UI_StopPan();
        RefreshCameraUI();
    }

    private void OnFollowToggleChanged(bool on)
    {
        if (orbitCam == null) return;
        orbitCam.UI_SetFollowEnabled(on);
        RefreshCameraUI();
    }

    private void OnResetZoomClicked()
    {
        if (orbitCam == null) return;
        orbitCam.UI_ResetZoom();
        RefreshCameraUI();
    }

    private void OnOrbitDistanceSliderChanged(float value)
    {
        if (orbitCam == null) return;
        orbitCam.UI_SetOrbitDistanceNormalized(value);
    }

    private void OnFollowDistanceSliderChanged(float value)
    {
        if (orbitCam == null) return;
        orbitCam.UI_SetFollowDistanceNormalized(value);
    }

    void OnXClicked()
    {
        if (orbitCam != null) orbitCam.ClearTargetAndEnterFlyMode();

        _lastTarget = null;
        RefreshForTarget(null);
    }

    private void RefreshCameraUI()
    {
        if (orbitCam == null) return;

        bool hasTarget = orbitCam.GetTarget() != null;

        if (followToggle != null)
            followToggle.SetIsOnWithoutNotify(hasTarget && orbitCam.FollowEnabled);

        if (orbitDistanceSlider != null)
            orbitDistanceSlider.SetValueWithoutNotify(orbitCam.UI_GetOrbitDistanceNormalized());

        if (followDistanceSlider != null)
            followDistanceSlider.SetValueWithoutNotify(orbitCam.UI_GetFollowDistanceNormalized());

        if (panHorizontalButton != null) panHorizontalButton.interactable = hasTarget;
        if (panVerticalButton != null) panVerticalButton.interactable = hasTarget;
        if (followToggle != null) followToggle.interactable = hasTarget;
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

    private void OnSitClicked()
    {
        if (_npc == null) return;
        _npc.RequestSitDown();
        RefreshForTarget(_lastTarget);
    }

    private void OnStandClicked()
    {
        if (_npc == null) return;
        _npc.RequestStandUp();
        RefreshForTarget(_lastTarget);
    }

    private void OnLieClicked()
    {
        if (_npc == null) return;
        _npc.RequestLieDown();
        RefreshForTarget(_lastTarget);
    }

    private void OnWakeClicked()
    {
        if (_npc == null) return;
        _npc.RequestWakeUp();
        RefreshForTarget(_lastTarget);
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
    private void OnRespawnClicked()
    {
        if (_npc == null) return;
        _npc.RespawnNPC();
        RefreshForTarget(_lastTarget);
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

    private void OnToggleOutfitOneClicked()
    {
        if (_npc == null) return;

        MeNPC me = _npc.gameObject.GetComponent<MeNPC>();
        if (me != null)
            me.ToggleFirstOutfit();

        RefreshForTarget(_lastTarget);
    }

    private void OnToggleOutfitTwoClicked()
    {
        if (_npc == null) return;

        MeNPC me = _npc.gameObject.GetComponent<MeNPC>();
        if (me != null)
            me.ToggleSecondOutfit();

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