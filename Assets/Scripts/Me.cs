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

    [Header("Hair")]
    public SkinnedMeshRenderer DefaultHair;
    public SkinnedMeshRenderer WorkHair;
    public SkinnedMeshRenderer WorkHairTwo;
    public SkinnedMeshRenderer CasualHair;
    public SkinnedMeshRenderer PyjamaHair;
    public SkinnedMeshRenderer DatingHair;
    public SkinnedMeshRenderer DatingHairTwo;
    public SkinnedMeshRenderer OutHair;
    public SkinnedMeshRenderer HomelessHair;
    public SkinnedMeshRenderer TwinTailsHair;
    public SkinnedMeshRenderer UpHair;
    public SkinnedMeshRenderer UpDo;
    public SkinnedMeshRenderer MessyHair;

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
    public OutfitHair hairForWorkOne = OutfitHair.DefaultHair;

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
    public OutfitHair hairForWorkTwo = OutfitHair.DefaultHair;

    [Header("Tartan Dress")]
    public bool WorkThreeOutfitEnabled = true;
    public List<GameObject> OutfitForWorkOneThree;
    public Color lipsColorForWorkThree = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColorForWorkThree = new Color(0.9f, 0.7f, 0.8f);
    public bool JiggleForWorkThree;
    public bool WingsEnabledForWorkThree;
    public bool ApronEnabledForWorkThree;
    public bool HatEnabledForWorkThree;
    public bool ChokerEnabledForWorkThree;
    public OutfitHair hairForWorkThree = OutfitHair.DefaultHair;

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
    public OutfitHair hairForWorkFour = OutfitHair.DefaultHair;

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
    public OutfitHair hairForWorkSuitThree = OutfitHair.DefaultHair;

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
    public OutfitHair hairForCasual = OutfitHair.DefaultHair;

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
    public OutfitHair hairForShortsAndTights = OutfitHair.DefaultHair;

    [Header("Clean Bandit")]
    public bool CleanBanditEnabled = true;
    public List<GameObject> OutfitForCleanBandit;
    public Color lipsColorForCleanBandit = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForCleanBandit = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleCleanBandit;
    public bool WingsEnabledForCleanBandit;
    public bool ApronEnabledForCleanBandit;
    public bool HatEnabledForCleanBandit;
    public bool ChokerEnabledForCleanBandit;
    public OutfitHair hairForCleanBandit = OutfitHair.DefaultHair;

    [Header("Church Dress")]
    public bool ChurchDressEnabled = true;
    public List<GameObject> OutfitForChurchDress;
    public Color lipsColorForChurchDress = new Color(0.88f, 0.22f, 0.32f);
    public Color nailsColorForChurchDress = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleChurchDress;
    public bool WingsEnabledForChurchDress;
    public bool ApronEnabledForChurchDress;
    public bool HatEnabledForChurchDress;
    public bool ChokerEnabledForChurchDress;
    public OutfitHair hairForChurchDress = OutfitHair.DefaultHair;

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
    public OutfitHair hairForFitness = OutfitHair.DefaultHair;

    [Header("Wooly Jumper & Jeans")]
    public bool WoolyAndJeansEnabled = true;
    public List<GameObject> OutfitForWoolyAndJeans;
    public Color lipsColorForWoolyAndJeans = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForWoolyAndJeans = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleWoolyAndJeans;
    public bool WingsEnabledForWoolyAndJeans;
    public bool ApronEnabledForWoolyAndJeans;
    public bool HatEnabledForWoolyAndJeans;
    public bool ChokerEnabledForWoolyAndJeans;
    public OutfitHair hairForWoolyAndJeans = OutfitHair.DefaultHair;

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
    public OutfitHair hairForPyjamas = OutfitHair.DefaultHair;

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
    public OutfitHair hairForHousecoat = OutfitHair.DefaultHair;

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
    public OutfitHair hairForNightie = OutfitHair.DefaultHair;

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
    public OutfitHair hairForFirstDate = OutfitHair.DefaultHair;

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
    public OutfitHair hairForSecondDate = OutfitHair.DefaultHair;

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
    public OutfitHair hairForThirdDate = OutfitHair.DefaultHair;

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
    public OutfitHair hairForGreyCheckHalterDress = OutfitHair.DefaultHair;

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
    public OutfitHair hairForSweaterAndSkirt = OutfitHair.DefaultHair;

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
    public OutfitHair hairForTurtleneckAndSkirt = OutfitHair.DefaultHair;

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
    public OutfitHair hairForCheckTopAndJeans = OutfitHair.DefaultHair;

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
    public OutfitHair hairForKnottedBlousseAndSkirt = OutfitHair.DefaultHair;

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
    public OutfitHair hairForRuffleBlousseAndSkirt = OutfitHair.DefaultHair;

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
    public OutfitHair hairForLooseTopAndLongSkirt = OutfitHair.DefaultHair;

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
    public OutfitHair hairForTurtleneckAndMediumSkirt = OutfitHair.DefaultHair;

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
    public OutfitHair hairForWoolenJumper = OutfitHair.DefaultHair;

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
    public OutfitHair hairForEdea = OutfitHair.DefaultHair;

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
    public OutfitHair hairForDitzyDress = OutfitHair.DefaultHair;

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
    public OutfitHair hairForLittleBlackDress = OutfitHair.DefaultHair;

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
    public OutfitHair hairForCasualPantsuit = OutfitHair.DefaultHair;

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
    public OutfitHair hairForConservative = OutfitHair.DefaultHair;

    [Header("Button Dress")]
    public bool ButtonDressEnabled = true;
    public List<GameObject> OutfitForButtonDress;
    public Color lipsColorForButtonDress = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForButtonDress = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleButtonDress;
    public bool WingsEnabledForButtonDress;
    public bool ApronEnabledForButtonDress;
    public bool HatEnabledForButtonDress;
    public bool ChokerEnabledForButtonDress;
    public OutfitHair hairForButtonDress = OutfitHair.DefaultHair;
    

    [Header("Traditional")]
    public bool TraditionalEnabled = true;
    public List<GameObject> OutfitForTraditional;
    public Color lipsColorForTraditional = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForTraditional = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleTraditional;
    public bool WingsEnabledForTraditional;
    public bool ApronEnabledForTraditional;
    public bool HatEnabledForTraditional;
    public bool ChokerEnabledForTraditional;
    public OutfitHair hairForTraditional = OutfitHair.DefaultHair;

    [Header("Modest Top & Skirt")]
    public bool ModestEnabled = true;
    public List<GameObject> OutfitForModest;
    public Color lipsColorForModest = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForModest = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleModest;
    public bool WingsEnabledForModest;
    public bool ApronEnabledForModest;
    public bool HatEnabledForModest;
    public bool ChokerEnabledForModest;
    public OutfitHair hairForModest = OutfitHair.DefaultHair;
    
    

    [Header("Strappy Top & Skirt")]
    public bool StrappyTopAndSkirtEnabled = true;
    public List<GameObject> OutfitForStrappyTopAndSkirt;
    public Color lipsColorForStrappyTopAndSkirt = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForStrappyTopAndSkirt = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleStrappyTopAndSkirt;
    public bool WingsEnabledForStrappyTopAndSkirt;
    public bool ApronEnabledForStrappyTopAndSkirt;
    public bool HatEnabledForStrappyTopAndSkirt;
    public bool ChokerEnabledForStrappyTopAndSkirt;
    public OutfitHair hairForStrappyTopAndSkirt = OutfitHair.DefaultHair;

    
    [Header("Shortie")]
    public bool ShortieEnabled = true;
    public List<GameObject> OutfitForShortie;
    public Color lipsColorForShortie = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForShortie = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleShortie;
    public bool WingsEnabledForShortie;
    public bool ApronEnabledForShortie;
    public bool HatEnabledForShortie;
    public bool ChokerEnabledForShortie;
    public OutfitHair hairForShortie = OutfitHair.DefaultHair;

    
    [Header("Top With Skirt")]
    public bool TopWithSkirtEnabled = true;
    public List<GameObject> OutfitForTopWithSkirt;
    public Color lipsColorForTopWithSkirt = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForTopWithSkirt = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleTopWithSkirt;
    public bool WingsEnabledForTopWithSkirt;
    public bool ApronEnabledForTopWithSkirt;
    public bool HatEnabledForTopWithSkirt;
    public bool ChokerEnabledForTopWithSkirt;
    public OutfitHair hairForTopWithSkirt = OutfitHair.DefaultHair;

    [Header("WoolyModesty")]
    public bool WoolyModestyEnabled = true;
    public List<GameObject> OutfitForWoolyModesty;
    public Color lipsColorForWoolyModesty = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForWoolyModesty = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleWoolyModesty;
    public bool WingsEnabledForWoolyModesty;
    public bool ApronEnabledForWoolyModesty;
    public bool HatEnabledForWoolyModesty;
    public bool ChokerEnabledForWoolyModesty;
    public OutfitHair hairForWoolyModesty = OutfitHair.DefaultHair;

    [Header("Froot Cardi & Dress")]
    public bool FrootCardiganTopEnabled = true;
    public List<GameObject> OutfitForFrootCardiganTop;
    public Color lipsColorForFrootCardiganTop = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForFrootCardiganTop = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleFrootCardiganTop;
    public bool WingsEnabledForFrootCardiganTop;
    public bool ApronEnabledForFrootCardiganTop;
    public bool HatEnabledForFrootCardiganTop;
    public bool ChokerEnabledForFrootCardiganTop;
    public OutfitHair hairForFrootCardiganTop = OutfitHair.DefaultHair;

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
    public OutfitHair hairForFrootDress = OutfitHair.DefaultHair;

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
    public OutfitHair hairForStraplessRuffleDress = OutfitHair.DefaultHair;

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
    public OutfitHair hairForCheckBodySuit = OutfitHair.DefaultHair;

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
    public OutfitHair hairForElegantDress = OutfitHair.DefaultHair;

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
    public OutfitHair hairForNightOutRuffle = OutfitHair.DefaultHair;

    [Header("Halter Top & Skirt")]
    public bool HalterSkirterEnabled = true;
    public List<GameObject> OutfitForHalterSkirter;
    public Color lipsColorForHalterSkirter = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForHalterSkirter = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleHalterSkirter;
    public bool WingsEnabledForHalterSkirter;
    public bool ApronEnabledForHalterSkirter;
    public bool HatEnabledForHalterSkirter;
    public bool ChokerEnabledForHalterSkirter;
    public OutfitHair hairForHalterSkirter = OutfitHair.DefaultHair;

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
    public OutfitHair hairForWedding = OutfitHair.DefaultHair;

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
    public OutfitHair hairForFuneral = OutfitHair.DefaultHair;

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
    public OutfitHair hairForHomelessness = OutfitHair.DefaultHair;

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
    public OutfitHair hairForStealthSuit = OutfitHair.DefaultHair;

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
    public OutfitHair hairForUndergarments = OutfitHair.DefaultHair;

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
    public OutfitHair hairForLingerie = OutfitHair.DefaultHair;

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
    public OutfitHair hairForFae = OutfitHair.DefaultHair;

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
    public OutfitHair hairForRisqueNightie = OutfitHair.DefaultHair;

    [Header("Knotted Top & Shorts")]
    public bool KnottedAndShortsEnabled = true;
    public List<GameObject> OutfitForKnottedAndShorts;
    public Color lipsColorForKnottedAndShorts = new Color(0.9f, 0.25f, 0.35f);
    public Color nailsColorForKnottedAndShorts = new Color(0.93f, 0.75f, 0.8f);
    public bool JiggleKnottedAndShorts;
    public bool WingsEnabledForKnottedAndShorts;
    public bool ApronEnabledForKnottedAndShorts;
    public bool HatEnabledForKnottedAndShorts;
    public bool ChokerEnabledForKnottedAndShorts;
    public OutfitHair hairForKnottedAndShorts = OutfitHair.DefaultHair;

    

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
        SetupHair();

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

    // ── Hair ────────────────────────────────────────────────────────────────
    private void SetupHair()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones  = Body != null ? Body.bones   : null;

        SkinnedMeshRenderer[] allHair =
        {
            DefaultHair, WorkHair, WorkHairTwo, CasualHair,
            PyjamaHair, DatingHair, DatingHairTwo, OutHair, UpHair, HomelessHair, TwinTailsHair, MessyHair, UpDo
        };

        foreach (var smr in allHair)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones    != null) { smr.bones    = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }

    private void HideAllHair()
    {
        if (DefaultHair   != null) { DefaultHair.enabled   = false; }
        if (WorkHair      != null) { WorkHair.enabled      = false; }
        if (WorkHairTwo   != null) { WorkHairTwo.enabled   = false; }
        if (CasualHair    != null) { CasualHair.enabled    = false; }
        if (PyjamaHair    != null) { PyjamaHair.enabled    = false; }
        if (DatingHair    != null) { DatingHair.enabled    = false; }
        if (DatingHairTwo != null) { DatingHairTwo.enabled = false; }
        if (OutHair       != null) { OutHair.enabled       = false; }
        if (HomelessHair  != null) { HomelessHair.enabled  = false; }
        if (TwinTailsHair != null) { TwinTailsHair.enabled = false; }
        if (UpHair        != null) { UpHair.enabled        = false; }
        if (UpDo        != null) { UpDo.enabled        = false; }
        if (MessyHair        != null) { MessyHair.enabled        = false; }
    }

    public void SetHair(OutfitHair hair)
    {
        HideAllHair();
        switch (hair)
        {
            case OutfitHair.DefaultHair:   if (DefaultHair   != null) { DefaultHair.enabled   = true; } break;
            case OutfitHair.WorkHair:      if (WorkHair      != null) { WorkHair.enabled      = true; } break;
            case OutfitHair.WorkHairTwo:   if (WorkHairTwo   != null) { WorkHairTwo.enabled   = true; } break;
            case OutfitHair.CasualHair:    if (CasualHair    != null) { CasualHair.enabled    = true; } break;
            case OutfitHair.PyjamaHair:    if (PyjamaHair    != null) { PyjamaHair.enabled    = true; } break;
            case OutfitHair.DatingHair:    if (DatingHair    != null) { DatingHair.enabled    = true; } break;
            case OutfitHair.DatingHairTwo: if (DatingHairTwo != null) { DatingHairTwo.enabled = true; } break;
            case OutfitHair.OutHair:       if (OutHair       != null) { OutHair.enabled       = true; } break;
            case OutfitHair.HomelessHair:  if (HomelessHair  != null) { HomelessHair.enabled  = true; } break;
            case OutfitHair.TwinTailsHair: if (TwinTailsHair != null) { TwinTailsHair.enabled = true; } break;
            case OutfitHair.UpHair:        if (UpHair        != null) { UpHair.enabled        = true; } break;
            case OutfitHair.UpDo:        if (UpDo        != null) { UpDo.enabled        = true; } break;
            case OutfitHair.MessyHair:        if (MessyHair        != null) { MessyHair.enabled        = true; } break;
        }
    }

    private OutfitHair GetHairForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:                     return hairForWorkOne;
            case OutfitType.WorkTwo:                  return hairForWorkTwo;
            case OutfitType.WorkThree:                return hairForWorkThree;
            case OutfitType.WorkFour:                 return hairForWorkFour;
            case OutfitType.WorkSuitThree:            return hairForWorkSuitThree;
            case OutfitType.Casual:                   return hairForCasual;
            case OutfitType.ShortsAndTights:          return hairForShortsAndTights;
            case OutfitType.CleanBandit:              return hairForCleanBandit;
            case OutfitType.ChurchDress:              return hairForChurchDress;
            case OutfitType.Fitness:                  return hairForFitness;
            case OutfitType.Pyjamas:                  return hairForPyjamas;
            case OutfitType.Housecoat:                return hairForHousecoat;
            case OutfitType.Nightie:                  return hairForNightie;
            case OutfitType.RisqueNightie:            return hairForRisqueNightie;
            case OutfitType.StealthSuit:              return hairForStealthSuit;
            case OutfitType.Date1:                    return hairForFirstDate;
            case OutfitType.Date2:                    return hairForSecondDate;
            case OutfitType.Date3:                    return hairForThirdDate;
            case OutfitType.GreyCheckHalterDress:     return hairForGreyCheckHalterDress;
            case OutfitType.SweaterAndSkirt:          return hairForSweaterAndSkirt;
            case OutfitType.TurtleneckAndSkirt:       return hairForTurtleneckAndSkirt;
            case OutfitType.CheckTopAndJeans:         return hairForCheckTopAndJeans;
            case OutfitType.KnottedBlousseAndSkirt:   return hairForKnottedBlousseAndSkirt;
            case OutfitType.RuffleBlousseAndSkirt:    return hairForRuffleBlousseAndSkirt;
            case OutfitType.LooseTopAndLongSkirt:     return hairForLooseTopAndLongSkirt;
            case OutfitType.TurtleneckAndMediumSkirt: return hairForTurtleneckAndMediumSkirt;
            case OutfitType.WoolenJumper:             return hairForWoolenJumper;
            case OutfitType.Edea:                     return hairForEdea;
            case OutfitType.DitzyDress:               return hairForDitzyDress;
            case OutfitType.LittleBlackDress:         return hairForLittleBlackDress;
            case OutfitType.CasualPantSuit:           return hairForCasualPantsuit;
            case OutfitType.Conservative:             return hairForConservative;
            case OutfitType.Casual3:                  return hairForFrootDress;
            case OutfitType.StraplessRuffleDress:     return hairForStraplessRuffleDress;
            case OutfitType.CheckBodySuit:            return hairForCheckBodySuit;
            case OutfitType.ElegantDress:             return hairForElegantDress;
            case OutfitType.NightOutRuffle:           return hairForNightOutRuffle;
            case OutfitType.HalterSkirter:            return hairForHalterSkirter;
            case OutfitType.Wedding:                  return hairForWedding;
            case OutfitType.Funeral:                  return hairForFuneral;
            case OutfitType.Homelessness:             return hairForHomelessness;
            case OutfitType.Undergarments:            return hairForUndergarments;
            case OutfitType.Lingerie:                 return hairForLingerie;
            case OutfitType.Fae:                      return hairForFae;
            case OutfitType.ButtonDress:              return hairForButtonDress;
            case OutfitType.Traditional:           return hairForTraditional;
            case OutfitType.WoolyAndJeans:           return hairForWoolyAndJeans;
            case OutfitType.KnottedAndShorts:           return hairForKnottedAndShorts;
            case OutfitType.Modest:           return hairForModest;
            case OutfitType.StrappyTopAndSkirt:           return hairForStrappyTopAndSkirt;
            case OutfitType.Shortie:           return hairForShortie;
            case OutfitType.TopWithSkirt:           return hairForTopWithSkirt;
            case OutfitType.WoolyModesty:           return hairForWoolyModesty;
            case OutfitType.FrootCardiganTop:           return hairForFrootCardiganTop;
            default:                                  return OutfitHair.DefaultHair;
        }
    }
    // ────────────────────────────────────────────────────────────────────────

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
            case OutfitType.Work:                     return WorkOneOutfitEnabled;
            case OutfitType.WorkTwo:                  return WorkTwoOutfitEnabled;
            case OutfitType.WorkThree:                return WorkThreeOutfitEnabled;
            case OutfitType.WorkFour:                 return WorkFourOutfitEnabled;
            case OutfitType.WorkSuitThree:            return WorkSuitThreeEnabled;
            case OutfitType.Casual:                   return CasualOutfitEnabled;
            case OutfitType.ShortsAndTights:          return ShortsAndTightsEnabled;
            case OutfitType.CleanBandit:              return CleanBanditEnabled;
            case OutfitType.ChurchDress:              return ChurchDressEnabled;
            case OutfitType.Fitness:                  return FitnessOutfitEnabled;
            case OutfitType.Pyjamas:                  return PyjamasOutfitEnabled;
            case OutfitType.Housecoat:                return HousecoatOutfitEnabled;
            case OutfitType.Nightie:                  return NightieEnabled;
            case OutfitType.RisqueNightie:            return RisqueNightieEnabled;
            case OutfitType.StealthSuit:              return StealthSuitEnabled;
            case OutfitType.Date1:                    return FirstDateOutfitEnabled;
            case OutfitType.Date2:                    return SecondDateOutfitEnabled;
            case OutfitType.Date3:                    return ThirdDateOutfitEnabled;
            case OutfitType.GreyCheckHalterDress:     return GreyCheckHalterDressOutfitEnabled;
            case OutfitType.SweaterAndSkirt:          return SweaterAndSkirtOutfitEnabled;
            case OutfitType.TurtleneckAndSkirt:       return TurtleneckAndSkirtOutfitEnabled;
            case OutfitType.CheckTopAndJeans:         return CheckTopAndJeansOutfitEnabled;
            case OutfitType.KnottedBlousseAndSkirt:   return KnottedBlousseAndSkirtOutfitEnabled;
            case OutfitType.RuffleBlousseAndSkirt:    return RuffleBlousseAndSkirtEnabled;
            case OutfitType.LooseTopAndLongSkirt:     return LooseTopAndLongSkirtOutfitEnabled;
            case OutfitType.TurtleneckAndMediumSkirt: return TurtleneckAndMediumSkirtOutfitEnabled;
            case OutfitType.WoolenJumper:             return WoolenJumperEnabled;
            case OutfitType.Edea:                     return EdeaOutfitEnabled;
            case OutfitType.DitzyDress:               return DitzyDressEnabled;
            case OutfitType.LittleBlackDress:         return LittleBlackDressEnabled;
            case OutfitType.CasualPantSuit:           return CasualPantsuitEnabled;
            case OutfitType.Conservative:             return ConservativeEnabled;
            case OutfitType.Casual3:                  return FrootDressOutfitEnabled;
            case OutfitType.StraplessRuffleDress:     return StraplessRuffleDressOutfitEnabled;
            case OutfitType.CheckBodySuit:            return CheckBodySuitOutfitEnabled;
            case OutfitType.ElegantDress:             return ElegantDressOutfitEnabled;
            case OutfitType.NightOutRuffle:           return NightOutRuffleEnabled;
            case OutfitType.HalterSkirter:            return HalterSkirterEnabled;
            case OutfitType.Wedding:                  return WeddingOutfitEnabled;
            case OutfitType.Funeral:                  return FuneralOutfitEnabled;
            case OutfitType.Homelessness:             return HomelessnessOutfitEnabled;
            case OutfitType.Lingerie:                 return LingerieEnabled;
            case OutfitType.Fae:                      return FaeEnabled;
            case OutfitType.ButtonDress:           return ButtonDressEnabled;
            case OutfitType.Traditional:           return TraditionalEnabled;
            case OutfitType.WoolyAndJeans:           return WoolyAndJeansEnabled;
            case OutfitType.KnottedAndShorts:           return KnottedAndShortsEnabled;
            case OutfitType.Modest:           return ModestEnabled;
            case OutfitType.StrappyTopAndSkirt:           return StrappyTopAndSkirtEnabled;
            case OutfitType.Shortie:           return ShortieEnabled;
            case OutfitType.TopWithSkirt:           return TopWithSkirtEnabled;
            case OutfitType.WoolyModesty:           return WoolyModestyEnabled;
            case OutfitType.FrootCardiganTop:           return FrootCardiganTopEnabled;
            default:                                  return true;
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
            case OutfitType.Work:                     wings = WingsEnabledForWorkOne;                  overall = ApronEnabledForWorkOne;                  hat = HatEnabledForWorkOne;                  choker = ChokerEnabledForWorkOne;                  break;
            case OutfitType.WorkTwo:                  wings = WingsEnabledForWorkTwo;                  overall = ApronEnabledForWorkTwo;                  hat = HatEnabledForWorkTwo;                  choker = ChokerEnabledForWorkTwo;                  break;
            case OutfitType.WorkThree:                wings = WingsEnabledForWorkThree;                overall = ApronEnabledForWorkThree;                hat = HatEnabledForWorkThree;                choker = ChokerEnabledForWorkThree;                break;
            case OutfitType.WorkFour:                 wings = WingsEnabledForWorkFour;                 overall = ApronEnabledForWorkFour;                 hat = HatEnabledForWorkFour;                 choker = ChokerEnabledForWorkFour;                 break;
            case OutfitType.WorkSuitThree:            wings = WingsEnabledForWorkSuitThree;            overall = ApronEnabledForWorkSuitThree;            hat = HatEnabledForWorkSuitThree;            choker = ChokerEnabledForWorkSuitThree;            break;
            case OutfitType.Casual:                   wings = WingsEnabledForCasual;                   overall = ApronEnabledForCasual;                   hat = HatEnabledForCasual;                   choker = ChokerEnabledForCasual;                   break;
            case OutfitType.ShortsAndTights:          wings = WingsEnabledForShortsAndTights;          overall = ApronEnabledForShortsAndTights;          hat = HatEnabledForShortsAndTights;          choker = ChokerEnabledForShortsAndTights;          break;
            case OutfitType.CleanBandit:              wings = WingsEnabledForCleanBandit;              overall = ApronEnabledForCleanBandit;              hat = HatEnabledForCleanBandit;              choker = ChokerEnabledForCleanBandit;              break;
            case OutfitType.ChurchDress:              wings = WingsEnabledForChurchDress;              overall = ApronEnabledForChurchDress;              hat = HatEnabledForChurchDress;              choker = ChokerEnabledForChurchDress;              break;
            case OutfitType.Fitness:                  wings = WingsEnabledForFitness;                  overall = ApronEnabledForFitness;                  hat = HatEnabledForFitness;                  choker = ChokerEnabledForFitness;                  break;
            case OutfitType.Pyjamas:                  wings = WingsEnabledForPyjamas;                  overall = ApronEnabledForPyjamas;                  hat = HatEnabledForPyjamas;                  choker = ChokerEnabledForPyjamas;                  break;
            case OutfitType.Housecoat:                wings = WingsEnabledForHousecoat;                overall = ApronEnabledForHousecoat;                hat = HatEnabledForHousecoat;                choker = ChokerEnabledForHousecoat;                break;
            case OutfitType.Nightie:                  wings = WingsEnabledForNightie;                  overall = ApronEnabledForNightie;                  hat = HatEnabledForNightie;                  choker = ChokerEnabledForNightie;                  break;
            case OutfitType.RisqueNightie:            wings = WingsEnabledForRisqueNightie;            overall = ApronEnabledForRisqueNightie;            hat = HatEnabledForRisqueNightie;            choker = ChokerEnabledForRisqueNightie;            break;
            case OutfitType.StealthSuit:              wings = WingsEnabledForStealthSuit;              overall = ApronEnabledForStealthSuit;              hat = HatEnabledForStealthSuit;              choker = ChokerEnabledForStealthSuit;              break;
            case OutfitType.Date1:                    wings = WingsEnabledForFirstDate;                overall = ApronEnabledForFirstDate;                hat = HatEnabledForFirstDate;                choker = ChokerEnabledForFirstDate;                break;
            case OutfitType.Date2:                    wings = WingsEnabledForSecondDate;               overall = ApronEnabledForSecondDate;               hat = HatEnabledForSecondDate;               choker = ChokerEnabledForSecondDate;               break;
            case OutfitType.Date3:                    wings = WingsEnabledForThirdDate;                overall = ApronEnabledForThirdDate;                hat = HatEnabledForThirdDate;                choker = ChokerEnabledForThirdDate;                break;
            case OutfitType.GreyCheckHalterDress:     wings = WingsEnabledForGreyCheckHalterDress;     overall = ApronEnabledForGreyCheckHalterDress;     hat = HatEnabledForGreyCheckHalterDress;     choker = ChokerEnabledForGreyCheckHalterDress;     break;
            case OutfitType.SweaterAndSkirt:          wings = WingsEnabledForSweaterAndSkirt;          overall = ApronEnabledForSweaterAndSkirt;          hat = HatEnabledForSweaterAndSkirt;          choker = ChokerEnabledForSweaterAndSkirt;          break;
            case OutfitType.TurtleneckAndSkirt:       wings = WingsEnabledForTurtleneckAndSkirt;       overall = ApronEnabledForTurtleneckAndSkirt;       hat = HatEnabledForTurtleneckAndSkirt;       choker = ChokerEnabledForTurtleneckAndSkirt;       break;
            case OutfitType.CheckTopAndJeans:         wings = WingsEnabledForCheckTopAndJeans;         overall = ApronEnabledForCheckTopAndJeans;         hat = HatEnabledForCheckTopAndJeans;         choker = ChokerEnabledForCheckTopAndJeans;         break;
            case OutfitType.KnottedBlousseAndSkirt:   wings = WingsEnabledForKnottedBlousseAndSkirt;   overall = ApronEnabledForKnottedBlousseAndSkirt;   hat = HatEnabledForKnottedBlousseAndSkirt;   choker = ChokerEnabledForKnottedBlousseAndSkirt;   break;
            case OutfitType.RuffleBlousseAndSkirt:    wings = WingsEnabledForRuffleBlousseAndSkirt;    overall = ApronEnabledForRuffleBlousseAndSkirt;    hat = HatEnabledForRuffleBlousseAndSkirt;    choker = ChokerEnabledForRuffleBlousseAndSkirt;    break;
            case OutfitType.LooseTopAndLongSkirt:     wings = WingsEnabledForLooseTopAndLongSkirt;     overall = ApronEnabledForLooseTopAndLongSkirt;     hat = HatEnabledForLooseTopAndLongSkirt;     choker = ChokerEnabledForLooseTopAndLongSkirt;     break;
            case OutfitType.TurtleneckAndMediumSkirt: wings = WingsEnabledForTurtleneckAndMediumSkirt; overall = ApronEnabledForTurtleneckAndMediumSkirt; hat = HatEnabledForTurtleneckAndMediumSkirt; choker = ChokerEnabledForTurtleneckAndMediumSkirt; break;
            case OutfitType.WoolenJumper:             wings = WingsEnabledForWoolenJumper;             overall = ApronEnabledForWoolenJumper;             hat = HatEnabledForWoolenJumper;             choker = ChokerEnabledForWoolenJumper;             break;
            case OutfitType.Edea:                     wings = WingsEnabledForEdea;                     overall = ApronEnabledForEdea;                     hat = HatEnabledForEdea;                     choker = ChokerEnabledForEdea;                     break;
            case OutfitType.DitzyDress:               wings = WingsEnabledForDitzyDress;               overall = ApronEnabledForDitzyDress;               hat = HatEnabledForDitzyDress;               choker = ChokerEnabledForDitzyDress;               break;
            case OutfitType.LittleBlackDress:         wings = WingsEnabledForLittleBlackDress;         overall = ApronEnabledForLittleBlackDress;         hat = HatEnabledForLittleBlackDress;         choker = ChokerEnabledForLittleBlackDress;         break;
            case OutfitType.CasualPantSuit:           wings = WingsEnabledForCasualPantsuit;           overall = ApronEnabledForCasualPantsuit;           hat = HatEnabledForCasualPantsuit;           choker = ChokerEnabledForCasualPantsuit;           break;
            case OutfitType.Conservative:             wings = WingsEnabledForConservative;             overall = ApronEnabledForConservative;             hat = HatEnabledForConservative;             choker = ChokerEnabledForConservative;             break;
            case OutfitType.Casual3:                  wings = WingsEnabledForFrootDress;               overall = ApronEnabledForFrootDress;               hat = HatEnabledForFrootDress;               choker = ChokerEnabledForFrootDress;               break;
            case OutfitType.StraplessRuffleDress:     wings = WingsEnabledForStraplessRuffleDress;     overall = ApronEnabledForStraplessRuffleDress;     hat = HatEnabledForStraplessRuffleDress;     choker = ChokerEnabledForStraplessRuffleDress;     break;
            case OutfitType.CheckBodySuit:            wings = WingsEnabledForCheckBodySuit;            overall = ApronEnabledForCheckBodySuit;            hat = HatEnabledForCheckBodySuit;            choker = ChokerEnabledForCheckBodySuit;            break;
            case OutfitType.ElegantDress:             wings = WingsEnabledForElegantDress;             overall = ApronEnabledForElegantDress;             hat = HatEnabledForElegantDress;             choker = ChokerEnabledForElegantDress;             break;
            case OutfitType.NightOutRuffle:           wings = WingsEnabledForNightOutRuffle;           overall = ApronEnabledForNightOutRuffle;           hat = HatEnabledForNightOutRuffle;           choker = ChokerEnabledForNightOutRuffle;           break;
            case OutfitType.HalterSkirter:            wings = WingsEnabledForHalterSkirter;            overall = ApronEnabledForHalterSkirter;            hat = HatEnabledForHalterSkirter;            choker = ChokerEnabledForHalterSkirter;            break;
            case OutfitType.Wedding:                  wings = WingsEnabledForWedding;                  overall = ApronEnabledForWedding;                  hat = HatEnabledForWedding;                  choker = ChokerEnabledForWedding;                  break;
            case OutfitType.Funeral:                  wings = WingsEnabledForFuneral;                  overall = ApronEnabledForFuneral;                  hat = HatEnabledForFuneral;                  choker = ChokerEnabledForFuneral;                  break;
            case OutfitType.Homelessness:             wings = WingsEnabledForHomelessness;             overall = ApronEnabledForHomelessness;             hat = HatEnabledForHomelessness;             choker = ChokerEnabledForHomelessness;             break;
            case OutfitType.Undergarments:            wings = WingsEnabledForUndergarments;            overall = ApronEnabledForUndergarments;            hat = HatEnabledForUndergarments;            choker = ChokerEnabledForUndergarments;            break;
            case OutfitType.Lingerie:                 wings = WingsEnabledForLingerie;                 overall = ApronEnabledForLingerie;                 hat = HatEnabledForLingerie;                 choker = ChokerEnabledForLingerie;                 break;
            case OutfitType.Fae:                      wings = WingsEnabledForFae;                      overall = ApronEnabledForFae;                      hat = HatEnabledForFae;                      choker = ChokerEnabledForFae;                      break;
            case OutfitType.ButtonDress:           wings = WingsEnabledForButtonDress;           overall = ApronEnabledForButtonDress;           hat = HatEnabledForButtonDress;           choker = ChokerEnabledForButtonDress;           break;
            case OutfitType.Traditional:           wings = WingsEnabledForTraditional;           overall = ApronEnabledForTraditional;           hat = HatEnabledForTraditional;           choker = ChokerEnabledForTraditional;           break;
            case OutfitType.WoolyAndJeans:           wings = WingsEnabledForWoolyAndJeans;           overall = ApronEnabledForWoolyAndJeans;           hat = HatEnabledForWoolyAndJeans;           choker = ChokerEnabledForWoolyAndJeans;           break;
            case OutfitType.KnottedAndShorts:           wings = WingsEnabledForKnottedAndShorts;           overall = ApronEnabledForKnottedAndShorts;           hat = HatEnabledForKnottedAndShorts;           choker = ChokerEnabledForKnottedAndShorts;           break;
            case OutfitType.Modest:           wings = WingsEnabledForModest;           overall = ApronEnabledForModest;           hat = HatEnabledForModest;           choker = ChokerEnabledForModest;           break;
            case OutfitType.StrappyTopAndSkirt:           wings = WingsEnabledForStrappyTopAndSkirt;           overall = ApronEnabledForStrappyTopAndSkirt;           hat = HatEnabledForStrappyTopAndSkirt;           choker = ChokerEnabledForStrappyTopAndSkirt;           break;
            case OutfitType.Shortie:           wings = WingsEnabledForShortie;           overall = ApronEnabledForShortie;           hat = HatEnabledForShortie;           choker = ChokerEnabledForShortie;           break;
            case OutfitType.TopWithSkirt:           wings = WingsEnabledForTopWithSkirt;           overall = ApronEnabledForTopWithSkirt;           hat = HatEnabledForTopWithSkirt;           choker = ChokerEnabledForTopWithSkirt;           break;
            case OutfitType.WoolyModesty:           wings = WingsEnabledForWoolyModesty;           overall = ApronEnabledForWoolyModesty;           hat = HatEnabledForWoolyModesty;           choker = ChokerEnabledForWoolyModesty;           break;
            case OutfitType.FrootCardiganTop:           wings = WingsEnabledForFrootCardiganTop;           overall = ApronEnabledForFrootCardiganTop;           hat = HatEnabledForFrootCardiganTop;           choker = ChokerEnabledForFrootCardiganTop;           break;
            default:                                  wings = false; overall = false; hat = false;      choker = false;                                                                                  break;
        }
    }

    private static bool IsUndergarmentOutfit(int index)
    {
        return index == (int)OutfitType.None
            || index == (int)OutfitType.Undergarments
            || index == (int)OutfitType.Lingerie
            || index == (int)OutfitType.Fae
            || index == (int)OutfitType.RisqueNightie
            || index == (int)OutfitType.KnottedAndShorts;
    }

    public void NextOutfit()
    {
        //if (GameMaster.Instance.PLAYERBUSY) { return; }
        int total = System.Enum.GetValues(typeof(OutfitType)).Length;
        int index = (int)currentOutfit;
        do { index = (index + 1) % total; }
        while (IsUndergarmentOutfit(index) || !IsOutfitEnabled((OutfitType)index));
        SwitchToOutfit((OutfitType)index);
    }

    public void PreviousOutfit()
    {
        //if (GameMaster.Instance.PLAYERBUSY) { return; }
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
            case OutfitType.Work:                     ToggleWorkOutfit(true);                     break;
            case OutfitType.WorkTwo:                  ToggleWorkTwoOutfit(true);                  break;
            case OutfitType.WorkThree:                ToggleWorkThreeOutfit(true);                break;
            case OutfitType.WorkFour:                 ToggleWorkFourOutfit(true);                 break;
            case OutfitType.WorkSuitThree:            ToggleWorkSuitThreeOutfit(true);            break;
            case OutfitType.Casual:                   ToggleCasualOutfit(true);                   break;
            case OutfitType.ShortsAndTights:          ToggleShortsAndTightsOutfit(true);          break;
            case OutfitType.CleanBandit:              ToggleCleanBanditOutfit(true);              break;
            case OutfitType.ChurchDress:              ToggleChurchDressOutfit(true);              break;
            case OutfitType.Fitness:                  ToggleFitnessOutfit(true);                  break;
            case OutfitType.Pyjamas:                  TogglePyjamasOutfit(true);                  break;
            case OutfitType.Housecoat:                ToggleHousecoatOutfit(true);                break;
            case OutfitType.Nightie:                  ToggleNightie(true);                        break;
            case OutfitType.StealthSuit:              ToggleStealthSuit(true);                    break;
            case OutfitType.RisqueNightie:            ToggleRisqueNightie(true);                  break;
            case OutfitType.Date1:                    ToggleFirstDateOutfit(true);                break;
            case OutfitType.Date2:                    ToggleSecondDateOutfit(true);               break;
            case OutfitType.Date3:                    ToggleThirdDateOutfit(true);                break;
            case OutfitType.GreyCheckHalterDress:     ToggleGreyCheckHalterDressOutfit(true);     break;
            case OutfitType.SweaterAndSkirt:          ToggleSweaterAndSkirtOutfit(true);          break;
            case OutfitType.TurtleneckAndSkirt:       ToggleTurtleneckAndSkirtOutfit(true);       break;
            case OutfitType.CheckTopAndJeans:         ToggleCheckTopAndJeansOutfit(true);         break;
            case OutfitType.KnottedBlousseAndSkirt:   ToggleKnottedBlousseAndSkirtOutfit(true);   break;
            case OutfitType.RuffleBlousseAndSkirt:    ToggleRuffleBlousseAndSkirtOutfit(true);    break;
            case OutfitType.LooseTopAndLongSkirt:     ToggleLooseTopAndLongSkirtOutfit(true);     break;
            case OutfitType.TurtleneckAndMediumSkirt: ToggleTurtleneckAndMediumSkirtOutfit(true); break;
            case OutfitType.Conservative:             ToggleConservativeOutfit(true);             break;
            case OutfitType.WoolenJumper:             ToggleWoolenJumperOutfit(true);             break;
            case OutfitType.Edea:                     ToggleEdeaOutfit(true);                     break;
            case OutfitType.DitzyDress:               ToggleDitzyDressOutfit(true);               break;
            case OutfitType.LittleBlackDress:         ToggleLittleBlackDressOutfit(true);         break;
            case OutfitType.CasualPantSuit:           ToggleCasualPantsuitOutfit(true);           break;
            case OutfitType.Casual3:                  ToggleFrootDressOutfit(true);               break;
            case OutfitType.StraplessRuffleDress:     ToggleStraplessRuffleDressOutfit(true);     break;
            case OutfitType.CheckBodySuit:            ToggleCheckBodySuitOutfit(true);            break;
            case OutfitType.ElegantDress:             ToggleElegantDressOutfit(true);             break;
            case OutfitType.NightOutRuffle:           ToggleNightOutRuffleOutfit(true);           break;
            case OutfitType.HalterSkirter:            ToggleHalterSkirterOutfit(true);            break;
            case OutfitType.Wedding:                  ToggleWeddingOutfit(true);                  break;
            case OutfitType.Funeral:                  ToggleFuneralOutfit(true);                  break;
            case OutfitType.Homelessness:             ToggleHomelessnessOutfit(true);             break;
            case OutfitType.Lingerie:                 ToggleLingerieOutfit(true);                 break;
            case OutfitType.Fae:                      ToggleFaeOutfit(true);                      break;
            case OutfitType.ButtonDress:           ToggleButtonDressOutfit(true);           break;
            case OutfitType.Traditional:           ToggleTraditionalOutfit(true);           break;
            case OutfitType.WoolyAndJeans:           ToggleWoolyAndJeansOutfit(true);           break;
            case OutfitType.KnottedAndShorts:           ToggleKnottedAndShortsOutfit(true);           break;
            case OutfitType.Modest:           ToggleModestOutfit(true);           break;
            case OutfitType.StrappyTopAndSkirt:           ToggleStrappyTopAndSkirtOutfit(true);           break;
            case OutfitType.Shortie:           ToggleShortieOutfit(true);           break;
            case OutfitType.TopWithSkirt:           ToggleTopWithSkirtOutfit(true);           break;
            case OutfitType.WoolyModesty:           ToggleWoolyModestyOutfit(true);           break;
            case OutfitType.FrootCardiganTop:           ToggleFrootCardiganTopOutfit(true);           break;
            default:                                  DisableAllMainOutfits();                    break;
        }
    }

    public void ToggleWorkOutfit(bool? forceOn = null)                     { JiggleToggle(JiggleForWorkOne);               ToggleMainOutfit(OutfitType.Work,                     forceOn); }
    public void ToggleWorkTwoOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleForWorkTwo);               ToggleMainOutfit(OutfitType.WorkTwo,                  forceOn); }
    public void ToggleWorkThreeOutfit(bool? forceOn = null)                { JiggleToggle(JiggleForWorkThree);             ToggleMainOutfit(OutfitType.WorkThree,                forceOn); }
    public void ToggleWorkFourOutfit(bool? forceOn = null)                 { JiggleToggle(JiggleForWorkFour);              ToggleMainOutfit(OutfitType.WorkFour,                 forceOn); }
    public void ToggleWorkSuitThreeOutfit(bool? forceOn = null)            { JiggleToggle(JiggleWorkSuitThree);            ToggleMainOutfit(OutfitType.WorkSuitThree,            forceOn); }
    public void ToggleCasualOutfit(bool? forceOn = null)                   { JiggleToggle(JiggleCasually);                 ToggleMainOutfit(OutfitType.Casual,                   forceOn); }
    public void ToggleShortsAndTightsOutfit(bool? forceOn = null)          { JiggleToggle(JiggleShortsAndTights);          ToggleMainOutfit(OutfitType.ShortsAndTights,          forceOn); }
    public void ToggleCleanBanditOutfit(bool? forceOn = null)              { JiggleToggle(JiggleCleanBandit);              ToggleMainOutfit(OutfitType.CleanBandit,              forceOn); }
    public void ToggleChurchDressOutfit(bool? forceOn = null)              { JiggleToggle(JiggleChurchDress);              ToggleMainOutfit(OutfitType.ChurchDress,              forceOn); }
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
    public void ToggleHalterSkirterOutfit(bool? forceOn = null)            { JiggleToggle(JiggleHalterSkirter);            ToggleMainOutfit(OutfitType.HalterSkirter,            forceOn); }
    public void ToggleWeddingOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleAtAWedding);               ToggleMainOutfit(OutfitType.Wedding,                  forceOn); }
    public void ToggleFuneralOutfit(bool? forceOn = null)                  { JiggleToggle(JiggleAtAFuneral);               ToggleMainOutfit(OutfitType.Funeral,                  forceOn); }
    public void ToggleHomelessnessOutfit(bool? forceOn = null)             { JiggleToggle(JiggleWhileHomeless);            ToggleMainOutfit(OutfitType.Homelessness,             forceOn); }
    public void ToggleUndergarments(bool? forceOn = null)                  { JiggleToggle(JiggleUndergarments);            ToggleMainOutfit(OutfitType.Undergarments,            forceOn); }
    public void ToggleLingerieOutfit(bool? forceOn = null)                 { JiggleToggle(JiggleLingerie);                 ToggleMainOutfit(OutfitType.Lingerie,                 forceOn); }
    public void ToggleFaeOutfit(bool? forceOn = null)                      { JiggleToggle(JiggleFae);                      ToggleMainOutfit(OutfitType.Fae,                      forceOn); }
    public void ToggleButtonDressOutfit(bool? forceOn = null)           { JiggleToggle(JiggleButtonDress);           ToggleMainOutfit(OutfitType.ButtonDress,           forceOn); }
    public void ToggleTraditionalOutfit(bool? forceOn = null)           { JiggleToggle(JiggleTraditional);           ToggleMainOutfit(OutfitType.Traditional,           forceOn); }
    public void ToggleWoolyAndJeansOutfit(bool? forceOn = null)           { JiggleToggle(JiggleWoolyAndJeans);           ToggleMainOutfit(OutfitType.WoolyAndJeans,           forceOn); }
    public void ToggleKnottedAndShortsOutfit(bool? forceOn = null)           { JiggleToggle(JiggleKnottedAndShorts);           ToggleMainOutfit(OutfitType.KnottedAndShorts,           forceOn); }
    public void ToggleModestOutfit(bool? forceOn = null)           { JiggleToggle(JiggleModest);           ToggleMainOutfit(OutfitType.Modest,           forceOn); }
    public void ToggleStrappyTopAndSkirtOutfit(bool? forceOn = null)           { JiggleToggle(JiggleStrappyTopAndSkirt);           ToggleMainOutfit(OutfitType.StrappyTopAndSkirt,           forceOn); }
    public void ToggleShortieOutfit(bool? forceOn = null)           { JiggleToggle(JiggleShortie);           ToggleMainOutfit(OutfitType.Shortie,           forceOn); }
    public void ToggleTopWithSkirtOutfit(bool? forceOn = null)           { JiggleToggle(JiggleTopWithSkirt);           ToggleMainOutfit(OutfitType.TopWithSkirt,           forceOn); }
    public void ToggleWoolyModestyOutfit(bool? forceOn = null)           { JiggleToggle(JiggleWoolyModesty);           ToggleMainOutfit(OutfitType.WoolyModesty,           forceOn); }
    public void ToggleFrootCardiganTopOutfit(bool? forceOn = null)           { JiggleToggle(JiggleFrootCardiganTop);           ToggleMainOutfit(OutfitType.FrootCardiganTop,           forceOn); }

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
        currentOutfit = outfit;
        switch (outfit)
        {
            case OutfitType.Work:                     InstantiatePrefabs(OutfitForWorkOne);                  break;
            case OutfitType.WorkTwo:                  InstantiatePrefabs(OutfitForWorkOneTwo);               break;
            case OutfitType.WorkThree:                InstantiatePrefabs(OutfitForWorkOneThree);             break;
            case OutfitType.WorkFour:                 InstantiatePrefabs(OutfitForWorkOneFour);              break;
            case OutfitType.WorkSuitThree:            InstantiatePrefabs(OutfitForWorkSuitThree);            break;
            case OutfitType.Casual:                   InstantiatePrefabs(OutfitForCasual);                   break;
            case OutfitType.ShortsAndTights:          InstantiatePrefabs(OutfitForShortsAndTights);          break;
            case OutfitType.CleanBandit:              InstantiatePrefabs(OutfitForCleanBandit);              break;
            case OutfitType.ChurchDress:              InstantiatePrefabs(OutfitForChurchDress);              break;
            case OutfitType.Fitness:                  InstantiatePrefabs(OutfitForFitness);                  break;
            case OutfitType.Pyjamas:                  InstantiatePrefabs(OutfitForPyjamas);                  break;
            case OutfitType.Housecoat:                InstantiatePrefabs(OutfitForHousecoat);                break;
            case OutfitType.Nightie:                  InstantiatePrefabs(OutfitForNightie);                  break;
            case OutfitType.RisqueNightie:            InstantiatePrefabs(OutfitForRisqueNightie);            break;
            case OutfitType.StealthSuit:              InstantiatePrefabs(OutfitForStealthSuit);              break;
            case OutfitType.Date1:                    InstantiatePrefabs(OutfitForFirstDate);                break;
            case OutfitType.Date2:                    InstantiatePrefabs(OutfitForSecondDate);               break;
            case OutfitType.Date3:                    InstantiatePrefabs(OutfitForThirdDate);                break;
            case OutfitType.GreyCheckHalterDress:     InstantiatePrefabs(OutfitForGreyCheckHalterDress);     break;
            case OutfitType.SweaterAndSkirt:          InstantiatePrefabs(OutfitForSweaterAndSkirt);          break;
            case OutfitType.TurtleneckAndSkirt:       InstantiatePrefabs(OutfitForTurtleneckAndSkirt);       break;
            case OutfitType.CheckTopAndJeans:         InstantiatePrefabs(OutfitForCheckTopAndJeans);         break;
            case OutfitType.KnottedBlousseAndSkirt:   InstantiatePrefabs(OutfitForKnottedBlousseAndSkirt);   break;
            case OutfitType.RuffleBlousseAndSkirt:    InstantiatePrefabs(OutfitForRuffleBlousseAndSkirt);    break;
            case OutfitType.LooseTopAndLongSkirt:     InstantiatePrefabs(OutfitForLooseTopAndLongSkirt);     break;
            case OutfitType.TurtleneckAndMediumSkirt: InstantiatePrefabs(OutfitForTurtleneckAndMediumSkirt); break;
            case OutfitType.WoolenJumper:             InstantiatePrefabs(OutfitForWoolenJumper);             break;
            case OutfitType.Edea:                     InstantiatePrefabs(OutfitForEdea);                     break;
            case OutfitType.DitzyDress:               InstantiatePrefabs(OutfitForDitzyDress);               break;
            case OutfitType.LittleBlackDress:         InstantiatePrefabs(OutfitForLittleBlackDress);         break;
            case OutfitType.CasualPantSuit:           InstantiatePrefabs(OutfitCasualPantsuit);              break;
            case OutfitType.Conservative:             InstantiatePrefabs(OutfitConservative);                break;
            case OutfitType.Casual3:                  InstantiatePrefabs(OutfitForFrootDress);               break;
            case OutfitType.StraplessRuffleDress:     InstantiatePrefabs(OutfitForStraplessRuffleDress);     break;
            case OutfitType.CheckBodySuit:            InstantiatePrefabs(OutfitForCheckBodySuit);            break;
            case OutfitType.ElegantDress:             InstantiatePrefabs(OutfitForElegantDress);             break;
            case OutfitType.NightOutRuffle:           InstantiatePrefabs(OutfitForNightOutRuffle);           break;
            case OutfitType.HalterSkirter:            InstantiatePrefabs(OutfitForHalterSkirter);            break;
            case OutfitType.Wedding:                  InstantiatePrefabs(OutfitForWedding);                  break;
            case OutfitType.Funeral:                  InstantiatePrefabs(OutfitForFuneral);                  break;
            case OutfitType.Homelessness:             InstantiatePrefabs(OutfitForHomelessness);             break;
            case OutfitType.Undergarments:            InstantiatePrefabs(OutfitForUndergarments);            break;
            case OutfitType.Lingerie:                 InstantiatePrefabs(OutfitForLingerie);                 break;
            case OutfitType.Fae:                      InstantiatePrefabs(OutfitForFae);                      break;
            case OutfitType.ButtonDress:           InstantiatePrefabs(OutfitForButtonDress);           break;
            case OutfitType.Traditional:           InstantiatePrefabs(OutfitForTraditional);           break;
            case OutfitType.WoolyAndJeans:           InstantiatePrefabs(OutfitForWoolyAndJeans);           break;
            case OutfitType.KnottedAndShorts:           InstantiatePrefabs(OutfitForKnottedAndShorts);           break;
            case OutfitType.Modest:           InstantiatePrefabs(OutfitForModest);           break;
            case OutfitType.StrappyTopAndSkirt:           InstantiatePrefabs(OutfitForStrappyTopAndSkirt);           break;
            case OutfitType.Shortie:           InstantiatePrefabs(OutfitForShortie);           break;
            case OutfitType.TopWithSkirt:           InstantiatePrefabs(OutfitForTopWithSkirt);           break;
            case OutfitType.WoolyModesty:           InstantiatePrefabs(OutfitForWoolyModesty);           break;
            case OutfitType.FrootCardiganTop:           InstantiatePrefabs(OutfitForFrootCardiganTop);           break;
        }
        ApplyBodyColors(outfit);
        ApplyAccessories();
        SetHair(GetHairForOutfit(outfit));

        EventManager.OutfitChanged(outfit);
    }

    public void DisableAllMainOutfits()
    {
        HideAllAccessories();
        ClearOutfitMeshes();
        currentOutfit = OutfitType.None;
        SetHair(OutfitHair.DefaultHair);
        ApplyAccessories();
    }

    private OutfitType PickRandom(OutfitType[] pool)
    {
        OutfitType[] eligible = pool.Where(o => IsOutfitEnabled(o) && o != currentOutfit).ToArray();
        if (eligible.Length == 0) { eligible = pool.Where(o => IsOutfitEnabled(o)).ToArray(); }
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
        OutfitType[] pool = { OutfitType.Casual, OutfitType.ShortsAndTights, OutfitType.CleanBandit, OutfitType.ChurchDress, OutfitType.WoolyAndJeans };
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
            OutfitType.GreyCheckHalterDress,  OutfitType.SweaterAndSkirt,       OutfitType.TurtleneckAndSkirt,      OutfitType.CheckTopAndJeans,
            OutfitType.KnottedBlousseAndSkirt, OutfitType.RuffleBlousseAndSkirt, OutfitType.LooseTopAndLongSkirt,    OutfitType.TurtleneckAndMediumSkirt,
            OutfitType.WoolenJumper,           OutfitType.Edea,                  OutfitType.DitzyDress,              OutfitType.LittleBlackDress,
            OutfitType.Conservative,           OutfitType.CasualPantSuit,           OutfitType.ButtonDress,           OutfitType.Traditional,          OutfitType.Modest,          OutfitType.StrappyTopAndSkirt, OutfitType.Shortie, OutfitType.TopWithSkirt, 
            OutfitType.WoolyModesty, OutfitType.FrootCardiganTop
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomNightOutOutfit()
    {
        OutfitType[] pool = { OutfitType.Casual3, OutfitType.StraplessRuffleDress, OutfitType.CheckBodySuit, OutfitType.ElegantDress, OutfitType.NightOutRuffle, OutfitType.HalterSkirter };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomRisqueOutfit()
    {
        OutfitType[] pool = { OutfitType.Lingerie, OutfitType.Fae, OutfitType.RisqueNightie, OutfitType.KnottedAndShorts  };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomStorylineOutfit()
    {
        OutfitType[] pool = { OutfitType.Wedding, OutfitType.Funeral, OutfitType.Homelessness, OutfitType.StealthSuit };
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
            case OutfitType.Work:                     return lipsColorForWorkOne;
            case OutfitType.WorkTwo:                  return lipsColorForWorkTwo;
            case OutfitType.WorkThree:                return lipsColorForWorkThree;
            case OutfitType.WorkFour:                 return lipsColorForWorkFour;
            case OutfitType.WorkSuitThree:            return lipsColorForWorkSuitThree;
            case OutfitType.Casual:                   return lipsColorForCasual;
            case OutfitType.ShortsAndTights:          return lipsColorForShortsAndTights;
            case OutfitType.CleanBandit:              return lipsColorForCleanBandit;
            case OutfitType.ChurchDress:              return lipsColorForChurchDress;
            case OutfitType.Fitness:                  return lipsColorForFitness;
            case OutfitType.Pyjamas:                  return lipsColorForPyjamas;
            case OutfitType.Housecoat:                return lipsColorForHousecoat;
            case OutfitType.Nightie:                  return lipsColorForNightie;
            case OutfitType.RisqueNightie:            return lipsColorForRisqueNightie;
            case OutfitType.StealthSuit:              return lipsColorForStealthSuit;
            case OutfitType.Date1:                    return lipsColorForFirstDate;
            case OutfitType.Date2:                    return lipsColorForSecondDate;
            case OutfitType.Date3:                    return lipsColorForThirdDate;
            case OutfitType.GreyCheckHalterDress:     return lipsColorForGreyCheckHalterDress;
            case OutfitType.SweaterAndSkirt:          return lipsColorForSweaterAndSkirt;
            case OutfitType.TurtleneckAndSkirt:       return lipsColorForTurtleneckAndSkirt;
            case OutfitType.CheckTopAndJeans:         return lipsColorForCheckTopAndJeans;
            case OutfitType.KnottedBlousseAndSkirt:   return lipsColorForKnottedBlousseAndSkirt;
            case OutfitType.RuffleBlousseAndSkirt:    return lipsColorForRuffleBlousseAndSkirt;
            case OutfitType.LooseTopAndLongSkirt:     return lipsColorForLooseTopAndLongSkirt;
            case OutfitType.TurtleneckAndMediumSkirt: return lipsColorForTurtleneckAndMediumSkirt;
            case OutfitType.WoolenJumper:             return lipsColorForWoolenJumper;
            case OutfitType.Edea:                     return lipsColorForEdea;
            case OutfitType.DitzyDress:               return lipsColorForDitzyDress;
            case OutfitType.LittleBlackDress:         return lipsColorForLittleBlackDress;
            case OutfitType.CasualPantSuit:           return lipsColorForCasualPantsuit;
            case OutfitType.Conservative:             return lipsColorForConservative;
            case OutfitType.Casual3:                  return lipsColorForFrootDress;
            case OutfitType.StraplessRuffleDress:     return lipsColorForStraplessRuffleDress;
            case OutfitType.CheckBodySuit:            return lipsColorForCheckBodySuit;
            case OutfitType.ElegantDress:             return lipsColorForElegantDress;
            case OutfitType.NightOutRuffle:           return lipsColorForNightOutRuffle;
            case OutfitType.HalterSkirter:            return lipsColorForHalterSkirter;
            case OutfitType.Wedding:                  return lipsColorForWedding;
            case OutfitType.Funeral:                  return lipsColorForFuneral;
            case OutfitType.Homelessness:             return lipsColorForHomelessness;
            case OutfitType.Undergarments:            return lipsColorForUndergarments;
            case OutfitType.Lingerie:                 return lipsColorForLingerie;
            case OutfitType.Fae:                      return lipsColorForFae;
            case OutfitType.ButtonDress:           return lipsColorForButtonDress;
            case OutfitType.Traditional:           return lipsColorForTraditional;
            case OutfitType.WoolyAndJeans:           return lipsColorForWoolyAndJeans;
            case OutfitType.KnottedAndShorts:           return lipsColorForKnottedAndShorts;
            case OutfitType.Modest:           return lipsColorForModest;
            case OutfitType.StrappyTopAndSkirt:           return lipsColorForStrappyTopAndSkirt;
            case OutfitType.Shortie:           return lipsColorForShortie;
            case OutfitType.TopWithSkirt:           return lipsColorForTopWithSkirt;
            case OutfitType.WoolyModesty:           return lipsColorForWoolyModesty;
            case OutfitType.FrootCardiganTop:           return lipsColorForFrootCardiganTop;
            default:                                  return Color.white;
        }
    }

    private Color GetNailColorForOutfit(OutfitType outfit)
    {
        switch (outfit)
        {
            case OutfitType.Work:                     return nailsColorForWorkOne;
            case OutfitType.WorkTwo:                  return nailsColorForWorkTwo;
            case OutfitType.WorkThree:                return nailsColorForWorkThree;
            case OutfitType.WorkFour:                 return nailsColorForWorkFour;
            case OutfitType.WorkSuitThree:            return nailsColorForWorkSuitThree;
            case OutfitType.Casual:                   return nailsColorForCasual;
            case OutfitType.ShortsAndTights:          return nailsColorForShortsAndTights;
            case OutfitType.CleanBandit:              return nailsColorForCleanBandit;
            case OutfitType.ChurchDress:              return nailsColorForChurchDress;
            case OutfitType.Fitness:                  return nailsColorForFitness;
            case OutfitType.Pyjamas:                  return nailsColorForPyjamas;
            case OutfitType.Housecoat:                return nailsColorForHousecoat;
            case OutfitType.Nightie:                  return nailsColorForNightie;
            case OutfitType.RisqueNightie:            return nailsColorForRisqueNightie;
            case OutfitType.StealthSuit:              return nailsColorStealthSuit;
            case OutfitType.Date1:                    return nailsColorForFirstDate;
            case OutfitType.Date2:                    return nailsColorForSecondDate;
            case OutfitType.Date3:                    return nailsColorForThirdDate;
            case OutfitType.GreyCheckHalterDress:     return nailsColorForGreyCheckHalterDress;
            case OutfitType.SweaterAndSkirt:          return nailsColorForSweaterAndSkirt;
            case OutfitType.TurtleneckAndSkirt:       return nailsColorForTurtleneckAndSkirt;
            case OutfitType.CheckTopAndJeans:         return nailsColorForCheckTopAndJeans;
            case OutfitType.KnottedBlousseAndSkirt:   return nailsColorForKnottedBlousseAndSkirt;
            case OutfitType.RuffleBlousseAndSkirt:    return nailsColorForRuffleBlousseAndSkirt;
            case OutfitType.LooseTopAndLongSkirt:     return nailsColorForLooseTopAndLongSkirt;
            case OutfitType.TurtleneckAndMediumSkirt: return nailsColorForTurtleneckAndMediumSkirt;
            case OutfitType.WoolenJumper:             return nailsColorForWoolenJumper;
            case OutfitType.Edea:                     return nailsColorForEdea;
            case OutfitType.DitzyDress:               return nailsColorForDitzyDress;
            case OutfitType.LittleBlackDress:         return nailsColorForLittleBlackDress;
            case OutfitType.CasualPantSuit:           return nailsColorForCasualPantsuit;
            case OutfitType.Conservative:             return nailsColorForConservative;
            case OutfitType.Casual3:                  return nailsColorForFrootDress;
            case OutfitType.StraplessRuffleDress:     return nailsColorForStraplessRuffleDress;
            case OutfitType.CheckBodySuit:            return nailsColorForCheckBodySuit;
            case OutfitType.ElegantDress:             return nailsColorForElegantDress;
            case OutfitType.NightOutRuffle:           return nailsColorForNightOutRuffle;
            case OutfitType.HalterSkirter:            return nailsColorForHalterSkirter;
            case OutfitType.Wedding:                  return nailsColorForWedding;
            case OutfitType.Funeral:                  return nailsColorForFuneral;
            case OutfitType.Homelessness:             return nailsColorForHomelessness;
            case OutfitType.Undergarments:            return nailsColorForUndergarments;
            case OutfitType.Lingerie:                 return nailsColorForLingerie;
            case OutfitType.Fae:                      return nailsColorForFae;
            case OutfitType.ButtonDress:              return nailsColorForButtonDress;
            case OutfitType.Traditional:              return nailsColorForTraditional;
            case OutfitType.WoolyAndJeans:            return nailsColorForWoolyAndJeans;
            case OutfitType.KnottedAndShorts:         return nailsColorForKnottedAndShorts;
            case OutfitType.Modest:                   return nailsColorForModest;
            case OutfitType.StrappyTopAndSkirt:       return nailsColorForStrappyTopAndSkirt;
            case OutfitType.Shortie:           return nailsColorForShortie;
            case OutfitType.TopWithSkirt:           return nailsColorForTopWithSkirt;
            case OutfitType.WoolyModesty:           return nailsColorForWoolyModesty;
            case OutfitType.FrootCardiganTop:           return nailsColorForFrootCardiganTop;
            default:                                  return Color.white;
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

        var clearAllStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal    = { textColor = Color.white }
        };

        EditorGUILayout.LabelField("Outfit Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous Outfit", clearAllStyle, GUILayout.Height(28))) { me.PreviousOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Next Outfit",     clearAllStyle, GUILayout.Height(28))) { me.NextOutfit();     EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = Color.cyan;

        EditorGUILayout.LabelField("Work Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Classic Nora Dress",        clearAllStyle, GUILayout.Height(28))) { me.ToggleWorkOutfit();          EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Black Skirt & Cream Shirt", clearAllStyle, GUILayout.Height(28))) { me.ToggleWorkThreeOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Tartan Dress",       clearAllStyle, GUILayout.Height(28))) { me.ToggleWorkTwoOutfit();       EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Black Suit & Shirt",        clearAllStyle, GUILayout.Height(28))) { me.ToggleWorkFourOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Suit Jacket & Trousers",    clearAllStyle, GUILayout.Height(28))) { me.ToggleWorkSuitThreeOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;

        EditorGUILayout.LabelField("Casual Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Uni Sweater & Jeans", clearAllStyle, GUILayout.Height(28))) { me.ToggleCasualOutfit();          EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Shorts & Tights",     clearAllStyle, GUILayout.Height(28))) { me.ToggleShortsAndTightsOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fitness",             clearAllStyle, GUILayout.Height(28))) { me.ToggleFitnessOutfit();         EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Clean Bandit",        clearAllStyle, GUILayout.Height(28))) { me.ToggleCleanBanditOutfit();     EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wooly Jumper & Jeans", clearAllStyle, GUILayout.Height(28))) { me.ToggleWoolyAndJeansOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Church Dress",        clearAllStyle, GUILayout.Height(28))) { me.ToggleChurchDressOutfit();     EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);

        EditorGUILayout.LabelField("PJs", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pyjamas",   clearAllStyle, GUILayout.Height(28))) { me.TogglePyjamasOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Housecoat", clearAllStyle, GUILayout.Height(28))) { me.ToggleHousecoatOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Nightie",   clearAllStyle, GUILayout.Height(28))) { me.ToggleNightie();         EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(1f, 0.6f, 0.8f);

        EditorGUILayout.LabelField("Dating", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("First Date",  clearAllStyle, GUILayout.Height(28))) { me.ToggleFirstDateOutfit();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Second Date", clearAllStyle, GUILayout.Height(28))) { me.ToggleSecondDateOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Third Date",  clearAllStyle, GUILayout.Height(28))) { me.ToggleThirdDateOutfit();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.7f, 0.4f, 1f);

        EditorGUILayout.LabelField("Cute Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Grey Check Halter Dress",  clearAllStyle, GUILayout.Height(28))) { me.ToggleGreyCheckHalterDressOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Sweater & Skirt",          clearAllStyle, GUILayout.Height(28))) { me.ToggleSweaterAndSkirtOutfit();        EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Turtleneck & Skirt",       clearAllStyle, GUILayout.Height(28))) { me.ToggleTurtleneckAndSkirtOutfit();     EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check Top & Jeans",        clearAllStyle, GUILayout.Height(28))) { me.ToggleCheckTopAndJeansOutfit();       EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Knotted Blousse & Skirt",  clearAllStyle, GUILayout.Height(28))) { me.ToggleKnottedBlousseAndSkirtOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Ruffle Blousse & Skirt",    clearAllStyle, GUILayout.Height(28))) { me.ToggleRuffleBlousseAndSkirtOutfit();    EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Loose Top & Long Skirt",    clearAllStyle, GUILayout.Height(28))) { me.ToggleLooseTopAndLongSkirtOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Turtleneck & Medium Skirt", clearAllStyle, GUILayout.Height(28))) { me.ToggleTurtleneckAndMediumSkirtOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wooly Jumper & Tights", clearAllStyle, GUILayout.Height(28))) { me.ToggleWoolenJumperOutfit();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Ditzy Dress",           clearAllStyle, GUILayout.Height(28))) { me.ToggleDitzyDressOutfit();       EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Little Black Dress",    clearAllStyle, GUILayout.Height(28))) { me.ToggleLittleBlackDressOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Casual Pantsuit",             clearAllStyle, GUILayout.Height(28))) { me.ToggleCasualPantsuitOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Conservative Jumper & Skirt", clearAllStyle, GUILayout.Height(28))) { me.ToggleConservativeOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Button Dress",                clearAllStyle, GUILayout.Height(28))) { me.ToggleButtonDressOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Traditional", clearAllStyle, GUILayout.Height(28))) { me.ToggleTraditionalOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Modest Top & Skirt", clearAllStyle, GUILayout.Height(28))) { me.ToggleModestOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Strappy Top & Skirt", clearAllStyle, GUILayout.Height(28))) { me.ToggleStrappyTopAndSkirtOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Shortie", clearAllStyle, GUILayout.Height(28))) { me.ToggleShortieOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Top With Skirt", clearAllStyle, GUILayout.Height(28))) { me.ToggleTopWithSkirtOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Wooly Modesty", clearAllStyle, GUILayout.Height(28))) { me.ToggleWoolyModestyOutfit(); EditorUtility.SetDirty(me); }        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Froot Cardi & Dress", clearAllStyle, GUILayout.Height(28))) { me.ToggleFrootCardiganTopOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Edea's Dress",                clearAllStyle, GUILayout.Height(28))) { me.ToggleEdeaOutfit();           EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.5f);

        EditorGUILayout.LabelField("Night Out Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Froot Dress",            clearAllStyle, GUILayout.Height(28))) { me.ToggleFrootDressOutfit();           EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Strapless Ruffle Dress", clearAllStyle, GUILayout.Height(28))) { me.ToggleStraplessRuffleDressOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Check Body Suit",        clearAllStyle, GUILayout.Height(28))) { me.ToggleCheckBodySuitOutfit();        EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Elegant Dress",        clearAllStyle, GUILayout.Height(28))) { me.ToggleElegantDressOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Layered Ruffle Dress", clearAllStyle, GUILayout.Height(28))) { me.ToggleNightOutRuffleOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Halter Top & Skirt",   clearAllStyle, GUILayout.Height(28))) { me.ToggleHalterSkirterOutfit();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.black;

        EditorGUILayout.LabelField("Storyline Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Wedding",      clearAllStyle, GUILayout.Height(28))) { me.ToggleWeddingOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Funeral",      clearAllStyle, GUILayout.Height(28))) { me.ToggleFuneralOutfit();      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Skid Row",     clearAllStyle, GUILayout.Height(28))) { me.ToggleHomelessnessOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Stealth Suit", clearAllStyle, GUILayout.Height(28))) { me.ToggleStealthSuit();        EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.red;

        EditorGUILayout.LabelField("Undergarments", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Lingerie",       clearAllStyle, GUILayout.Height(28))) { me.ToggleLingerieOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Underwear",      clearAllStyle, GUILayout.Height(28))) { me.ToggleUndergarments();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fae",            clearAllStyle, GUILayout.Height(28))) { me.ToggleFaeOutfit();      EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Risque Nightie", clearAllStyle, GUILayout.Height(28))) { me.ToggleRisqueNightie();  EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Knotted Top & Shorts", clearAllStyle, GUILayout.Height(28))) { me.ToggleKnottedAndShortsOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Nothin' At All", clearAllStyle, GUILayout.Height(28))) { me.Undress(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.yellow;

        EditorGUILayout.LabelField("Accessories (toggle independently — any combination)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Wings",   clearAllStyle, GUILayout.Height(28))) { me.ToggleWings();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Overall", clearAllStyle, GUILayout.Height(28))) { me.ToggleOverall(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Hat",     clearAllStyle, GUILayout.Height(28))) { me.ToggleHat();     EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Toggle Choker",  clearAllStyle, GUILayout.Height(28))) { me.ToggleChoker();  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);

        EditorGUILayout.LabelField("Hair (exclusive — only one at a time)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Default Hair",    clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.DefaultHair);   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Work Hair",       clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.WorkHair);      EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Work Hair Two",   clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.WorkHairTwo);   EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Casual Hair",     clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.CasualHair);    EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Pyjama Hair",     clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.PyjamaHair);    EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Dating Hair",     clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.DatingHair);    EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Dating Hair Two", clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.DatingHairTwo); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Out Hair",        clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.OutHair);       EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Homeless Hair",   clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.HomelessHair);  EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Twin Tails Hair", clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.TwinTailsHair); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Up Hair",         clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.UpHair);        EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Messy Hair",         clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.MessyHair);        EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Updo",         clearAllStyle, GUILayout.Height(28))) { me.SetHair(OutfitHair.UpDo);        EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Work",   clearAllStyle, GUILayout.Height(28))) { me.SetRandomWorkOutfit();   EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Casual", clearAllStyle, GUILayout.Height(28))) { me.SetRandomCasualOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Dating", clearAllStyle, GUILayout.Height(28))) { me.SetRandomDatingOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Cute",   clearAllStyle, GUILayout.Height(28))) { me.SetRandomCuteOutfit();   EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Night Out", clearAllStyle, GUILayout.Height(28))) { me.SetRandomNightOutOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Pyjamas",   clearAllStyle, GUILayout.Height(28))) { me.SetRandomPyjamas();        EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Storyline",     clearAllStyle, GUILayout.Height(28))) { me.SetRandomStorylineOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Random Risque Outfit", clearAllStyle, GUILayout.Height(28))) { me.SetRandomRisqueOutfit();    EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("---------------------", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
#endif

public enum OutfitHair
{
    DefaultHair,
    WorkHair,
    WorkHairTwo,
    CasualHair,
    PyjamaHair,
    DatingHair,
    DatingHairTwo,
    OutHair,
    HomelessHair,
    TwinTailsHair,
    UpHair,
    UpDo,
    MessyHair,
}

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
    HalterSkirter,
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
    CleanBandit,
    ChurchDress,
    Fae,
    Undergarments,
    StealthSuit,
    Conservative,
    ButtonDress,
    Traditional,
    WoolyAndJeans,
    KnottedAndShorts,
    Modest,
    StrappyTopAndSkirt,
    Shortie,
    TopWithSkirt,
    WoolyModesty,
    FrootCardiganTop
}