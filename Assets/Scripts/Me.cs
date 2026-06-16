using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Me : MonoBehaviour
{
    [Header("Settings")]
    public bool UnressedOnLoad;
    public string ThisCharacterName;

    [Header("Only applies to NPCs")]
    public NPCController npcController;

    [Header("Current Outfit")]
    public OutfitType currentOutfit = OutfitType.Work;

    [Header("Character Root")]
    public GameObject SkinnedMeshRendererParentOutfits;
    public GameObject SkinnedMeshRendererParentAccessories;

    [Header("Body")]
    public SkinnedMeshRenderer Body;

    [Header("Accessories")]
    public SkinnedMeshRenderer ButterflyWings;
    public SkinnedMeshRenderer Overall;
    public SkinnedMeshRenderer Hat;
    public SkinnedMeshRenderer Choker;

    [Header("Default Hair")]
    public SkinnedMeshRenderer DefaultHair;

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
    [Header("Grey Check Halter Dress")]
    public bool GreyCheckHalterDressOutfitEnabled = true;
    public List<GameObject> OutfitForGreyCheckHalterDress;
    public Color lipsColorForGreyCheckHalterDress = new Color(0.98f, 0.45f, 0.55f);
    public Color nailsColorForGreyCheckHalterDress = new Color(0.98f, 0.9f, 0.92f);
    public bool JiggleOnAGreyCheckHalterDress;
    public bool WingsEnabledForGreyCheckHalterDress;
    public bool ApronEnabledForGreyCheckHalterDress;
    public bool HatEnabledForGreyCheckHalterDress;
    public bool ChokerEnabledForGreyCheckHalterDress;

    [Header("Sweater & Skirt")]
    public bool SweaterAndSkirtOutfitEnabled = true;
    public List<GameObject> OutfitForSweaterAndSkirt;
    public Color lipsColorForSweaterAndSkirt = new Color(0.8f, 0.15f, 0.25f);
    public Color nailsColorForSweaterAndSkirt = new Color(0.88f, 0.65f, 0.75f);
    public bool JiggleSweaterAndSkirt;
    public bool WingsEnabledForSweaterAndSkirt;
    public bool ApronEnabledForSweaterAndSkirt;
    public bool HatEnabledForSweaterAndSkirt;
    public bool ChokerEnabledForSweaterAndSkirt;

    [Header("Turtleneck & Skirt")]
    public bool TurtleneckAndSkirtOutfitEnabled = true;
    public List<GameObject> OutfitForTurtleneckAndSkirt;
    public Color lipsColorForTurtleneckAndSkirt = new Color(0.95f, 0.35f, 0.45f);
    public Color nailsColorForTurtleneckAndSkirt = new Color(0.96f, 0.85f, 0.88f);
    public bool JiggleTurtleneckAndSkirt;
    public bool WingsEnabledForTurtleneckAndSkirt;
    public bool ApronEnabledForTurtleneckAndSkirt;
    public bool HatEnabledForTurtleneckAndSkirt;
    public bool ChokerEnabledForTurtleneckAndSkirt;

    [Header("Check Top & Jeans")]
    public bool CheckTopAndJeansOutfitEnabled = true;
    public List<GameObject> OutfitForCheckTopAndJeans;
    public Color lipsColorForCheckTopAndJeans = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForCheckTopAndJeans = new Color(0.94f, 0.8f, 0.85f);
    public bool JiggleCheckTopAndJeans;
    public bool WingsEnabledForCheckTopAndJeans;
    public bool ApronEnabledForCheckTopAndJeans;
    public bool HatEnabledForCheckTopAndJeans;
    public bool ChokerEnabledForCheckTopAndJeans;

    [Header("Knotted Blousse & Skirt")]
    public bool KnottedBlousseAndSkirtOutfitEnabled = true;
    public List<GameObject> OutfitForKnottedBlousseAndSkirt;
    public Color lipsColorForKnottedBlousseAndSkirt = new Color(0.88f, 0.2f, 0.3f);
    public Color nailsColorForKnottedBlousseAndSkirt = new Color(0.93f, 0.78f, 0.83f);
    public bool JiggleKnottedBlousseAndSkirt;
    public bool WingsEnabledForKnottedBlousseAndSkirt;
    public bool ApronEnabledForKnottedBlousseAndSkirt;
    public bool HatEnabledForKnottedBlousseAndSkirt;
    public bool ChokerEnabledForKnottedBlousseAndSkirt;

    [Header("Ruffle Blousse & Skirt")]
    public bool RuffleBlousseAndSkirtEnabled = true;
    public List<GameObject> OutfitForRuffleBlousseAndSkirt;
    public Color lipsColorForRuffleBlousseAndSkirt = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForRuffleBlousseAndSkirt = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleRuffleBlousseAndSkirt;
    public bool WingsEnabledForRuffleBlousseAndSkirt;
    public bool ApronEnabledForRuffleBlousseAndSkirt;
    public bool HatEnabledForRuffleBlousseAndSkirt;
    public bool ChokerEnabledForRuffleBlousseAndSkirt;

    [Header("Loose Top & Long Skirt")]
    public bool LooseTopAndLongSkirtOutfitEnabled = true;
    public List<GameObject> OutfitForLooseTopAndLongSkirt;
    public Color lipsColorForLooseTopAndLongSkirt = new Color(0.88f, 0.22f, 0.32f);
    public Color nailsColorForLooseTopAndLongSkirt = new Color(0.95f, 0.82f, 0.85f);
    public bool JiggleLooseTopAndLongSkirt;
    public bool WingsEnabledForLooseTopAndLongSkirt;
    public bool ApronEnabledForLooseTopAndLongSkirt;
    public bool HatEnabledForLooseTopAndLongSkirt;
    public bool ChokerEnabledForLooseTopAndLongSkirt;

    [Header("Turtleneck & Medium Skirt")]
    public bool TurtleneckAndMediumSkirtOutfitEnabled = true;
    public List<GameObject> OutfitForTurtleneckAndMediumSkirt;
    public Color lipsColorForTurtleneckAndMediumSkirt = new Color(0.85f, 0.18f, 0.28f);
    public Color nailsColorForTurtleneckAndMediumSkirt = new Color(0.88f, 0.68f, 0.73f);
    public bool JiggleTurtleneckAndMediumSkirt;
    public bool WingsEnabledForTurtleneckAndMediumSkirt;
    public bool ApronEnabledForTurtleneckAndMediumSkirt;
    public bool HatEnabledForTurtleneckAndMediumSkirt;
    public bool ChokerEnabledForTurtleneckAndMediumSkirt;

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

    [Header("Edea's Dress")]
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

    [Header("Casual Pantsuit")]
    public bool CasualPantsuitEnabled = true;
    public List<GameObject> OutfitCasualPantsuit;
    public Color lipsColorForCasualPantsuit = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForCasualPantsuit = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleCasualPantsuit;
    public bool WingsEnabledForCasualPantsuit;
    public bool ApronEnabledForCasualPantsuit;
    public bool HatEnabledForCasualPantsuit;
    public bool ChokerEnabledForCasualPantsuit;
    
    [Header("Conservative Jumper & Skirt")]
    public bool ConservativeEnabled = true;
    public List<GameObject> OutfitConservative;
    public Color lipsColorForConservative = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForConservative = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleConservative;
    public bool WingsEnabledForConservative;
    public bool ApronEnabledForConservative;
    public bool HatEnabledForConservative;
    public bool ChokerEnabledForConservative;

    [Header("Night Out")]
    [Header("Froot Dress")]
    public bool FrootDressOutfitEnabled = true;
    public List<GameObject> OutfitForFrootDress;
    public Color lipsColorForFrootDress = new Color(0.9f, 0.3f, 0.4f);
    public Color nailsColorForFrootDress = new Color(0.93f, 0.78f, 0.82f);
    public bool JiggleCasuallyThree;
    public bool WingsEnabledForFrootDress;
    public bool ApronEnabledForFrootDress;
    public bool HatEnabledForFrootDress;
    public bool ChokerEnabledForFrootDress;

    [Header("Strapless Ruffle Dress")]
    public bool StraplessRuffleDressOutfitEnabled = true;
    public List<GameObject> OutfitForStraplessRuffleDress;
    public Color lipsColorForStraplessRuffleDress = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForStraplessRuffleDress = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleStraplessRuffleDress;
    public bool WingsEnabledForStraplessRuffleDress;
    public bool ApronEnabledForStraplessRuffleDress;
    public bool HatEnabledForStraplessRuffleDress;
    public bool ChokerEnabledForStraplessRuffleDress;

    [Header("Check Body Suit")]
    public bool CheckBodySuitOutfitEnabled = true;
    public List<GameObject> OutfitForCheckBodySuit;
    public Color lipsColorForCheckBodySuit = new Color(0.82f, 0.12f, 0.22f);
    public Color nailsColorForCheckBodySuit = new Color(0.91f, 0.72f, 0.78f);
    public bool JiggleCheckBodySuit;
    public bool WingsEnabledForCheckBodySuit;
    public bool ApronEnabledForCheckBodySuit;
    public bool HatEnabledForCheckBodySuit;
    public bool ChokerEnabledForCheckBodySuit;

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

    [Header("Risque Nightie")]
    public bool RisqueNightieEnabled = true;
    public List<GameObject> OutfitForRisqueNightie;
    public Color lipsColorForRisqueNightie = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForRisqueNightie = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleRisqueNightie;
    public bool WingsEnabledForRisqueNightie;
    public bool ApronEnabledForRisqueNightie;
    public bool HatEnabledForRisqueNightie;
    public bool ChokerEnabledForRisqueNightie;

    [Header("Stealth Suit")]
    public bool StealthSuitEnabled = true;
    public List<GameObject> OutfitForStealthSuit;
    public Color lipsColorForStealthSuit = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorStealthSuit = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleStealthSuit;
    public bool WingsEnabledForStealthSuit;
    public bool ApronEnabledForStealthSuit;
    public bool HatEnabledForStealthSuit;
    public bool ChokerEnabledForStealthSuit;

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

        SetupAccessories();

        if (!UnressedOnLoad) { SwitchToOutfit(OutfitType.Work); }
        else { DisableAllMainOutfits(); }

        if (npcController == null) { SetupInput(); }
    }

    private void SetupInput()
    {
        if (holdToUnlock    != null) { holdToUnlock.action.performed    += OnToggleUndergarments; }
        if (holdToUnlockAll != null) { holdToUnlockAll.action.performed += OnToggleNothinatall; }
        if (nextOutfit      != null) { nextOutfit.action.performed      += OnNextOutfit; }
        if (previousOutfit  != null) { previousOutfit.action.performed  += OnPreviousOutfit; }
    }

    private void OnToggleUndergarments(InputAction.CallbackContext ctx) { ToggleUndergarments(); }
    private void OnToggleNothinatall(InputAction.CallbackContext ctx)   { Undress(); }
    private void OnNextOutfit(InputAction.CallbackContext ctx)          { NextOutfit(); }
    private void OnPreviousOutfit(InputAction.CallbackContext ctx)      { PreviousOutfit(); }

    private void OnDisable()
    {
        if (npcController == null)
        {
            if (nextOutfit      != null) { nextOutfit.action.performed      -= OnNextOutfit; }
            if (previousOutfit  != null) { previousOutfit.action.performed  -= OnPreviousOutfit; }
            if (holdToUnlock    != null) { holdToUnlock.action.performed    -= OnToggleUndergarments; }
            if (holdToUnlockAll != null) { holdToUnlockAll.action.performed -= OnToggleNothinatall; }
        }
    }

    private void ClearOutfitMeshes()
    {
        if (SkinnedMeshRendererParentOutfits == null) { return; }
        for (int i = SkinnedMeshRendererParentOutfits.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = SkinnedMeshRendererParentOutfits.transform.GetChild(i);
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    private void SetupAccessories()
    {
        if (SkinnedMeshRendererParentAccessories == null) { return; }
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones  = Body != null ? Body.bones   : null;
        foreach (var smr in SkinnedMeshRendererParentAccessories.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones    != null) { smr.bones    = bones; }
            smr.updateWhenOffscreen = true;
        }
    }

    private void InstantiatePrefabs(List<GameObject> prefabs)
    {
        if (prefabs == null || SkinnedMeshRendererParentOutfits == null) { return; }
        if (Body == null) { Debug.LogError("Body SkinnedMeshRenderer is not assigned on Me!"); return; }
        Transform rootBone = Body.rootBone;
        Transform[] bones  = Body.bones;
        foreach (var prefab in prefabs)
        {
            if (prefab == null) { continue; }
            GameObject instance = Instantiate(prefab, SkinnedMeshRendererParentOutfits.transform);
            foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr == null) { continue; }
                smr.rootBone            = rootBone;
                smr.bones               = bones;
                smr.updateWhenOffscreen = true;
                smr.enabled             = true;
            }
        }
    }

    public bool IsOutfitEnabled(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:                      return WorkOneOutfitEnabled;
            case OutfitType.WorkTwo:                   return WorkTwoOutfitEnabled;
            case OutfitType.WorkThree:                 return WorkThreeOutfitEnabled;
            case OutfitType.WorkFour:                  return WorkFourOutfitEnabled;
            case OutfitType.WorkSuitThree:             return WorkSuitThreeEnabled;
            case OutfitType.Casual:                    return CasualOutfitEnabled;
            case OutfitType.ShortsAndTights:           return ShortsAndTightsEnabled;
            case OutfitType.Fitness:                   return FitnessOutfitEnabled;
            case OutfitType.Pyjamas:                   return PyjamasOutfitEnabled;
            case OutfitType.Housecoat:                 return HousecoatOutfitEnabled;
            case OutfitType.Nightie:                   return NightieEnabled;
            case OutfitType.RisqueNightie:             return RisqueNightieEnabled;
            case OutfitType.StealthSuit:               return StealthSuitEnabled;
            case OutfitType.Date1:                     return FirstDateOutfitEnabled;  
            case OutfitType.Date2:                     return SecondDateOutfitEnabled;  
            case OutfitType.Date3:                     return ThirdDateOutfitEnabled;
            case OutfitType.GreyCheckHalterDress:      return GreyCheckHalterDressOutfitEnabled;
            case OutfitType.SweaterAndSkirt:           return SweaterAndSkirtOutfitEnabled;
            case OutfitType.TurtleneckAndSkirt:        return TurtleneckAndSkirtOutfitEnabled;
            case OutfitType.CheckTopAndJeans:          return CheckTopAndJeansOutfitEnabled;
            case OutfitType.KnottedBlousseAndSkirt:    return KnottedBlousseAndSkirtOutfitEnabled;
            case OutfitType.RuffleBlousseAndSkirt:     return RuffleBlousseAndSkirtEnabled;
            case OutfitType.LooseTopAndLongSkirt:      return LooseTopAndLongSkirtOutfitEnabled;
            case OutfitType.TurtleneckAndMediumSkirt:  return TurtleneckAndMediumSkirtOutfitEnabled;
            case OutfitType.WoolenJumper:              return WoolenJumperEnabled;
            case OutfitType.Edea:                      return EdeaOutfitEnabled;
            case OutfitType.DitzyDress:                return DitzyDressEnabled;
            case OutfitType.LittleBlackDress:          return LittleBlackDressEnabled;
            case OutfitType.CasualPantSuit:            return CasualPantsuitEnabled;
            case OutfitType.Conservative:              return ConservativeEnabled;
            case OutfitType.Casual3:                   return FrootDressOutfitEnabled;
            case OutfitType.StraplessRuffleDress:      return StraplessRuffleDressOutfitEnabled;
            case OutfitType.CheckBodySuit:             return CheckBodySuitOutfitEnabled;
            case OutfitType.ElegantDress:              return ElegantDressOutfitEnabled;
            case OutfitType.NightOutRuffle:            return NightOutRuffleEnabled;
            case OutfitType.Wedding:                   return WeddingOutfitEnabled;
            case OutfitType.Funeral:                   return FuneralOutfitEnabled;
            case OutfitType.Homelessness:              return HomelessnessOutfitEnabled;
            case OutfitType.Lingerie:                  return LingerieEnabled;
            case OutfitType.Fae:                       return FaeEnabled;
            default:                                   return true;
        }
    }

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

    public void ResetAccessoryOverrides()
    {
        _wingsOverride   = false;
        _overallOverride = false;
        _hatOverride     = false;
        _chokerOverride  = false;
    }

    private void ApplyAccessories()
    {
        GetOutfitAccessoryDefaults(currentOutfit, out bool w, out bool o, out bool h, out bool c);
        if (ButterflyWings != null) { ButterflyWings.enabled = w || _wingsOverride; }
        if (Overall        != null) { Overall.enabled        = o || _overallOverride; }
        if (Hat            != null) { Hat.enabled            = h || _hatOverride; }
        if (Choker         != null) { Choker.enabled         = c || _chokerOverride; }
    }

    private void HideAllAccessories()
    {
        if (ButterflyWings != null) { ButterflyWings.enabled = false; }
        if (Overall        != null) { Overall.enabled        = false; }
        if (Hat            != null) { Hat.enabled            = false; }
        if (Choker         != null) { Choker.enabled         = false; }
    }

    private void GetOutfitAccessoryDefaults(OutfitType outfit, out bool wings, out bool overall, out bool hat, out bool choker)
    {
        switch (outfit)
        {
            case OutfitType.Work:                      wings = WingsEnabledForWorkOne;                   overall = ApronEnabledForWorkOne;                   hat = HatEnabledForWorkOne;                   choker = ChokerEnabledForWorkOne;                   break;
            case OutfitType.WorkTwo:                   wings = WingsEnabledForWorkTwo;                   overall = ApronEnabledForWorkTwo;                   hat = HatEnabledForWorkTwo;                   choker = ChokerEnabledForWorkTwo;                   break;
            case OutfitType.WorkThree:                 wings = WingsEnabledForWorkThree;                 overall = ApronEnabledForWorkThree;                 hat = HatEnabledForWorkThree;                 choker = ChokerEnabledForWorkThree;                 break;
            case OutfitType.WorkFour:                  wings = WingsEnabledForWorkFour;                  overall = ApronEnabledForWorkFour;                  hat = HatEnabledForWorkFour;                  choker = ChokerEnabledForWorkFour;                  break;
            case OutfitType.WorkSuitThree:             wings = WingsEnabledForWorkSuitThree;             overall = ApronEnabledForWorkSuitThree;             hat = HatEnabledForWorkSuitThree;             choker = ChokerEnabledForWorkSuitThree;             break;
            case OutfitType.Casual:                    wings = WingsEnabledForCasual;                    overall = ApronEnabledForCasual;                    hat = HatEnabledForCasual;                    choker = ChokerEnabledForCasual;                    break;
            case OutfitType.ShortsAndTights:           wings = WingsEnabledForShortsAndTights;           overall = ApronEnabledForShortsAndTights;           hat = HatEnabledForShortsAndTights;           choker = ChokerEnabledForShortsAndTights;           break;
            case OutfitType.Fitness:                   wings = WingsEnabledForFitness;                   overall = ApronEnabledForFitness;                   hat = HatEnabledForFitness;                   choker = ChokerEnabledForFitness;                   break;
            case OutfitType.Pyjamas:                   wings = WingsEnabledForPyjamas;                   overall = ApronEnabledForPyjamas;                   hat = HatEnabledForPyjamas;                   choker = ChokerEnabledForPyjamas;                   break;
            case OutfitType.Housecoat:                 wings = WingsEnabledForHousecoat;                 overall = ApronEnabledForHousecoat;                 hat = HatEnabledForHousecoat;                 choker = ChokerEnabledForHousecoat;                 break;
            case OutfitType.Nightie:                   wings = WingsEnabledForNightie;                   overall = ApronEnabledForNightie;                   hat = HatEnabledForNightie;                   choker = ChokerEnabledForNightie;                   break;
            case OutfitType.RisqueNightie:             wings = WingsEnabledForRisqueNightie;             overall = ApronEnabledForRisqueNightie;             hat = HatEnabledForRisqueNightie;             choker = ChokerEnabledForRisqueNightie;             break;
            case OutfitType.StealthSuit:               wings = WingsEnabledForStealthSuit;               overall = ApronEnabledForStealthSuit;               hat = HatEnabledForStealthSuit;               choker = ChokerEnabledForStealthSuit;               break;
            case OutfitType.Date1:                     wings = WingsEnabledForFirstDate;                 overall = ApronEnabledForFirstDate;                 hat = HatEnabledForFirstDate;                 choker = ChokerEnabledForFirstDate;                 break; 
            case OutfitType.Date2:                     wings = WingsEnabledForSecondDate;                overall = ApronEnabledForSecondDate;                hat = HatEnabledForSecondDate;                choker = ChokerEnabledForSecondDate;                break; 
            case OutfitType.Date3:                     wings = WingsEnabledForThirdDate;                 overall = ApronEnabledForThirdDate;                 hat = HatEnabledForThirdDate;                 choker = ChokerEnabledForThirdDate;                 break;
            case OutfitType.GreyCheckHalterDress:      wings = WingsEnabledForGreyCheckHalterDress;      overall = ApronEnabledForGreyCheckHalterDress;      hat = HatEnabledForGreyCheckHalterDress;      choker = ChokerEnabledForGreyCheckHalterDress;      break;
            case OutfitType.SweaterAndSkirt:           wings = WingsEnabledForSweaterAndSkirt;           overall = ApronEnabledForSweaterAndSkirt;           hat = HatEnabledForSweaterAndSkirt;           choker = ChokerEnabledForSweaterAndSkirt;           break;
            case OutfitType.TurtleneckAndSkirt:        wings = WingsEnabledForTurtleneckAndSkirt;        overall = ApronEnabledForTurtleneckAndSkirt;        hat = HatEnabledForTurtleneckAndSkirt;        choker = ChokerEnabledForTurtleneckAndSkirt;        break;
            case OutfitType.CheckTopAndJeans:          wings = WingsEnabledForCheckTopAndJeans;          overall = ApronEnabledForCheckTopAndJeans;          hat = HatEnabledForCheckTopAndJeans;          choker = ChokerEnabledForCheckTopAndJeans;          break;
            case OutfitType.KnottedBlousseAndSkirt:    wings = WingsEnabledForKnottedBlousseAndSkirt;    overall = ApronEnabledForKnottedBlousseAndSkirt;    hat = HatEnabledForKnottedBlousseAndSkirt;    choker = ChokerEnabledForKnottedBlousseAndSkirt;    break;
            case OutfitType.RuffleBlousseAndSkirt:     wings = WingsEnabledForRuffleBlousseAndSkirt;     overall = ApronEnabledForRuffleBlousseAndSkirt;     hat = HatEnabledForRuffleBlousseAndSkirt;     choker = ChokerEnabledForRuffleBlousseAndSkirt;     break;
            case OutfitType.LooseTopAndLongSkirt:      wings = WingsEnabledForLooseTopAndLongSkirt;      overall = ApronEnabledForLooseTopAndLongSkirt;      hat = HatEnabledForLooseTopAndLongSkirt;      choker = ChokerEnabledForLooseTopAndLongSkirt;      break;
            case OutfitType.TurtleneckAndMediumSkirt:  wings = WingsEnabledForTurtleneckAndMediumSkirt;  overall = ApronEnabledForTurtleneckAndMediumSkirt;  hat = HatEnabledForTurtleneckAndMediumSkirt;  choker = ChokerEnabledForTurtleneckAndMediumSkirt;  break;
            case OutfitType.WoolenJumper:              wings = WingsEnabledForWoolenJumper;              overall = ApronEnabledForWoolenJumper;              hat = HatEnabledForWoolenJumper;              choker = ChokerEnabledForWoolenJumper;              break;
            case OutfitType.Edea:                      wings = WingsEnabledForEdea;                      overall = ApronEnabledForEdea;                      hat = HatEnabledForEdea;                      choker = ChokerEnabledForEdea;                      break;
            case OutfitType.DitzyDress:                wings = WingsEnabledForDitzyDress;                overall = ApronEnabledForDitzyDress;                hat = HatEnabledForDitzyDress;                choker = ChokerEnabledForDitzyDress;                break;
            case OutfitType.LittleBlackDress:          wings = WingsEnabledForLittleBlackDress;          overall = ApronEnabledForLittleBlackDress;          hat = HatEnabledForLittleBlackDress;          choker = ChokerEnabledForLittleBlackDress;          break;
            case OutfitType.CasualPantSuit:            wings = WingsEnabledForCasualPantsuit;            overall = ApronEnabledForCasualPantsuit;            hat = HatEnabledForCasualPantsuit;            choker = ChokerEnabledForCasualPantsuit;            break;
            case OutfitType.Conservative:              wings = WingsEnabledForConservative;              overall = ApronEnabledForConservative;              hat = HatEnabledForConservative;              choker = ChokerEnabledForConservative;            break;
            case OutfitType.Casual3:                   wings = WingsEnabledForFrootDress;                overall = ApronEnabledForFrootDress;                hat = HatEnabledForFrootDress;                choker = ChokerEnabledForFrootDress;                break;
            case OutfitType.StraplessRuffleDress:      wings = WingsEnabledForStraplessRuffleDress;      overall = ApronEnabledForStraplessRuffleDress;      hat = HatEnabledForStraplessRuffleDress;      choker = ChokerEnabledForStraplessRuffleDress;      break;
            case OutfitType.CheckBodySuit:             wings = WingsEnabledForCheckBodySuit;             overall = ApronEnabledForCheckBodySuit;             hat = HatEnabledForCheckBodySuit;             choker = ChokerEnabledForCheckBodySuit;             break;
            case OutfitType.ElegantDress:              wings = WingsEnabledForElegantDress;              overall = ApronEnabledForElegantDress;              hat = HatEnabledForElegantDress;              choker = ChokerEnabledForElegantDress;              break;
            case OutfitType.NightOutRuffle:            wings = WingsEnabledForNightOutRuffle;            overall = ApronEnabledForNightOutRuffle;            hat = HatEnabledForNightOutRuffle;            choker = ChokerEnabledForNightOutRuffle;            break;
            case OutfitType.Wedding:                   wings = WingsEnabledForWedding;                   overall = ApronEnabledForWedding;                   hat = HatEnabledForWedding;                   choker = ChokerEnabledForWedding;                   break;
            case OutfitType.Funeral:                   wings = WingsEnabledForFuneral;                   overall = ApronEnabledForFuneral;                   hat = HatEnabledForFuneral;                   choker = ChokerEnabledForFuneral;                   break;
            case OutfitType.Homelessness:              wings = WingsEnabledForHomelessness;              overall = ApronEnabledForHomelessness;              hat = HatEnabledForHomelessness;              choker = ChokerEnabledForHomelessness;              break;
            case OutfitType.Undergarments:             wings = WingsEnabledForUndergarments;             overall = ApronEnabledForUndergarments;             hat = HatEnabledForUndergarments;             choker = ChokerEnabledForUndergarments;             break;
            case OutfitType.Lingerie:                  wings = WingsEnabledForLingerie;                  overall = ApronEnabledForLingerie;                  hat = HatEnabledForLingerie;                  choker = ChokerEnabledForLingerie;                  break;
            case OutfitType.Fae:                       wings = WingsEnabledForFae;                       overall = ApronEnabledForFae;                       hat = HatEnabledForFae;                       choker = ChokerEnabledForFae;                       break;
            default:                                   wings = false; overall = false; hat = false;       choker = false;                                                                                    break;
        }
    }

    // FIX: Added RisqueNightie to the skip list so it is excluded from Next/Previous cycling
    private static bool IsUndergarmentOutfit(int index)
    {
        return index == (int)OutfitType.None
            || index == (int)OutfitType.Undergarments
            || index == (int)OutfitType.Lingerie
            || index == (int)OutfitType.Fae
            || index == (int)OutfitType.RisqueNightie;
    }

    public void NextOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) { return; }
        int total = System.Enum.GetValues(typeof(OutfitType)).Length;
        int index = (int)currentOutfit;
        do { index = (index + 1) % total; }
        while (IsUndergarmentOutfit(index) || !IsOutfitEnabled((OutfitType)index));
        SwitchToOutfit((OutfitType)index);
    }

    public void PreviousOutfit()
    {
        if (GameMaster.Instance.PLAYERBUSY) { return; }
        int total = System.Enum.GetValues(typeof(OutfitType)).Length;
        int index = (int)currentOutfit;
        do { index = (index - 1 + total) % total; }
        while (IsUndergarmentOutfit(index) || !IsOutfitEnabled((OutfitType)index));
        SwitchToOutfit((OutfitType)index);
    }

    private void SwitchToOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:                      ToggleWorkOutfit(true);                      break;
            case OutfitType.WorkTwo:                   ToggleWorkTwoOutfit(true);                   break;
            case OutfitType.WorkThree:                 ToggleWorkThreeOutfit(true);                 break;
            case OutfitType.WorkFour:                  ToggleWorkFourOutfit(true);                  break;
            case OutfitType.WorkSuitThree:             ToggleWorkSuitThreeOutfit(true);             break;
            case OutfitType.Casual:                    ToggleCasualOutfit(true);                    break;
            case OutfitType.ShortsAndTights:           ToggleShortsAndTightsOutfit(true);           break;
            case OutfitType.Fitness:                   ToggleFitnessOutfit(true);                   break;
            case OutfitType.Pyjamas:                   TogglePyjamasOutfit(true);                   break;
            case OutfitType.Housecoat:                 ToggleHousecoatOutfit(true);                 break;
            case OutfitType.Nightie:                   ToggleNightie(true);                         break;
            case OutfitType.StealthSuit:               ToggleStealthSuit(true);                     break;
            case OutfitType.RisqueNightie:             ToggleRisqueNightie(true);                   break;
            case OutfitType.Date1:                     ToggleFirstDateOutfit(true);                 break; 
            case OutfitType.Date2:                     ToggleSecondDateOutfit(true);                break; 
            case OutfitType.Date3:                     ToggleThirdDateOutfit(true);                 break;
            case OutfitType.GreyCheckHalterDress:      ToggleGreyCheckHalterDressOutfit(true);      break;
            case OutfitType.SweaterAndSkirt:           ToggleSweaterAndSkirtOutfit(true);           break;
            case OutfitType.TurtleneckAndSkirt:        ToggleTurtleneckAndSkirtOutfit(true);        break;
            case OutfitType.CheckTopAndJeans:          ToggleCheckTopAndJeansOutfit(true);          break;
            case OutfitType.KnottedBlousseAndSkirt:    ToggleKnottedBlousseAndSkirtOutfit(true);    break;
            case OutfitType.RuffleBlousseAndSkirt:     ToggleRuffleBlousseAndSkirtOutfit(true);     break;
            case OutfitType.LooseTopAndLongSkirt:      ToggleLooseTopAndLongSkirtOutfit(true);      break;
            case OutfitType.TurtleneckAndMediumSkirt:  ToggleTurtleneckAndMediumSkirtOutfit(true);  break;
            case OutfitType.Conservative:              ToggleConservativeOutfit(true);              break;
            case OutfitType.WoolenJumper:              ToggleWoolenJumperOutfit(true);              break;
            case OutfitType.Edea:                      ToggleEdeaOutfit(true);                      break;
            case OutfitType.DitzyDress:                ToggleDitzyDressOutfit(true);                break;
            case OutfitType.LittleBlackDress:          ToggleLittleBlackDressOutfit(true);          break;
            case OutfitType.CasualPantSuit:            ToggleCasualPantsuitOutfit(true);            break; 
            case OutfitType.Casual3:                   ToggleFrootDressOutfit(true);                break;
            case OutfitType.StraplessRuffleDress:      ToggleStraplessRuffleDressOutfit(true);      break;
            case OutfitType.CheckBodySuit:             ToggleCheckBodySuitOutfit(true);             break;
            case OutfitType.ElegantDress:              ToggleElegantDressOutfit(true);              break;
            case OutfitType.NightOutRuffle:            ToggleNightOutRuffleOutfit(true);            break;
            case OutfitType.Wedding:                   ToggleWeddingOutfit(true);                   break;
            case OutfitType.Funeral:                   ToggleFuneralOutfit(true);                   break;
            case OutfitType.Homelessness:              ToggleHomelessnessOutfit(true);              break;
            case OutfitType.Lingerie:                  ToggleLingerieOutfit(true);                  break;
            case OutfitType.Fae:                       ToggleFaeOutfit(true);                       break;
            default:                                   DisableAllMainOutfits();                     break;
        }
    }

    public void ToggleWorkOutfit(bool? forceOn = null)                     { JiggleToggle(JiggleForWorkOne);               ToggleMainOutfit(OutfitType.Work,                     forceOn); }
    public void ToggleWorkTwoOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleForWorkTwo);               ToggleMainOutfit(OutfitType.WorkTwo,                  forceOn); }
    public void ToggleWorkThreeOutfit(bool? forceOn = null)                { JiggleToggle(JiggleForWorkThree);             ToggleMainOutfit(OutfitType.WorkThree,                forceOn); }
    public void ToggleWorkFourOutfit(bool? forceOn = null)                 { JiggleToggle(JiggleForWorkFour);              ToggleMainOutfit(OutfitType.WorkFour,                 forceOn); }
    public void ToggleWorkSuitThreeOutfit(bool? forceOn = null)            { JiggleToggle(JiggleWorkSuitThree);            ToggleMainOutfit(OutfitType.WorkSuitThree,            forceOn); }
    public void ToggleCasualOutfit(bool? forceOn = null)                   { JiggleToggle(JiggleCasually);                 ToggleMainOutfit(OutfitType.Casual,                   forceOn); }
    public void ToggleShortsAndTightsOutfit(bool? forceOn = null)          { JiggleToggle(JiggleShortsAndTights);          ToggleMainOutfit(OutfitType.ShortsAndTights,          forceOn); }
    public void ToggleFitnessOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleFitness);                  ToggleMainOutfit(OutfitType.Fitness,                  forceOn); }
    public void TogglePyjamasOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleInPyjamas);                ToggleMainOutfit(OutfitType.Pyjamas,                  forceOn); }
    public void ToggleHousecoatOutfit(bool? forceOn = null)                { JiggleToggle(JiggleInHousecoat);              ToggleMainOutfit(OutfitType.Housecoat,                forceOn); }
    public void ToggleNightie(bool? forceOn = null)                        { JiggleToggle(JiggleNightie);                  ToggleMainOutfit(OutfitType.Nightie,                  forceOn); }
    public void ToggleRisqueNightie(bool? forceOn = null)                  { JiggleToggle(JiggleRisqueNightie);            ToggleMainOutfit(OutfitType.RisqueNightie,            forceOn); }
    public void ToggleStealthSuit(bool? forceOn = null)                    { JiggleToggle(JiggleStealthSuit);              ToggleMainOutfit(OutfitType.StealthSuit,              forceOn); }
    public void ToggleFirstDateOutfit(bool? forceOn = null)                { JiggleToggle(JiggleOnAFirstDate);             ToggleMainOutfit(OutfitType.Date1,                    forceOn); }
    public void ToggleSecondDateOutfit(bool? forceOn = null)               { JiggleToggle(JiggleOnASecondDate);            ToggleMainOutfit(OutfitType.Date2,                    forceOn); }
    public void ToggleThirdDateOutfit(bool? forceOn = null)                { JiggleToggle(JiggleOnAThirdDate);             ToggleMainOutfit(OutfitType.Date3,                    forceOn); }
    public void ToggleGreyCheckHalterDressOutfit(bool? forceOn = null)     { JiggleToggle(JiggleOnAGreyCheckHalterDress);  ToggleMainOutfit(OutfitType.GreyCheckHalterDress,     forceOn); }
    public void ToggleSweaterAndSkirtOutfit(bool? forceOn = null)          { JiggleToggle(JiggleSweaterAndSkirt);          ToggleMainOutfit(OutfitType.SweaterAndSkirt,          forceOn); }
    public void ToggleTurtleneckAndSkirtOutfit(bool? forceOn = null)       { JiggleToggle(JiggleTurtleneckAndSkirt);       ToggleMainOutfit(OutfitType.TurtleneckAndSkirt,       forceOn); }
    public void ToggleCheckTopAndJeansOutfit(bool? forceOn = null)         { JiggleToggle(JiggleCheckTopAndJeans);         ToggleMainOutfit(OutfitType.CheckTopAndJeans,         forceOn); }
    public void ToggleKnottedBlousseAndSkirtOutfit(bool? forceOn = null)   { JiggleToggle(JiggleKnottedBlousseAndSkirt);   ToggleMainOutfit(OutfitType.KnottedBlousseAndSkirt,   forceOn); }
    public void ToggleRuffleBlousseAndSkirtOutfit(bool? forceOn = null)    { JiggleToggle(JiggleRuffleBlousseAndSkirt);    ToggleMainOutfit(OutfitType.RuffleBlousseAndSkirt,    forceOn); }
    public void ToggleLooseTopAndLongSkirtOutfit(bool? forceOn = null)     { JiggleToggle(JiggleLooseTopAndLongSkirt);     ToggleMainOutfit(OutfitType.LooseTopAndLongSkirt,     forceOn); }
    public void ToggleTurtleneckAndMediumSkirtOutfit(bool? forceOn = null) { JiggleToggle(JiggleTurtleneckAndMediumSkirt); ToggleMainOutfit(OutfitType.TurtleneckAndMediumSkirt, forceOn); }
    public void ToggleWoolenJumperOutfit(bool? forceOn = null)             { JiggleToggle(JiggleWoolenJumper);             ToggleMainOutfit(OutfitType.WoolenJumper,             forceOn); }
    public void ToggleEdeaOutfit(bool? forceOn = null)                     { JiggleToggle(JiggleCasuallyFour);             ToggleMainOutfit(OutfitType.Edea,                     forceOn); }
    public void ToggleDitzyDressOutfit(bool? forceOn = null)               { JiggleToggle(JiggleDitzyDress);               ToggleMainOutfit(OutfitType.DitzyDress,               forceOn); }
    public void ToggleLittleBlackDressOutfit(bool? forceOn = null)         { JiggleToggle(JiggleLittleBlackDress);         ToggleMainOutfit(OutfitType.LittleBlackDress,         forceOn); }
    public void ToggleCasualPantsuitOutfit(bool? forceOn = null)           { JiggleToggle(JiggleCasualPantsuit);           ToggleMainOutfit(OutfitType.CasualPantSuit,           forceOn); }
    public void ToggleConservativeOutfit(bool? forceOn = null)             { JiggleToggle(JiggleConservative);             ToggleMainOutfit(OutfitType.Conservative,             forceOn); }
    public void ToggleFrootDressOutfit(bool? forceOn = null)               { JiggleToggle(JiggleCasuallyThree);            ToggleMainOutfit(OutfitType.Casual3,                  forceOn); }
    public void ToggleStraplessRuffleDressOutfit(bool? forceOn = null)     { JiggleToggle(JiggleStraplessRuffleDress);     ToggleMainOutfit(OutfitType.StraplessRuffleDress,     forceOn); }
    public void ToggleCheckBodySuitOutfit(bool? forceOn = null)            { JiggleToggle(JiggleCheckBodySuit);            ToggleMainOutfit(OutfitType.CheckBodySuit,            forceOn); }
    public void ToggleElegantDressOutfit(bool? forceOn = null)             { JiggleToggle(JiggleElegantDress);             ToggleMainOutfit(OutfitType.ElegantDress,             forceOn); }
    public void ToggleNightOutRuffleOutfit(bool? forceOn = null)           { JiggleToggle(JiggleNightOutRuffle);           ToggleMainOutfit(OutfitType.NightOutRuffle,           forceOn); }
    public void ToggleWeddingOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleAtAWedding);               ToggleMainOutfit(OutfitType.Wedding,                  forceOn); }
    public void ToggleFuneralOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleAtAFuneral);               ToggleMainOutfit(OutfitType.Funeral,                  forceOn); }
    public void ToggleHomelessnessOutfit(bool? forceOn = null)             { JiggleToggle(JiggleWhileHomeless);            ToggleMainOutfit(OutfitType.Homelessness,             forceOn); }
    public void ToggleUndergarments(bool? forceOn = null)                  { JiggleToggle(JiggleUndergarments);            ToggleMainOutfit(OutfitType.Undergarments,            forceOn); }
    public void ToggleLingerieOutfit(bool? forceOn = null)                 { JiggleToggle(JiggleLingerie);                 ToggleMainOutfit(OutfitType.Lingerie,                 forceOn); }
    public void ToggleFaeOutfit(bool? forceOn = null)                      { JiggleToggle(JiggleFae);                      ToggleMainOutfit(OutfitType.Fae,                      forceOn); }

    private void JiggleToggle(bool isJiggly = false)
    {
        if (JiggleLeftBoob       != null) { JiggleLeftBoob.SetActive(isJiggly); }
        if (JiggleRightBoob      != null) { JiggleRightBoob.SetActive(isJiggly); }
        if (JiggleLeftButtcheek  != null) { JiggleLeftButtcheek.SetActive(isJiggly); }
        if (JiggleRightButtcheek != null) { JiggleRightButtcheek.SetActive(isJiggly); }
    }

    private void ToggleMainOutfit(OutfitType outfit, bool? forceOn = null)
    {
        if (forceOn.HasValue)
        {
            if (forceOn.Value) { SetMainOutfit(outfit); }
            else               { DisableAllMainOutfits(); }
        }
        else
        {
            if (currentOutfit == outfit) { DisableAllMainOutfits(); }
            else                         { SetMainOutfit(outfit); }
        }
    }

    public void SetMainOutfit(OutfitType outfit)
    {
        HideAllAccessories();
        ClearOutfitMeshes();
        if (DefaultHair != null) { DefaultHair.enabled = false; }
        currentOutfit = outfit;
        switch (outfit)
        {
            case OutfitType.Work:                      InstantiatePrefabs(OutfitForWorkOne);                  break;
            case OutfitType.WorkTwo:                   InstantiatePrefabs(OutfitForWorkOneTwo);               break;
            case OutfitType.WorkThree:                 InstantiatePrefabs(OutfitForWorkOneThree);             break;
            case OutfitType.WorkFour:                  InstantiatePrefabs(OutfitForWorkOneFour);              break;
            case OutfitType.WorkSuitThree:             InstantiatePrefabs(OutfitForWorkSuitThree);            break;
            case OutfitType.Casual:                    InstantiatePrefabs(OutfitForCasual);                   break;
            case OutfitType.ShortsAndTights:           InstantiatePrefabs(OutfitForShortsAndTights);          break;
            case OutfitType.Fitness:                   InstantiatePrefabs(OutfitForFitness);                  break;
            case OutfitType.Pyjamas:                   InstantiatePrefabs(OutfitForPyjamas);                  break;
            case OutfitType.Housecoat:                 InstantiatePrefabs(OutfitForHousecoat);                break;
            case OutfitType.Nightie:                   InstantiatePrefabs(OutfitForNightie);                  break;
            case OutfitType.RisqueNightie:             InstantiatePrefabs(OutfitForRisqueNightie);            break;
            case OutfitType.StealthSuit:               InstantiatePrefabs(OutfitForStealthSuit);              break;
            case OutfitType.Date1:                     InstantiatePrefabs(OutfitForFirstDate);                break; 
            case OutfitType.Date2:                     InstantiatePrefabs(OutfitForSecondDate);               break; 
            case OutfitType.Date3:                     InstantiatePrefabs(OutfitForThirdDate);                break;
            case OutfitType.GreyCheckHalterDress:      InstantiatePrefabs(OutfitForGreyCheckHalterDress);     break;
            case OutfitType.SweaterAndSkirt:           InstantiatePrefabs(OutfitForSweaterAndSkirt);          break;
            case OutfitType.TurtleneckAndSkirt:        InstantiatePrefabs(OutfitForTurtleneckAndSkirt);       break;
            case OutfitType.CheckTopAndJeans:          InstantiatePrefabs(OutfitForCheckTopAndJeans);         break;
            case OutfitType.KnottedBlousseAndSkirt:    InstantiatePrefabs(OutfitForKnottedBlousseAndSkirt);   break;
            case OutfitType.RuffleBlousseAndSkirt:     InstantiatePrefabs(OutfitForRuffleBlousseAndSkirt);    break;
            case OutfitType.LooseTopAndLongSkirt:      InstantiatePrefabs(OutfitForLooseTopAndLongSkirt);     break;
            case OutfitType.TurtleneckAndMediumSkirt:  InstantiatePrefabs(OutfitForTurtleneckAndMediumSkirt); break;
            case OutfitType.WoolenJumper:              InstantiatePrefabs(OutfitForWoolenJumper);             break;
            case OutfitType.Edea:                      InstantiatePrefabs(OutfitForEdea);                     break;
            case OutfitType.DitzyDress:                InstantiatePrefabs(OutfitForDitzyDress);               break;
            case OutfitType.LittleBlackDress:          InstantiatePrefabs(OutfitForLittleBlackDress);         break;
            case OutfitType.CasualPantSuit:            InstantiatePrefabs(OutfitCasualPantsuit);              break;
            case OutfitType.Conservative:              InstantiatePrefabs(OutfitConservative);                break;
            case OutfitType.Casual3:                   InstantiatePrefabs(OutfitForFrootDress);               break;
            case OutfitType.StraplessRuffleDress:      InstantiatePrefabs(OutfitForStraplessRuffleDress);     break;
            case OutfitType.CheckBodySuit:             InstantiatePrefabs(OutfitForCheckBodySuit);            break;
            case OutfitType.ElegantDress:              InstantiatePrefabs(OutfitForElegantDress);             break;
            case OutfitType.NightOutRuffle:            InstantiatePrefabs(OutfitForNightOutRuffle);           break;
            case OutfitType.Wedding:                   InstantiatePrefabs(OutfitForWedding);                  break;
            case OutfitType.Funeral:                   InstantiatePrefabs(OutfitForFuneral);                  break;
            case OutfitType.Homelessness:              InstantiatePrefabs(OutfitForHomelessness);             break;
            case OutfitType.Undergarments:             InstantiatePrefabs(OutfitForUndergarments);            break;
            case OutfitType.Lingerie:                  InstantiatePrefabs(OutfitForLingerie);                 break;
            case OutfitType.Fae:                       InstantiatePrefabs(OutfitForFae);                      break;
        }
        ApplyBodyColors(outfit);
        ApplyAccessories();
        
        EventManager.OutfitChanged(outfit);
    }

    public void DisableAllMainOutfits()
    {
        HideAllAccessories();
        ClearOutfitMeshes();
        currentOutfit = OutfitType.None;
        if (DefaultHair != null) { DefaultHair.enabled = true; }
        ApplyAccessories();
    }

    private OutfitType PickRandom(OutfitType[] pool)
    {
        OutfitType[] eligible = pool.Where(o => IsOutfitEnabled(o) && o != currentOutfit).ToArray();
        if (eligible.Length == 0)
        {
            eligible = pool.Where(o => IsOutfitEnabled(o)).ToArray();
        }
        if (eligible.Length == 0) { return currentOutfit; }
        return eligible[Random.Range(0, eligible.Length)];
    }

    public void SetRandomWorkOutfit()
    {
        OutfitType[] pool = { OutfitType.Work, OutfitType.WorkTwo, OutfitType.WorkThree, OutfitType.WorkFour, OutfitType.WorkSuitThree };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomCasualOutfit()
    {
        OutfitType[] pool = { OutfitType.Casual, OutfitType.ShortsAndTights };
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
            OutfitType.GreyCheckHalterDress,   OutfitType.SweaterAndSkirt,       OutfitType.TurtleneckAndSkirt,       OutfitType.CheckTopAndJeans,
            OutfitType.KnottedBlousseAndSkirt,  OutfitType.RuffleBlousseAndSkirt, OutfitType.LooseTopAndLongSkirt,     OutfitType.TurtleneckAndMediumSkirt,
            OutfitType.WoolenJumper,            OutfitType.Edea,                  OutfitType.DitzyDress,               OutfitType.LittleBlackDress, 
            OutfitType.Conservative, OutfitType.CasualPantSuit
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomNightOutOutfit()
    {
        OutfitType[] pool = { OutfitType.Casual3, OutfitType.StraplessRuffleDress, OutfitType.CheckBodySuit, OutfitType.ElegantDress, OutfitType.NightOutRuffle };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomRisqueOutfit()
    {
        OutfitType[] pool = { OutfitType.Lingerie, OutfitType.Fae, OutfitType.RisqueNightie, OutfitType.StealthSuit };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomStorylineOutfit()
    {
        OutfitType[] pool = { OutfitType.Wedding, OutfitType.Funeral, OutfitType.Homelessness };
        SwitchToOutfit(PickRandom(pool));
    }

    private void ApplyBodyColors(OutfitType outfit)
    {
        if (Body == null) { return; }
        Color lipColor  = GetLipColorForOutfit(outfit);
        Color nailColor = GetNailColorForOutfit(outfit);
        Material[] materials = Application.isPlaying ? Body.materials : Body.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null) { continue; }
            string matName = materials[i].name.ToLower();
            if (matName.Contains("lips"))                                        { materials[i].color = lipColor; }
            else if (matName.Contains("fingernail") || matName.Contains("nail")) { materials[i].color = nailColor; }
        }
        if (Application.isPlaying) { Body.materials = materials; }
    }

    private Color GetLipColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:                      return lipsColorForWorkOne;
            case OutfitType.WorkTwo:                   return lipsColorForWorkTwo;
            case OutfitType.WorkThree:                 return lipsColorForWorkThree;
            case OutfitType.WorkFour:                  return lipsColorForWorkFour;
            case OutfitType.WorkSuitThree:             return lipsColorForWorkSuitThree;
            case OutfitType.Casual:                    return lipsColorForCasual;
            case OutfitType.ShortsAndTights:           return lipsColorForShortsAndTights;
            case OutfitType.Fitness:                   return lipsColorForFitness;
            case OutfitType.Pyjamas:                   return lipsColorForPyjamas;
            case OutfitType.Housecoat:                 return lipsColorForHousecoat;
            case OutfitType.Nightie:                   return lipsColorForNightie;
            case OutfitType.RisqueNightie:             return lipsColorForRisqueNightie;
            case OutfitType.StealthSuit:               return lipsColorForStealthSuit;
            case OutfitType.Date1:                     return lipsColorForFirstDate;   
            case OutfitType.Date2:                     return lipsColorForSecondDate;  
            case OutfitType.Date3:                     return lipsColorForThirdDate;
            case OutfitType.GreyCheckHalterDress:      return lipsColorForGreyCheckHalterDress;
            case OutfitType.SweaterAndSkirt:           return lipsColorForSweaterAndSkirt;
            case OutfitType.TurtleneckAndSkirt:        return lipsColorForTurtleneckAndSkirt;
            case OutfitType.CheckTopAndJeans:          return lipsColorForCheckTopAndJeans;
            case OutfitType.KnottedBlousseAndSkirt:    return lipsColorForKnottedBlousseAndSkirt;
            case OutfitType.RuffleBlousseAndSkirt:     return lipsColorForRuffleBlousseAndSkirt;
            case OutfitType.LooseTopAndLongSkirt:      return lipsColorForLooseTopAndLongSkirt;
            case OutfitType.TurtleneckAndMediumSkirt:  return lipsColorForTurtleneckAndMediumSkirt;
            case OutfitType.WoolenJumper:              return lipsColorForWoolenJumper;
            case OutfitType.Edea:                      return lipsColorForEdea;
            case OutfitType.DitzyDress:                return lipsColorForDitzyDress;
            case OutfitType.LittleBlackDress:          return lipsColorForLittleBlackDress;
            case OutfitType.CasualPantSuit:            return lipsColorForCasualPantsuit;
            case OutfitType.Conservative:              return lipsColorForConservative;
            case OutfitType.Casual3:                   return lipsColorForFrootDress;
            case OutfitType.StraplessRuffleDress:      return lipsColorForStraplessRuffleDress;
            case OutfitType.CheckBodySuit:             return lipsColorForCheckBodySuit;
            case OutfitType.ElegantDress:              return lipsColorForElegantDress;
            case OutfitType.NightOutRuffle:            return lipsColorForNightOutRuffle;
            case OutfitType.Wedding:                   return lipsColorForWedding;
            case OutfitType.Funeral:                   return lipsColorForFuneral;
            case OutfitType.Homelessness:              return lipsColorForHomelessness;
            case OutfitType.Undergarments:             return lipsColorForUndergarments;
            case OutfitType.Lingerie:                  return lipsColorForLingerie;
            case OutfitType.Fae:                       return lipsColorForFae;
            default:                                   return Color.white;
        }
    }

    private Color GetNailColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:                      return nailsColorForWorkOne;
            case OutfitType.WorkTwo:                   return nailsColorForWorkTwo;
            case OutfitType.WorkThree:                 return nailsColorForWorkThree;
            case OutfitType.WorkFour:                  return nailsColorForWorkFour;
            case OutfitType.WorkSuitThree:             return nailsColorForWorkSuitThree;
            case OutfitType.Casual:                    return nailsColorForCasual;
            case OutfitType.ShortsAndTights:           return nailsColorForShortsAndTights;
            case OutfitType.Fitness:                   return nailsColorForFitness;
            case OutfitType.Pyjamas:                   return nailsColorForPyjamas;
            case OutfitType.Housecoat:                 return nailsColorForHousecoat;
            case OutfitType.Nightie:                   return nailsColorForNightie;
            case OutfitType.RisqueNightie:             return nailsColorForRisqueNightie;
            case OutfitType.StealthSuit:               return nailsColorStealthSuit;
            case OutfitType.Date1:                     return nailsColorForFirstDate;   
            case OutfitType.Date2:                     return nailsColorForSecondDate;  
            case OutfitType.Date3:                     return nailsColorForThirdDate;
            case OutfitType.GreyCheckHalterDress:      return nailsColorForGreyCheckHalterDress;
            case OutfitType.SweaterAndSkirt:           return nailsColorForSweaterAndSkirt;
            case OutfitType.TurtleneckAndSkirt:        return nailsColorForTurtleneckAndSkirt;
            case OutfitType.CheckTopAndJeans:          return nailsColorForCheckTopAndJeans;
            case OutfitType.KnottedBlousseAndSkirt:    return nailsColorForKnottedBlousseAndSkirt;
            case OutfitType.RuffleBlousseAndSkirt:     return nailsColorForRuffleBlousseAndSkirt;
            case OutfitType.LooseTopAndLongSkirt:      return nailsColorForLooseTopAndLongSkirt;
            case OutfitType.TurtleneckAndMediumSkirt:  return nailsColorForTurtleneckAndMediumSkirt;
            case OutfitType.WoolenJumper:              return nailsColorForWoolenJumper;
            case OutfitType.Edea:                      return nailsColorForEdea;
            case OutfitType.DitzyDress:                return nailsColorForDitzyDress;
            case OutfitType.LittleBlackDress:          return nailsColorForLittleBlackDress;
            case OutfitType.CasualPantSuit:            return nailsColorForCasualPantsuit;
            case OutfitType.Conservative:              return nailsColorForConservative;
            case OutfitType.Casual3:                   return nailsColorForFrootDress;
            case OutfitType.StraplessRuffleDress:      return nailsColorForStraplessRuffleDress;
            case OutfitType.CheckBodySuit:             return nailsColorForCheckBodySuit;
            case OutfitType.ElegantDress:              return nailsColorForElegantDress;
            case OutfitType.NightOutRuffle:            return nailsColorForNightOutRuffle;
            case OutfitType.Wedding:                   return nailsColorForWedding;
            case OutfitType.Funeral:                   return nailsColorForFuneral;
            case OutfitType.Homelessness:              return nailsColorForHomelessness;
            case OutfitType.Undergarments:             return nailsColorForUndergarments;
            case OutfitType.Lingerie:                  return nailsColorForLingerie;
            case OutfitType.Fae:                       return nailsColorForFae;
            default:                                   return Color.white;
        }
    }

    public void Undress()
    {
        DisableAllMainOutfits();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Me))]
