using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Me : MonoBehaviour
{
    [Header("Body")]
    public SkinnedMeshRenderer Body;

    
    [Header("\nWork\n")]
    // WORK OUTFITS
    [Header("Work")]
    public List<SkinnedMeshRenderer> OutfitForWork;
    public Color lipsColorForWork = new Color(0.9f, 0.2f, 0.3f);
    public Color nailsColorForWork = new Color(0.95f, 0.8f, 0.85f);
    public bool JiggleForWork;

    [Header("Work Two")]
    public List<SkinnedMeshRenderer> OutfitForWorkTwo;
    public Color lipsColorForWorkTwo = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkTwo = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkTwo;
    
    
    
    
    [Header("\nCasual\n")]
    // Casual Outfits
    [Header("Casual One")]
    public List<SkinnedMeshRenderer> OutfitForCasual;
    public Color lipsColorForCasual = new Color(0.85f, 0.25f, 0.35f);
    public Color nailsColorForCasual = new Color(0.92f, 0.75f, 0.8f);
    public bool JiggleCasually;
    

    [Header("Fitness")]
    public List<SkinnedMeshRenderer> OutfitForFitness;
    public Color lipsColorForFitness = new Color(0.92f, 0.28f, 0.38f);
    public Color nailsColorForFitness = new Color(0.9f, 0.75f, 0.8f);
    public bool JiggleFitness;
    
    
    [Header("First Date")]
    public List<SkinnedMeshRenderer> OutfitForFirstDate;
    public Color lipsColorForFirstDate = new Color(0.96f, 0.4f, 0.5f);
    public Color nailsColorForFirstDate = new Color(0.97f, 0.88f, 0.9f);
    public bool JiggleOnAFirstDate;

    
    [Header("Pyjamas")]
    public List<SkinnedMeshRenderer> OutfitForPyjamas;
    public Color lipsColorForPyjamas = new Color(0.8f, 0.15f, 0.25f);
    public Color nailsColorForPyjamas = new Color(0.85f, 0.6f, 0.7f);
    public bool JiggleInPyjamas;
    
  
    [Header("\nCute\n")]
    // Cute Outfits
    [Header("Cute One")]
    public List<SkinnedMeshRenderer> OutfitForCuteOne;
    public Color lipsColorForCuteOne = new Color(0.98f, 0.45f, 0.55f);
    public Color nailsColorForCuteOne = new Color(0.98f, 0.9f, 0.92f);
    public bool JiggleOnACuteOne;

    
    [Header("Cute Two")]
    public List<SkinnedMeshRenderer> OutfitForCuteTwo;
    public Color lipsColorForCuteTwo = new Color(0.8f, 0.15f, 0.25f);
    public Color nailsColorForCuteTwo = new Color(0.88f, 0.65f, 0.75f);
    public bool JiggleCasuallyTwo;

    
    [Header("Cute Three")]
    public List<SkinnedMeshRenderer> OutfitForCuteThree;
    public Color lipsColorForCuteThree = new Color(0.95f, 0.35f, 0.45f);
    public Color nailsColorForCuteThree = new Color(0.96f, 0.85f, 0.88f);
    public bool JiggleCuteThree;
    
    

    [Header("Cute Four")]
    public List<SkinnedMeshRenderer> OutfitForCuteFour;
    public Color lipsColorForCuteFour = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForCuteFour = new Color(0.94f, 0.8f, 0.85f);
    public bool JiggleCuteFour;
    
    
    [Header("Cute Five")]
    public List<SkinnedMeshRenderer> OutfitForCuteFive;
    public Color lipsColorForCuteFive = new Color(0.88f, 0.2f, 0.3f);
    public Color nailsColorForCuteFive = new Color(0.93f, 0.78f, 0.83f);
    public bool JiggleCuteFive;


    
    [Header("Cute Six")]
    public List<SkinnedMeshRenderer> OutfitForCuteSix;
    public Color lipsColorForCuteSix = new Color(0.9f, 0.3f, 0.4f);
    public Color nailsColorForCuteSix = new Color(0.95f, 0.82f, 0.87f);
    public bool JiggleOnACuteSix;

    
    
    [Header("Cute Seven")]
    public List<SkinnedMeshRenderer> OutfitForCuteSeven;
    public Color lipsColorForCuteSeven = new Color(0.88f, 0.22f, 0.32f);
    public Color nailsColorForCuteSeven = new Color(0.95f, 0.82f, 0.85f);
    public bool JiggleCuteSeven;

    [Header("Cute Eight")]
    public List<SkinnedMeshRenderer> OutfitForCuteEight;
    public Color lipsColorForCuteEight = new Color(0.85f, 0.18f, 0.28f);
    public Color nailsColorForCuteEight = new Color(0.88f, 0.68f, 0.73f);
    public bool JiggleCuteEight;


    
    [Header("Edea")]
    public List<SkinnedMeshRenderer> OutfitForEdea;
    public Color lipsColorForEdea = new Color(0.75f, 0.1f, 0.2f);
    public Color nailsColorForEdea = new Color(0.9f, 0.7f, 0.75f);
    public bool JiggleCasuallyFour;

    
    [Header("\nStoryline\n")]
    
    [Header("Wedding")]
    public List<SkinnedMeshRenderer> OutfitForWedding;
    public Color lipsColorForWedding = new Color(0.95f, 0.55f, 0.6f);
    public Color nailsColorForWedding = new Color(0.96f, 0.88f, 0.9f);
    public bool JiggleAtAWedding;

    [Header("Funeral")]
    public List<SkinnedMeshRenderer> OutfitForFuneral;
    public Color lipsColorForFuneral = new Color(0.65f, 0.08f, 0.15f);
    public Color nailsColorForFuneral = new Color(0.7f, 0.5f, 0.55f);
    public bool JiggleAtAFuneral;
    
    [Header("On Skid Row")]
    public List<SkinnedMeshRenderer> OutfitForHomelessness;
    public Color lipsColorForHomelessness = new Color(0.7f, 0.1f, 0.2f);
    public Color nailsColorForHomelessness = new Color(0.75f, 0.55f, 0.6f);
    public bool JiggleWhileHomeless;


    
    [Header("\nNight Out\n")]
    // Night Out Outfits
    [Header("Third Date")]
    public List<SkinnedMeshRenderer> OutfitForThirdDate;
    public Color lipsColorForThirdDate = new Color(0.85f, 0.22f, 0.32f);
    public Color nailsColorForThirdDate = new Color(0.94f, 0.8f, 0.85f);
    public bool JiggleOnAThirdDate;
    
    [Header("Casual Three")]
    public List<SkinnedMeshRenderer> OutfitForCasualThree;
    public Color lipsColorForCasualThree = new Color(0.9f, 0.3f, 0.4f);
    public Color nailsColorForCasualThree = new Color(0.93f, 0.78f, 0.82f);
    public bool JiggleCasuallyThree;
    
    [Header("Essie")]
    public List<SkinnedMeshRenderer> OutfitForEssie;
    public Color lipsColorForEssie = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForEssie = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleEssie;


    
    
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
        if (nextIndex == 0) nextIndex = 1;

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

    private void SwitchToOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:         ToggleWorkOutfit(true);          break;
            case OutfitType.WorkTwo:      ToggleWorkTwoOutfit(true);       break;
            case OutfitType.Casual:       ToggleCasualOutfit(true);        break;
            case OutfitType.CuteTwo:      ToggleCuteTwoOutfit(true);     break;
            case OutfitType.Casual3:      ToggleCasualThreeOutfit(true);   break;
            case OutfitType.Edea:      ToggleEdeaOutfit(true);    break;
            case OutfitType.CuteSeven:       ToggleCuteSevenOutfit(true);        break;
            case OutfitType.Fitness:       ToggleFitnessOutfit(true);        break;
            case OutfitType.CuteEight:       ToggleCuteEightOutfit(true);        break;
            case OutfitType.CuteFour:      ToggleCuteFourOutfit(true);       break;
            case OutfitType.Essie:        ToggleEssieOutfit(true);         break;
            case OutfitType.CuteThree:      ToggleCuteThreeOutfit(true);       break;
            case OutfitType.CuteFive:       ToggleCuteFiveOutfit(true);        break;
            case OutfitType.CuteSix:        ToggleFirstDateOutfit(true);     break;
            case OutfitType.Date2:        ToggleCuteSixOutfit(true);    break;
            case OutfitType.Date3:        ToggleThirdDateOutfit(true);     break;
            case OutfitType.CuteOne:     ToggleCuteOneOutfit(true);      break;
            case OutfitType.Pyjamas:      TogglePyjamasOutfit(true);       break;
            case OutfitType.Homelessness: ToggleHomelessnessOutfit(true);  break;
            case OutfitType.Wedding:      ToggleWeddingOutfit(true);       break;
            case OutfitType.Funeral:      ToggleFuneralOutfit(true);       break;
            default:                      DisableAllMainOutfits();         break;
        }
    }

    // ====================== TOGGLE METHODS ======================
    public void ToggleWorkOutfit(bool? forceOn = null) { JiggleToggle(JiggleForWork); ToggleMainOutfit(OutfitType.Work, forceOn); }
    public void ToggleWorkTwoOutfit(bool? forceOn = null) { JiggleToggle(JiggleForWorkTwo); ToggleMainOutfit(OutfitType.WorkTwo, forceOn); }
    public void ToggleCasualOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasually); ToggleMainOutfit(OutfitType.Casual, forceOn); }
    public void ToggleCuteTwoOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasuallyTwo); ToggleMainOutfit(OutfitType.CuteTwo, forceOn); }
    public void ToggleCasualThreeOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasuallyThree); ToggleMainOutfit(OutfitType.Casual3, forceOn); }
    public void ToggleEdeaOutfit(bool? forceOn = null) { JiggleToggle(JiggleCasuallyFour); ToggleMainOutfit(OutfitType.Edea, forceOn); }
    public void ToggleCuteSevenOutfit(bool? forceOn = null) { JiggleToggle(JiggleCuteSeven); ToggleMainOutfit(OutfitType.CuteSeven, forceOn); }
    public void ToggleFitnessOutfit(bool? forceOn = null) { JiggleToggle(JiggleFitness); ToggleMainOutfit(OutfitType.Fitness, forceOn); }
    public void ToggleCuteEightOutfit(bool? forceOn = null) { JiggleToggle(JiggleCuteEight); ToggleMainOutfit(OutfitType.CuteEight, forceOn); }
    public void ToggleCuteFourOutfit(bool? forceOn = null) { JiggleToggle(JiggleCuteFour); ToggleMainOutfit(OutfitType.CuteFour, forceOn); }
    public void ToggleEssieOutfit(bool? forceOn = null) { JiggleToggle(JiggleEssie); ToggleMainOutfit(OutfitType.Essie, forceOn); }
    public void ToggleCuteThreeOutfit(bool? forceOn = null) { JiggleToggle(JiggleCuteThree); ToggleMainOutfit(OutfitType.CuteThree, forceOn); }
    public void ToggleCuteFiveOutfit(bool? forceOn = null) { JiggleToggle(JiggleCuteFive); ToggleMainOutfit(OutfitType.CuteFive, forceOn); }

    public void ToggleFirstDateOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnAFirstDate); ToggleMainOutfit(OutfitType.CuteSix, forceOn); }
    public void ToggleCuteSixOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnACuteSix); ToggleMainOutfit(OutfitType.Date2, forceOn); }
    public void ToggleThirdDateOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnAThirdDate); ToggleMainOutfit(OutfitType.Date3, forceOn); }
    public void ToggleCuteOneOutfit(bool? forceOn = null) { JiggleToggle(JiggleOnACuteOne); ToggleMainOutfit(OutfitType.CuteOne, forceOn); }
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

        // Enable clothing
        switch (outfit)
        {
            case OutfitType.Work:         SetListEnabled(OutfitForWork, true); break;
            case OutfitType.WorkTwo:      SetListEnabled(OutfitForWorkTwo, true); break;
            case OutfitType.Casual:       SetListEnabled(OutfitForCasual, true); break;
            case OutfitType.CuteTwo:      SetListEnabled(OutfitForCuteTwo, true); break;
            case OutfitType.Casual3:      SetListEnabled(OutfitForCasualThree, true); break;
            case OutfitType.Edea:      SetListEnabled(OutfitForEdea, true); break;
            case OutfitType.CuteSeven:       SetListEnabled(OutfitForCuteSeven, true); break;
            case OutfitType.Fitness:       SetListEnabled(OutfitForFitness, true); break;
            case OutfitType.CuteEight:       SetListEnabled(OutfitForCuteEight, true); break;
            case OutfitType.CuteFour:      SetListEnabled(OutfitForCuteFour, true); break;
            case OutfitType.Essie:        SetListEnabled(OutfitForEssie, true); break;
            case OutfitType.CuteThree:      SetListEnabled(OutfitForCuteThree, true); break;
            case OutfitType.CuteFive:       SetListEnabled(OutfitForCuteFive, true); break;
            case OutfitType.CuteSix:        SetListEnabled(OutfitForFirstDate, true); break;
            case OutfitType.Date2:        SetListEnabled(OutfitForCuteSix, true); break;
            case OutfitType.Date3:        SetListEnabled(OutfitForThirdDate, true); break;
            case OutfitType.CuteOne:     SetListEnabled(OutfitForCuteOne, true); break;
            case OutfitType.Pyjamas:      SetListEnabled(OutfitForPyjamas, true); break;
            case OutfitType.Homelessness: SetListEnabled(OutfitForHomelessness, true); break;
            case OutfitType.Wedding:      SetListEnabled(OutfitForWedding, true); break;
            case OutfitType.Funeral:      SetListEnabled(OutfitForFuneral, true); break;
        }

        ApplyBodyColors(outfit);
    }

    
        // ====================== RANDOM OUTFIT SELECTION ======================

    /// <summary>
    /// Picks a completely random outfit from all available outfits.
    /// </summary>
    public void SetRandomOutfit()
    {
        OutfitType randomType = (OutfitType)Random.Range(1, System.Enum.GetValues(typeof(OutfitType)).Length);
        SwitchToOutfit(randomType);
    }

    /// <summary>
    /// Random outfit from the Work group
    /// </summary>
    public void SetRandomWorkOutfit()
    {
        OutfitType[] workOutfits = { OutfitType.Work, OutfitType.WorkTwo };
        SwitchToOutfit(workOutfits[Random.Range(0, workOutfits.Length)]);
    }

    /// <summary>
    /// Random outfit from the Casual group
    /// </summary>
    public void SetRandomCasualOutfit()
    {
        OutfitType[] casualOutfits = 
        {
            OutfitType.Casual,
            OutfitType.Fitness,
            OutfitType.CuteSix,     
            OutfitType.Pyjamas
        };
        SwitchToOutfit(casualOutfits[Random.Range(0, casualOutfits.Length)]);
    }

    /// <summary>
    /// Random outfit from the Cute group
    /// </summary>
    public void SetRandomCuteOutfit()
    {
        OutfitType[] cuteOutfits =
        {
            OutfitType.CuteOne,
            OutfitType.CuteTwo,
            OutfitType.CuteThree,
            OutfitType.CuteFour,
            OutfitType.CuteFive,
            OutfitType.Date2,       
            OutfitType.CuteSeven,
            OutfitType.CuteEight,
            OutfitType.Edea
        };
        SwitchToOutfit(cuteOutfits[Random.Range(0, cuteOutfits.Length)]);
    }
    /// <summary>
    /// Random outfit from the Cute group
    /// </summary>
    public void SetRandomStorylineOutfit()
    {
        OutfitType[] cuteOutfits =
        {
            OutfitType.Wedding,
            OutfitType.Funeral,
            OutfitType.Homelessness
        };
        SwitchToOutfit(cuteOutfits[Random.Range(0, cuteOutfits.Length)]);
    }

    /// <summary>
    /// Random outfit from the Night Out group
    /// </summary>
    public void SetRandomNightOutOutfit()
    {
        OutfitType[] nightOutOutfits =
        {
            OutfitType.Date3,       // Third Date
            OutfitType.Casual3,
            OutfitType.Essie
        };
        SwitchToOutfit(nightOutOutfits[Random.Range(0, nightOutOutfits.Length)]);
    }
    
    
    
    
    private void ApplyBodyColors(OutfitType outfit)
    {
        if (Body == null) return;

        Color lipColor = GetLipColorForOutfit(outfit);
        Color nailColor = GetNailColorForOutfit(outfit);

        Material[] materials = Application.isPlaying ? Body.materials : Body.sharedMaterials;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null) continue;

            string matName = materials[i].name.ToLower();

            if (matName.Contains("lips"))
            {
                materials[i].color = lipColor;
            }
            else if (matName.Contains("fingernail") || matName.Contains("nail"))
            {
                materials[i].color = nailColor;
            }
        }

        if (Application.isPlaying)
        {
            Body.materials = materials;
        }
    }

    private Color GetLipColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:         return lipsColorForWork;
            case OutfitType.WorkTwo:      return lipsColorForWorkTwo;
            case OutfitType.Casual:       return lipsColorForCasual;
            case OutfitType.CuteTwo:      return lipsColorForCuteTwo;
            case OutfitType.Casual3:      return lipsColorForCasualThree;
            case OutfitType.Edea:      return lipsColorForEdea;
            case OutfitType.CuteSeven:       return lipsColorForCuteSeven;
            case OutfitType.Fitness:       return lipsColorForFitness;
            case OutfitType.CuteEight:       return lipsColorForCuteEight;
            case OutfitType.CuteFour:      return lipsColorForCuteFour;
            case OutfitType.Essie:        return lipsColorForEssie;
            case OutfitType.CuteThree:      return lipsColorForCuteThree;
            case OutfitType.CuteFive:       return lipsColorForCuteFive;
            case OutfitType.CuteSix:        return lipsColorForFirstDate;
            case OutfitType.Date2:        return lipsColorForCuteSix;
            case OutfitType.Date3:        return lipsColorForThirdDate;
            case OutfitType.CuteOne:     return lipsColorForCuteOne;
            case OutfitType.Pyjamas:      return lipsColorForPyjamas;
            case OutfitType.Homelessness: return lipsColorForHomelessness;
            case OutfitType.Wedding:      return lipsColorForWedding;
            case OutfitType.Funeral:      return lipsColorForFuneral;
            default:                      return Color.white;
        }
    }

    private Color GetNailColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:         return nailsColorForWork;
            case OutfitType.WorkTwo:      return nailsColorForWorkTwo;
            case OutfitType.Casual:       return nailsColorForCasual;
            case OutfitType.CuteTwo:      return nailsColorForCuteTwo;
            case OutfitType.Casual3:      return nailsColorForCasualThree;
            case OutfitType.Edea:      return nailsColorForEdea;
            case OutfitType.CuteSeven:       return nailsColorForCuteSeven;
            case OutfitType.Fitness:       return nailsColorForFitness;
            case OutfitType.CuteEight:       return nailsColorForCuteEight;
            case OutfitType.CuteFour:      return nailsColorForCuteFour;
            case OutfitType.Essie:        return nailsColorForEssie;
            case OutfitType.CuteThree:      return nailsColorForCuteThree;
            case OutfitType.CuteFive:       return nailsColorForCuteFive;
            case OutfitType.CuteSix:        return nailsColorForFirstDate;
            case OutfitType.Date2:        return nailsColorForCuteSix;
            case OutfitType.Date3:        return nailsColorForThirdDate;
            case OutfitType.CuteOne:     return nailsColorForCuteOne;
            case OutfitType.Pyjamas:      return nailsColorForPyjamas;
            case OutfitType.Homelessness: return nailsColorForHomelessness;
            case OutfitType.Wedding:      return nailsColorForWedding;
            case OutfitType.Funeral:      return nailsColorForFuneral;
            default:                      return Color.white;
        }
    }

    public void DisableAllMainOutfits()
    {
        SetListEnabled(OutfitForWork, false);
        SetListEnabled(OutfitForWorkTwo, false);
        SetListEnabled(OutfitForCasual, false);
        SetListEnabled(OutfitForCuteTwo, false);
        SetListEnabled(OutfitForCasualThree, false);
        SetListEnabled(OutfitForEdea, false);
        SetListEnabled(OutfitForCuteSeven, false);
        SetListEnabled(OutfitForFitness, false);
        SetListEnabled(OutfitForCuteEight, false);
        SetListEnabled(OutfitForCuteFour, false);
        SetListEnabled(OutfitForEssie, false);
        SetListEnabled(OutfitForCuteThree, false);
        SetListEnabled(OutfitForCuteFive, false);
        SetListEnabled(OutfitForFirstDate, false);
        SetListEnabled(OutfitForCuteSix, false);
        SetListEnabled(OutfitForThirdDate, false);
        SetListEnabled(OutfitForCuteOne, false);
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
    WorkTwo,
    Casual,
    CuteTwo,
    Casual3,
    Edea,
    CuteSeven,
    Fitness,
    CuteEight,
    CuteFour,
    Essie,
    CuteThree,
    CuteFive,
    CuteSix,
    Date2,
    Date3,
    CuteOne,
    Pyjamas,
    Homelessness,
    Wedding,
    Funeral
}

