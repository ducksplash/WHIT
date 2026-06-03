using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Me : MonoBehaviour
{
    [Header("Work")]
    public List<SkinnedMeshRenderer> OutfitForWork;
    public bool JiggleForWork;

    [Header("Casual")]
    public List<SkinnedMeshRenderer> OutfitForCasual;
    public bool JiggleCasually;

    [Header("Casual Two")]
    public List<SkinnedMeshRenderer> OutfitForCasualTwo;
    public bool JiggleCasuallyTwo;

    [Header("Casual Three")]
    public List<SkinnedMeshRenderer> OutfitForCasualThree;
    public bool JiggleCasuallyThree;

    [Header("Casual Four")]
    public List<SkinnedMeshRenderer> OutfitForCasualFour;
    public bool JiggleCasuallyFour;

    [Header("Casual Five")]
    public List<SkinnedMeshRenderer> OutfitForCasualFive;
    public bool JiggleCasuallyFive;

    [Header("First Date")]
    public List<SkinnedMeshRenderer> OutfitForFirstDate;
    public bool JiggleOnAFirstDate;

    [Header("Second Date")]
    public List<SkinnedMeshRenderer> OutfitForSecondDate;
    public bool JiggleOnASecondDate;

    [Header("Third Date")]
    public List<SkinnedMeshRenderer> OutfitForThirdDate;
    public bool JiggleOnAThirdDate;

    [Header("Night Out")]
    public List<SkinnedMeshRenderer> OutfitForNightOut;
    public bool JiggleOnANightOut;

    [Header("Cosy Evening")]
    public List<SkinnedMeshRenderer> OutfitForPyjamas;
    public bool JiggleInPyjamas;

    [Header("On Skid Row")]
    public List<SkinnedMeshRenderer> OutfitForHomelessness;
    public bool JiggleWhileHomeless;

    [Header("Wedding")]
    public List<SkinnedMeshRenderer> OutfitForWedding;
    public bool JiggleAtAWedding;

    [Header("Funeral")]
    public List<SkinnedMeshRenderer> OutfitForFuneral;
    public bool JiggleAtAFuneral;

    [Header("Jigglers")]
    public GameObject JiggleLeftBoob;
    public GameObject JiggleRightBoob;
    public GameObject JiggleLeftButtcheek;
    public GameObject JiggleRightButtcheek;

    [Header("Input")]
    public InputActionReference nextOutfit;
    public InputActionReference previousOutfit;

    [Header("Settings")]
    public bool UnressedOnLoad;
    public string ThisCharacterName;

    [Header("Only applies to NPCs")]
    public NPCController npcController;

    [Header("Current Outfit")]
    public OutfitType currentOutfit = OutfitType.Work;

    private void Start()
    {
        try
        {
            npcController = GetComponent<NPCController>();
            ThisCharacterName = npcController.thisNPC.ToString().Replace("_", " ");
        }
        catch
        {
            ThisCharacterName = "Nora";
        }

        if (!UnressedOnLoad)
            SwitchToOutfit(OutfitType.Work);

        if (npcController == null)
            SetupInput();
    }

    private void SetupInput()
    {
        if (nextOutfit != null) nextOutfit.action.performed += OnNextOutfit;
        if (previousOutfit != null) previousOutfit.action.performed += OnPreviousOutfit;
    }

    private void OnNextOutfit(InputAction.CallbackContext ctx) => NextOutfit();
    private void OnPreviousOutfit(InputAction.CallbackContext ctx) => PreviousOutfit();

    private void OnDisable()
    {
        if (npcController == null)
        {
            if (nextOutfit != null) nextOutfit.action.performed -= OnNextOutfit;
            if (previousOutfit != null) previousOutfit.action.performed -= OnPreviousOutfit;
        }
    }

    // ====================== CYCLICAL NEXT / PREV ======================
    public void NextOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) return;

        int currentIndex = (int)currentOutfit;
        int nextIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(OutfitType)).Length;
        if (nextIndex == 0) nextIndex = 1; // Skip None

        SwitchToOutfit((OutfitType)nextIndex);
    }

    public void PreviousOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) return;

        int currentIndex = (int)currentOutfit;
        int prevIndex = currentIndex - 1;
        if (prevIndex < 1) prevIndex = System.Enum.GetValues(typeof(OutfitType)).Length - 1;

        SwitchToOutfit((OutfitType)prevIndex);
    }

    // ====================== CENTRALIZED OUTFIT SWITCHER ======================
    private void SwitchToOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:         ToggleWorkOutfit(true);          break;
            case OutfitType.Casual:       ToggleCasualOutfit(true);        break;
            case OutfitType.Casual2:      ToggleCasualTwoOutfit(true);     break;
            case OutfitType.Casual3:      ToggleCasualThreeOutfit(true);   break;
            case OutfitType.Casual4:      ToggleCasualFourOutfit(true);    break;
            case OutfitType.Casual5:      ToggleCasualFiveOutfit(true);    break;
            case OutfitType.Date1:        ToggleFirstDateOutfit(true);     break;
            case OutfitType.Date2:        ToggleSecondDateOutfit(true);    break;
            case OutfitType.Date3:        ToggleThirdDateOutfit(true);     break;
            case OutfitType.NightOut:     ToggleNightOutOutfit(true);      break;
            case OutfitType.Pyjamas:      TogglePyjamasOutfit(true);       break;
            case OutfitType.Homelessness: ToggleHomelessnessOutfit(true);  break;
            case OutfitType.Wedding:      ToggleWeddingOutfit(true);       break;
            case OutfitType.Funeral:      ToggleFuneralOutfit(true);       break;
            default:                      DisableAllMainOutfits();         break;
        }
    }

    // ====================== TOGGLE METHODS ======================
    public void ToggleWorkOutfit(bool? forceOn = null) { JiggleToggle(JiggleForWork); ToggleMainOutfit(OutfitType.Work, forceOn); }
    public void ToggleCasualOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasually); ToggleMainOutfit(OutfitType.Casual, forceOn); }
    public void ToggleCasualTwoOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasuallyTwo); ToggleMainOutfit(OutfitType.Casual2, forceOn); }
    public void ToggleCasualThreeOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasuallyThree); ToggleMainOutfit(OutfitType.Casual3, forceOn); }
    public void ToggleCasualFourOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasuallyFour); ToggleMainOutfit(OutfitType.Casual4, forceOn); }
    public void ToggleCasualFiveOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasuallyFive); ToggleMainOutfit(OutfitType.Casual5, forceOn); }

    public void ToggleFirstDateOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnAFirstDate); ToggleMainOutfit(OutfitType.Date1, forceOn); }
    public void ToggleSecondDateOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnASecondDate); ToggleMainOutfit(OutfitType.Date2, forceOn); }
    public void ToggleThirdDateOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnAThirdDate); ToggleMainOutfit(OutfitType.Date3, forceOn); }
    public void ToggleNightOutOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnANightOut); ToggleMainOutfit(OutfitType.NightOut, forceOn); }
    public void TogglePyjamasOutfit(bool? forceOn = null) { JiggleToggle(JiggleInPyjamas); ToggleMainOutfit(OutfitType.Pyjamas, forceOn); }
    public void ToggleHomelessnessOutfit(bool? forceOn = null) { JiggleToggle(JiggleWhileHomeless); ToggleMainOutfit(OutfitType.Homelessness, forceOn); }
    public void ToggleWeddingOutfit(bool? forceOn = null) { JiggleToggle(JiggleAtAWedding); ToggleMainOutfit(OutfitType.Wedding, forceOn); }
    public void ToggleFuneralOutfit(bool? forceOn = null) { JiggleToggle(JiggleAtAFuneral); ToggleMainOutfit(OutfitType.Funeral, forceOn); }

    private void JiggleToggle(bool isJiggly = false)
    {
        if (JiggleLeftBoob != null) JiggleLeftBoob.SetActive(isJiggly);
        if (JiggleRightBoob != null) JiggleRightBoob.SetActive(isJiggly);
        if (JiggleLeftButtcheek != null) JiggleLeftButtcheek.SetActive(isJiggly);
        if (JiggleRightButtcheek != null) JiggleRightButtcheek.SetActive(isJiggly);
    }

    private void ToggleMainOutfit(OutfitType outfit, bool? forceOn = null)
    {
        if (forceOn.HasValue)
        {
            if (forceOn.Value)
                SetMainOutfit(outfit);
            else
                DisableAllMainOutfits();
        }
        else
        {
            if (currentOutfit == outfit)
                DisableAllMainOutfits();
            else
                SetMainOutfit(outfit);
        }
    }

    public void SetMainOutfit(OutfitType outfit)
    {
        DisableAllMainOutfits();
        currentOutfit = outfit;

        switch (outfit)
        {
            case OutfitType.Work:         SetListEnabled(OutfitForWork, true); break;
            case OutfitType.Casual:       SetListEnabled(OutfitForCasual, true); break;
            case OutfitType.Casual2:      SetListEnabled(OutfitForCasualTwo, true); break;
            case OutfitType.Casual3:      SetListEnabled(OutfitForCasualThree, true); break;
            case OutfitType.Casual4:      SetListEnabled(OutfitForCasualFour, true); break;
            case OutfitType.Casual5:      SetListEnabled(OutfitForCasualFive, true); break;
            case OutfitType.Date1:        SetListEnabled(OutfitForFirstDate, true); break;
            case OutfitType.Date2:        SetListEnabled(OutfitForSecondDate, true); break;
            case OutfitType.Date3:        SetListEnabled(OutfitForThirdDate, true); break;
            case OutfitType.NightOut:     SetListEnabled(OutfitForNightOut, true); break;
            case OutfitType.Pyjamas:      SetListEnabled(OutfitForPyjamas, true); break;
            case OutfitType.Homelessness: SetListEnabled(OutfitForHomelessness, true); break;
            case OutfitType.Wedding:      SetListEnabled(OutfitForWedding, true); break;
            case OutfitType.Funeral:      SetListEnabled(OutfitForFuneral, true); break;
        }
    }

    public void DisableAllMainOutfits()
    {
        SetListEnabled(OutfitForWork, false);
        SetListEnabled(OutfitForCasual, false);
        SetListEnabled(OutfitForCasualTwo, false);
        SetListEnabled(OutfitForCasualThree, false);
        SetListEnabled(OutfitForCasualFour, false);
        SetListEnabled(OutfitForCasualFive, false);
        SetListEnabled(OutfitForFirstDate, false);
        SetListEnabled(OutfitForSecondDate, false);
        SetListEnabled(OutfitForThirdDate, false);
        SetListEnabled(OutfitForNightOut, false);
        SetListEnabled(OutfitForPyjamas, false);
        SetListEnabled(OutfitForHomelessness, false);
        SetListEnabled(OutfitForWedding, false);
        SetListEnabled(OutfitForFuneral, false);

        currentOutfit = OutfitType.None;
    }

    private void SetListEnabled(List<SkinnedMeshRenderer> list, bool enabled)
    {
        if (list == null) return;
        foreach (var renderer in list)
        {
            if (renderer != null)
                renderer.enabled = enabled;
        }
    }
}

