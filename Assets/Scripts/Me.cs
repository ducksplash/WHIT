using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Me : MonoBehaviour
{
    [Header("Body")]
    public SkinnedMeshRenderer Body;

    [Header("\nAccessories\n")]
    public SkinnedMeshRenderer ButterflyWings;
    public SkinnedMeshRenderer Overall;
    public SkinnedMeshRenderer Hat;
    
    [Header("\nDefault Hair\n")]
    public SkinnedMeshRenderer DefaultHair;

    [Header("\nOutfits\n")]

    [Header("\nWork\n")]
    [Header("Work")]
    public bool WorkOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForWork;
    public Color lipsColorForWork = new Color(0.9f, 0.2f, 0.3f);
    public Color nailsColorForWork = new Color(0.95f, 0.8f, 0.85f);
    public bool JiggleForWork;
    public bool AccessoryWingsEnabledForWork;
    public bool AccessoryOverallEnabledForWork;
    public bool AccessoryHatEnabledForWork;

    [Header("Work Two")]
    public bool WorkTwoOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForWorkTwo;
    public Color lipsColorForWorkTwo = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkTwo = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkTwo;
    public bool AccessoryWingsEnabledForWorkTwo;
    public bool AccessoryOverallEnabledForWorkTwo;
    public bool AccessoryHatEnabledForWorkTwo;

    [Header("Work Three")]
    public bool WorkThreeOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForWorkThree;
    public Color lipsColorForWorkThree = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkThree = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkThree;
    public bool AccessoryWingsEnabledForWorkThree;
    public bool AccessoryOverallEnabledForWorkThree;
    public bool AccessoryHatEnabledForWorkThree;

    [Header("Work Four")]
    public bool WorkFourOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForWorkFour;
    public Color lipsColorForWorkFour = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkFour = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkFour;
    public bool AccessoryWingsEnabledForWorkFour;
    public bool AccessoryOverallEnabledForWorkFour;
    public bool AccessoryHatEnabledForWorkFour;

    [Header("\nCasual\n")]
    [Header("Casual One")]
    public bool CasualOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCasual;
    public Color lipsColorForCasual = new Color(0.85f, 0.25f, 0.35f);
    public Color nailsColorForCasual = new Color(0.92f, 0.75f, 0.8f);
    public bool JiggleCasually;
    public bool AccessoryWingsEnabledForCasual;
    public bool AccessoryOverallEnabledForCasual;
    public bool AccessoryHatEnabledForCasual;

    [Header("Fitness")]
    public bool FitnessOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForFitness;
    public Color lipsColorForFitness = new Color(0.92f, 0.28f, 0.38f);
    public Color nailsColorForFitness = new Color(0.9f, 0.75f, 0.8f);
    public bool JiggleFitness;
    public bool AccessoryWingsEnabledForFitness;
    public bool AccessoryOverallEnabledForFitness;
    public bool AccessoryHatEnabledForFitness;

    [Header("First Date")]
    public bool FirstDateOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForFirstDate;
    public Color lipsColorForFirstDate = new Color(0.96f, 0.4f, 0.5f);
    public Color nailsColorForFirstDate = new Color(0.97f, 0.88f, 0.9f);
    public bool JiggleOnAFirstDate;
    public bool AccessoryWingsEnabledForFirstDate;
    public bool AccessoryOverallEnabledForFirstDate;
    public bool AccessoryHatEnabledForFirstDate;

    [Header("Pyjamas")]
    public bool PyjamasOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForPyjamas;
    public Color lipsColorForPyjamas = new Color(0.8f, 0.15f, 0.25f);
    public Color nailsColorForPyjamas = new Color(0.85f, 0.6f, 0.7f);
    public bool JiggleInPyjamas;
    public bool AccessoryWingsEnabledForPyjamas;
    public bool AccessoryOverallEnabledForPyjamas;
    public bool AccessoryHatEnabledForPyjamas;

    [Header("Housecoat")]
    public bool HousecoatOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForHousecoat;
    public Color lipsColorForHousecoat = new Color(0.82f, 0.18f, 0.28f);
    public Color nailsColorForHousecoat = new Color(0.88f, 0.65f, 0.72f);
    public bool JiggleInHousecoat;
    public bool AccessoryWingsEnabledForHousecoat;
    public bool AccessoryOverallEnabledForHousecoat;
    public bool AccessoryHatEnabledForHousecoat;

    [Header("New Outfit One")]
    public bool RuffleBlousseAndSkirtEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForRuffleBlousseAndSkirt;
    public Color lipsColorForRuffleBlousseAndSkirt = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForRuffleBlousseAndSkirt = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleRuffleBlousseAndSkirt;
    public bool AccessoryWingsEnabledForRuffleBlousseAndSkirt;
    public bool AccessoryOverallEnabledForRuffleBlousseAndSkirt;
    public bool AccessoryHatEnabledForRuffleBlousseAndSkirt;

    [Header("New Outfit Two")]
    public bool NightOutRuffleEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNightOutRuffle;
    public Color lipsColorForNightOutRuffle = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNightOutRuffle = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNightOutRuffle;
    public bool AccessoryWingsEnabledForNightOutRuffle;
    public bool AccessoryOverallEnabledForNightOutRuffle;
    public bool AccessoryHatEnabledForNightOutRuffle;

    [Header("New Outfit Three")]
    public bool NewOutfitThreeEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitThree;
    public Color lipsColorForNewOutfitThree = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitThree = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitThree;
    public bool AccessoryWingsEnabledForNewOutfitThree;
    public bool AccessoryOverallEnabledForNewOutfitThree;
    public bool AccessoryHatEnabledForNewOutfitThree;

    [Header("New Outfit Four")]
    public bool NewOutfitFourEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitFour;
    public Color lipsColorForNewOutfitFour = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitFour = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitFour;
    public bool AccessoryWingsEnabledForNewOutfitFour;
    public bool AccessoryOverallEnabledForNewOutfitFour;
    public bool AccessoryHatEnabledForNewOutfitFour;

    [Header("New Outfit Five")]
    public bool NewOutfitFiveEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitFive;
    public Color lipsColorForNewOutfitFive = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitFive = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitFive;
    public bool AccessoryWingsEnabledForNewOutfitFive;
    public bool AccessoryOverallEnabledForNewOutfitFive;
    public bool AccessoryHatEnabledForNewOutfitFive;

    [Header("New Outfit Six")]
    public bool NewOutfitSixEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitSix;
    public Color lipsColorForNewOutfitSix = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitSix = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitSix;
    public bool AccessoryWingsEnabledForNewOutfitSix;
    public bool AccessoryOverallEnabledForNewOutfitSix;
    public bool AccessoryHatEnabledForNewOutfitSix;

    [Header("New Outfit Seven")]
    public bool NewOutfitSevenEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitSeven;
    public Color lipsColorForNewOutfitSeven = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitSeven = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitSeven;
    public bool AccessoryWingsEnabledForNewOutfitSeven;
    public bool AccessoryOverallEnabledForNewOutfitSeven;
    public bool AccessoryHatEnabledForNewOutfitSeven;

    [Header("New Outfit Eight")]
    public bool NewOutfitEightEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitEight;
    public Color lipsColorForNewOutfitEight = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitEight = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitEight;
    public bool AccessoryWingsEnabledForNewOutfitEight;
    public bool AccessoryOverallEnabledForNewOutfitEight;
    public bool AccessoryHatEnabledForNewOutfitEight;

    [Header("New Outfit Nine")]
    public bool NewOutfitNineEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitNine;
    public Color lipsColorForNewOutfitNine = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitNine = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitNine;
    public bool AccessoryWingsEnabledForNewOutfitNine;
    public bool AccessoryOverallEnabledForNewOutfitNine;
    public bool AccessoryHatEnabledForNewOutfitNine;

    [Header("New Outfit Ten")]
    public bool NewOutfitTenEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNewOutfitTen;
    public Color lipsColorForNewOutfitTen = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNewOutfitTen = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNewOutfitTen;
    public bool AccessoryWingsEnabledForNewOutfitTen;
    public bool AccessoryOverallEnabledForNewOutfitTen;
    public bool AccessoryHatEnabledForNewOutfitTen;

    [Header("\nCute\n")]
    [Header("Cute One")]
    public bool CuteOneOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteOne;
    public Color lipsColorForCuteOne = new Color(0.98f, 0.45f, 0.55f);
    public Color nailsColorForCuteOne = new Color(0.98f, 0.9f, 0.92f);
    public bool JiggleOnACuteOne;
    public bool AccessoryWingsEnabledForCuteOne;
    public bool AccessoryOverallEnabledForCuteOne;
    public bool AccessoryHatEnabledForCuteOne;

    [Header("Cute Two")]
    public bool CuteTwoOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteTwo;
    public Color lipsColorForCuteTwo = new Color(0.8f, 0.15f, 0.25f);
    public Color nailsColorForCuteTwo = new Color(0.88f, 0.65f, 0.75f);
    public bool JiggleCasuallyTwo;
    public bool AccessoryWingsEnabledForCuteTwo;
    public bool AccessoryOverallEnabledForCuteTwo;
    public bool AccessoryHatEnabledForCuteTwo;

    [Header("Cute Three")]
    public bool CuteThreeOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteThree;
    public Color lipsColorForCuteThree = new Color(0.95f, 0.35f, 0.45f);
    public Color nailsColorForCuteThree = new Color(0.96f, 0.85f, 0.88f);
    public bool JiggleCuteThree;
    public bool AccessoryWingsEnabledForCuteThree;
    public bool AccessoryOverallEnabledForCuteThree;
    public bool AccessoryHatEnabledForCuteThree;

    [Header("Cute Four")]
    public bool CuteFourOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteFour;
    public Color lipsColorForCuteFour = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForCuteFour = new Color(0.94f, 0.8f, 0.85f);
    public bool JiggleCuteFour;
    public bool AccessoryWingsEnabledForCuteFour;
    public bool AccessoryOverallEnabledForCuteFour;
    public bool AccessoryHatEnabledForCuteFour;

    [Header("Cute Five")]
    public bool CuteFiveOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteFive;
    public Color lipsColorForCuteFive = new Color(0.88f, 0.2f, 0.3f);
    public Color nailsColorForCuteFive = new Color(0.93f, 0.78f, 0.83f);
    public bool JiggleCuteFive;
    public bool AccessoryWingsEnabledForCuteFive;
    public bool AccessoryOverallEnabledForCuteFive;
    public bool AccessoryHatEnabledForCuteFive;

    [Header("Cute Six")]
    public bool CuteSixOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteSix;
    public Color lipsColorForCuteSix = new Color(0.9f, 0.3f, 0.4f);
    public Color nailsColorForCuteSix = new Color(0.95f, 0.82f, 0.87f);
    public bool JiggleOnACuteSix;
    public bool AccessoryWingsEnabledForCuteSix;
    public bool AccessoryOverallEnabledForCuteSix;
    public bool AccessoryHatEnabledForCuteSix;

    [Header("Cute Seven")]
    public bool CuteSevenOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteSeven;
    public Color lipsColorForCuteSeven = new Color(0.88f, 0.22f, 0.32f);
    public Color nailsColorForCuteSeven = new Color(0.95f, 0.82f, 0.85f);
    public bool JiggleCuteSeven;
    public bool AccessoryWingsEnabledForCuteSeven;
    public bool AccessoryOverallEnabledForCuteSeven;
    public bool AccessoryHatEnabledForCuteSeven;

    [Header("Cute Eight")]
    public bool CuteEightOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCuteEight;
    public Color lipsColorForCuteEight = new Color(0.85f, 0.18f, 0.28f);
    public Color nailsColorForCuteEight = new Color(0.88f, 0.68f, 0.73f);
    public bool JiggleCuteEight;
    public bool AccessoryWingsEnabledForCuteEight;
    public bool AccessoryOverallEnabledForCuteEight;
    public bool AccessoryHatEnabledForCuteEight;

    [Header("Edea")]
    public bool EdeaOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForEdea;
    public Color lipsColorForEdea = new Color(0.75f, 0.1f, 0.2f);
    public Color nailsColorForEdea = new Color(0.9f, 0.7f, 0.75f);
    public bool JiggleCasuallyFour;
    public bool AccessoryWingsEnabledForEdea;
    public bool AccessoryOverallEnabledForEdea;
    public bool AccessoryHatEnabledForEdea;

    [Header("\nStoryline\n")]
    [Header("Wedding")]
    public bool WeddingOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForWedding;
    public Color lipsColorForWedding = new Color(0.95f, 0.55f, 0.6f);
    public Color nailsColorForWedding = new Color(0.96f, 0.88f, 0.9f);
    public bool JiggleAtAWedding;
    public bool AccessoryWingsEnabledForWedding;
    public bool AccessoryOverallEnabledForWedding;
    public bool AccessoryHatEnabledForWedding;

    [Header("Funeral")]
    public bool FuneralOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForFuneral;
    public Color lipsColorForFuneral = new Color(0.65f, 0.08f, 0.15f);
    public Color nailsColorForFuneral = new Color(0.7f, 0.5f, 0.55f);
    public bool JiggleAtAFuneral;
    public bool AccessoryWingsEnabledForFuneral;
    public bool AccessoryOverallEnabledForFuneral;
    public bool AccessoryHatEnabledForFuneral;

    [Header("On Skid Row")]
    public bool HomelessnessOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForHomelessness;
    public Color lipsColorForHomelessness = new Color(0.7f, 0.1f, 0.2f);
    public Color nailsColorForHomelessness = new Color(0.75f, 0.55f, 0.6f);
    public bool JiggleWhileHomeless;
    public bool AccessoryWingsEnabledForHomelessness;
    public bool AccessoryOverallEnabledForHomelessness;
    public bool AccessoryHatEnabledForHomelessness;

    [Header("\nNight Out\n")]
    [Header("Third Date")]
    public bool ThirdDateOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForThirdDate;
    public Color lipsColorForThirdDate = new Color(0.85f, 0.22f, 0.32f);
    public Color nailsColorForThirdDate = new Color(0.94f, 0.8f, 0.85f);
    public bool JiggleOnAThirdDate;
    public bool AccessoryWingsEnabledForThirdDate;
    public bool AccessoryOverallEnabledForThirdDate;
    public bool AccessoryHatEnabledForThirdDate;

    [Header("Casual Three")]
    public bool CasualThreeOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForCasualThree;
    public Color lipsColorForCasualThree = new Color(0.9f, 0.3f, 0.4f);
    public Color nailsColorForCasualThree = new Color(0.93f, 0.78f, 0.82f);
    public bool JiggleCasuallyThree;
    public bool AccessoryWingsEnabledForCasualThree;
    public bool AccessoryOverallEnabledForCasualThree;
    public bool AccessoryHatEnabledForCasualThree;

    [Header("Essie")]
    public bool EssieOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForEssie;
    public Color lipsColorForEssie = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForEssie = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleEssie;
    public bool AccessoryWingsEnabledForEssie;
    public bool AccessoryOverallEnabledForEssie;
    public bool AccessoryHatEnabledForEssie;

    [Header("Night Out Four")]
    public bool NightOutFourOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForNightOutFour;
    public Color lipsColorForNightOutFour = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForNightOutFour = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleNightOutFour;
    public bool AccessoryWingsEnabledForNightOutFour;
    public bool AccessoryOverallEnabledForNightOutFour;
    public bool AccessoryHatEnabledForNightOutFour;

    [Header("Elegant Dress")]
    public bool ElegantDressOutfitEnabled = true;
    public List<SkinnedMeshRenderer> OutfitForElegantDress;
    public Color lipsColorForElegantDress = new Color(0.78f, 0.08f, 0.18f);
    public Color nailsColorForElegantDress = new Color(0.92f, 0.76f, 0.82f);
    public bool JiggleElegantDress;
    public bool AccessoryWingsEnabledForElegantDress;
    public bool AccessoryOverallEnabledForElegantDress;
    public bool AccessoryHatEnabledForElegantDress;

    [Header("\nUndergarments\n")]
    public List<SkinnedMeshRenderer> OutfitForUndergarments;
    public Color lipsColorForUndergarments = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForUndergarments = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleUndergarments;
    public bool AccessoryWingsEnabledForUndergarments;
    public bool AccessoryOverallEnabledForUndergarments;
    public bool AccessoryHatEnabledForUndergarments;

    [Header("Jigglers")]
    public GameObject JiggleLeftBoob;
    public GameObject JiggleRightBoob;
    public GameObject JiggleLeftButtcheek;
    public GameObject JiggleRightButtcheek;

    [Header("Input")]
    public InputActionReference nextOutfit;
    public InputActionReference previousOutfit;
    public InputActionReference holdToUnlock;
    public InputActionReference holdToUnlockAll;

    [Header("Settings")]
    public bool UnressedOnLoad;
    public string ThisCharacterName;

    [Header("Only applies to NPCs")]
    public NPCController npcController;

    [Header("Current Outfit")]
    public OutfitType currentOutfit = OutfitType.Work;

    private bool _wingsOverride = false;
    private bool _overallOverride = false;
    private bool _hatOverride = false;

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

        if (!UnressedOnLoad) SwitchToOutfit(OutfitType.Work);

        if (npcController == null) SetupInput();
    }

    private void SetupInput()
    {
        if (holdToUnlock != null) holdToUnlock.action.performed += OnToggleUndergarments;
        if (holdToUnlockAll != null) holdToUnlockAll.action.performed += OnToggleNothinatall;
        if (nextOutfit != null) nextOutfit.action.performed += OnNextOutfit;
        if (previousOutfit != null) previousOutfit.action.performed += OnPreviousOutfit;
    }

    private void OnToggleUndergarments(InputAction.CallbackContext ctx) => ToggleUndergarments();
    private void OnToggleNothinatall(InputAction.CallbackContext ctx) => Undress();
    private void OnNextOutfit(InputAction.CallbackContext ctx) => NextOutfit();
    private void OnPreviousOutfit(InputAction.CallbackContext ctx) => PreviousOutfit();

    private void OnDisable()
    {
        if (npcController == null)
        {
            if (nextOutfit != null) nextOutfit.action.performed -= OnNextOutfit;
            if (previousOutfit != null) previousOutfit.action.performed -= OnPreviousOutfit;
            if (holdToUnlock != null) holdToUnlock.action.performed -= OnToggleUndergarments;
            if (holdToUnlockAll != null) holdToUnlockAll.action.performed -= OnToggleNothinatall;
        }
    }

    // ====================== ENABLED CHECK ======================
    public bool IsOutfitEnabled(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:           return WorkOutfitEnabled;
            case OutfitType.WorkTwo:        return WorkTwoOutfitEnabled;
            case OutfitType.WorkThree:      return WorkThreeOutfitEnabled;
            case OutfitType.WorkFour:       return WorkFourOutfitEnabled;
            case OutfitType.Casual:         return CasualOutfitEnabled;
            case OutfitType.Fitness:        return FitnessOutfitEnabled;
            case OutfitType.CuteSix:        return FirstDateOutfitEnabled;
            case OutfitType.Pyjamas:        return PyjamasOutfitEnabled;
            case OutfitType.Housecoat:      return HousecoatOutfitEnabled;
            case OutfitType.RuffleBlousseAndSkirt:   return RuffleBlousseAndSkirtEnabled;
            case OutfitType.NightOutRuffle:   return NightOutRuffleEnabled;
            case OutfitType.NewOutfitThree: return NewOutfitThreeEnabled;
            case OutfitType.NewOutfitFour:  return NewOutfitFourEnabled;
            case OutfitType.NewOutfitFive:  return NewOutfitFiveEnabled;
            case OutfitType.NewOutfitSix:   return NewOutfitSixEnabled;
            case OutfitType.NewOutfitSeven: return NewOutfitSevenEnabled;
            case OutfitType.NewOutfitEight: return NewOutfitEightEnabled;
            case OutfitType.NewOutfitNine:  return NewOutfitNineEnabled;
            case OutfitType.NewOutfitTen:   return NewOutfitTenEnabled;
            case OutfitType.CuteOne:        return CuteOneOutfitEnabled;
            case OutfitType.CuteTwo:        return CuteTwoOutfitEnabled;
            case OutfitType.CuteThree:      return CuteThreeOutfitEnabled;
            case OutfitType.CuteFour:       return CuteFourOutfitEnabled;
            case OutfitType.CuteFive:       return CuteFiveOutfitEnabled;
            case OutfitType.Date2:          return CuteSixOutfitEnabled;
            case OutfitType.CuteSeven:      return CuteSevenOutfitEnabled;
            case OutfitType.CuteEight:      return CuteEightOutfitEnabled;
            case OutfitType.Edea:           return EdeaOutfitEnabled;
            case OutfitType.Casual3:        return CasualThreeOutfitEnabled;
            case OutfitType.Wedding:        return WeddingOutfitEnabled;
            case OutfitType.Funeral:        return FuneralOutfitEnabled;
            case OutfitType.Homelessness:   return HomelessnessOutfitEnabled;
            case OutfitType.Date3:          return ThirdDateOutfitEnabled;
            case OutfitType.Essie:          return EssieOutfitEnabled;
            case OutfitType.NightOutFour:   return NightOutFourOutfitEnabled;
            case OutfitType.ElegantDress:   return ElegantDressOutfitEnabled;
            default:                        return true;
        }
    }

    // ====================== ACCESSORY TOGGLES ======================
    public void ToggleWings(bool? forceOn = null)
    {
        _wingsOverride = forceOn.HasValue ? forceOn.Value : !_wingsOverride;
        ApplyAccessories();
    }

    public void ToggleOverall(bool? forceOn = null)
    {
        _overallOverride = forceOn.HasValue ? forceOn.Value : !_overallOverride;
        ApplyAccessories();
    }

    public void ToggleHat(bool? forceOn = null)
    {
        _hatOverride = forceOn.HasValue ? forceOn.Value : !_hatOverride;
        ApplyAccessories();
    }

    private void ApplyAccessories()
    {
        GetOutfitAccessoryDefaults(currentOutfit, out bool wingsDefault, out bool overallDefault, out bool hatDefault);
        if (ButterflyWings != null) ButterflyWings.enabled = wingsDefault  || _wingsOverride;
        if (Overall        != null) Overall.enabled        = overallDefault || _overallOverride;
        if (Hat            != null) Hat.enabled            = hatDefault     || _hatOverride;
    }

    private void GetOutfitAccessoryDefaults(OutfitType outfit, out bool wings, out bool overall, out bool hat)
    {
        switch (outfit)
        {
            case OutfitType.Work:             wings = AccessoryWingsEnabledForWork;             overall = AccessoryOverallEnabledForWork;             hat = AccessoryHatEnabledForWork;             break;
            case OutfitType.WorkTwo:          wings = AccessoryWingsEnabledForWorkTwo;          overall = AccessoryOverallEnabledForWorkTwo;          hat = AccessoryHatEnabledForWorkTwo;          break;
            case OutfitType.WorkThree:        wings = AccessoryWingsEnabledForWorkThree;        overall = AccessoryOverallEnabledForWorkThree;        hat = AccessoryHatEnabledForWorkThree;        break;
            case OutfitType.WorkFour:         wings = AccessoryWingsEnabledForWorkFour;         overall = AccessoryOverallEnabledForWorkFour;         hat = AccessoryHatEnabledForWorkFour;         break;
            case OutfitType.Casual:           wings = AccessoryWingsEnabledForCasual;           overall = AccessoryOverallEnabledForCasual;           hat = AccessoryHatEnabledForCasual;           break;
            case OutfitType.Fitness:          wings = AccessoryWingsEnabledForFitness;          overall = AccessoryOverallEnabledForFitness;          hat = AccessoryHatEnabledForFitness;          break;
            case OutfitType.CuteSix:          wings = AccessoryWingsEnabledForFirstDate;        overall = AccessoryOverallEnabledForFirstDate;        hat = AccessoryHatEnabledForFirstDate;        break;
            case OutfitType.Pyjamas:          wings = AccessoryWingsEnabledForPyjamas;          overall = AccessoryOverallEnabledForPyjamas;          hat = AccessoryHatEnabledForPyjamas;          break;
            case OutfitType.Housecoat:        wings = AccessoryWingsEnabledForHousecoat;        overall = AccessoryOverallEnabledForHousecoat;        hat = AccessoryHatEnabledForHousecoat;        break;
            case OutfitType.RuffleBlousseAndSkirt:     wings = AccessoryWingsEnabledForRuffleBlousseAndSkirt;     overall = AccessoryOverallEnabledForRuffleBlousseAndSkirt;     hat = AccessoryHatEnabledForRuffleBlousseAndSkirt;     break;
            case OutfitType.NightOutRuffle:     wings = AccessoryWingsEnabledForNightOutRuffle;     overall = AccessoryOverallEnabledForNightOutRuffle;     hat = AccessoryHatEnabledForNightOutRuffle;     break;
            case OutfitType.NewOutfitThree:   wings = AccessoryWingsEnabledForNewOutfitThree;   overall = AccessoryOverallEnabledForNewOutfitThree;   hat = AccessoryHatEnabledForNewOutfitThree;   break;
            case OutfitType.NewOutfitFour:    wings = AccessoryWingsEnabledForNewOutfitFour;    overall = AccessoryOverallEnabledForNewOutfitFour;    hat = AccessoryHatEnabledForNewOutfitFour;    break;
            case OutfitType.NewOutfitFive:    wings = AccessoryWingsEnabledForNewOutfitFive;    overall = AccessoryOverallEnabledForNewOutfitFive;    hat = AccessoryHatEnabledForNewOutfitFive;    break;
            case OutfitType.NewOutfitSix:     wings = AccessoryWingsEnabledForNewOutfitSix;     overall = AccessoryOverallEnabledForNewOutfitSix;     hat = AccessoryHatEnabledForNewOutfitSix;     break;
            case OutfitType.NewOutfitSeven:   wings = AccessoryWingsEnabledForNewOutfitSeven;   overall = AccessoryOverallEnabledForNewOutfitSeven;   hat = AccessoryHatEnabledForNewOutfitSeven;   break;
            case OutfitType.NewOutfitEight:   wings = AccessoryWingsEnabledForNewOutfitEight;   overall = AccessoryOverallEnabledForNewOutfitEight;   hat = AccessoryHatEnabledForNewOutfitEight;   break;
            case OutfitType.NewOutfitNine:    wings = AccessoryWingsEnabledForNewOutfitNine;    overall = AccessoryOverallEnabledForNewOutfitNine;    hat = AccessoryHatEnabledForNewOutfitNine;    break;
            case OutfitType.NewOutfitTen:     wings = AccessoryWingsEnabledForNewOutfitTen;     overall = AccessoryOverallEnabledForNewOutfitTen;     hat = AccessoryHatEnabledForNewOutfitTen;     break;
            case OutfitType.CuteOne:          wings = AccessoryWingsEnabledForCuteOne;          overall = AccessoryOverallEnabledForCuteOne;          hat = AccessoryHatEnabledForCuteOne;          break;
            case OutfitType.CuteTwo:          wings = AccessoryWingsEnabledForCuteTwo;          overall = AccessoryOverallEnabledForCuteTwo;          hat = AccessoryHatEnabledForCuteTwo;          break;
            case OutfitType.CuteThree:        wings = AccessoryWingsEnabledForCuteThree;        overall = AccessoryOverallEnabledForCuteThree;        hat = AccessoryHatEnabledForCuteThree;        break;
            case OutfitType.CuteFour:         wings = AccessoryWingsEnabledForCuteFour;         overall = AccessoryOverallEnabledForCuteFour;         hat = AccessoryHatEnabledForCuteFour;         break;
            case OutfitType.CuteFive:         wings = AccessoryWingsEnabledForCuteFive;         overall = AccessoryOverallEnabledForCuteFive;         hat = AccessoryHatEnabledForCuteFive;         break;
            case OutfitType.Date2:            wings = AccessoryWingsEnabledForCuteSix;          overall = AccessoryOverallEnabledForCuteSix;          hat = AccessoryHatEnabledForCuteSix;          break;
            case OutfitType.CuteSeven:        wings = AccessoryWingsEnabledForCuteSeven;        overall = AccessoryOverallEnabledForCuteSeven;        hat = AccessoryHatEnabledForCuteSeven;        break;
            case OutfitType.CuteEight:        wings = AccessoryWingsEnabledForCuteEight;        overall = AccessoryOverallEnabledForCuteEight;        hat = AccessoryHatEnabledForCuteEight;        break;
            case OutfitType.Edea:             wings = AccessoryWingsEnabledForEdea;             overall = AccessoryOverallEnabledForEdea;             hat = AccessoryHatEnabledForEdea;             break;
            case OutfitType.Casual3:          wings = AccessoryWingsEnabledForCasualThree;      overall = AccessoryOverallEnabledForCasualThree;      hat = AccessoryHatEnabledForCasualThree;      break;
            case OutfitType.Wedding:          wings = AccessoryWingsEnabledForWedding;          overall = AccessoryOverallEnabledForWedding;          hat = AccessoryHatEnabledForWedding;          break;
            case OutfitType.Funeral:          wings = AccessoryWingsEnabledForFuneral;          overall = AccessoryOverallEnabledForFuneral;          hat = AccessoryHatEnabledForFuneral;          break;
            case OutfitType.Homelessness:     wings = AccessoryWingsEnabledForHomelessness;     overall = AccessoryOverallEnabledForHomelessness;     hat = AccessoryHatEnabledForHomelessness;     break;
            case OutfitType.Date3:            wings = AccessoryWingsEnabledForThirdDate;        overall = AccessoryOverallEnabledForThirdDate;        hat = AccessoryHatEnabledForThirdDate;        break;
            case OutfitType.Essie:            wings = AccessoryWingsEnabledForEssie;            overall = AccessoryOverallEnabledForEssie;            hat = AccessoryHatEnabledForEssie;            break;
            case OutfitType.NightOutFour:     wings = AccessoryWingsEnabledForNightOutFour;     overall = AccessoryOverallEnabledForNightOutFour;     hat = AccessoryHatEnabledForNightOutFour;     break;
            case OutfitType.ElegantDress:     wings = AccessoryWingsEnabledForElegantDress;     overall = AccessoryOverallEnabledForElegantDress;     hat = AccessoryHatEnabledForElegantDress;     break;
            case OutfitType.Undergarments:    wings = AccessoryWingsEnabledForUndergarments;    overall = AccessoryOverallEnabledForUndergarments;    hat = AccessoryHatEnabledForUndergarments;    break;
            default:                          wings = false; overall = false; hat = false; break;
        }
    }

    public void NextOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) return;

        int total = System.Enum.GetValues(typeof(OutfitType)).Length;
        int index = (int)currentOutfit;

        do
        {
            index = (index + 1) % total;
        }
        while (index == (int)OutfitType.None
            || index == (int)OutfitType.Undergarments
            || !IsOutfitEnabled((OutfitType)index));

        SwitchToOutfit((OutfitType)index);
    }

    public void PreviousOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) return;

        int total = System.Enum.GetValues(typeof(OutfitType)).Length;
        int index = (int)currentOutfit;

        do
        {
            index = (index - 1 + total) % total;
        }
        while (index == (int)OutfitType.None
            || index == (int)OutfitType.Undergarments
            || !IsOutfitEnabled((OutfitType)index));

        SwitchToOutfit((OutfitType)index);
    }

    private void SwitchToOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:           ToggleWorkOutfit(true);           break;
            case OutfitType.WorkTwo:        ToggleWorkTwoOutfit(true);        break;
            case OutfitType.WorkThree:      ToggleWorkThreeOutfit(true);      break;
            case OutfitType.WorkFour:       ToggleWorkFourOutfit(true);       break;
            case OutfitType.Casual:         ToggleCasualOutfit(true);         break;
            case OutfitType.CuteTwo:        ToggleCuteTwoOutfit(true);        break;
            case OutfitType.Casual3:        ToggleCasualThreeOutfit(true);    break;
            case OutfitType.Edea:           ToggleEdeaOutfit(true);           break;
            case OutfitType.CuteSeven:      ToggleCuteSevenOutfit(true);      break;
            case OutfitType.Fitness:        ToggleFitnessOutfit(true);        break;
            case OutfitType.CuteEight:      ToggleCuteEightOutfit(true);      break;
            case OutfitType.CuteFour:       ToggleCuteFourOutfit(true);       break;
            case OutfitType.Essie:          ToggleEssieOutfit(true);          break;
            case OutfitType.NightOutFour:   ToggleNightOutFourOutfit(true);   break;
            case OutfitType.ElegantDress:   ToggleElegantDressOutfit(true);   break;
            case OutfitType.CuteThree:      ToggleCuteThreeOutfit(true);      break;
            case OutfitType.CuteFive:       ToggleCuteFiveOutfit(true);       break;
            case OutfitType.CuteSix:        ToggleFirstDateOutfit(true);      break;
            case OutfitType.Date2:          ToggleCuteSixOutfit(true);        break;
            case OutfitType.Date3:          ToggleThirdDateOutfit(true);      break;
            case OutfitType.CuteOne:        ToggleCuteOneOutfit(true);        break;
            case OutfitType.Pyjamas:        TogglePyjamasOutfit(true);        break;
            case OutfitType.Housecoat:      ToggleHousecoatOutfit(true);      break;
            case OutfitType.RuffleBlousseAndSkirt:   ToggleRuffleBlousseAndSkirtOutfit(true);   break;
            case OutfitType.NightOutRuffle:   ToggleNightOutRuffleOutfit(true);   break;
            case OutfitType.NewOutfitThree: ToggleNewOutfitThreeOutfit(true); break;
            case OutfitType.NewOutfitFour:  ToggleNewOutfitFourOutfit(true);  break;
            case OutfitType.NewOutfitFive:  ToggleNewOutfitFiveOutfit(true);  break;
            case OutfitType.NewOutfitSix:   ToggleNewOutfitSixOutfit(true);   break;
            case OutfitType.NewOutfitSeven: ToggleNewOutfitSevenOutfit(true); break;
            case OutfitType.NewOutfitEight: ToggleNewOutfitEightOutfit(true); break;
            case OutfitType.NewOutfitNine:  ToggleNewOutfitNineOutfit(true);  break;
            case OutfitType.NewOutfitTen:   ToggleNewOutfitTenOutfit(true);   break;
            case OutfitType.Homelessness:   ToggleHomelessnessOutfit(true);   break;
            case OutfitType.Wedding:        ToggleWeddingOutfit(true);        break;
            case OutfitType.Funeral:        ToggleFuneralOutfit(true);        break;
            default:                        DisableAllMainOutfits();          break;
        }
    }

    // ====================== TOGGLE METHODS ======================
    public void ToggleWorkOutfit(bool? forceOn = null)           { JiggleToggle(JiggleForWork);          ToggleMainOutfit(OutfitType.Work, forceOn); }
    public void ToggleWorkTwoOutfit(bool? forceOn = null)        { JiggleToggle(JiggleForWorkTwo);       ToggleMainOutfit(OutfitType.WorkTwo, forceOn); }
    public void ToggleWorkThreeOutfit(bool? forceOn = null)      { JiggleToggle(JiggleForWorkThree);     ToggleMainOutfit(OutfitType.WorkThree, forceOn); }
    public void ToggleWorkFourOutfit(bool? forceOn = null)       { JiggleToggle(JiggleForWorkFour);      ToggleMainOutfit(OutfitType.WorkFour, forceOn); }
    public void ToggleCasualOutfit(bool? forceOn = null)         { JiggleToggle(JiggleCasually);         ToggleMainOutfit(OutfitType.Casual, forceOn); }
    public void ToggleCuteTwoOutfit(bool? forceOn = null)        { JiggleToggle(JiggleCasuallyTwo);      ToggleMainOutfit(OutfitType.CuteTwo, forceOn); }
    public void ToggleCasualThreeOutfit(bool? forceOn = null)    { JiggleToggle(JiggleCasuallyThree);    ToggleMainOutfit(OutfitType.Casual3, forceOn); }
    public void ToggleEdeaOutfit(bool? forceOn = null)           { JiggleToggle(JiggleCasuallyFour);     ToggleMainOutfit(OutfitType.Edea, forceOn); }
    public void ToggleCuteSevenOutfit(bool? forceOn = null)      { JiggleToggle(JiggleCuteSeven);        ToggleMainOutfit(OutfitType.CuteSeven, forceOn); }
    public void ToggleFitnessOutfit(bool? forceOn = null)        { JiggleToggle(JiggleFitness);          ToggleMainOutfit(OutfitType.Fitness, forceOn); }
    public void ToggleCuteEightOutfit(bool? forceOn = null)      { JiggleToggle(JiggleCuteEight);        ToggleMainOutfit(OutfitType.CuteEight, forceOn); }
    public void ToggleCuteFourOutfit(bool? forceOn = null)       { JiggleToggle(JiggleCuteFour);         ToggleMainOutfit(OutfitType.CuteFour, forceOn); }
    public void ToggleEssieOutfit(bool? forceOn = null)          { JiggleToggle(JiggleEssie);            ToggleMainOutfit(OutfitType.Essie, forceOn); }
    public void ToggleNightOutFourOutfit(bool? forceOn = null)   { JiggleToggle(JiggleNightOutFour);     ToggleMainOutfit(OutfitType.NightOutFour, forceOn); }
    public void ToggleElegantDressOutfit(bool? forceOn = null)   { JiggleToggle(JiggleElegantDress);     ToggleMainOutfit(OutfitType.ElegantDress, forceOn); }
    public void ToggleUndergarments(bool? forceOn = null)        { JiggleToggle(JiggleUndergarments);    ToggleMainOutfit(OutfitType.Undergarments, forceOn); }
    public void ToggleCuteThreeOutfit(bool? forceOn = null)      { JiggleToggle(JiggleCuteThree);        ToggleMainOutfit(OutfitType.CuteThree, forceOn); }
    public void ToggleCuteFiveOutfit(bool? forceOn = null)       { JiggleToggle(JiggleCuteFive);         ToggleMainOutfit(OutfitType.CuteFive, forceOn); }
    public void ToggleFirstDateOutfit(bool? forceOn = null)      { JiggleToggle(JiggleOnAFirstDate);     ToggleMainOutfit(OutfitType.CuteSix, forceOn); }
    public void ToggleCuteSixOutfit(bool? forceOn = null)        { JiggleToggle(JiggleOnACuteSix);       ToggleMainOutfit(OutfitType.Date2, forceOn); }
    public void ToggleThirdDateOutfit(bool? forceOn = null)      { JiggleToggle(JiggleOnAThirdDate);     ToggleMainOutfit(OutfitType.Date3, forceOn); }
    public void ToggleCuteOneOutfit(bool? forceOn = null)        { JiggleToggle(JiggleOnACuteOne);       ToggleMainOutfit(OutfitType.CuteOne, forceOn); }
    public void TogglePyjamasOutfit(bool? forceOn = null)        { JiggleToggle(JiggleInPyjamas);        ToggleMainOutfit(OutfitType.Pyjamas, forceOn); }
    public void ToggleHousecoatOutfit(bool? forceOn = null)      { JiggleToggle(JiggleInHousecoat);      ToggleMainOutfit(OutfitType.Housecoat, forceOn); }
    public void ToggleRuffleBlousseAndSkirtOutfit(bool? forceOn = null)   { JiggleToggle(JiggleRuffleBlousseAndSkirt);     ToggleMainOutfit(OutfitType.RuffleBlousseAndSkirt, forceOn); }
    public void ToggleNightOutRuffleOutfit(bool? forceOn = null)   { JiggleToggle(JiggleNightOutRuffle);     ToggleMainOutfit(OutfitType.NightOutRuffle, forceOn); }
    public void ToggleNewOutfitThreeOutfit(bool? forceOn = null) { JiggleToggle(JiggleNewOutfitThree);   ToggleMainOutfit(OutfitType.NewOutfitThree, forceOn); }
    public void ToggleNewOutfitFourOutfit(bool? forceOn = null)  { JiggleToggle(JiggleNewOutfitFour);    ToggleMainOutfit(OutfitType.NewOutfitFour, forceOn); }
    public void ToggleNewOutfitFiveOutfit(bool? forceOn = null)  { JiggleToggle(JiggleNewOutfitFive);    ToggleMainOutfit(OutfitType.NewOutfitFive, forceOn); }
    public void ToggleNewOutfitSixOutfit(bool? forceOn = null)   { JiggleToggle(JiggleNewOutfitSix);     ToggleMainOutfit(OutfitType.NewOutfitSix, forceOn); }
    public void ToggleNewOutfitSevenOutfit(bool? forceOn = null) { JiggleToggle(JiggleNewOutfitSeven);   ToggleMainOutfit(OutfitType.NewOutfitSeven, forceOn); }
    public void ToggleNewOutfitEightOutfit(bool? forceOn = null) { JiggleToggle(JiggleNewOutfitEight);   ToggleMainOutfit(OutfitType.NewOutfitEight, forceOn); }
    public void ToggleNewOutfitNineOutfit(bool? forceOn = null)  { JiggleToggle(JiggleNewOutfitNine);    ToggleMainOutfit(OutfitType.NewOutfitNine, forceOn); }
    public void ToggleNewOutfitTenOutfit(bool? forceOn = null)   { JiggleToggle(JiggleNewOutfitTen);     ToggleMainOutfit(OutfitType.NewOutfitTen, forceOn); }
    public void ToggleHomelessnessOutfit(bool? forceOn = null)   { JiggleToggle(JiggleWhileHomeless);    ToggleMainOutfit(OutfitType.Homelessness, forceOn); }
    public void ToggleWeddingOutfit(bool? forceOn = null)        { JiggleToggle(JiggleAtAWedding);       ToggleMainOutfit(OutfitType.Wedding, forceOn); }
    public void ToggleFuneralOutfit(bool? forceOn = null)        { JiggleToggle(JiggleAtAFuneral);       ToggleMainOutfit(OutfitType.Funeral, forceOn); }

    private void JiggleToggle(bool isJiggly = false)
    {
        if (JiggleLeftBoob       != null) JiggleLeftBoob.SetActive(isJiggly);
        if (JiggleRightBoob      != null) JiggleRightBoob.SetActive(isJiggly);
        if (JiggleLeftButtcheek  != null) JiggleLeftButtcheek.SetActive(isJiggly);
        if (JiggleRightButtcheek != null) JiggleRightButtcheek.SetActive(isJiggly);
    }

    private void ToggleMainOutfit(OutfitType outfit, bool? forceOn = null)
    {
        if (forceOn.HasValue)
        {
            if (forceOn.Value) SetMainOutfit(outfit);
            else               DisableAllMainOutfits();
        }
        else
        {
            if (currentOutfit == outfit) DisableAllMainOutfits();
            else                         SetMainOutfit(outfit);
        }
    }

    public void SetMainOutfit(OutfitType outfit)
    {
        DisableAllMainOutfits();
        if (DefaultHair != null) DefaultHair.enabled = false;
        currentOutfit = outfit;

        switch (outfit)
        {
            case OutfitType.Work:           SetListEnabled(OutfitForWork, true);           break;
            case OutfitType.WorkTwo:        SetListEnabled(OutfitForWorkTwo, true);        break;
            case OutfitType.WorkThree:      SetListEnabled(OutfitForWorkThree, true);      break;
            case OutfitType.WorkFour:       SetListEnabled(OutfitForWorkFour, true);       break;
            case OutfitType.Casual:         SetListEnabled(OutfitForCasual, true);         break;
            case OutfitType.CuteTwo:        SetListEnabled(OutfitForCuteTwo, true);        break;
            case OutfitType.Casual3:        SetListEnabled(OutfitForCasualThree, true);    break;
            case OutfitType.Edea:           SetListEnabled(OutfitForEdea, true);           break;
            case OutfitType.CuteSeven:      SetListEnabled(OutfitForCuteSeven, true);      break;
            case OutfitType.Fitness:        SetListEnabled(OutfitForFitness, true);        break;
            case OutfitType.CuteEight:      SetListEnabled(OutfitForCuteEight, true);      break;
            case OutfitType.CuteFour:       SetListEnabled(OutfitForCuteFour, true);       break;
            case OutfitType.Essie:          SetListEnabled(OutfitForEssie, true);          break;
            case OutfitType.NightOutFour:   SetListEnabled(OutfitForNightOutFour, true);   break;
            case OutfitType.ElegantDress:   SetListEnabled(OutfitForElegantDress, true);   break;
            case OutfitType.Undergarments:  SetListEnabled(OutfitForUndergarments, true);  break;
            case OutfitType.CuteThree:      SetListEnabled(OutfitForCuteThree, true);      break;
            case OutfitType.CuteFive:       SetListEnabled(OutfitForCuteFive, true);       break;
            case OutfitType.CuteSix:        SetListEnabled(OutfitForFirstDate, true);      break;
            case OutfitType.Date2:          SetListEnabled(OutfitForCuteSix, true);        break;
            case OutfitType.Date3:          SetListEnabled(OutfitForThirdDate, true);      break;
            case OutfitType.CuteOne:        SetListEnabled(OutfitForCuteOne, true);        break;
            case OutfitType.Pyjamas:        SetListEnabled(OutfitForPyjamas, true);        break;
            case OutfitType.Housecoat:      SetListEnabled(OutfitForHousecoat, true);      break;
            case OutfitType.RuffleBlousseAndSkirt:   SetListEnabled(OutfitForRuffleBlousseAndSkirt, true);   break;
            case OutfitType.NightOutRuffle:   SetListEnabled(OutfitForNightOutRuffle, true);   break;
            case OutfitType.NewOutfitThree: SetListEnabled(OutfitForNewOutfitThree, true); break;
            case OutfitType.NewOutfitFour:  SetListEnabled(OutfitForNewOutfitFour, true);  break;
            case OutfitType.NewOutfitFive:  SetListEnabled(OutfitForNewOutfitFive, true);  break;
            case OutfitType.NewOutfitSix:   SetListEnabled(OutfitForNewOutfitSix, true);   break;
            case OutfitType.NewOutfitSeven: SetListEnabled(OutfitForNewOutfitSeven, true); break;
            case OutfitType.NewOutfitEight: SetListEnabled(OutfitForNewOutfitEight, true); break;
            case OutfitType.NewOutfitNine:  SetListEnabled(OutfitForNewOutfitNine, true);  break;
            case OutfitType.NewOutfitTen:   SetListEnabled(OutfitForNewOutfitTen, true);   break;
            case OutfitType.Homelessness:   SetListEnabled(OutfitForHomelessness, true);   break;
            case OutfitType.Wedding:        SetListEnabled(OutfitForWedding, true);        break;
            case OutfitType.Funeral:        SetListEnabled(OutfitForFuneral, true);        break;
        }

        ApplyBodyColors(outfit);
        ApplyAccessories();
    }

    // ====================== RANDOM OUTFIT SELECTION ======================
    private OutfitType PickRandom(OutfitType[] pool)
    {
        OutfitType[] enabled = pool.Where(o => IsOutfitEnabled(o)).ToArray();
        if (enabled.Length == 0) return currentOutfit; // fallback: stay put
        return enabled[Random.Range(0, enabled.Length)];
    }

    public void SetRandomOutfit()
    {
        var all = System.Enum.GetValues(typeof(OutfitType))
            .Cast<OutfitType>()
            .Where(o => o != OutfitType.None && o != OutfitType.Undergarments && IsOutfitEnabled(o))
            .ToArray();
        if (all.Length > 0) SwitchToOutfit(all[Random.Range(0, all.Length)]);
    }

    public void SetRandomWorkOutfit()
    {
        OutfitType[] pool = { OutfitType.Work, OutfitType.WorkTwo, OutfitType.WorkThree, OutfitType.WorkFour };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomCasualOutfit()
    {
        OutfitType[] pool =
        {
            OutfitType.Casual, OutfitType.Fitness, OutfitType.CuteSix, OutfitType.Pyjamas,
            OutfitType.Housecoat,
            OutfitType.RuffleBlousseAndSkirt, OutfitType.NightOutRuffle, OutfitType.NewOutfitThree,
            OutfitType.NewOutfitFour, OutfitType.NewOutfitFive, OutfitType.NewOutfitSix,
            OutfitType.NewOutfitSeven, OutfitType.NewOutfitEight, OutfitType.NewOutfitNine,
            OutfitType.NewOutfitTen
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomCuteOutfit()
    {
        OutfitType[] pool =
        {
            OutfitType.CuteOne, OutfitType.CuteTwo, OutfitType.CuteThree, OutfitType.CuteFour,
            OutfitType.CuteFive, OutfitType.Date2, OutfitType.CuteSeven, OutfitType.CuteEight, OutfitType.Edea
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomStorylineOutfit()
    {
        OutfitType[] pool = { OutfitType.Wedding, OutfitType.Funeral, OutfitType.Homelessness };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomNightOutOutfit()
    {
        OutfitType[] pool = { OutfitType.Date3, OutfitType.Casual3, OutfitType.Essie, OutfitType.NightOutFour, OutfitType.ElegantDress };
        SwitchToOutfit(PickRandom(pool));
    }

    // ====================== BODY COLOURS ======================
    private void ApplyBodyColors(OutfitType outfit)
    {
        if (Body == null) return;
        Color lipColor  = GetLipColorForOutfit(outfit);
        Color nailColor = GetNailColorForOutfit(outfit);
        Material[] materials = Application.isPlaying ? Body.materials : Body.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null) continue;
            string matName = materials[i].name.ToLower();
            if (matName.Contains("lips"))
                materials[i].color = lipColor;
            else if (matName.Contains("fingernail") || matName.Contains("nail"))
                materials[i].color = nailColor;
        }
        if (Application.isPlaying) Body.materials = materials;
    }

    private Color GetLipColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:           return lipsColorForWork;
            case OutfitType.WorkTwo:        return lipsColorForWorkTwo;
            case OutfitType.WorkThree:      return lipsColorForWorkThree;
            case OutfitType.WorkFour:       return lipsColorForWorkFour;
            case OutfitType.Casual:         return lipsColorForCasual;
            case OutfitType.CuteTwo:        return lipsColorForCuteTwo;
            case OutfitType.Casual3:        return lipsColorForCasualThree;
            case OutfitType.Edea:           return lipsColorForEdea;
            case OutfitType.CuteSeven:      return lipsColorForCuteSeven;
            case OutfitType.Fitness:        return lipsColorForFitness;
            case OutfitType.CuteEight:      return lipsColorForCuteEight;
            case OutfitType.CuteFour:       return lipsColorForCuteFour;
            case OutfitType.Essie:          return lipsColorForEssie;
            case OutfitType.NightOutFour:   return lipsColorForNightOutFour;
            case OutfitType.ElegantDress:   return lipsColorForElegantDress;
            case OutfitType.Undergarments:  return lipsColorForUndergarments;
            case OutfitType.CuteThree:      return lipsColorForCuteThree;
            case OutfitType.CuteFive:       return lipsColorForCuteFive;
            case OutfitType.CuteSix:        return lipsColorForFirstDate;
            case OutfitType.Date2:          return lipsColorForCuteSix;
            case OutfitType.Date3:          return lipsColorForThirdDate;
            case OutfitType.CuteOne:        return lipsColorForCuteOne;
            case OutfitType.Pyjamas:        return lipsColorForPyjamas;
            case OutfitType.Housecoat:      return lipsColorForHousecoat;
            case OutfitType.RuffleBlousseAndSkirt:   return lipsColorForRuffleBlousseAndSkirt;
            case OutfitType.NightOutRuffle:   return lipsColorForNightOutRuffle;
            case OutfitType.NewOutfitThree: return lipsColorForNewOutfitThree;
            case OutfitType.NewOutfitFour:  return lipsColorForNewOutfitFour;
            case OutfitType.NewOutfitFive:  return lipsColorForNewOutfitFive;
            case OutfitType.NewOutfitSix:   return lipsColorForNewOutfitSix;
            case OutfitType.NewOutfitSeven: return lipsColorForNewOutfitSeven;
            case OutfitType.NewOutfitEight: return lipsColorForNewOutfitEight;
            case OutfitType.NewOutfitNine:  return lipsColorForNewOutfitNine;
            case OutfitType.NewOutfitTen:   return lipsColorForNewOutfitTen;
            case OutfitType.Homelessness:   return lipsColorForHomelessness;
            case OutfitType.Wedding:        return lipsColorForWedding;
            case OutfitType.Funeral:        return lipsColorForFuneral;
            default:                        return Color.white;
        }
    }

    private Color GetNailColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:           return nailsColorForWork;
            case OutfitType.WorkTwo:        return nailsColorForWorkTwo;
            case OutfitType.WorkThree:      return nailsColorForWorkThree;
            case OutfitType.WorkFour:       return nailsColorForWorkFour;
            case OutfitType.Casual:         return nailsColorForCasual;
            case OutfitType.CuteTwo:        return nailsColorForCuteTwo;
            case OutfitType.Casual3:        return nailsColorForCasualThree;
            case OutfitType.Edea:           return nailsColorForEdea;
            case OutfitType.CuteSeven:      return nailsColorForCuteSeven;
            case OutfitType.Fitness:        return nailsColorForFitness;
            case OutfitType.CuteEight:      return nailsColorForCuteEight;
            case OutfitType.CuteFour:       return nailsColorForCuteFour;
            case OutfitType.Essie:          return nailsColorForEssie;
            case OutfitType.NightOutFour:   return nailsColorForNightOutFour;
            case OutfitType.ElegantDress:   return nailsColorForElegantDress;
            case OutfitType.Undergarments:  return nailsColorForUndergarments;
            case OutfitType.CuteThree:      return nailsColorForCuteThree;
            case OutfitType.CuteFive:       return nailsColorForCuteFive;
            case OutfitType.CuteSix:        return nailsColorForFirstDate;
            case OutfitType.Date2:          return nailsColorForCuteSix;
            case OutfitType.Date3:          return nailsColorForThirdDate;
            case OutfitType.CuteOne:        return nailsColorForCuteOne;
            case OutfitType.Pyjamas:        return nailsColorForPyjamas;
            case OutfitType.Housecoat:      return nailsColorForHousecoat;
            case OutfitType.RuffleBlousseAndSkirt:   return nailsColorForRuffleBlousseAndSkirt;
            case OutfitType.NightOutRuffle:   return nailsColorForNightOutRuffle;
            case OutfitType.NewOutfitThree: return nailsColorForNewOutfitThree;
            case OutfitType.NewOutfitFour:  return nailsColorForNewOutfitFour;
            case OutfitType.NewOutfitFive:  return nailsColorForNewOutfitFive;
            case OutfitType.NewOutfitSix:   return nailsColorForNewOutfitSix;
            case OutfitType.NewOutfitSeven: return nailsColorForNewOutfitSeven;
            case OutfitType.NewOutfitEight: return nailsColorForNewOutfitEight;
            case OutfitType.NewOutfitNine:  return nailsColorForNewOutfitNine;
            case OutfitType.NewOutfitTen:   return nailsColorForNewOutfitTen;
            case OutfitType.Homelessness:   return nailsColorForHomelessness;
            case OutfitType.Wedding:        return nailsColorForWedding;
            case OutfitType.Funeral:        return nailsColorForFuneral;
            default:                        return Color.white;
        }
    }

    public void DisableAllMainOutfits()
    {
        SetListEnabled(OutfitForWork, false);
        SetListEnabled(OutfitForWorkTwo, false);
        SetListEnabled(OutfitForWorkThree, false);
        SetListEnabled(OutfitForWorkFour, false);
        SetListEnabled(OutfitForCasual, false);
        SetListEnabled(OutfitForCuteTwo, false);
        SetListEnabled(OutfitForCasualThree, false);
        SetListEnabled(OutfitForEdea, false);
        SetListEnabled(OutfitForCuteSeven, false);
        SetListEnabled(OutfitForFitness, false);
        SetListEnabled(OutfitForCuteEight, false);
        SetListEnabled(OutfitForCuteFour, false);
        SetListEnabled(OutfitForEssie, false);
        SetListEnabled(OutfitForNightOutFour, false);
        SetListEnabled(OutfitForElegantDress, false);
        SetListEnabled(OutfitForUndergarments, false);
        SetListEnabled(OutfitForCuteThree, false);
        SetListEnabled(OutfitForCuteFive, false);
        SetListEnabled(OutfitForFirstDate, false);
        SetListEnabled(OutfitForCuteSix, false);
        SetListEnabled(OutfitForThirdDate, false);
        SetListEnabled(OutfitForCuteOne, false);
        SetListEnabled(OutfitForPyjamas, false);
        SetListEnabled(OutfitForHousecoat, false);
        SetListEnabled(OutfitForRuffleBlousseAndSkirt, false);
        SetListEnabled(OutfitForNightOutRuffle, false);
        SetListEnabled(OutfitForNewOutfitThree, false);
        SetListEnabled(OutfitForNewOutfitFour, false);
        SetListEnabled(OutfitForNewOutfitFive, false);
        SetListEnabled(OutfitForNewOutfitSix, false);
        SetListEnabled(OutfitForNewOutfitSeven, false);
        SetListEnabled(OutfitForNewOutfitEight, false);
        SetListEnabled(OutfitForNewOutfitNine, false);
        SetListEnabled(OutfitForNewOutfitTen, false);
        SetListEnabled(OutfitForHomelessness, false);
        SetListEnabled(OutfitForWedding, false);
        SetListEnabled(OutfitForFuneral, false);
        currentOutfit = OutfitType.None;
        if (DefaultHair != null) DefaultHair.enabled = true;
    }

    private void SetListEnabled(List<SkinnedMeshRenderer> list, bool enabled)
    {
        if (list == null) return;
        foreach (var renderer in list)
            if (renderer != null)
                renderer.enabled = enabled;
    }

    public void Undress()
    {
        DisableAllMainOutfits();
    }
}