// ====================== EDITOR ======================
// ====================== EDITOR ======================
#if UNITY_EDITOR
[CustomEditor(typeof(Me))]
public class MeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Me me = (Me)target;

        EditorGUILayout.LabelField("Outfit Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Work
        EditorGUILayout.LabelField("Work Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Work", GUILayout.Height(35))) { me.ToggleWorkOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Work Two", GUILayout.Height(35))) { me.ToggleWorkTwoOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Casual
        EditorGUILayout.LabelField("Casual Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Casual One", GUILayout.Height(35))) { me.ToggleCasualOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("First Date", GUILayout.Height(35))) { me.ToggleFirstDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fitness", GUILayout.Height(35))) { me.ToggleFitnessOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Pyjamas", GUILayout.Height(35))) { me.TogglePyjamasOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();

        // Cute
        EditorGUILayout.LabelField("Cute Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cute One", GUILayout.Height(35))) { me.ToggleCuteOneOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Two", GUILayout.Height(35))) { me.ToggleCuteTwoOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Three", GUILayout.Height(35))) { me.ToggleCuteThreeOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Four", GUILayout.Height(35))) { me.ToggleCuteFourOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cute Five", GUILayout.Height(35))) { me.ToggleCuteFiveOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Six", GUILayout.Height(35))) { me.ToggleCuteSixOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Seven", GUILayout.Height(35))) { me.ToggleCuteSevenOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Eight", GUILayout.Height(35))) { me.ToggleCuteEightOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Edea", GUILayout.Height(35))) { me.ToggleEdeaOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Storyline
        EditorGUILayout.LabelField("Storyline Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wedding", GUILayout.Height(35))) { me.ToggleWeddingOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Funeral", GUILayout.Height(35))) { me.ToggleFuneralOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Skid Row", GUILayout.Height(35))) { me.ToggleHomelessnessOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Night Out
        EditorGUILayout.LabelField("Night Out Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Essie", GUILayout.Height(35))) { me.ToggleEssieOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Third Date", GUILayout.Height(35))) { me.ToggleThirdDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casual Three", GUILayout.Height(35))) { me.ToggleCasualThreeOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // ────────────────────────────────────────
        EditorGUILayout.LabelField("──────────────────────────────────────────────────────────────", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Random Outfits
        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random All", GUILayout.Height(40))) { me.SetRandomOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Work", GUILayout.Height(40))) { me.SetRandomWorkOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Casual", GUILayout.Height(40))) { me.SetRandomCasualOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Cute", GUILayout.Height(40))) { me.SetRandomCuteOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Storyline", GUILayout.Height(40))) { me.SetRandomStorylineOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Night Out", GUILayout.Height(40))) { me.SetRandomNightOutOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
#endif