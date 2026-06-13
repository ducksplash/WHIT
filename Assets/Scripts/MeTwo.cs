using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeTwo : MonoBehaviour
{
    [Header("Character Root")]
    public GameObject SkinnedMeshRendererParent;

    [Header("Body")]
    public SkinnedMeshRenderer Body;

    [Header("Accessories")]
    public SkinnedMeshRenderer ButterflyWings;
    public SkinnedMeshRenderer Overall;
    public SkinnedMeshRenderer Hat;
    public SkinnedMeshRenderer Choker;

    [Header("Default Hair")]
    public SkinnedMeshRenderer DefaultHair;

    [Header("Outfits")]

    [Header("Work")]
    [Header("Classic Nora Dress")]
    public bool WorkOneOutfitEnabled = true;
    public List<GameObject> OutfitForWorkOne;
    public Color lipsColorForWorkOne = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkOne = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkOne;
    public bool WingsEnabledForWorkOne;
    public bool ApronEnabledForWorkOne;
    public bool HatEnabledForWorkOne;
    public bool ChokerEnabledForWorkOne;

    [Header("Black Skirt & Cream Shirt")]
    public bool WorkTwoOutfitEnabled = true;
    public List<GameObject> OutfitForWorkOneTwo;
    public Color lipsColorForWorkTwo = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkTwo = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkTwo;
    public bool WingsEnabledForWorkTwo;
    public bool ApronEnabledForWorkTwo;
    public bool HatEnabledForWorkTwo;
    public bool ChokerEnabledForWorkTwo;

    [Header("Tartan & Grey Dress")]
    public bool WorkThreeOutfitEnabled = true;
    public List<GameObject> OutfitForWorkOneThree;
    public Color lipsColorForWorkThree = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkThree = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkThree;
    public bool WingsEnabledForWorkThree;
    public bool ApronEnabledForWorkThree;
    public bool HatEnabledForWorkThree;
    public bool ChokerEnabledForWorkThree;

    [Header("Black Suit & White Shirt")]
    public bool WorkFourOutfitEnabled = true;
    public List<GameObject> OutfitForWorkOneFour;
    public Color lipsColorForWorkFour = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkFour = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkFour;
    public bool WingsEnabledForWorkFour;
    public bool ApronEnabledForWorkFour;
    public bool HatEnabledForWorkFour;
    public bool ChokerEnabledForWorkFour;

    [Header("Work Suit Three")]
    public bool WorkSuitThreeEnabled = true;
    public List<GameObject> OutfitForWorkSuitThree;
    public Color lipsColorForWorkSuitThree = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForWorkSuitThree = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleWorkSuitThree;
    public bool WingsEnabledForWorkSuitThree;
    public bool ApronEnabledForWorkSuitThree;
    public bool HatEnabledForWorkSuitThree;
    public bool ChokerEnabledForWorkSuitThree;

    [Header("Casual Stuff")]
    [Header("Uni Sweater & Jeans")]
    public bool CasualOutfitEnabled = true;
    public List<GameObject> OutfitForCasual;
    public Color lipsColorForCasual = new Color(0.85f, 0.25f, 0.35f);
    public Color nailsColorForCasual = new Color(0.92f, 0.75f, 0.8f);
    public bool JiggleCasually;
    public bool WingsEnabledForCasual;
    public bool ApronEnabledForCasual;
    public bool HatEnabledForCasual;
    public bool ChokerEnabledForCasual;

    [Header("Shorts & Tights")]
    public bool ShortsAndTightsEnabled = true;
    public List<GameObject> OutfitForShortsAndTights;
    public Color lipsColorForShortsAndTights = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForShortsAndTights = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleShortsAndTights;
    public bool WingsEnabledForShortsAndTights;
    public bool ApronEnabledForShortsAndTights;
    public bool HatEnabledForShortsAndTights;
    public bool ChokerEnabledForShortsAndTights;

    [Header("Fitness")]
    public bool FitnessOutfitEnabled = true;
    public List<GameObject> OutfitForFitness;
    public Color lipsColorForFitness = new Color(0.92f, 0.28f, 0.38f);
    public Color nailsColorForFitness = new Color(0.9f, 0.75f, 0.8f);
    public bool JiggleFitness;
    public bool WingsEnabledForFitness;
    public bool ApronEnabledForFitness;
    public bool HatEnabledForFitness;
    public bool ChokerEnabledForFitness;

    [Header("Pyjamas")]
    [Header("Flannels")]
    public bool PyjamasOutfitEnabled = true;
    public List<GameObject> OutfitForPyjamas;
    public Color lipsColorForPyjamas = new Color(0.8f, 0.15f, 0.25f);
    public Color nailsColorForPyjamas = new Color(0.85f, 0.6f, 0.7f);
    public bool JiggleInPyjamas;
    public bool WingsEnabledForPyjamas;
    public bool ApronEnabledForPyjamas;
    public bool HatEnabledForPyjamas;
    public bool ChokerEnabledForPyjamas;

    [Header("Housecoat")]
    public bool HousecoatOutfitEnabled = true;
    public List<GameObject> OutfitForHousecoat;
    public Color lipsColorForHousecoat = new Color(0.82f, 0.18f, 0.28f);
    public Color nailsColorForHousecoat = new Color(0.88f, 0.65f, 0.72f);
    public bool JiggleInHousecoat;
    public bool WingsEnabledForHousecoat;
    public bool ApronEnabledForHousecoat;
    public bool HatEnabledForHousecoat;
    public bool ChokerEnabledForHousecoat;

    [Header("Nightie")]
    public bool NightieEnabled = true;
    public List<GameObject> OutfitForNightie;
    public Color lipsColorForNightie = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNightie = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNightie;
    public bool WingsEnabledForNightie;
    public bool ApronEnabledForNightie;
    public bool HatEnabledForNightie;
    public bool ChokerEnabledForNightie;

    [Header("Dating")]
    [Header("First Date")]
    public bool FirstDateOutfitEnabled = true;
    public List<GameObject> OutfitForFirstDate;
    public Color lipsColorForFirstDate = new Color(0.9f, 0.3f, 0.4f);
    public Color nailsColorForFirstDate = new Color(0.95f, 0.82f, 0.87f);
    public bool JiggleOnAFirstDate;
    public bool WingsEnabledForFirstDate;
    public bool ApronEnabledForFirstDate;
    public bool HatEnabledForFirstDate;
    public bool ChokerEnabledForFirstDate;

    [Header("Second Date")]
    public bool SecondDateOutfitEnabled = true;
    public List<GameObject> OutfitForSecondDate;
    public Color lipsColorForSecondDate = new Color(0.96f, 0.4f, 0.5f);
    public Color nailsColorForSecondDate = new Color(0.97f, 0.88f, 0.9f);
    public bool JiggleOnASecondDate;
    public bool WingsEnabledForSecondDate;
    public bool ApronEnabledForSecondDate;
    public bool HatEnabledForSecondDate;
    public bool ChokerEnabledForSecondDate;

    [Header("Third Date")]
    public bool ThirdDateOutfitEnabled = true;
    public List<GameObject> OutfitForThirdDate;
    public Color lipsColorForThirdDate = new Color(0.85f, 0.22f, 0.32f);
    public Color nailsColorForThirdDate = new Color(0.94f, 0.8f, 0.85f);
    public bool JiggleOnAThirdDate;
    public bool WingsEnabledForThirdDate;
    public bool ApronEnabledForThirdDate;
    public bool HatEnabledForThirdDate;
    public bool ChokerEnabledForThirdDate;

    [Header("Cute")]
    [Header("Cute One")]
    public bool CuteOneOutfitEnabled = true;
    public List<GameObject> OutfitForCuteOne;
    public Color lipsColorForCuteOne = new Color(0.98f, 0.45f, 0.55f);
    public Color nailsColorForCuteOne = new Color(0.98f, 0.9f, 0.92f);
    public bool JiggleOnACuteOne;
    public bool WingsEnabledForCuteOne;
    public bool ApronEnabledForCuteOne;
    public bool HatEnabledForCuteOne;
    public bool ChokerEnabledForCuteOne;

    [Header("Cute Two")]
    public bool CuteTwoOutfitEnabled = true;
    public List<GameObject> OutfitForCuteTwo;
    public Color lipsColorForCuteTwo = new Color(0.8f, 0.15f, 0.25f);
    public Color nailsColorForCuteTwo = new Color(0.88f, 0.65f, 0.75f);
    public bool JiggleCasuallyTwo;
    public bool WingsEnabledForCuteTwo;
    public bool ApronEnabledForCuteTwo;
    public bool HatEnabledForCuteTwo;
    public bool ChokerEnabledForCuteTwo;

    [Header("Cute Three")]
    public bool CuteThreeOutfitEnabled = true;
    public List<GameObject> OutfitForCuteThree;
    public Color lipsColorForCuteThree = new Color(0.95f, 0.35f, 0.45f);
    public Color nailsColorForCuteThree = new Color(0.96f, 0.85f, 0.88f);
    public bool JiggleCuteThree;
    public bool WingsEnabledForCuteThree;
    public bool ApronEnabledForCuteThree;
    public bool HatEnabledForCuteThree;
    public bool ChokerEnabledForCuteThree;

    [Header("Cute Four")]
    public bool CuteFourOutfitEnabled = true;
    public List<GameObject> OutfitForCuteFour;
    public Color lipsColorForCuteFour = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForCuteFour = new Color(0.94f, 0.8f, 0.85f);
    public bool JiggleCuteFour;
    public bool WingsEnabledForCuteFour;
    public bool ApronEnabledForCuteFour;
    public bool HatEnabledForCuteFour;
    public bool ChokerEnabledForCuteFour;

    [Header("Cute Five")]
    public bool CuteFiveOutfitEnabled = true;
    public List<GameObject> OutfitForCuteFive;
    public Color lipsColorForCuteFive = new Color(0.88f, 0.2f, 0.3f);
    public Color nailsColorForCuteFive = new Color(0.93f, 0.78f, 0.83f);
    public bool JiggleCuteFive;
    public bool WingsEnabledForCuteFive;
    public bool ApronEnabledForCuteFive;
    public bool HatEnabledForCuteFive;
    public bool ChokerEnabledForCuteFive;

    [Header("Cute Six")]
    public bool RuffleBlousseAndSkirtEnabled = true;
    public List<GameObject> OutfitForRuffleBlousseAndSkirt;
    public Color lipsColorForRuffleBlousseAndSkirt = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForRuffleBlousseAndSkirt = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleRuffleBlousseAndSkirt;
    public bool WingsEnabledForRuffleBlousseAndSkirt;
    public bool ApronEnabledForRuffleBlousseAndSkirt;
    public bool HatEnabledForRuffleBlousseAndSkirt;
    public bool ChokerEnabledForRuffleBlousseAndSkirt;

    [Header("Cute Seven")]
    public bool CuteSevenOutfitEnabled = true;
    public List<GameObject> OutfitForCuteSeven;
    public Color lipsColorForCuteSeven = new Color(0.88f, 0.22f, 0.32f);
    public Color nailsColorForCuteSeven = new Color(0.95f, 0.82f, 0.85f);
    public bool JiggleCuteSeven;
    public bool WingsEnabledForCuteSeven;
    public bool ApronEnabledForCuteSeven;
    public bool HatEnabledForCuteSeven;
    public bool ChokerEnabledForCuteSeven;

    [Header("Cute Eight")]
    public bool CuteEightOutfitEnabled = true;
    public List<GameObject> OutfitForCuteEight;
    public Color lipsColorForCuteEight = new Color(0.85f, 0.18f, 0.28f);
    public Color nailsColorForCuteEight = new Color(0.88f, 0.68f, 0.73f);
    public bool JiggleCuteEight;
    public bool WingsEnabledForCuteEight;
    public bool ApronEnabledForCuteEight;
    public bool HatEnabledForCuteEight;
    public bool ChokerEnabledForCuteEight;

    [Header("Wooly Jumper & Tights")]
    public bool WoolenJumperEnabled = true;
    public List<GameObject> OutfitForWoolenJumper;
    public Color lipsColorForWoolenJumper = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForWoolenJumper = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleWoolenJumper;
    public bool WingsEnabledForWoolenJumper;
    public bool ApronEnabledForWoolenJumper;
    public bool HatEnabledForWoolenJumper;
    public bool ChokerEnabledForWoolenJumper;

    [Header("Edea")]
    public bool EdeaOutfitEnabled = true;
    public List<GameObject> OutfitForEdea;
    public Color lipsColorForEdea = new Color(0.75f, 0.1f, 0.2f);
    public Color nailsColorForEdea = new Color(0.9f, 0.7f, 0.75f);
    public bool JiggleCasuallyFour;
    public bool WingsEnabledForEdea;
    public bool ApronEnabledForEdea;
    public bool HatEnabledForEdea;
    public bool ChokerEnabledForEdea;

    [Header("Ditzy Dress")]
    public bool DitzyDressEnabled = true;
    public List<GameObject> OutfitForDitzyDress;
    public Color lipsColorForDitzyDress = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForDitzyDress = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleDitzyDress;
    public bool WingsEnabledForDitzyDress;
    public bool ApronEnabledForDitzyDress;
    public bool HatEnabledForDitzyDress;
    public bool ChokerEnabledForDitzyDress;

    [Header("Little Black Dress")]
    public bool LittleBlackDressEnabled = true;
    public List<GameObject> OutfitForLittleBlackDress;
    public Color lipsColorForLittleBlackDress = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForLittleBlackDress = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleLittleBlackDress;
    public bool WingsEnabledForLittleBlackDress;
    public bool ApronEnabledForLittleBlackDress;
    public bool HatEnabledForLittleBlackDress;
    public bool ChokerEnabledForLittleBlackDress;

    [Header("Night Out")]
    [Header("Froot Dress")]
    public bool NightOutOneOutfitEnabled = true;
    public List<GameObject> OutfitForNightOutOne;
    public Color lipsColorForNightOutOne = new Color(0.9f, 0.3f, 0.4f);
    public Color nailsColorForNightOutOne = new Color(0.93f, 0.78f, 0.82f);
    public bool JiggleCasuallyThree;
    public bool WingsEnabledForNightOutOne;
    public bool ApronEnabledForNightOutOne;
    public bool HatEnabledForNightOutOne;
    public bool ChokerEnabledForNightOutOne;

    [Header("Essie")]
    public bool EssieOutfitEnabled = true;
    public List<GameObject> OutfitForEssie;
    public Color lipsColorForEssie = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForEssie = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleEssie;
    public bool WingsEnabledForEssie;
    public bool ApronEnabledForEssie;
    public bool HatEnabledForEssie;
    public bool ChokerEnabledForEssie;

    [Header("Night Out Four")]
    public bool NightOutFourOutfitEnabled = true;
    public List<GameObject> OutfitForNightOutFour;
    public Color lipsColorForNightOutFour = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForNightOutFour = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleNightOutFour;
    public bool WingsEnabledForNightOutFour;
    public bool ApronEnabledForNightOutFour;
    public bool HatEnabledForNightOutFour;
    public bool ChokerEnabledForNightOutFour;

    [Header("Elegant Dress")]
    public bool ElegantDressOutfitEnabled = true;
    public List<GameObject> OutfitForElegantDress;
    public Color lipsColorForElegantDress = new Color(0.78f, 0.08f, 0.18f);
    public Color nailsColorForElegantDress = new Color(0.92f, 0.76f, 0.82f);
    public bool JiggleElegantDress;
    public bool WingsEnabledForElegantDress;
    public bool ApronEnabledForElegantDress;
    public bool HatEnabledForElegantDress;
    public bool ChokerEnabledForElegantDress;

    [Header("Layered Ruffle Dress")]
    public bool NightOutRuffleEnabled = true;
    public List<GameObject> OutfitForNightOutRuffle;
    public Color lipsColorForNightOutRuffle = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForNightOutRuffle = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleNightOutRuffle;
    public bool WingsEnabledForNightOutRuffle;
    public bool ApronEnabledForNightOutRuffle;
    public bool HatEnabledForNightOutRuffle;
    public bool ChokerEnabledForNightOutRuffle;

    [Header("Storyline")]
    [Header("Wedding")]
    public bool WeddingOutfitEnabled = true;
    public List<GameObject> OutfitForWedding;
    public Color lipsColorForWedding = new Color(0.95f, 0.55f, 0.6f);
    public Color nailsColorForWedding = new Color(0.96f, 0.88f, 0.9f);
    public bool JiggleAtAWedding;
    public bool WingsEnabledForWedding;
    public bool ApronEnabledForWedding;
    public bool HatEnabledForWedding;
    public bool ChokerEnabledForWedding;

    [Header("Funeral")]
    public bool FuneralOutfitEnabled = true;
    public List<GameObject> OutfitForFuneral;
    public Color lipsColorForFuneral = new Color(0.65f, 0.08f, 0.15f);
    public Color nailsColorForFuneral = new Color(0.7f, 0.5f, 0.55f);
    public bool JiggleAtAFuneral;
    public bool WingsEnabledForFuneral;
    public bool ApronEnabledForFuneral;
    public bool HatEnabledForFuneral;
    public bool ChokerEnabledForFuneral;

    [Header("On Skid Row")]
    public bool HomelessnessOutfitEnabled = true;
    public List<GameObject> OutfitForHomelessness;
    public Color lipsColorForHomelessness = new Color(0.7f, 0.1f, 0.2f);
    public Color nailsColorForHomelessness = new Color(0.75f, 0.55f, 0.6f);
    public bool JiggleWhileHomeless;
    public bool WingsEnabledForHomelessness;
    public bool ApronEnabledForHomelessness;
    public bool HatEnabledForHomelessness;
    public bool ChokerEnabledForHomelessness;

    [Header("Underwear")]
    [Header("Undergarments")]
    public List<GameObject> OutfitForUndergarments;
    public Color lipsColorForUndergarments = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForUndergarments = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleUndergarments;
    public bool WingsEnabledForUndergarments;
    public bool ApronEnabledForUndergarments;
    public bool HatEnabledForUndergarments;
    public bool ChokerEnabledForUndergarments;

    [Header("Lingerie")]
    public bool LingerieEnabled = true;
    public List<GameObject> OutfitForLingerie;
    public Color lipsColorForLingerie = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForLingerie = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleLingerie;
    public bool WingsEnabledForLingerie;
    public bool ApronEnabledForLingerie;
    public bool HatEnabledForLingerie;
    public bool ChokerEnabledForLingerie;

    [Header("Fae")]
    public bool FaeEnabled = true;
    public List<GameObject> OutfitForFae;
    public Color lipsColorForFae = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForFae = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleFae;
    public bool WingsEnabledForFae;
    public bool ApronEnabledForFae;
    public bool HatEnabledForFae;
    public bool ChokerEnabledForFae;

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

    private bool _wingsOverride   = false;
    private bool _overallOverride = false;
    private bool _hatOverride     = false;
    private bool _chokerOverride  = false;

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
        if (holdToUnlock    != null) holdToUnlock.action.performed    += OnToggleUndergarments;
        if (holdToUnlockAll != null) holdToUnlockAll.action.performed += OnToggleNothinatall;
        if (nextOutfit      != null) nextOutfit.action.performed      += OnNextOutfit;
        if (previousOutfit  != null) previousOutfit.action.performed  += OnPreviousOutfit;
    }

    private void OnToggleUndergarments(InputAction.CallbackContext ctx) => ToggleUndergarments();
    private void OnToggleNothinatall(InputAction.CallbackContext ctx)   => Undress();
    private void OnNextOutfit(InputAction.CallbackContext ctx)          => NextOutfit();
    private void OnPreviousOutfit(InputAction.CallbackContext ctx)      => PreviousOutfit();

    private void OnDisable()
    {
        if (npcController == null)
        {
            if (nextOutfit      != null) nextOutfit.action.performed      -= OnNextOutfit;
            if (previousOutfit  != null) previousOutfit.action.performed  -= OnPreviousOutfit;
            if (holdToUnlock    != null) holdToUnlock.action.performed    -= OnToggleUndergarments;
            if (holdToUnlockAll != null) holdToUnlockAll.action.performed -= OnToggleNothinatall;
        }
    }

    // ====================== INSTANTIATE / DESTROY ======================
    private void ClearOutfitMeshes()
    {
        if (SkinnedMeshRendererParent == null) return;
        for (int i = SkinnedMeshRendererParent.transform.childCount - 1; i >= 0; i--)
        {
            #if UNITY_EDITOR
            
            DestroyImmediate(SkinnedMeshRendererParent.transform.GetChild(i).gameObject);
            #else
            
            Destroy(SkinnedMeshRendererParent.transform.GetChild(i).gameObject);

            #endif
        }
    }


    
    // private void RebindAllSkinnedMeshes(Transform parent)
    // {
    //     if (Body == null) return;
    //
    //     Transform rootBone = Body.rootBone;
    //     Transform[] bones = Body.bones;
    //
    //     foreach (var smr in parent.GetComponentsInChildren<SkinnedMeshRenderer>(true))
    //     {
    //         smr.rootBone = rootBone;
    //         smr.bones = bones;
    //         smr.updateWhenOffscreen = true;
    //     }
    // }
    
    
    // ====================== ENABLED CHECK ======================
    public bool IsOutfitEnabled(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:             return WorkOneOutfitEnabled;
            case OutfitType.WorkTwo:          return WorkTwoOutfitEnabled;
            case OutfitType.WorkThree:        return WorkThreeOutfitEnabled;
            case OutfitType.WorkFour:         return WorkFourOutfitEnabled;
            case OutfitType.WorkSuitThree:    return WorkSuitThreeEnabled;
            case OutfitType.Casual:           return CasualOutfitEnabled;
            case OutfitType.ShortsAndTights:  return ShortsAndTightsEnabled;
            case OutfitType.Fitness:          return FitnessOutfitEnabled;
            case OutfitType.Pyjamas:          return PyjamasOutfitEnabled;
            case OutfitType.Housecoat:        return HousecoatOutfitEnabled;
            case OutfitType.Nightie:          return NightieEnabled;
            case OutfitType.Date1:            return SecondDateOutfitEnabled;
            case OutfitType.Date2:            return FirstDateOutfitEnabled;
            case OutfitType.Date3:            return ThirdDateOutfitEnabled;
            case OutfitType.CuteOne:          return CuteOneOutfitEnabled;
            case OutfitType.CuteTwo:          return CuteTwoOutfitEnabled;
            case OutfitType.CuteThree:        return CuteThreeOutfitEnabled;
            case OutfitType.CuteFour:         return CuteFourOutfitEnabled;
            case OutfitType.CuteFive:         return CuteFiveOutfitEnabled;
            case OutfitType.CuteSix:          return RuffleBlousseAndSkirtEnabled;
            case OutfitType.CuteSeven:        return CuteSevenOutfitEnabled;
            case OutfitType.CuteEight:        return CuteEightOutfitEnabled;
            case OutfitType.WoolenJumper:     return WoolenJumperEnabled;
            case OutfitType.Edea:             return EdeaOutfitEnabled;
            case OutfitType.DitzyDress:       return DitzyDressEnabled;
            case OutfitType.LittleBlackDress: return LittleBlackDressEnabled;
            case OutfitType.Casual3:          return NightOutOneOutfitEnabled;
            case OutfitType.Essie:            return EssieOutfitEnabled;
            case OutfitType.NightOutFour:     return NightOutFourOutfitEnabled;
            case OutfitType.ElegantDress:     return ElegantDressOutfitEnabled;
            case OutfitType.NightOutRuffle:   return NightOutRuffleEnabled;
            case OutfitType.Wedding:          return WeddingOutfitEnabled;
            case OutfitType.Funeral:          return FuneralOutfitEnabled;
            case OutfitType.Homelessness:     return HomelessnessOutfitEnabled;
            case OutfitType.Lingerie:         return LingerieEnabled;
            case OutfitType.Fae:              return FaeEnabled;
            default:                          return true;
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

    public void ToggleChoker(bool? forceOn = null)
    {
        _chokerOverride = forceOn.HasValue ? forceOn.Value : !_chokerOverride;
        ApplyAccessories();
    }

    private void ApplyAccessories()
    {
        GetOutfitAccessoryDefaults(currentOutfit, out bool wingsDefault, out bool overallDefault, out bool hatDefault, out bool chokerDefault);
        if (ButterflyWings != null) ButterflyWings.enabled = wingsDefault  || _wingsOverride;
        if (Overall        != null) Overall.enabled        = overallDefault || _overallOverride;
        if (Hat            != null) Hat.enabled            = hatDefault     || _hatOverride;
        if (Choker         != null) Choker.enabled         = chokerDefault  || _chokerOverride;
    }

    private void GetOutfitAccessoryDefaults(OutfitType outfit, out bool wings, out bool overall, out bool hat, out bool choker)
    {
        switch (outfit)
        {
            case OutfitType.Work:             wings = WingsEnabledForWorkOne;               overall = ApronEnabledForWorkOne;               hat = HatEnabledForWorkOne;               choker = ChokerEnabledForWorkOne;               break;
            case OutfitType.WorkTwo:          wings = WingsEnabledForWorkTwo;               overall = ApronEnabledForWorkTwo;               hat = HatEnabledForWorkTwo;               choker = ChokerEnabledForWorkTwo;               break;
            case OutfitType.WorkThree:        wings = WingsEnabledForWorkThree;             overall = ApronEnabledForWorkThree;             hat = HatEnabledForWorkThree;             choker = ChokerEnabledForWorkThree;             break;
            case OutfitType.WorkFour:         wings = WingsEnabledForWorkFour;              overall = ApronEnabledForWorkFour;              hat = HatEnabledForWorkFour;              choker = ChokerEnabledForWorkFour;              break;
            case OutfitType.WorkSuitThree:    wings = WingsEnabledForWorkSuitThree;         overall = ApronEnabledForWorkSuitThree;         hat = HatEnabledForWorkSuitThree;         choker = ChokerEnabledForWorkSuitThree;         break;
            case OutfitType.Casual:           wings = WingsEnabledForCasual;                overall = ApronEnabledForCasual;                hat = HatEnabledForCasual;                choker = ChokerEnabledForCasual;                break;
            case OutfitType.ShortsAndTights:  wings = WingsEnabledForShortsAndTights;       overall = ApronEnabledForShortsAndTights;       hat = HatEnabledForShortsAndTights;       choker = ChokerEnabledForShortsAndTights;       break;
            case OutfitType.Fitness:          wings = WingsEnabledForFitness;               overall = ApronEnabledForFitness;               hat = HatEnabledForFitness;               choker = ChokerEnabledForFitness;               break;
            case OutfitType.Pyjamas:          wings = WingsEnabledForPyjamas;               overall = ApronEnabledForPyjamas;               hat = HatEnabledForPyjamas;               choker = ChokerEnabledForPyjamas;               break;
            case OutfitType.Housecoat:        wings = WingsEnabledForHousecoat;             overall = ApronEnabledForHousecoat;             hat = HatEnabledForHousecoat;             choker = ChokerEnabledForHousecoat;             break;
            case OutfitType.Nightie:          wings = WingsEnabledForNightie;               overall = ApronEnabledForNightie;               hat = HatEnabledForNightie;               choker = ChokerEnabledForNightie;               break;
            case OutfitType.Date1:            wings = WingsEnabledForSecondDate;            overall = ApronEnabledForSecondDate;            hat = HatEnabledForSecondDate;            choker = ChokerEnabledForSecondDate;            break;
            case OutfitType.Date2:            wings = WingsEnabledForFirstDate;             overall = ApronEnabledForFirstDate;             hat = HatEnabledForFirstDate;             choker = ChokerEnabledForFirstDate;             break;
            case OutfitType.Date3:            wings = WingsEnabledForThirdDate;             overall = ApronEnabledForThirdDate;             hat = HatEnabledForThirdDate;             choker = ChokerEnabledForThirdDate;             break;
            case OutfitType.CuteOne:          wings = WingsEnabledForCuteOne;               overall = ApronEnabledForCuteOne;               hat = HatEnabledForCuteOne;               choker = ChokerEnabledForCuteOne;               break;
            case OutfitType.CuteTwo:          wings = WingsEnabledForCuteTwo;               overall = ApronEnabledForCuteTwo;               hat = HatEnabledForCuteTwo;               choker = ChokerEnabledForCuteTwo;               break;
            case OutfitType.CuteThree:        wings = WingsEnabledForCuteThree;             overall = ApronEnabledForCuteThree;             hat = HatEnabledForCuteThree;             choker = ChokerEnabledForCuteThree;             break;
            case OutfitType.CuteFour:         wings = WingsEnabledForCuteFour;              overall = ApronEnabledForCuteFour;              hat = HatEnabledForCuteFour;              choker = ChokerEnabledForCuteFour;              break;
            case OutfitType.CuteFive:         wings = WingsEnabledForCuteFive;              overall = ApronEnabledForCuteFive;              hat = HatEnabledForCuteFive;              choker = ChokerEnabledForCuteFive;              break;
            case OutfitType.CuteSix:          wings = WingsEnabledForRuffleBlousseAndSkirt; overall = ApronEnabledForRuffleBlousseAndSkirt; hat = HatEnabledForRuffleBlousseAndSkirt; choker = ChokerEnabledForRuffleBlousseAndSkirt; break;
            case OutfitType.CuteSeven:        wings = WingsEnabledForCuteSeven;             overall = ApronEnabledForCuteSeven;             hat = HatEnabledForCuteSeven;             choker = ChokerEnabledForCuteSeven;             break;
            case OutfitType.CuteEight:        wings = WingsEnabledForCuteEight;             overall = ApronEnabledForCuteEight;             hat = HatEnabledForCuteEight;             choker = ChokerEnabledForCuteEight;             break;
            case OutfitType.WoolenJumper:     wings = WingsEnabledForWoolenJumper;          overall = ApronEnabledForWoolenJumper;          hat = HatEnabledForWoolenJumper;          choker = ChokerEnabledForWoolenJumper;          break;
            case OutfitType.Edea:             wings = WingsEnabledForEdea;                  overall = ApronEnabledForEdea;                  hat = HatEnabledForEdea;                  choker = ChokerEnabledForEdea;                  break;
            case OutfitType.DitzyDress:       wings = WingsEnabledForDitzyDress;            overall = ApronEnabledForDitzyDress;            hat = HatEnabledForDitzyDress;            choker = ChokerEnabledForDitzyDress;            break;
            case OutfitType.LittleBlackDress: wings = WingsEnabledForLittleBlackDress;      overall = ApronEnabledForLittleBlackDress;      hat = HatEnabledForLittleBlackDress;      choker = ChokerEnabledForLittleBlackDress;      break;
            case OutfitType.Casual3:          wings = WingsEnabledForNightOutOne;           overall = ApronEnabledForNightOutOne;           hat = HatEnabledForNightOutOne;           choker = ChokerEnabledForNightOutOne;           break;
            case OutfitType.Essie:            wings = WingsEnabledForEssie;                 overall = ApronEnabledForEssie;                 hat = HatEnabledForEssie;                 choker = ChokerEnabledForEssie;                 break;
            case OutfitType.NightOutFour:     wings = WingsEnabledForNightOutFour;          overall = ApronEnabledForNightOutFour;          hat = HatEnabledForNightOutFour;          choker = ChokerEnabledForNightOutFour;          break;
            case OutfitType.ElegantDress:     wings = WingsEnabledForElegantDress;          overall = ApronEnabledForElegantDress;          hat = HatEnabledForElegantDress;          choker = ChokerEnabledForElegantDress;          break;
            case OutfitType.NightOutRuffle:   wings = WingsEnabledForNightOutRuffle;        overall = ApronEnabledForNightOutRuffle;        hat = HatEnabledForNightOutRuffle;        choker = ChokerEnabledForNightOutRuffle;        break;
            case OutfitType.Wedding:          wings = WingsEnabledForWedding;               overall = ApronEnabledForWedding;               hat = HatEnabledForWedding;               choker = ChokerEnabledForWedding;               break;
            case OutfitType.Funeral:          wings = WingsEnabledForFuneral;               overall = ApronEnabledForFuneral;               hat = HatEnabledForFuneral;               choker = ChokerEnabledForFuneral;               break;
            case OutfitType.Homelessness:     wings = WingsEnabledForHomelessness;          overall = ApronEnabledForHomelessness;          hat = HatEnabledForHomelessness;          choker = ChokerEnabledForHomelessness;          break;
            case OutfitType.Undergarments:    wings = WingsEnabledForUndergarments;         overall = ApronEnabledForUndergarments;         hat = HatEnabledForUndergarments;         choker = ChokerEnabledForUndergarments;         break;
            case OutfitType.Lingerie:         wings = WingsEnabledForLingerie;              overall = ApronEnabledForLingerie;              hat = HatEnabledForLingerie;              choker = ChokerEnabledForLingerie;              break;
            case OutfitType.Fae:              wings = WingsEnabledForFae;                   overall = ApronEnabledForFae;                   hat = HatEnabledForFae;                   choker = ChokerEnabledForFae;                   break;
            default:                          wings = false; overall = false; hat = false;   choker = false;                                                                           break;
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
            || index == (int)OutfitType.Lingerie
            || index == (int)OutfitType.Fae
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
            || index == (int)OutfitType.Lingerie
            || index == (int)OutfitType.Fae
            || !IsOutfitEnabled((OutfitType)index));

        SwitchToOutfit((OutfitType)index);
    }

    private void SwitchToOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:             ToggleWorkOutfit(true);             break;
            case OutfitType.WorkTwo:          ToggleWorkTwoOutfit(true);          break;
            case OutfitType.WorkThree:        ToggleWorkThreeOutfit(true);        break;
            case OutfitType.WorkFour:         ToggleWorkFourOutfit(true);         break;
            case OutfitType.WorkSuitThree:    ToggleWorkSuitThreeOutfit(true);    break;
            case OutfitType.Casual:           ToggleCasualOutfit(true);           break;
            case OutfitType.ShortsAndTights:  ToggleShortsAndTightsOutfit(true);  break;
            case OutfitType.Fitness:          ToggleFitnessOutfit(true);          break;
            case OutfitType.Pyjamas:          TogglePyjamasOutfit(true);          break;
            case OutfitType.Housecoat:        ToggleHousecoatOutfit(true);        break;
            case OutfitType.Nightie:          ToggleNightie(true);                break;
            case OutfitType.Date1:            ToggleSecondDateOutfit(true);       break;
            case OutfitType.Date2:            ToggleFirstDateOutfit(true);        break;
            case OutfitType.Date3:            ToggleThirdDateOutfit(true);        break;
            case OutfitType.CuteOne:          ToggleCuteOneOutfit(true);          break;
            case OutfitType.CuteTwo:          ToggleCuteTwoOutfit(true);          break;
            case OutfitType.CuteThree:        ToggleCuteThreeOutfit(true);        break;
            case OutfitType.CuteFour:         ToggleCuteFourOutfit(true);         break;
            case OutfitType.CuteFive:         ToggleCuteFiveOutfit(true);         break;
            case OutfitType.CuteSix:          ToggleCuteSixOutfit(true);          break;
            case OutfitType.CuteSeven:        ToggleCuteSevenOutfit(true);        break;
            case OutfitType.CuteEight:        ToggleCuteEightOutfit(true);        break;
            case OutfitType.WoolenJumper:     ToggleWoolenJumperOutfit(true);     break;
            case OutfitType.Edea:             ToggleEdeaOutfit(true);             break;
            case OutfitType.DitzyDress:       ToggleDitzyDressOutfit(true);       break;
            case OutfitType.LittleBlackDress: ToggleLittleBlackDressOutfit(true); break;
            case OutfitType.Casual3:          ToggleNightOutOneOutfit(true);      break;
            case OutfitType.Essie:            ToggleEssieOutfit(true);            break;
            case OutfitType.NightOutFour:     ToggleNightOutFourOutfit(true);     break;
            case OutfitType.ElegantDress:     ToggleElegantDressOutfit(true);     break;
            case OutfitType.NightOutRuffle:   ToggleNightOutRuffleOutfit(true);   break;
            case OutfitType.Wedding:          ToggleWeddingOutfit(true);          break;
            case OutfitType.Funeral:          ToggleFuneralOutfit(true);          break;
            case OutfitType.Homelessness:     ToggleHomelessnessOutfit(true);     break;
            case OutfitType.Lingerie:         ToggleLingerieOutfit(true);         break;
            case OutfitType.Fae:              ToggleFaeOutfit(true);              break;
            default:                          DisableAllMainOutfits();            break;
        }
    }

    // ====================== TOGGLE METHODS ======================
    public void ToggleWorkOutfit(bool? forceOn = null)            { JiggleToggle(JiggleForWorkOne);            ToggleMainOutfit(OutfitType.Work,             forceOn); }
    public void ToggleWorkTwoOutfit(bool? forceOn = null)         { JiggleToggle(JiggleForWorkTwo);            ToggleMainOutfit(OutfitType.WorkTwo,           forceOn); }
    public void ToggleWorkThreeOutfit(bool? forceOn = null)       { JiggleToggle(JiggleForWorkThree);          ToggleMainOutfit(OutfitType.WorkThree,         forceOn); }
    public void ToggleWorkFourOutfit(bool? forceOn = null)        { JiggleToggle(JiggleForWorkFour);           ToggleMainOutfit(OutfitType.WorkFour,          forceOn); }
    public void ToggleWorkSuitThreeOutfit(bool? forceOn = null)   { JiggleToggle(JiggleWorkSuitThree);         ToggleMainOutfit(OutfitType.WorkSuitThree,     forceOn); }
    public void ToggleCasualOutfit(bool? forceOn = null)          { JiggleToggle(JiggleCasually);              ToggleMainOutfit(OutfitType.Casual,            forceOn); }
    public void ToggleShortsAndTightsOutfit(bool? forceOn = null) { JiggleToggle(JiggleShortsAndTights);       ToggleMainOutfit(OutfitType.ShortsAndTights,   forceOn); }
    public void ToggleFitnessOutfit(bool? forceOn = null)         { JiggleToggle(JiggleFitness);               ToggleMainOutfit(OutfitType.Fitness,           forceOn); }
    public void TogglePyjamasOutfit(bool? forceOn = null)         { JiggleToggle(JiggleInPyjamas);             ToggleMainOutfit(OutfitType.Pyjamas,           forceOn); }
    public void ToggleHousecoatOutfit(bool? forceOn = null)       { JiggleToggle(JiggleInHousecoat);           ToggleMainOutfit(OutfitType.Housecoat,         forceOn); }
    public void ToggleNightie(bool? forceOn = null)               { JiggleToggle(JiggleNightie);               ToggleMainOutfit(OutfitType.Nightie,           forceOn); }
    public void ToggleSecondDateOutfit(bool? forceOn = null)      { JiggleToggle(JiggleOnASecondDate);         ToggleMainOutfit(OutfitType.Date1,             forceOn); }
    public void ToggleFirstDateOutfit(bool? forceOn = null)       { JiggleToggle(JiggleOnAFirstDate);          ToggleMainOutfit(OutfitType.Date2,             forceOn); }
    public void ToggleThirdDateOutfit(bool? forceOn = null)       { JiggleToggle(JiggleOnAThirdDate);          ToggleMainOutfit(OutfitType.Date3,             forceOn); }
    public void ToggleCuteOneOutfit(bool? forceOn = null)         { JiggleToggle(JiggleOnACuteOne);            ToggleMainOutfit(OutfitType.CuteOne,           forceOn); }
    public void ToggleCuteTwoOutfit(bool? forceOn = null)         { JiggleToggle(JiggleCasuallyTwo);           ToggleMainOutfit(OutfitType.CuteTwo,           forceOn); }
    public void ToggleCuteThreeOutfit(bool? forceOn = null)       { JiggleToggle(JiggleCuteThree);             ToggleMainOutfit(OutfitType.CuteThree,         forceOn); }
    public void ToggleCuteFourOutfit(bool? forceOn = null)        { JiggleToggle(JiggleCuteFour);              ToggleMainOutfit(OutfitType.CuteFour,          forceOn); }
    public void ToggleCuteFiveOutfit(bool? forceOn = null)        { JiggleToggle(JiggleCuteFive);              ToggleMainOutfit(OutfitType.CuteFive,          forceOn); }
    public void ToggleCuteSixOutfit(bool? forceOn = null)         { JiggleToggle(JiggleRuffleBlousseAndSkirt); ToggleMainOutfit(OutfitType.CuteSix,           forceOn); }
    public void ToggleCuteSevenOutfit(bool? forceOn = null)       { JiggleToggle(JiggleCuteSeven);             ToggleMainOutfit(OutfitType.CuteSeven,         forceOn); }
    public void ToggleCuteEightOutfit(bool? forceOn = null)       { JiggleToggle(JiggleCuteEight);             ToggleMainOutfit(OutfitType.CuteEight,         forceOn); }
    public void ToggleWoolenJumperOutfit(bool? forceOn = null)    { JiggleToggle(JiggleWoolenJumper);          ToggleMainOutfit(OutfitType.WoolenJumper,      forceOn); }
    public void ToggleEdeaOutfit(bool? forceOn = null)            { JiggleToggle(JiggleCasuallyFour);          ToggleMainOutfit(OutfitType.Edea,              forceOn); }
    public void ToggleDitzyDressOutfit(bool? forceOn = null)      { JiggleToggle(JiggleDitzyDress);            ToggleMainOutfit(OutfitType.DitzyDress,        forceOn); }
    public void ToggleLittleBlackDressOutfit(bool? forceOn = null){ JiggleToggle(JiggleLittleBlackDress);      ToggleMainOutfit(OutfitType.LittleBlackDress,  forceOn); }
    public void ToggleNightOutOneOutfit(bool? forceOn = null)     { JiggleToggle(JiggleCasuallyThree);         ToggleMainOutfit(OutfitType.Casual3,           forceOn); }
    public void ToggleEssieOutfit(bool? forceOn = null)           { JiggleToggle(JiggleEssie);                 ToggleMainOutfit(OutfitType.Essie,             forceOn); }
    public void ToggleNightOutFourOutfit(bool? forceOn = null)    { JiggleToggle(JiggleNightOutFour);          ToggleMainOutfit(OutfitType.NightOutFour,      forceOn); }
    public void ToggleElegantDressOutfit(bool? forceOn = null)    { JiggleToggle(JiggleElegantDress);          ToggleMainOutfit(OutfitType.ElegantDress,      forceOn); }
    public void ToggleNightOutRuffleOutfit(bool? forceOn = null)  { JiggleToggle(JiggleNightOutRuffle);        ToggleMainOutfit(OutfitType.NightOutRuffle,    forceOn); }
    public void ToggleWeddingOutfit(bool? forceOn = null)         { JiggleToggle(JiggleAtAWedding);            ToggleMainOutfit(OutfitType.Wedding,           forceOn); }
    public void ToggleFuneralOutfit(bool? forceOn = null)         { JiggleToggle(JiggleAtAFuneral);            ToggleMainOutfit(OutfitType.Funeral,           forceOn); }
    public void ToggleHomelessnessOutfit(bool? forceOn = null)    { JiggleToggle(JiggleWhileHomeless);         ToggleMainOutfit(OutfitType.Homelessness,      forceOn); }
    public void ToggleUndergarments(bool? forceOn = null)         { JiggleToggle(JiggleUndergarments);         ToggleMainOutfit(OutfitType.Undergarments,     forceOn); }
    public void ToggleLingerieOutfit(bool? forceOn = null)        { JiggleToggle(JiggleLingerie);              ToggleMainOutfit(OutfitType.Lingerie,          forceOn); }
    public void ToggleFaeOutfit(bool? forceOn = null)             { JiggleToggle(JiggleFae);                   ToggleMainOutfit(OutfitType.Fae,               forceOn); }

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
        ClearOutfitMeshes();
        if (DefaultHair != null) DefaultHair.enabled = false;
        currentOutfit = outfit;

        switch (outfit)
        {
            case OutfitType.Work:             InstantiatePrefabs(OutfitForWorkOne);               break;
            case OutfitType.WorkTwo:          InstantiatePrefabs(OutfitForWorkOneTwo);            break;
            case OutfitType.WorkThree:        InstantiatePrefabs(OutfitForWorkOneThree);          break;
            case OutfitType.WorkFour:         InstantiatePrefabs(OutfitForWorkOneFour);           break;
            case OutfitType.WorkSuitThree:    InstantiatePrefabs(OutfitForWorkSuitThree);         break;
            case OutfitType.Casual:           InstantiatePrefabs(OutfitForCasual);                break;
            case OutfitType.ShortsAndTights:  InstantiatePrefabs(OutfitForShortsAndTights);       break;
            case OutfitType.Fitness:          InstantiatePrefabs(OutfitForFitness);               break;
            case OutfitType.Pyjamas:          InstantiatePrefabs(OutfitForPyjamas);               break;
            case OutfitType.Housecoat:        InstantiatePrefabs(OutfitForHousecoat);             break;
            case OutfitType.Nightie:          InstantiatePrefabs(OutfitForNightie);               break;
            case OutfitType.Date1:            InstantiatePrefabs(OutfitForSecondDate);            break;
            case OutfitType.Date2:            InstantiatePrefabs(OutfitForFirstDate);             break;
            case OutfitType.Date3:            InstantiatePrefabs(OutfitForThirdDate);             break;
            case OutfitType.CuteOne:          InstantiatePrefabs(OutfitForCuteOne);               break;
            case OutfitType.CuteTwo:          InstantiatePrefabs(OutfitForCuteTwo);               break;
            case OutfitType.CuteThree:        InstantiatePrefabs(OutfitForCuteThree);             break;
            case OutfitType.CuteFour:         InstantiatePrefabs(OutfitForCuteFour);              break;
            case OutfitType.CuteFive:         InstantiatePrefabs(OutfitForCuteFive);              break;
            case OutfitType.CuteSix:          InstantiatePrefabs(OutfitForRuffleBlousseAndSkirt); break;
            case OutfitType.CuteSeven:        InstantiatePrefabs(OutfitForCuteSeven);             break;
            case OutfitType.CuteEight:        InstantiatePrefabs(OutfitForCuteEight);             break;
            case OutfitType.WoolenJumper:     InstantiatePrefabs(OutfitForWoolenJumper);          break;
            case OutfitType.Edea:             InstantiatePrefabs(OutfitForEdea);                  break;
            case OutfitType.DitzyDress:       InstantiatePrefabs(OutfitForDitzyDress);            break;
            case OutfitType.LittleBlackDress: InstantiatePrefabs(OutfitForLittleBlackDress);      break;
            case OutfitType.Casual3:          InstantiatePrefabs(OutfitForNightOutOne);           break;
            case OutfitType.Essie:            InstantiatePrefabs(OutfitForEssie);                 break;
            case OutfitType.NightOutFour:     InstantiatePrefabs(OutfitForNightOutFour);          break;
            case OutfitType.ElegantDress:     InstantiatePrefabs(OutfitForElegantDress);          break;
            case OutfitType.NightOutRuffle:   InstantiatePrefabs(OutfitForNightOutRuffle);        break;
            case OutfitType.Wedding:          InstantiatePrefabs(OutfitForWedding);               break;
            case OutfitType.Funeral:          InstantiatePrefabs(OutfitForFuneral);               break;
            case OutfitType.Homelessness:     InstantiatePrefabs(OutfitForHomelessness);          break;
            case OutfitType.Undergarments:    InstantiatePrefabs(OutfitForUndergarments);         break;
            case OutfitType.Lingerie:         InstantiatePrefabs(OutfitForLingerie);              break;
            case OutfitType.Fae:              InstantiatePrefabs(OutfitForFae);                   break;
        }

        ApplyBodyColors(outfit);
        ApplyAccessories();

        // Rebind after everything is instantiated
        RebindAllSkinnedMeshes(SkinnedMeshRendererParent.transform);
    }

    private void RebindAllSkinnedMeshes(Transform parent)
    {
        if (Body == null) return;

        Transform rootBone = Body.rootBone;
        Transform[] bones = Body.bones;

        foreach (var smr in parent.GetComponentsInChildren<SkinnedMeshRenderer>(false))
        {
            if (smr == null) continue;
        
            smr.rootBone = rootBone;
            smr.bones = bones;
            smr.updateWhenOffscreen = true;
            smr.enabled = true;           // Force enable
        }
    }
    private void InstantiatePrefabs(List<GameObject> prefabs)
    {
        if (prefabs == null || SkinnedMeshRendererParent == null) return;

        if (Body == null)
        {
            Debug.LogError("Body SkinnedMeshRenderer is not assigned on MeTwo!");
            return;
        }

        Transform rootBone = Body.rootBone;
        Transform[] bones = Body.bones;

        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;

            GameObject instance = Instantiate(prefab, SkinnedMeshRendererParent.transform);

            // Rebind ONLY active SkinnedMeshRenderers (or ones that should be visible)
            foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(false))
            {
                if (smr == null) continue;

                smr.rootBone = rootBone;
                smr.bones = bones;
                smr.updateWhenOffscreen = true;

                // Ensure the renderer is enabled (this fixes the disabling issue)
                smr.enabled = true;
            }
        }
    }
    
    public void DisableAllMainOutfits()
    {
        ClearOutfitMeshes();
        currentOutfit = OutfitType.None;
        if (DefaultHair != null) DefaultHair.enabled = true;
    }

    // ====================== RANDOM OUTFIT SELECTION ======================
    private OutfitType PickRandom(OutfitType[] pool)
    {
        OutfitType[] enabled = pool.Where(o => IsOutfitEnabled(o)).ToArray();
        if (enabled.Length == 0) return currentOutfit;
        return enabled[Random.Range(0, enabled.Length)];
    }

    public void SetRandomWorkOutfit()
    {
        OutfitType[] pool = { OutfitType.Work, OutfitType.WorkTwo, OutfitType.WorkThree, OutfitType.WorkFour, OutfitType.WorkSuitThree };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomCasualOutfit()
    {
        OutfitType[] pool = { OutfitType.Casual, OutfitType.ShortsAndTights, OutfitType.Fitness };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomPyjamas()
    {
        OutfitType[] pool = { OutfitType.Pyjamas, OutfitType.Housecoat, OutfitType.Nightie };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomDatingOutfit()
    {
        OutfitType[] pool = { OutfitType.Date1, OutfitType.Date2, OutfitType.Date3 };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomCuteOutfit()
    {
        OutfitType[] pool =
        {
            OutfitType.CuteOne,    OutfitType.CuteTwo,        OutfitType.CuteThree,     OutfitType.CuteFour,
            OutfitType.CuteFive,   OutfitType.CuteSix,        OutfitType.CuteSeven,     OutfitType.CuteEight,
            OutfitType.WoolenJumper, OutfitType.Edea,         OutfitType.DitzyDress,    OutfitType.LittleBlackDress
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomNightOutOutfit()
    {
        OutfitType[] pool =
        {
            OutfitType.Casual3, OutfitType.Essie, OutfitType.NightOutFour,
            OutfitType.ElegantDress, OutfitType.NightOutRuffle
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomStorylineOutfit()
    {
        OutfitType[] pool = { OutfitType.Wedding, OutfitType.Funeral, OutfitType.Homelessness };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomPlaceholderOutfit()
    {
        OutfitType[] pool = { OutfitType.Lingerie, OutfitType.Fae };
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
            case OutfitType.Work:             return lipsColorForWorkOne;
            case OutfitType.WorkTwo:          return lipsColorForWorkTwo;
            case OutfitType.WorkThree:        return lipsColorForWorkThree;
            case OutfitType.WorkFour:         return lipsColorForWorkFour;
            case OutfitType.WorkSuitThree:    return lipsColorForWorkSuitThree;
            case OutfitType.Casual:           return lipsColorForCasual;
            case OutfitType.ShortsAndTights:  return lipsColorForShortsAndTights;
            case OutfitType.Fitness:          return lipsColorForFitness;
            case OutfitType.Pyjamas:          return lipsColorForPyjamas;
            case OutfitType.Housecoat:        return lipsColorForHousecoat;
            case OutfitType.Nightie:          return lipsColorForNightie;
            case OutfitType.Date1:            return lipsColorForSecondDate;
            case OutfitType.Date2:            return lipsColorForFirstDate;
            case OutfitType.Date3:            return lipsColorForThirdDate;
            case OutfitType.CuteOne:          return lipsColorForCuteOne;
            case OutfitType.CuteTwo:          return lipsColorForCuteTwo;
            case OutfitType.CuteThree:        return lipsColorForCuteThree;
            case OutfitType.CuteFour:         return lipsColorForCuteFour;
            case OutfitType.CuteFive:         return lipsColorForCuteFive;
            case OutfitType.CuteSix:          return lipsColorForRuffleBlousseAndSkirt;
            case OutfitType.CuteSeven:        return lipsColorForCuteSeven;
            case OutfitType.CuteEight:        return lipsColorForCuteEight;
            case OutfitType.WoolenJumper:     return lipsColorForWoolenJumper;
            case OutfitType.Edea:             return lipsColorForEdea;
            case OutfitType.DitzyDress:       return lipsColorForDitzyDress;
            case OutfitType.LittleBlackDress: return lipsColorForLittleBlackDress;
            case OutfitType.Casual3:          return lipsColorForNightOutOne;
            case OutfitType.Essie:            return lipsColorForEssie;
            case OutfitType.NightOutFour:     return lipsColorForNightOutFour;
            case OutfitType.ElegantDress:     return lipsColorForElegantDress;
            case OutfitType.NightOutRuffle:   return lipsColorForNightOutRuffle;
            case OutfitType.Wedding:          return lipsColorForWedding;
            case OutfitType.Funeral:          return lipsColorForFuneral;
            case OutfitType.Homelessness:     return lipsColorForHomelessness;
            case OutfitType.Undergarments:    return lipsColorForUndergarments;
            case OutfitType.Lingerie:         return lipsColorForLingerie;
            case OutfitType.Fae:              return lipsColorForFae;
            default:                          return Color.white;
        }
    }

    private Color GetNailColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:             return nailsColorForWorkOne;
            case OutfitType.WorkTwo:          return nailsColorForWorkTwo;
            case OutfitType.WorkThree:        return nailsColorForWorkThree;
            case OutfitType.WorkFour:         return nailsColorForWorkFour;
            case OutfitType.WorkSuitThree:    return nailsColorForWorkSuitThree;
            case OutfitType.Casual:           return nailsColorForCasual;
            case OutfitType.ShortsAndTights:  return nailsColorForShortsAndTights;
            case OutfitType.Fitness:          return nailsColorForFitness;
            case OutfitType.Pyjamas:          return nailsColorForPyjamas;
            case OutfitType.Housecoat:        return nailsColorForHousecoat;
            case OutfitType.Nightie:          return nailsColorForNightie;
            case OutfitType.Date1:            return nailsColorForSecondDate;
            case OutfitType.Date2:            return nailsColorForFirstDate;
            case OutfitType.Date3:            return nailsColorForThirdDate;
            case OutfitType.CuteOne:          return nailsColorForCuteOne;
            case OutfitType.CuteTwo:          return nailsColorForCuteTwo;
            case OutfitType.CuteThree:        return nailsColorForCuteThree;
            case OutfitType.CuteFour:         return nailsColorForCuteFour;
            case OutfitType.CuteFive:         return nailsColorForCuteFive;
            case OutfitType.CuteSix:          return nailsColorForRuffleBlousseAndSkirt;
            case OutfitType.CuteSeven:        return nailsColorForCuteSeven;
            case OutfitType.CuteEight:        return nailsColorForCuteEight;
            case OutfitType.WoolenJumper:     return nailsColorForWoolenJumper;
            case OutfitType.Edea:             return nailsColorForEdea;
            case OutfitType.DitzyDress:       return nailsColorForDitzyDress;
            case OutfitType.LittleBlackDress: return nailsColorForLittleBlackDress;
            case OutfitType.Casual3:          return nailsColorForNightOutOne;
            case OutfitType.Essie:            return nailsColorForEssie;
            case OutfitType.NightOutFour:     return nailsColorForNightOutFour;
            case OutfitType.ElegantDress:     return nailsColorForElegantDress;
            case OutfitType.NightOutRuffle:   return nailsColorForNightOutRuffle;
            case OutfitType.Wedding:          return nailsColorForWedding;
            case OutfitType.Funeral:          return nailsColorForFuneral;
            case OutfitType.Homelessness:     return nailsColorForHomelessness;
            case OutfitType.Undergarments:    return nailsColorForUndergarments;
            case OutfitType.Lingerie:         return nailsColorForLingerie;
            case OutfitType.Fae:              return nailsColorForFae;
            default:                          return Color.white;
        }
    }

    public void Undress()
    {
        DisableAllMainOutfits();
    }
}
//
// public enum OutfitType
// {
//     None,
//     Work,
//     WorkTwo,
//     WorkThree,
//     WorkFour,
//     WorkSuitThree,
//     Casual,
//     ShortsAndTights,
//     Fitness,
//     Pyjamas,
//     Housecoat,
//     Nightie,
//     Date1,
//     Date2,
//     Date3,
//     CuteOne,
//     CuteTwo,
//     CuteThree,
//     CuteFour,
//     CuteFive,
//     CuteSix,
//     CuteSeven,
//     CuteEight,
//     WoolenJumper,
//     Edea,
//     DitzyDress,
//     LittleBlackDress,
//     Casual3,
//     Essie,
//     NightOutFour,
//     ElegantDress,
//     NightOutRuffle,
//     Wedding,
//     Funeral,
//     Homelessness,
//     Lingerie,
//     Fae,
//     Undergarments
// }

// ====================== EDITOR ======================
#if UNITY_EDITOR
[CustomEditor(typeof(MeTwo))]
public class MeTwoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeTwo me = (MeTwo)target;

        EditorGUILayout.LabelField("Outfit Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Work
        EditorGUILayout.LabelField("Work Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Classic Nora Dress",     GUILayout.Height(35))) { me.ToggleWorkOutfit();          EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Black Skirt & Cream",    GUILayout.Height(35))) { me.ToggleWorkThreeOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Tartan & Grey Dress",    GUILayout.Height(35))) { me.ToggleWorkTwoOutfit();       EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Black Suit & Shirt",     GUILayout.Height(35))) { me.ToggleWorkFourOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Suit Jacket & Trousers", GUILayout.Height(35))) { me.ToggleWorkSuitThreeOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Casual
        EditorGUILayout.LabelField("Casual Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Uni Sweater & Jeans", GUILayout.Height(35))) { me.ToggleCasualOutfit();           EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Shorts & Tights",     GUILayout.Height(35))) { me.ToggleShortsAndTightsOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fitness",             GUILayout.Height(35))) { me.ToggleFitnessOutfit();          EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // PJs
        EditorGUILayout.LabelField("PJs", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pyjamas",   GUILayout.Height(35))) { me.TogglePyjamasOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Housecoat", GUILayout.Height(35))) { me.ToggleHousecoatOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Nightie",   GUILayout.Height(35))) { me.ToggleNightie();         EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Dating
        EditorGUILayout.LabelField("Dating", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("First Date",  GUILayout.Height(35))) { me.ToggleFirstDateOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Second Date", GUILayout.Height(35))) { me.ToggleSecondDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Third Date",  GUILayout.Height(35))) { me.ToggleThirdDateOutfit();  EditorUtility.SetDirty(me); }
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
        if (GUILayout.Button("Cute Five",             GUILayout.Height(35))) { me.ToggleCuteFiveOutfit();        EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Ruffle Blouse & Skirt", GUILayout.Height(35))) { me.ToggleCuteSixOutfit();         EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Seven",            GUILayout.Height(35))) { me.ToggleCuteSevenOutfit();       EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Cute Eight",            GUILayout.Height(35))) { me.ToggleCuteEightOutfit();       EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wooly Jumper & Tights", GUILayout.Height(35))) { me.ToggleWoolenJumperOutfit();    EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Ditzy Dress",           GUILayout.Height(35))) { me.ToggleDitzyDressOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Little Black Dress",    GUILayout.Height(35))) { me.ToggleLittleBlackDressOutfit();EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Edea",                  GUILayout.Height(35))) { me.ToggleEdeaOutfit();            EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Night Out
        EditorGUILayout.LabelField("Night Out Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Froot Dress",    GUILayout.Height(35))) { me.ToggleNightOutOneOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Essie",          GUILayout.Height(35))) { me.ToggleEssieOutfit();         EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Night Out Four", GUILayout.Height(35))) { me.ToggleNightOutFourOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Elegant Dress",  GUILayout.Height(35))) { me.ToggleElegantDressOutfit();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Layered Ruffle Dress", GUILayout.Height(35))) { me.ToggleNightOutRuffleOutfit(); EditorUtility.SetDirty(me); }
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

        // Accessories
        EditorGUILayout.LabelField("Accessories (toggle independently)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Wings",   GUILayout.Height(35))) { me.ToggleWings();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Overall", GUILayout.Height(35))) { me.ToggleOverall(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Hat",     GUILayout.Height(35))) { me.ToggleHat();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Choker",  GUILayout.Height(35))) { me.ToggleChoker();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Undergarments
        EditorGUILayout.LabelField("Undergarments", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Lingerie",       GUILayout.Height(35))) { me.ToggleLingerieOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Underwear",      GUILayout.Height(35))) { me.ToggleUndergarments();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fae",            GUILayout.Height(35))) { me.ToggleFaeOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Nothin' At All", GUILayout.Height(35))) { me.Undress();              EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Random
        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Work",    GUILayout.Height(40))) { me.SetRandomWorkOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Casual",  GUILayout.Height(40))) { me.SetRandomCasualOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Dating",  GUILayout.Height(40))) { me.SetRandomDatingOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Cute",    GUILayout.Height(40))) { me.SetRandomCuteOutfit();   EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Night Out", GUILayout.Height(40))) { me.SetRandomNightOutOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Pyjamas",   GUILayout.Height(40))) { me.SetRandomPyjamas();         EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Storyline",   GUILayout.Height(40))) { me.SetRandomStorylineOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Placeholder", GUILayout.Height(40))) { me.SetRandomPlaceholderOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("---------------------", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
#endif