public enum OutfitType
{
    None,
    Work,
    WorkTwo,
    WorkThree,
    WorkFour,
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
    Housecoat,
    RuffleBlousseAndSkirt,
    NightOutRuffle,
    NewOutfitThree,
    NewOutfitFour,
    NewOutfitFive,
    NewOutfitSix,
    NewOutfitSeven,
    NewOutfitEight,
    NewOutfitNine,
    NewOutfitTen,
    Homelessness,
    Wedding,
    Funeral,
    NightOutFour,
    ElegantDress,
    Undergarments
}

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
        if (GUILayout.Button("Work",       GUILayout.Height(35))) { me.ToggleWorkOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Work Two",   GUILayout.Height(35))) { me.ToggleWorkTwoOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Work Three", GUILayout.Height(35))) { me.ToggleWorkThreeOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Work Four",  GUILayout.Height(35))) { me.ToggleWorkFourOutfit();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Casual
        EditorGUILayout.LabelField("Casual Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Casual One", GUILayout.Height(35))) { me.ToggleCasualOutfit();    EditorUtility.SetDirty(me); }
        if (GUILayout.Button("First Date", GUILayout.Height(35))) { me.ToggleFirstDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fitness",    GUILayout.Height(35))) { me.ToggleFitnessOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Pyjamas",    GUILayout.Height(35))) { me.TogglePyjamasOutfit();   EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Housecoat",        GUILayout.Height(35))) { me.ToggleHousecoatOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Ruffle Blousse & Skirt",   GUILayout.Height(35))) { me.ToggleRuffleBlousseAndSkirtOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Night Out Ruffle Dress",   GUILayout.Height(35))) { me.ToggleNightOutRuffleOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("New Outfit Three", GUILayout.Height(35))) { me.ToggleNewOutfitThreeOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New Outfit Four",  GUILayout.Height(35))) { me.ToggleNewOutfitFourOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("New Outfit Five",  GUILayout.Height(35))) { me.ToggleNewOutfitFiveOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("New Outfit Six",   GUILayout.Height(35))) { me.ToggleNewOutfitSixOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("New Outfit Seven", GUILayout.Height(35))) { me.ToggleNewOutfitSevenOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New Outfit Eight", GUILayout.Height(35))) { me.ToggleNewOutfitEightOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("New Outfit Nine",  GUILayout.Height(35))) { me.ToggleNewOutfitNineOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("New Outfit Ten",   GUILayout.Height(35))) { me.ToggleNewOutfitTenOutfit();   EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Cute
        EditorGUILayout.LabelField("Cute Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cute One",   GUILayout.Height(35))) { me.ToggleCuteOneOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Two",   GUILayout.Height(35))) { me.ToggleCuteTwoOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Three", GUILayout.Height(35))) { me.ToggleCuteThreeOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Four",  GUILayout.Height(35))) { me.ToggleCuteFourOutfit();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cute Five",  GUILayout.Height(35))) { me.ToggleCuteFiveOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Six",   GUILayout.Height(35))) { me.ToggleCuteSixOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Seven", GUILayout.Height(35))) { me.ToggleCuteSevenOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Eight", GUILayout.Height(35))) { me.ToggleCuteEightOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Edea",       GUILayout.Height(35))) { me.ToggleEdeaOutfit();      EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Storyline
        EditorGUILayout.LabelField("Storyline Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wedding",  GUILayout.Height(35))) { me.ToggleWeddingOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Funeral",  GUILayout.Height(35))) { me.ToggleFuneralOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Skid Row", GUILayout.Height(35))) { me.ToggleHomelessnessOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Night Out
        EditorGUILayout.LabelField("Night Out Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Essie",          GUILayout.Height(35))) { me.ToggleEssieOutfit();        EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Third Date",     GUILayout.Height(35))) { me.ToggleThirdDateOutfit();    EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casual Three",   GUILayout.Height(35))) { me.ToggleCasualThreeOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Night Out Four", GUILayout.Height(35))) { me.ToggleNightOutFourOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Elegant Dress",  GUILayout.Height(35))) { me.ToggleElegantDressOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("────────────────────────────────────────────────────────────", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Accessories
        EditorGUILayout.LabelField("Accessories (toggle independently)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Wings",   GUILayout.Height(35))) { me.ToggleWings();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Overall", GUILayout.Height(35))) { me.ToggleOverall(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Hat",     GUILayout.Height(35))) { me.ToggleHat();     EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("────────────────────────────────────────────────────────────", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Undergarments
        EditorGUILayout.LabelField("Undergarments", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Underwear", GUILayout.Height(35))) { me.ToggleUndergarments(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Nothin' At All",   GUILayout.Height(35))) { me.Undress();             EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("────────────────────────────────────────────────────────────", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Random
        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random All",  GUILayout.Height(40))) { me.SetRandomOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Work", GUILayout.Height(40))) { me.SetRandomWorkOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Casual",    GUILayout.Height(40))) { me.SetRandomCasualOutfit();    EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Cute",      GUILayout.Height(40))) { me.SetRandomCuteOutfit();      EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Storyline", GUILayout.Height(40))) { me.SetRandomStorylineOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Night Out", GUILayout.Height(40))) { me.SetRandomNightOutOutfit();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
#endif