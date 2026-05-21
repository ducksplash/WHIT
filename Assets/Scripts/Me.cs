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
            SetMainOutfit(OutfitType.Work);

        if (npcController == null)
        {
            SetupInput();
        }
    }

    private void SetupInput()
    {
        if (nextOutfit != null)
        {
            nextOutfit.action.performed += OnNextOutfit;
        }

        if (previousOutfit != null)
        {
            previousOutfit.action.performed += OnPreviousOutfit;
        }
    }

    private void OnNextOutfit(InputAction.CallbackContext ctx) => NextOutfit();
    private void OnPreviousOutfit(InputAction.CallbackContext ctx) => PreviousOutfit();

    private void OnDisable()
    {
        if (npcController == null)
        {
            if (nextOutfit != null)
            {
                nextOutfit.action.performed -= OnNextOutfit;
            }

            if (previousOutfit != null)
            {
                previousOutfit.action.performed -= OnPreviousOutfit;
            }
        }
    }

    // ====================== CYCLICAL NEXT / PREV ======================
    public void NextOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) return;
        
        int currentIndex = (int)currentOutfit;
        int nextIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(OutfitType)).Length;

        if (nextIndex == 0) nextIndex = 1; // Skip None

        SetMainOutfit((OutfitType)nextIndex);
    }

    public void PreviousOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) return;
        
        int currentIndex = (int)currentOutfit;
        int prevIndex = currentIndex - 1;

        if (prevIndex < 1)
            prevIndex = System.Enum.GetValues(typeof(OutfitType)).Length - 1;

        SetMainOutfit((OutfitType)prevIndex);
    }


    public void ToggleWorkOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleForWork);
        ToggleMainOutfit(OutfitType.Work, forceOn);
    }

    public void ToggleCasualOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleCasually);
        ToggleMainOutfit(OutfitType.Casual, forceOn);
    }

    public void ToggleFirstDateOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleOnAFirstDate);
        ToggleMainOutfit(OutfitType.Date1, forceOn);
    }

    public void ToggleSecondDateOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleOnASecondDate);
        ToggleMainOutfit(OutfitType.Date2, forceOn);
    }

    public void ToggleThirdDateOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleOnAThirdDate);
        ToggleMainOutfit(OutfitType.Date3, forceOn);
    }

    public void ToggleNightOutOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleOnANightOut);
        ToggleMainOutfit(OutfitType.NightOut, forceOn);
    }

    public void TogglePyjamasOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleInPyjamas);
        ToggleMainOutfit(OutfitType.Pyjamas, forceOn);
    }

    public void ToggleHomelessnessOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleWhileHomeless);
        ToggleMainOutfit(OutfitType.Homelessness, forceOn);
    }

    public void ToggleWeddingOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleAtAWedding);
        ToggleMainOutfit(OutfitType.Wedding, forceOn);
    }

    public void ToggleFuneralOutfit(bool? forceOn = null)
    {
        JiggleToggle(JiggleAtAFuneral);
        ToggleMainOutfit(OutfitType.Funeral, forceOn);
    }




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
            case OutfitType.Work:        SetListEnabled(OutfitForWork, true); break;
            case OutfitType.Casual:      SetListEnabled(OutfitForCasual, true); break;
            case OutfitType.Date1:       SetListEnabled(OutfitForFirstDate, true); break;
            case OutfitType.Date2:       SetListEnabled(OutfitForSecondDate, true); break;
            case OutfitType.Date3:       SetListEnabled(OutfitForThirdDate, true); break;
            case OutfitType.NightOut:    SetListEnabled(OutfitForNightOut, true); break;
            case OutfitType.Pyjamas:     SetListEnabled(OutfitForPyjamas, true); break;
            case OutfitType.Homelessness:SetListEnabled(OutfitForHomelessness, true); break;
            case OutfitType.Wedding:     SetListEnabled(OutfitForWedding, true); break;
            case OutfitType.Funeral:     SetListEnabled(OutfitForFuneral, true); break;
        }
    }

    public void DisableAllMainOutfits()
    {
        SetListEnabled(OutfitForWork, false);
        SetListEnabled(OutfitForCasual, false);
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

        if (GUILayout.Button("Work Stuff", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Work); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casuals", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Casual); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("First Date", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Date1); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Second Date", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Date2); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Third Date", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Date3); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Gladrags", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.NightOut); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Pyjamas", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Pyjamas); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Skid Row", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Homelessness); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Wedding", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Wedding); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Funeral", GUILayout.Height(35))) { me.SetMainOutfit(OutfitType.Funeral); EditorUtility.SetDirty(me); }
    }
}
#endif