public class MeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Me me = (Me)target;

        EditorGUILayout.LabelField("Outfit Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Work Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Classic Nora Dress",       GUILayout.Height(35))) { me.ToggleWorkOutfit();          EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Black Skirt & Cream Shirt", GUILayout.Height(35))) { me.ToggleWorkThreeOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Tartan & Grey Dress",      GUILayout.Height(35))) { me.ToggleWorkTwoOutfit();       EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Black Suit & Shirt",       GUILayout.Height(35))) { me.ToggleWorkFourOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Suit Jacket & Trousers",   GUILayout.Height(35))) { me.ToggleWorkSuitThreeOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Casual Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Uni Sweater & Jeans", GUILayout.Height(35))) { me.ToggleCasualOutfit();          EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Shorts & Tights",     GUILayout.Height(35))) { me.ToggleShortsAndTightsOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fitness",             GUILayout.Height(35))) { me.ToggleFitnessOutfit();         EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("PJs", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pyjamas",   GUILayout.Height(35))) { me.TogglePyjamasOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Housecoat", GUILayout.Height(35))) { me.ToggleHousecoatOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Nightie",   GUILayout.Height(35))) { me.ToggleNightie();         EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Dating", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("First Date",  GUILayout.Height(35))) { me.ToggleFirstDateOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Second Date", GUILayout.Height(35))) { me.ToggleSecondDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Third Date",  GUILayout.Height(35))) { me.ToggleThirdDateOutfit();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Cute Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Grey Check Halter Dress", GUILayout.Height(35))) { me.ToggleGreyCheckHalterDressOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Sweater & Skirt",         GUILayout.Height(35))) { me.ToggleSweaterAndSkirtOutfit();        EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Turtleneck & Skirt",      GUILayout.Height(35))) { me.ToggleTurtleneckAndSkirtOutfit();     EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check Top & Jeans",       GUILayout.Height(35))) { me.ToggleCheckTopAndJeansOutfit();       EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Knotted Blousse & Skirt", GUILayout.Height(35))) { me.ToggleKnottedBlousseAndSkirtOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Ruffle Blousse & Skirt",    GUILayout.Height(35))) { me.ToggleRuffleBlousseAndSkirtOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Loose Top & Long Skirt",    GUILayout.Height(35))) { me.ToggleLooseTopAndLongSkirtOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Turtleneck & Medium Skirt", GUILayout.Height(35))) { me.ToggleTurtleneckAndMediumSkirtOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wooly Jumper & Tights", GUILayout.Height(35))) { me.ToggleWoolenJumperOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Ditzy Dress",           GUILayout.Height(35))) { me.ToggleDitzyDressOutfit();       EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Little Black Dress",    GUILayout.Height(35))) { me.ToggleLittleBlackDressOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Casual Pantsuit", GUILayout.Height(35))) { me.ToggleCasualPantsuitOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Conservative Jumper & Skirt", GUILayout.Height(35))) { me.ToggleConservativeOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Edea's Dress",    GUILayout.Height(35))) { me.ToggleEdeaOutfit();           EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Night Out Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Froot Dress",            GUILayout.Height(35))) { me.ToggleFrootDressOutfit();           EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Strapless Ruffle Dress", GUILayout.Height(35))) { me.ToggleStraplessRuffleDressOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Check Body Suit",        GUILayout.Height(35))) { me.ToggleCheckBodySuitOutfit();        EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Elegant Dress",        GUILayout.Height(35))) { me.ToggleElegantDressOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Layered Ruffle Dress", GUILayout.Height(35))) { me.ToggleNightOutRuffleOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Storyline Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wedding",  GUILayout.Height(35))) { me.ToggleWeddingOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Funeral",  GUILayout.Height(35))) { me.ToggleFuneralOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Skid Row", GUILayout.Height(35))) { me.ToggleHomelessnessOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Accessories (toggle independently)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Wings",   GUILayout.Height(35))) { me.ToggleWings();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Overall", GUILayout.Height(35))) { me.ToggleOverall(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Hat",     GUILayout.Height(35))) { me.ToggleHat();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Choker",  GUILayout.Height(35))) { me.ToggleChoker();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Undergarments", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Lingerie",       GUILayout.Height(35))) { me.ToggleLingerieOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Underwear",      GUILayout.Height(35))) { me.ToggleUndergarments();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fae",            GUILayout.Height(35))) { me.ToggleFaeOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Risque Nightie", GUILayout.Height(35))) { me.ToggleRisqueNightie();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Stealth Suit",   GUILayout.Height(35))) { me.ToggleStealthSuit();    EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Nothin' At All", GUILayout.Height(35))) { me.Undress(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Work",   GUILayout.Height(40))) { me.SetRandomWorkOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Casual", GUILayout.Height(40))) { me.SetRandomCasualOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Dating", GUILayout.Height(40))) { me.SetRandomDatingOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Cute",   GUILayout.Height(40))) { me.SetRandomCuteOutfit();   EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Night Out", GUILayout.Height(40))) { me.SetRandomNightOutOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Pyjamas",   GUILayout.Height(40))) { me.SetRandomPyjamas();        EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Storyline",     GUILayout.Height(40))) { me.SetRandomStorylineOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Risque Outfit", GUILayout.Height(40))) { me.SetRandomRisqueOutfit();    EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("---------------------", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
#endif

public enum OutfitType
{
    None,
    Work,
    WorkTwo,
    WorkThree,
    WorkFour,
    WorkSuitThree,
    Casual,
    Fitness,
    Pyjamas,
    Housecoat,
    Nightie,
    RisqueNightie,
    Date1,
    Date2,
    Date3,
    GreyCheckHalterDress,
    SweaterAndSkirt,
    TurtleneckAndSkirt,
    CheckTopAndJeans,
    KnottedBlousseAndSkirt,
    RuffleBlousseAndSkirt,
    LooseTopAndLongSkirt,
    TurtleneckAndMediumSkirt,
    WoolenJumper,
    Edea,
    DitzyDress,
    LittleBlackDress,
    CasualPantSuit,
    Casual3,
    StraplessRuffleDress,
    CheckBodySuit,
    ElegantDress,
    NightOutRuffle,
    Wedding,
    Funeral,
    Homelessness,
    Lingerie,
    ShortsAndTights,
    Fae,
    Undergarments,
    StealthSuit,
    Conservative
}