public enum OutfitType
{
    None,
    Work,
    Casual,
    Casual2,
    Casual3,
    Casual4,
    Casual5,
    Date1,
    Date2,
    Date3,
    NightOut,
    Pyjamas,
    Homelessness,
    Wedding,
    Funeral
}

// ====================== EDITOR ======================
#if UNITY_EDITOR
[CustomEditor(typeof(Me))]
public class MeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Outfit Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        Me me = (Me)target;

        if (GUILayout.Button("Work Stuff", GUILayout.Height(35))) { me.ToggleWorkOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casual One", GUILayout.Height(35))) { me.ToggleCasualOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casual Two", GUILayout.Height(35))) { me.ToggleCasualTwoOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casual Three", GUILayout.Height(35))) { me.ToggleCasualThreeOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casual Four", GUILayout.Height(35))) { me.ToggleCasualFourOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casual Five", GUILayout.Height(35))) { me.ToggleCasualFiveOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("First Date", GUILayout.Height(35))) { me.ToggleFirstDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Second Date", GUILayout.Height(35))) { me.ToggleSecondDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Third Date", GUILayout.Height(35))) { me.ToggleThirdDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Gladrags (Night Out)", GUILayout.Height(35))) { me.ToggleNightOutOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Pyjamas", GUILayout.Height(35))) { me.TogglePyjamasOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Skid Row", GUILayout.Height(35))) { me.ToggleHomelessnessOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Wedding", GUILayout.Height(35))) { me.ToggleWeddingOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Funeral", GUILayout.Height(35))) { me.ToggleFuneralOutfit(); EditorUtility.SetDirty(me); }
    }
}
#endif