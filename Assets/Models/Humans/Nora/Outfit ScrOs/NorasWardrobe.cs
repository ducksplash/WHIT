using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NorasWardrobe : MonoBehaviour
{
    [Header("Settings")] 
    public bool DebugMode;
    public bool UnressedOnLoad;
    public bool PreviewEnabled;

    [Header("Current Outfit")]
    public OutfitName currentOutfit = OutfitName.Work;

    [Header("Preview Character Root")]
    public GameObject SkinnedMeshRendererParentOutfitsPreview;
    public GameObject SkinnedMeshRendererParentAccessoriesPreview;

    [Header("Preview Body")]
    public SkinnedMeshRenderer BodyPreview;

    [Header("Preview Accessories")]
    public SkinnedMeshRenderer ButterflyWingsPreview;
    public SkinnedMeshRenderer OverallPreview;
    public SkinnedMeshRenderer HatPreview;
    public SkinnedMeshRenderer ChokerPreview;
    public SkinnedMeshRenderer GlassesPreview;
    public SkinnedMeshRenderer CigarettePreview;

    [Header("Preview Shoes")]
    public SkinnedMeshRenderer WorkFlatsShoesPreview;
    public SkinnedMeshRenderer SilentShoesShoesPreview;
    public SkinnedMeshRenderer SandalsShoesPreview;
    public SkinnedMeshRenderer BootsShoesPreview;
    public SkinnedMeshRenderer WhiteFlatsShoesPreview;
    public SkinnedMeshRenderer ShittyTrainersShoesPreview;
    public SkinnedMeshRenderer FMBShoesPreview;
    public SkinnedMeshRenderer BlackBowFlatsShoesPreview;
    public SkinnedMeshRenderer WhiteBowFlatsShoesPreview;
    public SkinnedMeshRenderer BlackBootsShoesPreview;
    public SkinnedMeshRenderer BlackFMBShoesPreview;
    public SkinnedMeshRenderer PrimAndProperShoesPreview;
    public SkinnedMeshRenderer ShuPlaceholder4ShoesPreview;

    [Header("Preview Hair")]
    public SkinnedMeshRenderer DefaultHairPreview;
    public SkinnedMeshRenderer WorkHairPreview;
    public SkinnedMeshRenderer WorkHairTwoPreview;
    public SkinnedMeshRenderer CasualHairPreview;
    public SkinnedMeshRenderer PyjamaHairPreview;
    public SkinnedMeshRenderer DatingHairPreview;
    public SkinnedMeshRenderer DatingHairTwoPreview;
    public SkinnedMeshRenderer OutHairPreview;
    public SkinnedMeshRenderer HomelessHairPreview;
    public SkinnedMeshRenderer TwinTailsHairPreview;
    public SkinnedMeshRenderer UpHairPreview;
    public SkinnedMeshRenderer UpDoPreview;

    [Header("Preview Necklace")]
    public SkinnedMeshRenderer ButterflyNecklacePreview;
    public SkinnedMeshRenderer PearlsNecklacePreview;
    public SkinnedMeshRenderer CrossNecklacePreview;
    public SkinnedMeshRenderer NekPlaceholder1NecklacePreview;
    public SkinnedMeshRenderer NekPlaceholder2NecklacePreview;
    public SkinnedMeshRenderer NekPlaceholder3NecklacePreview;
    public SkinnedMeshRenderer NekPlaceholder4NecklacePreview;
    
    [Header("Preview Glasses")]
    public SkinnedMeshRenderer ProfessionalGlassesPreview;
    public SkinnedMeshRenderer GlaPlaceholder1GlassesPreview;
    public SkinnedMeshRenderer GlaPlaceholder2GlassesPreview;
    public SkinnedMeshRenderer GlaPlaceholder3GlassesPreview;
    public SkinnedMeshRenderer GlaPlaceholder4GlassesPreview;
    
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
    public SkinnedMeshRenderer Cigarette;

    [Header("Shoes")]
    public SkinnedMeshRenderer WorkFlatsShoes;
    public SkinnedMeshRenderer SilentShoesShoes;
    public SkinnedMeshRenderer SandalsShoes;
    public SkinnedMeshRenderer BootsShoes;
    public SkinnedMeshRenderer WhiteFlatsShoes;
    public SkinnedMeshRenderer ShittyTrainersShoes;
    public SkinnedMeshRenderer FMBShoes;
    public SkinnedMeshRenderer BlackBowFlatsShoes;
    public SkinnedMeshRenderer WhiteBowFlatsShoes;
    public SkinnedMeshRenderer BlackBootsShoes;
    public SkinnedMeshRenderer BlackFMBShoes;
    public SkinnedMeshRenderer PrimAndProperShoes;
    public SkinnedMeshRenderer ShuPlaceholder4Shoes;

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
    
    [Header("Necklace")]
    public SkinnedMeshRenderer ButterflyNecklace;
    public SkinnedMeshRenderer PearlsNecklace;
    public SkinnedMeshRenderer CrossNecklace;
    public SkinnedMeshRenderer NekPlaceholder1Necklace;
    public SkinnedMeshRenderer NekPlaceholder2Necklace;
    public SkinnedMeshRenderer NekPlaceholder3Necklace;
    public SkinnedMeshRenderer NekPlaceholder4Necklace;
    
    [Header("Glasses")]
    public SkinnedMeshRenderer ProfessionalGlasses;
    public SkinnedMeshRenderer GlaPlaceholder1Glasses;
    public SkinnedMeshRenderer GlaPlaceholder2Glasses;
    public SkinnedMeshRenderer GlaPlaceholder3Glasses;
    public SkinnedMeshRenderer GlaPlaceholder4Glasses;
    
    [Header("Outfits")]
    public List<Outfit> Outfits = new List<Outfit>();

    [Header("Jigglers")]
    public GameObject JiggleLeftBoob;
    public GameObject JiggleRightBoob;
    public GameObject JiggleLeftButtcheek;
    public GameObject JiggleRightButtcheek;

    [Header("Input")]
    public InputActionReference nextOutfit;
    public InputActionReference previousOutfit;
    public InputActionReference holdToggleDebug;

    private bool _wingsOverride = false;
    private bool _overallOverride = false;
    private bool _hatOverride = false;
    private bool _chokerOverride = false;
    private bool _glassesOverride = false;
    private bool _cigaretteOverride = false;

    private Dictionary<OutfitName, Outfit> _outfitLookup = new Dictionary<OutfitName, Outfit>();

    private static readonly OutfitType[] InspectorSectionOrder =
    {
        OutfitType.Work,
        OutfitType.Main,
        OutfitType.Pyjamas,
        OutfitType.NightOut,
        OutfitType.Special,
        OutfitType.Storyline,
        //OutfitType.Undergarments
    };

    private List<OutfitName> _inspectorOrderedOutfits;

    public List<OutfitName> BurnedOutfits = new List<OutfitName>();


    private void Awake()
    {
        BuildOutfitLookup();
    }

    private void Start()
    {
        SetupAccessories();
        SetupHair();
        SetupShoes();
        SetupNecklaces();
        SetupGlasses();
        SetupAccessoriesPreview();
        SetupHairPreview();
        SetupShoesPreview();
        SetupNecklacesPreview();
        SetupGlassesPreview();

        if (!UnressedOnLoad) 
        { SwitchToOutfit(OutfitName.Work); }
        else { DisableAllMainOutfits(); }

        SetupInput();

        BurnedOutfits = new List<OutfitName>(StoredPrefs.Instance.GetCollection<List<OutfitName>>("BurnedOutfits"));

    }


    public void BurnOutfit()
    {
        if (!BurnedOutfits.Contains(currentOutfit))
        {
            BurnedOutfits.Add(currentOutfit);

            StoredPrefs.Instance.SetCollection("BurnedOutfits", BurnedOutfits, CollectionType.list);
            StoredPrefs.Instance.Save();
        }
    }
    
    

    private void OnValidate()
    {
        _outfitLookup = null;
        _inspectorOrderedOutfits = null;
    }

    private void SetupInput()
    {
        if (holdToggleDebug != null) { holdToggleDebug.action.performed += OnToggleDebug; }
        if (nextOutfit != null) { nextOutfit.action.performed += OnNextOutfit; }
        if (previousOutfit != null) { previousOutfit.action.performed += OnPreviousOutfit; }
    }

    private void OnToggleDebug(InputAction.CallbackContext ctx) { ToggleDebug(); }
    private void OnNextOutfit(InputAction.CallbackContext ctx) { NextOutfit(); }
    private void OnPreviousOutfit(InputAction.CallbackContext ctx) { PreviousOutfit(); }

    private void OnDisable()
    {
        if (nextOutfit != null) { nextOutfit.action.performed -= OnNextOutfit; }
        if (previousOutfit != null) { previousOutfit.action.performed -= OnPreviousOutfit; }
        if (holdToggleDebug != null) { holdToggleDebug.action.performed -= OnToggleDebug; }
    }



    private void ToggleDebug()
    {
        DebugMode = !DebugMode;
    }
    
    
    private List<OutfitName> GetOutfitsForStageExcludingBurned(OutfitStage stage)
    {
        return Outfits
            .Where(o => o != null && o.outfitStage == stage && o.SpawnAs && !BurnedOutfits.Contains(o.thisOutfit))
            .Select(o => o.thisOutfit)
            .ToList();
    }

    public void SelectAndApplyOutfitForDeaths(int deaths)
    {
        OutfitStage stage;

        if (deaths < 46) { stage = OutfitStage.StageOne; }
        else if (deaths < 92) { stage = OutfitStage.StageTwo; }
        else { stage = OutfitStage.StageThree; }

        List<OutfitName> pool = GetOutfitsForStageExcludingBurned(stage);

        if (pool.Count == 0)
        {
            Debug.LogWarning($"NorasWardrobe: No unburned outfits left for {stage}.");
            return;
        }

        OutfitName selected = pool[Random.Range(0, pool.Count)];

        SetMainOutfit(selected);

        StoredPrefs.Instance.SetInt("NorasOutfit", (int)selected);
        StoredPrefs.Instance.Save();
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

    private void ClearOutfitMeshesPreview()
    {
        if (SkinnedMeshRendererParentOutfitsPreview == null) { return; }
        for (int i = SkinnedMeshRendererParentOutfitsPreview.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = SkinnedMeshRendererParentOutfitsPreview.transform.GetChild(i);
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    private void SetupGlasses()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allGlasses =
        {
            ProfessionalGlasses, GlaPlaceholder1Glasses, GlaPlaceholder2Glasses, GlaPlaceholder3Glasses, GlaPlaceholder4Glasses
        };

        foreach (var smr in allGlasses)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }

    private void SetupGlassesPreview()
    {
        Transform rootBone = BodyPreview != null ? BodyPreview.rootBone : null;
        Transform[] bones = BodyPreview != null ? BodyPreview.bones : null;

        SkinnedMeshRenderer[] allGlassesPreview =
        {
            ProfessionalGlassesPreview, GlaPlaceholder1GlassesPreview, GlaPlaceholder2GlassesPreview, GlaPlaceholder3GlassesPreview, GlaPlaceholder4GlassesPreview
        };

        foreach (var smr in allGlassesPreview)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }
    
    
    private void SetupAccessories()
    {
        if (SkinnedMeshRendererParentAccessories == null) { return; }
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;
        foreach (var smr in SkinnedMeshRendererParentAccessories.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
        }
    }

    private void SetupAccessoriesPreview()
    {
        if (SkinnedMeshRendererParentAccessoriesPreview == null) { return; }
        Transform rootBone = BodyPreview != null ? BodyPreview.rootBone : null;
        Transform[] bones = BodyPreview != null ? BodyPreview.bones : null;
        foreach (var smr in SkinnedMeshRendererParentAccessoriesPreview.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
        }
    }

    private void SetupHair()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allHair =
        {
            DefaultHair, WorkHair, WorkHairTwo, CasualHair,
            PyjamaHair, DatingHair, DatingHairTwo, OutHair, UpHair, HomelessHair, TwinTailsHair, UpDo
        };

        foreach (var smr in allHair)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }

    private void SetupHairPreview()
    {
        Transform rootBone = BodyPreview != null ? BodyPreview.rootBone : null;
        Transform[] bones = BodyPreview != null ? BodyPreview.bones : null;

        SkinnedMeshRenderer[] allHairPreview =
        {
            DefaultHairPreview, WorkHairPreview, WorkHairTwoPreview, CasualHairPreview,
            PyjamaHairPreview, DatingHairPreview, DatingHairTwoPreview, OutHairPreview,
            UpHairPreview, HomelessHairPreview, TwinTailsHairPreview, UpDoPreview
        };

        foreach (var smr in allHairPreview)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }
    private void SetupShoes()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allShoes =
        {
            WorkFlatsShoes, SilentShoesShoes, SandalsShoes, BootsShoes, WhiteFlatsShoes,
            ShittyTrainersShoes, FMBShoes, BlackBowFlatsShoes, WhiteBowFlatsShoes,
            BlackBootsShoes, BlackFMBShoes, PrimAndProperShoes, ShuPlaceholder4Shoes
        };

        foreach (var smr in allShoes)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }

    private void SetupShoesPreview()
    {
        Transform rootBone = BodyPreview != null ? BodyPreview.rootBone : null;
        Transform[] bones = BodyPreview != null ? BodyPreview.bones : null;

        SkinnedMeshRenderer[] allShoesPreview =
        {
            WorkFlatsShoesPreview, SilentShoesShoesPreview, SandalsShoesPreview, BootsShoesPreview, WhiteFlatsShoesPreview,
            ShittyTrainersShoesPreview, FMBShoesPreview, BlackBowFlatsShoesPreview, WhiteBowFlatsShoesPreview,
            BlackBootsShoesPreview, BlackFMBShoesPreview, PrimAndProperShoesPreview, ShuPlaceholder4ShoesPreview
        };

        foreach (var smr in allShoesPreview)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }

    private void SetupNecklaces()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allNecklaces =
        {
            ButterflyNecklace, PearlsNecklace, CrossNecklace,
            NekPlaceholder1Necklace, NekPlaceholder2Necklace, NekPlaceholder3Necklace, NekPlaceholder4Necklace
        };

        foreach (var smr in allNecklaces)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }

    private void SetupNecklacesPreview()
    {
        Transform rootBone = BodyPreview != null ? BodyPreview.rootBone : null;
        Transform[] bones = BodyPreview != null ? BodyPreview.bones : null;

        SkinnedMeshRenderer[] allNecklacesPreview =
        {
            ButterflyNecklacePreview, PearlsNecklacePreview, CrossNecklacePreview,
            NekPlaceholder1NecklacePreview, NekPlaceholder2NecklacePreview, NekPlaceholder3NecklacePreview, NekPlaceholder4NecklacePreview
        };

        foreach (var smr in allNecklacesPreview)
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            smr.enabled = false;
        }
    }
    
    private void HideAllGlasses()
    {
        if (ProfessionalGlasses != null) { ProfessionalGlasses.enabled = false; }
        if (GlaPlaceholder1Glasses != null) { GlaPlaceholder1Glasses.enabled = false; }
        if (GlaPlaceholder2Glasses != null) { GlaPlaceholder2Glasses.enabled = false; }
        if (GlaPlaceholder3Glasses != null) { GlaPlaceholder3Glasses.enabled = false; }
        if (GlaPlaceholder4Glasses != null) { GlaPlaceholder4Glasses.enabled = false; }
    }

    private void HideAllGlassesPreview()
    {
        if (ProfessionalGlassesPreview != null) { ProfessionalGlassesPreview.enabled = false; }
        if (GlaPlaceholder1GlassesPreview != null) { GlaPlaceholder1GlassesPreview.enabled = false; }
        if (GlaPlaceholder2GlassesPreview != null) { GlaPlaceholder2GlassesPreview.enabled = false; }
        if (GlaPlaceholder3GlassesPreview != null) { GlaPlaceholder3GlassesPreview.enabled = false; }
        if (GlaPlaceholder4GlassesPreview != null) { GlaPlaceholder4GlassesPreview.enabled = false; }
    }
    
    private void HideAllHair()
    {
        if (DefaultHair != null) { DefaultHair.enabled = false; }
        if (WorkHair != null) { WorkHair.enabled = false; }
        if (WorkHairTwo != null) { WorkHairTwo.enabled = false; }
        if (CasualHair != null) { CasualHair.enabled = false; }
        if (PyjamaHair != null) { PyjamaHair.enabled = false; }
        if (DatingHair != null) { DatingHair.enabled = false; }
        if (DatingHairTwo != null) { DatingHairTwo.enabled = false; }
        if (OutHair != null) { OutHair.enabled = false; }
        if (HomelessHair != null) { HomelessHair.enabled = false; }
        if (TwinTailsHair != null) { TwinTailsHair.enabled = false; }
        if (UpHair != null) { UpHair.enabled = false; }
        if (UpDo != null) { UpDo.enabled = false; }
    }

    private void HideAllHairPreview()
    {
        if (DefaultHairPreview != null) { DefaultHairPreview.enabled = false; }
        if (WorkHairPreview != null) { WorkHairPreview.enabled = false; }
        if (WorkHairTwoPreview != null) { WorkHairTwoPreview.enabled = false; }
        if (CasualHairPreview != null) { CasualHairPreview.enabled = false; }
        if (PyjamaHairPreview != null) { PyjamaHairPreview.enabled = false; }
        if (DatingHairPreview != null) { DatingHairPreview.enabled = false; }
        if (DatingHairTwoPreview != null) { DatingHairTwoPreview.enabled = false; }
        if (OutHairPreview != null) { OutHairPreview.enabled = false; }
        if (HomelessHairPreview != null) { HomelessHairPreview.enabled = false; }
        if (TwinTailsHairPreview != null) { TwinTailsHairPreview.enabled = false; }
        if (UpHairPreview != null) { UpHairPreview.enabled = false; }
        if (UpDoPreview != null) { UpDoPreview.enabled = false; }
    }

    private void HideAllShoes()
    {
        if (WorkFlatsShoes != null) { WorkFlatsShoes.enabled = false; }

        if (SilentShoesShoes != null) { SilentShoesShoes.enabled = false; }

        if (SandalsShoes != null) { SandalsShoes.enabled = false; }

        if (BootsShoes != null) { BootsShoes.enabled = false; }

        if (WhiteFlatsShoes != null) { WhiteFlatsShoes.enabled = false; }

        if (ShittyTrainersShoes != null) { ShittyTrainersShoes.enabled = false; }

        if (FMBShoes != null) { FMBShoes.enabled = false; }

        if (BlackBowFlatsShoes != null) { BlackBowFlatsShoes.enabled = false; }

        if (WhiteBowFlatsShoes != null) { WhiteBowFlatsShoes.enabled = false; }

        if (BlackBootsShoes != null) { BlackBootsShoes.enabled = false; }

        if (BlackFMBShoes != null) { BlackFMBShoes.enabled = false; }

        if (PrimAndProperShoes != null) { PrimAndProperShoes.enabled = false; }

        if (ShuPlaceholder4Shoes != null) { ShuPlaceholder4Shoes.enabled = false; }
    }

    private void HideAllShoesPreview()
    {
        if (WorkFlatsShoesPreview != null) { WorkFlatsShoesPreview.enabled = false; }

        if (SilentShoesShoesPreview != null) { SilentShoesShoesPreview.enabled = false; }

        if (SandalsShoesPreview != null) { SandalsShoesPreview.enabled = false; }

        if (BootsShoesPreview != null) { BootsShoesPreview.enabled = false; }

        if (WhiteFlatsShoesPreview != null) { WhiteFlatsShoesPreview.enabled = false; }

        if (ShittyTrainersShoesPreview != null) { ShittyTrainersShoesPreview.enabled = false; }

        if (FMBShoesPreview != null) { FMBShoesPreview.enabled = false; }

        if (BlackBowFlatsShoesPreview != null) { BlackBowFlatsShoesPreview.enabled = false; }

        if (WhiteBowFlatsShoesPreview != null) { WhiteBowFlatsShoesPreview.enabled = false; }

        if (BlackBootsShoesPreview != null) { BlackBootsShoesPreview.enabled = false; }

        if (BlackFMBShoesPreview != null) { BlackFMBShoesPreview.enabled = false; }

        if (PrimAndProperShoesPreview != null) { PrimAndProperShoesPreview.enabled = false; }

        if (ShuPlaceholder4ShoesPreview != null) { ShuPlaceholder4ShoesPreview.enabled = false; }
    }
    
    private void HideAllNecklaces()
    {
        if (ButterflyNecklace != null) { ButterflyNecklace.enabled = false; }
        if (PearlsNecklace != null) { PearlsNecklace.enabled = false; }
        if (CrossNecklace != null) { CrossNecklace.enabled = false; }
        if (NekPlaceholder1Necklace != null) { NekPlaceholder1Necklace.enabled = false; }
        if (NekPlaceholder2Necklace != null) { NekPlaceholder2Necklace.enabled = false; }
        if (NekPlaceholder3Necklace != null) { NekPlaceholder3Necklace.enabled = false; }
        if (NekPlaceholder4Necklace != null) { NekPlaceholder4Necklace.enabled = false; }
    }

    private void HideAllNecklacesPreview()
    {
        if (ButterflyNecklacePreview != null) { ButterflyNecklacePreview.enabled = false; }
        if (PearlsNecklacePreview != null) { PearlsNecklacePreview.enabled = false; }
        if (CrossNecklacePreview != null) { CrossNecklacePreview.enabled = false; }
        if (NekPlaceholder1NecklacePreview != null) { NekPlaceholder1NecklacePreview.enabled = false; }
        if (NekPlaceholder2NecklacePreview != null) { NekPlaceholder2NecklacePreview.enabled = false; }
        if (NekPlaceholder3NecklacePreview != null) { NekPlaceholder3NecklacePreview.enabled = false; }
        if (NekPlaceholder4NecklacePreview != null) { NekPlaceholder4NecklacePreview.enabled = false; }
    }

    public void SetHair(HairName hair)
    {
        HideAllHair();
        switch (hair)
        {
            case HairName.DefaultHair: if (DefaultHair != null) { DefaultHair.enabled = true; } break;
            case HairName.WorkHair: if (WorkHair != null) { WorkHair.enabled = true; } break;
            case HairName.WorkHairTwo: if (WorkHairTwo != null) { WorkHairTwo.enabled = true; } break;
            case HairName.CasualHair: if (CasualHair != null) { CasualHair.enabled = true; } break;
            case HairName.PyjamaHair: if (PyjamaHair != null) { PyjamaHair.enabled = true; } break;
            case HairName.DatingHair: if (DatingHair != null) { DatingHair.enabled = true; } break;
            case HairName.DatingHairTwo: if (DatingHairTwo != null) { DatingHairTwo.enabled = true; } break;
            case HairName.OutHair: if (OutHair != null) { OutHair.enabled = true; } break;
            case HairName.HomelessHair: if (HomelessHair != null) { HomelessHair.enabled = true; } break;
            case HairName.TwinTailsHair: if (TwinTailsHair != null) { TwinTailsHair.enabled = true; } break;
            case HairName.UpHair: if (UpHair != null) { UpHair.enabled = true; } break;
            case HairName.UpDo: if (UpDo != null) { UpDo.enabled = true; } break;
        }
    }

    public void SetHairPreview(HairName hair)
    {
        HideAllHairPreview();
        switch (hair)
        {
            case HairName.DefaultHair: if (DefaultHairPreview != null) { DefaultHairPreview.enabled = true; } break;
            case HairName.WorkHair: if (WorkHairPreview != null) { WorkHairPreview.enabled = true; } break;
            case HairName.WorkHairTwo: if (WorkHairTwoPreview != null) { WorkHairTwoPreview.enabled = true; } break;
            case HairName.CasualHair: if (CasualHairPreview != null) { CasualHairPreview.enabled = true; } break;
            case HairName.PyjamaHair: if (PyjamaHairPreview != null) { PyjamaHairPreview.enabled = true; } break;
            case HairName.DatingHair: if (DatingHairPreview != null) { DatingHairPreview.enabled = true; } break;
            case HairName.DatingHairTwo: if (DatingHairTwoPreview != null) { DatingHairTwoPreview.enabled = true; } break;
            case HairName.OutHair: if (OutHairPreview != null) { OutHairPreview.enabled = true; } break;
            case HairName.HomelessHair: if (HomelessHairPreview != null) { HomelessHairPreview.enabled = true; } break;
            case HairName.TwinTailsHair: if (TwinTailsHairPreview != null) { TwinTailsHairPreview.enabled = true; } break;
            case HairName.UpHair: if (UpHairPreview != null) { UpHairPreview.enabled = true; } break;
            case HairName.UpDo: if (UpDoPreview != null) { UpDoPreview.enabled = true; } break;
        }
    }

    public void SetShoes(ShoesName shoes)
    {
        HideAllShoes();
        
        switch (shoes)
        {
            case ShoesName.WorkFlats:
                if (WorkFlatsShoes != null) { WorkFlatsShoes.enabled = true; }

                break;
            case ShoesName.SilentShoes:
                if (SilentShoesShoes != null) { SilentShoesShoes.enabled = true; }

                break;
            case ShoesName.Sandals:
                if (SandalsShoes != null) { SandalsShoes.enabled = true; }

                break;
            case ShoesName.Boots:
                if (BootsShoes != null) { BootsShoes.enabled = true; }

                break;
            case ShoesName.WhiteFlats:
                if (WhiteFlatsShoes != null) { WhiteFlatsShoes.enabled = true; }

                break;
            case ShoesName.ShittyTrainers:
                if (ShittyTrainersShoes != null) { ShittyTrainersShoes.enabled = true; }

                break;
            case ShoesName.FMB:
                if (FMBShoes != null) { FMBShoes.enabled = true; }

                break;
            case ShoesName.BlackBowFlats:
                if (BlackBowFlatsShoes != null) { BlackBowFlatsShoes.enabled = true; }

                break;
            case ShoesName.WhiteBowFlats:
                if (WhiteBowFlatsShoes != null) { WhiteBowFlatsShoes.enabled = true; }

                break;
            case ShoesName.BlackBoots:
                if (BlackBootsShoes != null) { BlackBootsShoes.enabled = true; }

                break;
            case ShoesName.BlackFMB:
                if (BlackFMBShoes != null) { BlackFMBShoes.enabled = true; }

                break;
            case ShoesName.PrimAndProper:
                if (PrimAndProperShoes != null) { PrimAndProperShoes.enabled = true; }

                break;
            case ShoesName.ShuPlaceholder4:
                if (ShuPlaceholder4Shoes != null) { ShuPlaceholder4Shoes.enabled = true; }

                break;
            case ShoesName.None:

                break;
        }
    }

    public void SetShoesPreview(ShoesName shoes)
    {
        HideAllShoesPreview();
        switch (shoes)
        {
            case ShoesName.WorkFlats: if (WorkFlatsShoesPreview != null) { WorkFlatsShoesPreview.enabled = true; }

            break; case ShoesName.SilentShoes: if (SilentShoesShoesPreview != null) { SilentShoesShoesPreview.enabled = true; }

            break; case ShoesName.Sandals: if (SandalsShoesPreview != null) { SandalsShoesPreview.enabled = true; }

            break; case ShoesName.Boots: if (BootsShoesPreview != null) { BootsShoesPreview.enabled = true; }

            break; case ShoesName.WhiteFlats: if (WhiteFlatsShoesPreview != null) { WhiteFlatsShoesPreview.enabled = true; }

            break; case ShoesName.ShittyTrainers: if (ShittyTrainersShoesPreview != null) { ShittyTrainersShoesPreview.enabled = true; }

            break; case ShoesName.FMB: if (FMBShoesPreview != null) { FMBShoesPreview.enabled = true; }

            break; case ShoesName.BlackBowFlats: if (BlackBowFlatsShoesPreview != null) { BlackBowFlatsShoesPreview.enabled = true; }

            break; case ShoesName.WhiteBowFlats: if (WhiteBowFlatsShoesPreview != null) { WhiteBowFlatsShoesPreview.enabled = true; }

            break; case ShoesName.BlackBoots: if (BlackBootsShoesPreview != null) { BlackBootsShoesPreview.enabled = true; }

            break; case ShoesName.BlackFMB: if (BlackFMBShoesPreview != null) { BlackFMBShoesPreview.enabled = true; }

            break; case ShoesName.PrimAndProper: if (PrimAndProperShoesPreview != null) { PrimAndProperShoesPreview.enabled = true; }

            break; case ShoesName.ShuPlaceholder4: if (ShuPlaceholder4ShoesPreview != null) { ShuPlaceholder4ShoesPreview.enabled = true; }

            break;
        }
    }
    
    public void SetNecklace(NecklaceName necklace)
    {
        HideAllNecklaces();
        switch (necklace)
        {
            case NecklaceName.Butterfly: if (ButterflyNecklace != null) { ButterflyNecklace.enabled = true; } break;
            case NecklaceName.Pearls: if (PearlsNecklace != null) { PearlsNecklace.enabled = true; } break;
            case NecklaceName.Cross: if (CrossNecklace != null) { CrossNecklace.enabled = true; } break;
            case NecklaceName.NekPlaceholder1: if (NekPlaceholder1Necklace != null) { NekPlaceholder1Necklace.enabled = true; } break;
            case NecklaceName.NekPlaceholder2: if (NekPlaceholder2Necklace != null) { NekPlaceholder2Necklace.enabled = true; } break;
            case NecklaceName.NekPlaceholder3: if (NekPlaceholder3Necklace != null) { NekPlaceholder3Necklace.enabled = true; } break;
            case NecklaceName.NekPlaceholder4: if (NekPlaceholder4Necklace != null) { NekPlaceholder4Necklace.enabled = true; } break;
            case NecklaceName.None: break;
        }
    }
    
    public void SetNecklacePreview(NecklaceName necklace)
    {
        HideAllNecklacesPreview();
        switch (necklace)
        {
            case NecklaceName.Butterfly: if (ButterflyNecklacePreview != null) { ButterflyNecklacePreview.enabled = true; } break;
            case NecklaceName.Pearls: if (PearlsNecklacePreview != null) { PearlsNecklacePreview.enabled = true; } break;
            case NecklaceName.Cross: if (CrossNecklacePreview != null) { CrossNecklacePreview.enabled = true; } break;
            case NecklaceName.NekPlaceholder1: if (NekPlaceholder1NecklacePreview != null) { NekPlaceholder1NecklacePreview.enabled = true; } break;
            case NecklaceName.NekPlaceholder2: if (NekPlaceholder2NecklacePreview != null) { NekPlaceholder2NecklacePreview.enabled = true; } break;
            case NecklaceName.NekPlaceholder3: if (NekPlaceholder3NecklacePreview != null) { NekPlaceholder3NecklacePreview.enabled = true; } break;
            case NecklaceName.NekPlaceholder4: if (NekPlaceholder4NecklacePreview != null) { NekPlaceholder4NecklacePreview.enabled = true; } break;
            case NecklaceName.None: break;
        }
    }
    
    public void SetGlasses(GlassesName glasses)
    {
        HideAllGlasses();
        switch (glasses)
        {
            case GlassesName.Professional: if (ProfessionalGlasses != null) { ProfessionalGlasses.enabled = true; } break;
            case GlassesName.GlaPlaceholder1: if (GlaPlaceholder1Glasses != null) { GlaPlaceholder1Glasses.enabled = true; } break;
            case GlassesName.GlaPlaceholder2: if (GlaPlaceholder2Glasses != null) { GlaPlaceholder2Glasses.enabled = true; } break;
            case GlassesName.GlaPlaceholder3: if (GlaPlaceholder3Glasses != null) { GlaPlaceholder3Glasses.enabled = true; } break;
            case GlassesName.GlaPlaceholder4: if (GlaPlaceholder4Glasses != null) { GlaPlaceholder4Glasses.enabled = true; } break;
            case GlassesName.None: break;
        }
    }

    public void SetGlassesPreview(GlassesName glasses)
    {
        HideAllGlassesPreview();
        switch (glasses)
        {
            case GlassesName.Professional: if (ProfessionalGlassesPreview != null) { ProfessionalGlassesPreview.enabled = true; } break;
            case GlassesName.GlaPlaceholder1: if (GlaPlaceholder1GlassesPreview != null) { GlaPlaceholder1GlassesPreview.enabled = true; } break;
            case GlassesName.GlaPlaceholder2: if (GlaPlaceholder2GlassesPreview != null) { GlaPlaceholder2GlassesPreview.enabled = true; } break;
            case GlassesName.GlaPlaceholder3: if (GlaPlaceholder3GlassesPreview != null) { GlaPlaceholder3GlassesPreview.enabled = true; } break;
            case GlassesName.GlaPlaceholder4: if (GlaPlaceholder4GlassesPreview != null) { GlaPlaceholder4GlassesPreview.enabled = true; } break;
            case GlassesName.None: break;
        }
    }
    
    private HairName GetHairForOutfit(OutfitName outfit)
    {
        return GetOutfit(outfit)?.Hair ?? HairName.DefaultHair;
    }

    private ShoesName GetShoesForOutfit(OutfitName outfit)
    {
        return GetOutfit(outfit)?.Shoes ?? ShoesName.WorkFlats;
    }
    
    private NecklaceName GetNecklaceForOutfit(OutfitName outfit)
    {
        return GetOutfit(outfit)?.Necklace ?? NecklaceName.None;
    }
    
    private GlassesName GetGlassesForOutfit(OutfitName outfit)
    {
        return GetOutfit(outfit)?.Glasses ?? GlassesName.None;
    }
    
    private Outfit GetOutfit(OutfitName outfit)
    {
        if (_outfitLookup == null) { BuildOutfitLookup(); }
        return _outfitLookup.TryGetValue(outfit, out Outfit data) ? data : null;
    }

    private void BuildOutfitLookup()
    {
        _outfitLookup = new Dictionary<OutfitName, Outfit>();
        if (Outfits == null) { return; }
        foreach (var outfit in Outfits)
        {
            if (outfit == null) { continue; }
            _outfitLookup[outfit.thisOutfit] = outfit;
        }
    }

    private List<OutfitName> GetInspectorOrderedOutfits()
    {
        if (_inspectorOrderedOutfits != null) { return _inspectorOrderedOutfits; }

        _inspectorOrderedOutfits = new List<OutfitName>();
        if (Outfits == null) { return _inspectorOrderedOutfits; }

        foreach (var type in InspectorSectionOrder)
        {
            foreach (var outfit in Outfits)
            {
                if (outfit == null || outfit.thisOutfit == OutfitName.None) { continue; }
                if (outfit.outfitType != type) { continue; }
                _inspectorOrderedOutfits.Add(outfit.thisOutfit);
            }
        }

        return _inspectorOrderedOutfits;
    }

    private void InstantiatePrefabs(List<GameObject> prefabs)
    {
        if (prefabs == null || SkinnedMeshRendererParentOutfits == null) { return; }
        if (Body == null) { Debug.LogError("Body SkinnedMeshRenderer is not assigned on NorasWardrobe!"); return; }
        Transform rootBone = Body.rootBone;
        Transform[] bones = Body.bones;
        foreach (var prefab in prefabs)
        {
            if (prefab == null) { continue; }
            GameObject instance = Instantiate(prefab, SkinnedMeshRendererParentOutfits.transform);
            foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr == null) { continue; }
                smr.rootBone = rootBone;
                smr.bones = bones;
                smr.updateWhenOffscreen = true;
                smr.enabled = true;
            }
        }
    }

    private void InstantiatePrefabsPreview(List<GameObject> prefabs)
    {
        if (prefabs == null || SkinnedMeshRendererParentOutfitsPreview == null) { return; }
        if (BodyPreview == null) { Debug.LogError("BodyPreview SkinnedMeshRenderer is not assigned on NorasWardrobe!"); return; }
        Transform rootBone = BodyPreview.rootBone;
        Transform[] bones = BodyPreview.bones;
        foreach (var prefab in prefabs)
        {
            if (prefab == null) { continue; }
            GameObject instance = Instantiate(prefab, SkinnedMeshRendererParentOutfitsPreview.transform);
            foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr == null) { continue; }
                smr.rootBone = rootBone;
                smr.bones = bones;
                smr.updateWhenOffscreen = true;
                smr.enabled = true;
            }
        }
    }

    public bool IsOutfitEnabled(OutfitName outfit)
    {
        bool canSpawn = false;

        var data = GetOutfit(outfit);
        
        if (DebugMode)
        {
            canSpawn = true;
        }
        else
        {
            canSpawn = data.SpawnAs;
        }
        
        
        return data == null || canSpawn;
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
    

    public void ToggleCigarette(bool? forceOn = null)
    {
        _cigaretteOverride = forceOn.HasValue ? forceOn.Value : !_cigaretteOverride;
        ApplyAccessories();
    }

    public void ResetAccessoryOverrides()
    {
        _wingsOverride = false;
        _overallOverride = false;
        _hatOverride = false;
        _chokerOverride = false;
        _cigaretteOverride = false;
    }

    private void ApplyAccessories()
    {
        var data = GetOutfit(currentOutfit);
        bool w = data != null && data.Wings;
        bool o = data != null && data.Apron;
        bool h = data != null && data.Hat;
        bool c = data != null && data.Choker;
        bool ci = data != null && data.Cigarette;
        if (ButterflyWings != null) { ButterflyWings.enabled = w || _wingsOverride; }
        if (Overall != null) { Overall.enabled = o || _overallOverride; }
        if (Hat != null) { Hat.enabled = h || _hatOverride; }
        if (Choker != null) { Choker.enabled = c || _chokerOverride; }
        if (Cigarette != null) { Cigarette.enabled = ci || _cigaretteOverride; }
    }

    private void ApplyAccessoriesPreview(OutfitName outfit)
    {
        var data = GetOutfit(outfit);
        bool w = data != null && data.Wings;
        bool o = data != null && data.Apron;
        bool h = data != null && data.Hat;
        bool c = data != null && data.Choker;
        bool ci = data != null && data.Cigarette;
        if (ButterflyWingsPreview != null) { ButterflyWingsPreview.enabled = w; }
        if (OverallPreview != null) { OverallPreview.enabled = o; }
        if (HatPreview != null) { HatPreview.enabled = h; }
        if (ChokerPreview != null) { ChokerPreview.enabled = c; }
        if (CigarettePreview != null) { CigarettePreview.enabled = ci; }
    }
    
    private void HideAllAccessories()
    {
        if (ButterflyWings != null) { ButterflyWings.enabled = false; }
        if (Overall != null) { Overall.enabled = false; }
        if (Hat != null) { Hat.enabled = false; }
        if (Choker != null) { Choker.enabled = false; }
        if (Cigarette != null) { Cigarette.enabled = false; }
    }

    private void HideAllAccessoriesPreview()
    {
        if (ButterflyWingsPreview != null) { ButterflyWingsPreview.enabled = false; }
        if (OverallPreview != null) { OverallPreview.enabled = false; }
        if (HatPreview != null) { HatPreview.enabled = false; }
        if (ChokerPreview != null) { ChokerPreview.enabled = false; }
        if (CigarettePreview != null) { CigarettePreview.enabled = false; }
    }
    
    public void NextOutfit()
    {
        List<OutfitName> order = GetInspectorOrderedOutfits();
        if (order.Count == 0) { return; }

        int index = order.IndexOf(currentOutfit);

        for (int i = 0; i < order.Count; i++)
        {
            index = (index + 1) % order.Count;
            if (IsOutfitEnabled(order[index]))
            {
                SwitchToOutfit(order[index]);
                return;
            }
        }
    }

    public void PreviousOutfit()
    {
        List<OutfitName> order = GetInspectorOrderedOutfits();
        if (order.Count == 0) { return; }

        int index = order.IndexOf(currentOutfit);
        if (index < 0) { index = 0; }

        for (int i = 0; i < order.Count; i++)
        {
            index = (index - 1 + order.Count) % order.Count;
            if (IsOutfitEnabled(order[index]))
            {
                SwitchToOutfit(order[index]);
                return;
            }
        }
    }

    private void SwitchToOutfit(OutfitName outfit)
    {
        if (outfit == OutfitName.None) { DisableAllMainOutfits(); return; }
        ToggleOutfit(outfit, true);
    }

    public void ToggleOutfit(OutfitName outfit, bool? forceOn = null)
    {
        var data = GetOutfit(outfit);
        JiggleToggle(data != null && data.Jiggle);
        ToggleMainOutfit(outfit, forceOn);
    }

    public void ToggleUndergarments(bool? forceOn = null) { ToggleOutfit(OutfitName.Undergarments, forceOn); }

    private void JiggleToggle(bool isJiggly = false)
    {
        if (JiggleLeftBoob != null) { JiggleLeftBoob.SetActive(isJiggly); }
        if (JiggleRightBoob != null) { JiggleRightBoob.SetActive(isJiggly); }
        if (JiggleLeftButtcheek != null) { JiggleLeftButtcheek.SetActive(isJiggly); }
        if (JiggleRightButtcheek != null) { JiggleRightButtcheek.SetActive(isJiggly); }
    }

    private void ToggleMainOutfit(OutfitName outfit, bool? forceOn = null)
    {
        if (forceOn.HasValue)
        {
            if (forceOn.Value) { SetMainOutfit(outfit); }
            else { DisableAllMainOutfits(); }
        }
        else
        {
            if (currentOutfit == outfit) { DisableAllMainOutfits(); }
            else { SetMainOutfit(outfit); }
        }
    }

    public void SetMainOutfit(OutfitName outfit)
    {
        HideAllAccessories();
        ClearOutfitMeshes();
        currentOutfit = outfit;

        var data = GetOutfit(outfit);
        if (data != null) { InstantiatePrefabs(data.OutfitPrefabs); }

        ApplyBodyColors(outfit);
        ApplyAccessories();
        SetHair(GetHairForOutfit(outfit));
        SetShoes(GetShoesForOutfit(outfit));
        SetNecklace(GetNecklaceForOutfit(outfit));
        SetGlasses(GetGlassesForOutfit(outfit));

        Debug.Log("outfit name passed: "+outfit);
        Debug.Log("wardrobe count when asked: "+_outfitLookup.Count);

        EventManager.OutfitWasChanged(data.outfitTitle);
    }
    
    public void SetMainOutfitPreview(OutfitName outfit)
    {
        HideAllAccessoriesPreview();
        ClearOutfitMeshesPreview();

        var data = GetOutfit(outfit);
        if (data != null) { InstantiatePrefabsPreview(data.OutfitPrefabs); }

        ApplyAccessoriesPreview(outfit);
        SetHairPreview(GetHairForOutfit(outfit));
        SetShoesPreview(GetShoesForOutfit(outfit));
        SetNecklacePreview(GetNecklaceForOutfit(outfit));
        SetGlassesPreview(GetGlassesForOutfit(outfit));
    }

    public void ApplyPreviewOutfit(OutfitName outfit)
    {
        ToggleOutfit(outfit);
        ClearPreview();
    }

    public void CancelPreviewOutfit()
    {
        ClearPreview();
    }

    public void ClearPreview()
    {
    }

    public void DisableAllMainOutfits()
    {
        HideAllAccessories();
        ClearOutfitMeshes();
        currentOutfit = OutfitName.None;
        SetHair(HairName.DefaultHair);
        HideAllShoes();
        HideAllNecklaces();
        HideAllGlasses();
        ApplyAccessories();
    }

    private OutfitName PickRandom(OutfitName[] pool)
    {
        OutfitName[] eligible = pool.Where(o => IsOutfitEnabled(o) && o != currentOutfit).ToArray();
        if (eligible.Length == 0)
            eligible = pool.Where(o => IsOutfitEnabled(o)).ToArray();

        if (eligible.Length == 0)
            return currentOutfit;

        return eligible[Random.Range(0, eligible.Length)];
    }

    private OutfitName[] GetOutfitsOfType(OutfitType type)
    {
        if (Outfits == null) { return new OutfitName[0]; }
        return Outfits
            .Where(o => o != null && o.outfitType == type && o.thisOutfit != OutfitName.None)
            .Select(o => o.thisOutfit)
            .ToArray();
    }

    public void SetRandomOutfitOfType(OutfitType type)
    {
        OutfitName[] pool;

        switch (type)
        {
            case OutfitType.Work:
                pool = GetOutfitsOfType(OutfitType.Work);
                break;
            case OutfitType.Main:
                pool = GetOutfitsOfType(OutfitType.Main);
                break;
            case OutfitType.Pyjamas:
                pool = GetOutfitsOfType(OutfitType.Pyjamas);
                break;
            case OutfitType.NightOut:
                pool = GetOutfitsOfType(OutfitType.NightOut);
                break;
            case OutfitType.Special:
                pool = GetOutfitsOfType(OutfitType.Special);
                break;
            case OutfitType.Storyline:
                pool = GetOutfitsOfType(OutfitType.Storyline);
                break;
            case OutfitType.Undergarments:
                pool = GetOutfitsOfType(OutfitType.Undergarments);
                break;
            default:
                Debug.LogWarning($"NorasWardrobe: No outfit pool defined for type {type}");
                return;
        }

        if (pool.Length == 0)
        {
            Debug.LogWarning($"NorasWardrobe: No outfits found for type {type}");
            return;
        }

        SwitchToOutfit(PickRandom(pool));
    }


    public void SetRandomOutfitOfTypeAndStage(OutfitType type, OutfitStage outfitStage = OutfitStage.StageOne)
    {
        OutfitName[] pool;

        switch (type)
        {
            case OutfitType.Work:
                pool = GetOutfitsOfType(OutfitType.Work);
                break;
            case OutfitType.Main:
                pool = GetOutfitsOfType(OutfitType.Main);
                break;
            case OutfitType.Pyjamas:
                pool = GetOutfitsOfType(OutfitType.Pyjamas);
                break;
            case OutfitType.NightOut:
                pool = GetOutfitsOfType(OutfitType.NightOut);
                break;
            case OutfitType.Special:
                pool = GetOutfitsOfType(OutfitType.Special);
                break;
            case OutfitType.Storyline:
                pool = GetOutfitsOfType(OutfitType.Storyline);
                break;
            case OutfitType.Undergarments:
                pool = GetOutfitsOfType(OutfitType.Undergarments);
                break;
            default:
                Debug.LogWarning($"NorasWardrobe: No outfit pool defined for type {type}");
                return;
        }

        if (pool.Length == 0)
        {
            Debug.LogWarning($"NorasWardrobe: No outfits found for type {type}");
            return;
        }

        SwitchToOutfit(PickRandom(pool));
    }

    private void ApplyBodyColors(OutfitName outfit)
    {
        if (Body == null) { return; }
        Color lipColor = GetLipColorForOutfit(outfit);
        Color nailColor = GetNailColorForOutfit(outfit);
        Material[] materials = Application.isPlaying ? Body.materials : Body.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null) { continue; }
            string matName = materials[i].name.ToLower();
            if (matName.Contains("lips")) { materials[i].color = lipColor; }
            else if (matName.Contains("fingernail") || matName.Contains("nail")) { materials[i].color = nailColor; }
        }
        if (Application.isPlaying) { Body.materials = materials; }
    }

    private Color GetLipColorForOutfit(OutfitName outfit)
    {
        var data = GetOutfit(outfit);
        return data != null ? data.lipsColor : Color.white;
    }

    private Color GetNailColorForOutfit(OutfitName outfit)
    {
        var data = GetOutfit(outfit);
        return data != null ? data.nailsColor : Color.white;
    }

    public void Undress()
    {
        DisableAllMainOutfits();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NorasWardrobe))]
public class NorasWardrobeEditor : Editor
{
    private const float ButtonHeight = 28f;
    private const float ButtonSpacing = 4f;
    private const float SideMargin = 40f;
    private const int DefaultColumns = 3;

    private OutfitName? _pendingPreviewOutfit;
    private string _outfitSearchText = string.Empty;
    private Rect _searchAreaRect;

    private void OnEnable()
    {
        _pendingPreviewOutfit = null;
        _outfitSearchText = string.Empty;
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (string.IsNullOrEmpty(_outfitSearchText)) { return; }

        EditorWindow focused = EditorWindow.focusedWindow;
        bool inspectorFocused = focused != null && focused.GetType().Name == "InspectorWindow";

        if (!inspectorFocused)
        {
            _outfitSearchText = string.Empty;
            Repaint();
        }
    }

    public override void OnInspectorGUI()
    {
        NorasWardrobe me = (NorasWardrobe)target;

        if (!me.PreviewEnabled && _pendingPreviewOutfit.HasValue)
        {
            me.CancelPreviewOutfit();
            _pendingPreviewOutfit = null;
        }

        var clearAllStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };


        

        var redButton = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(3, 3, 3, 3)
        };
        
        
        Texture2D myTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Editor/butterflyeditor.png"
        );

        var boxStyle = new GUIStyle
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(5, 5, 5, 5)
        };

        boxStyle.normal.textColor = Color.white;
        boxStyle.normal.background = myTexture;


        var statsTitle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white },
            padding = new RectOffset(3, 3, 3, 3)
        };


        var statsStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white },
            padding = new RectOffset(3, 3, 3, 3)
        };


        var statsbuttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            padding = new RectOffset(3, 3, 3, 3)
        };


        var resultsbuttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.7f, 0.4f, 1f) },
            padding = new RectOffset(3, 3, 3, 3)
        };


        
        EditorGUILayout.BeginVertical(boxStyle);
        
        DrawOutfitStats(me,  boxStyle,statsTitle, statsStyle, statsbuttonStyle);

        EditorGUILayout.Space();
        

        GUI.backgroundColor = Color.red;
        DrawButtonRow(me, redButton, ("Previous Outfit", () => { me.PreviousOutfit(); SelectOutfitAsset(me); }), ("Next Outfit", () => { me.NextOutfit(); SelectOutfitAsset(me); }));
        
        
        EditorGUILayout.Space();

        
        
        
        
        
        DrawOutfitSearch(me, statsTitle, resultsbuttonStyle);

        
        
        EditorGUILayout.Space();
        DrawOutfitTypeSection(me, clearAllStyle, "Work Outfits", OutfitType.Work, Color.cyan);

        EditorGUILayout.Space();
        DrawOutfitTypeSection(me, clearAllStyle, "Main Outfits", OutfitType.Main, Color.green);

        EditorGUILayout.Space();
        DrawOutfitTypeSection(me, clearAllStyle, "PJs", OutfitType.Pyjamas, new Color(0.5f, 0.8f, 1f));

        EditorGUILayout.Space();
        DrawOutfitTypeSection(me, clearAllStyle, "Night Out", OutfitType.NightOut, new Color(1f, 0.6f, 0.8f));

        EditorGUILayout.Space();
        DrawOutfitTypeSection(me, clearAllStyle, "Special Outfits", OutfitType.Special, new Color(0.7f, 0.4f, 1f));

        EditorGUILayout.Space();
        DrawOutfitTypeSection(me, clearAllStyle, "Storyline Outfits", OutfitType.Storyline, new Color(0.1f, 0.1f, 0.5f));

        EditorGUILayout.Space();
        DrawOutfitTypeSection(me, clearAllStyle, "Undergarments", OutfitType.Undergarments, Color.red);

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.yellow;

        EditorGUILayout.LabelField("Accessories", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Toggle Wings", () => me.ToggleWings()),
            ("Toggle Overall", () => me.ToggleOverall()),
            ("Toggle Hat", () => me.ToggleHat()),
            ("Toggle Choker", () => me.ToggleChoker()));
        DrawButtonRow(me, clearAllStyle,
            ("Toggle Cigarette", () => me.ToggleCigarette()));

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.5f);

        EditorGUILayout.LabelField("Shoes", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("WorkFlats", () => me.SetShoes(ShoesName.WorkFlats)),
            ("SilentShoes", () => me.SetShoes(ShoesName.SilentShoes)),
            ("Sandals", () => me.SetShoes(ShoesName.Sandals)));
        DrawButtonRow(me, clearAllStyle,
            ("Boots", () => me.SetShoes(ShoesName.Boots)),
            ("WhiteFlats", () => me.SetShoes(ShoesName.WhiteFlats)));
        DrawButtonRow(me, clearAllStyle,
            ("ShittyTrainers", () => me.SetShoes(ShoesName.ShittyTrainers)),
            ("FMB", () => me.SetShoes(ShoesName.FMB)));
        DrawButtonRow(me, clearAllStyle,
            ("BlackBowFlats", () => me.SetShoes(ShoesName.BlackBowFlats)),
            ("WhiteBowFlats", () => me.SetShoes(ShoesName.WhiteBowFlats)));
        DrawButtonRow(me, clearAllStyle,
            ("BlackBoots", () => me.SetShoes(ShoesName.BlackBoots)),
            ("BlackFMB", () => me.SetShoes(ShoesName.BlackFMB)));
        DrawButtonRow(me, clearAllStyle,
            ("PrimAndProper", () => me.SetShoes(ShoesName.PrimAndProper)),
            ("ShuPlaceholder4", () => me.SetShoes(ShoesName.ShuPlaceholder4)));
        DrawButtonRow(me, clearAllStyle, ("None", () => me.SetShoes(ShoesName.None)));

        
        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.8f, 0.7f, 0.9f);

        EditorGUILayout.LabelField("Necklace", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Butterfly", () => me.SetNecklace(NecklaceName.Butterfly)),
            ("Pearls", () => me.SetNecklace(NecklaceName.Pearls)),
            ("Cross", () => me.SetNecklace(NecklaceName.Cross)));
        DrawButtonRow(me, clearAllStyle,
            ("NekPlaceholder1", () => me.SetNecklace(NecklaceName.NekPlaceholder1)),
            ("NekPlaceholder2", () => me.SetNecklace(NecklaceName.NekPlaceholder2)));
        DrawButtonRow(me, clearAllStyle,
            ("NekPlaceholder3", () => me.SetNecklace(NecklaceName.NekPlaceholder3)),
            ("NekPlaceholder4", () => me.SetNecklace(NecklaceName.NekPlaceholder4)));
        DrawButtonRow(me, clearAllStyle,
            ("None", () => me.SetNecklace(NecklaceName.None)));
        
        
        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.7f, 0.85f, 0.95f);

        EditorGUILayout.LabelField("Glasses", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Professional", () => me.SetGlasses(GlassesName.Professional)),
            ("GlaPlaceholder1", () => me.SetGlasses(GlassesName.GlaPlaceholder1)));
        DrawButtonRow(me, clearAllStyle,
            ("GlaPlaceholder2", () => me.SetGlasses(GlassesName.GlaPlaceholder2)),
            ("GlaPlaceholder3", () => me.SetGlasses(GlassesName.GlaPlaceholder3)));
        DrawButtonRow(me, clearAllStyle,
            ("GlaPlaceholder4", () => me.SetGlasses(GlassesName.GlaPlaceholder4)),
            ("None", () => me.SetGlasses(GlassesName.None)));
        
        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);

        EditorGUILayout.LabelField("Hair", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Default Hair", () => me.SetHair(HairName.DefaultHair)),
            ("Work Hair", () => me.SetHair(HairName.WorkHair)),
            ("Work Hair Two", () => me.SetHair(HairName.WorkHairTwo)));
        DrawButtonRow(me, clearAllStyle,
            ("Casual Hair", () => me.SetHair(HairName.CasualHair)),
            ("Pyjama Hair", () => me.SetHair(HairName.PyjamaHair)),
            ("Dating Hair", () => me.SetHair(HairName.DatingHair)));
        DrawButtonRow(me, clearAllStyle,
            ("Dating Hair Two", () => me.SetHair(HairName.DatingHairTwo)),
            ("Out Hair", () => me.SetHair(HairName.OutHair)),
            ("Homeless Hair", () => me.SetHair(HairName.HomelessHair)));
        DrawButtonRow(me, clearAllStyle,
            ("Twin Tails Hair", () => me.SetHair(HairName.TwinTailsHair)),
            ("Up Hair", () => me.SetHair(HairName.UpHair)));
        DrawButtonRow(me, clearAllStyle,
            ("Updo", () => me.SetHair(HairName.UpDo)));

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Random Work", () => me.SetRandomOutfitOfType(OutfitType.Work)),
            ("Random Main", () => me.SetRandomOutfitOfType(OutfitType.Main)));
        DrawButtonRow(me, clearAllStyle,
            ("Random Night Out", () => me.SetRandomOutfitOfType(OutfitType.NightOut)),
            ("Random Pyjamas", () => me.SetRandomOutfitOfType(OutfitType.Pyjamas)));
        DrawButtonRow(me, clearAllStyle,
            ("Random Storyline", () => me.SetRandomOutfitOfType(OutfitType.Storyline)),
            ("Random Undergarments", () => me.SetRandomOutfitOfType(OutfitType.Undergarments)));
        DrawButtonRow(me, clearAllStyle,
            ("Random Special Outfit", () => me.SetRandomOutfitOfType(OutfitType.Special)));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("---------------------", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
        DrawDefaultInspector();
    }

    private void DrawOutfitSearch(NorasWardrobe me, GUIStyle titleStyle, GUIStyle labelStyle)
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Search Outfits", titleStyle);

        var searchFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };

        _outfitSearchText = EditorGUILayout.TextField(_outfitSearchText, searchFieldStyle, GUILayout.Height(ButtonHeight));
        
        
        if (!string.IsNullOrEmpty(_outfitSearchText))
        {
            string query = _outfitSearchText.ToLowerInvariant();

            List<OutfitName> matches = System.Enum.GetValues(typeof(OutfitName))
                .Cast<OutfitName>()
                .Where(o => o != OutfitName.None && o.ToString().ToLowerInvariant().Contains(query))
                .ToList();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (matches.Count == 0)
            {
                EditorGUILayout.LabelField("No matches", EditorStyles.miniLabel);
            }
            else
            {
                Color previousColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.cyan;

                foreach (var match in matches)
                {
                    if (GUILayout.Button(match.ToString(), labelStyle))
                    {
                        me.ToggleOutfit(match, true);
                        SelectOutfitAsset(me);
                        EditorUtility.SetDirty(me);
                    }
                }

                GUI.backgroundColor = previousColor;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.Repaint)
        {
            _searchAreaRect = GUILayoutUtility.GetLastRect();
        }

        if (!string.IsNullOrEmpty(_outfitSearchText) &&
            Event.current.type == EventType.MouseDown &&
            !_searchAreaRect.Contains(Event.current.mousePosition))
        {
            _outfitSearchText = string.Empty;
            GUI.FocusControl(null);
            Repaint();
        }
    }

    private static int CountCanSpawn(List<Outfit> outfits, OutfitType? type = null)
    {
        return outfits.Count(o => o.SpawnAs && (type == null || o.outfitType == type));
    }

    private static int CountByStage(List<Outfit> outfits, OutfitStage stage)
    {
        return outfits.Count(o => o.outfitStage == stage);
    }

    private static void DrawOutfitStats(NorasWardrobe me, GUIStyle boxStyle, GUIStyle titleStyle, GUIStyle statStyle, GUIStyle buttonStyle)
    {
        var outfits = (me.Outfits ?? new List<Outfit>()).Where(o => o != null && o.thisOutfit != OutfitName.None).ToList();

        int canSpawnCount = CountCanSpawn(outfits);
        
        
        EditorGUILayout.BeginVertical();
        
        
        EditorGUILayout.LabelField("Number of outfits: ", titleStyle);

        EditorGUILayout.Space();
        foreach (OutfitType type in System.Enum.GetValues(typeof(OutfitType)))
        {
            int count = outfits.Count(o => o.outfitType == type);
            int canSpawn = CountCanSpawn(outfits, type);
            EditorGUILayout.LabelField($"{count} {type}, of which {(canSpawn == 0 ? "none" : $"{canSpawn}")} are spawnable.", statStyle);
        }

        //
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Outfits by stage:", titleStyle);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{CountByStage(outfits, OutfitStage.StageOne)} Outfits for Stage One", statStyle);
        EditorGUILayout.LabelField($"{CountByStage(outfits, OutfitStage.StageTwo)} Outfits for Stage Two", statStyle);
        EditorGUILayout.LabelField($"{CountByStage(outfits, OutfitStage.StageThree)} Outfits for Stage Three", statStyle);
        EditorGUILayout.LabelField($"{CountByStage(outfits, OutfitStage.Special)} Outfits excluded from Staging", statStyle);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{canSpawnCount} Can be used when spawning Nora", statStyle);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Total {outfits.Count} Outfits", statStyle);
        EditorGUILayout.LabelField($"Current Outfit ID: {me.currentOutfit}", statStyle);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button($"Jump To Asset", buttonStyle, GUILayout.Height(ButtonHeight)))
        {
            SelectOutfitAsset(me);
        }
        if (GUILayout.Button($"Copy Outfit ID", buttonStyle, GUILayout.Height(ButtonHeight)))
        {
            EditorGUIUtility.systemCopyBuffer = me.currentOutfit.ToString();
        }

        //
        
        
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button($"Re-Apply\n{me.currentOutfit}", buttonStyle, GUILayout.Height(42))) { me.SetMainOutfit(me.currentOutfit); }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private static void SelectOutfitAsset(NorasWardrobe me)
    {
        Outfit matchingOutfit = (me.Outfits ?? new List<Outfit>()).FirstOrDefault(o => o != null && o.thisOutfit == me.currentOutfit);

        if (matchingOutfit == null)
        {
            Debug.LogWarning($"NorasWardrobeEditor: No Outfit asset found matching {me.currentOutfit}.");
            return;
        }

        Selection.activeObject = matchingOutfit;
        EditorGUIUtility.PingObject(matchingOutfit);
    }

    private void DrawOutfitTypeSection(NorasWardrobe me, GUIStyle style, string sectionLabel, OutfitType type, Color color)
    {
        GUI.backgroundColor = color;
        EditorGUILayout.LabelField(sectionLabel, EditorStyles.boldLabel);

        var outfits = (me.Outfits ?? new List<Outfit>())
            .Where(o => o != null && o.outfitType == type)
            .ToList();

        if (outfits.Count == 0)
        {
            EditorGUILayout.LabelField("(No outfits assigned)", EditorStyles.miniLabel);
            return;
        }

        for (int i = 0; i < outfits.Count; i += DefaultColumns)
        {
            var rowOutfits = outfits.Skip(i).Take(DefaultColumns).ToArray();
            DrawOutfitButtonRow(me, style, rowOutfits);
        }
    }

    private void DrawOutfitButtonRow(NorasWardrobe me, GUIStyle style, Outfit[] outfits)
    {
        if (outfits == null || outfits.Length == 0) { return; }

        float slotWidth = GetButtonWidth(outfits.Length);

        EditorGUILayout.BeginHorizontal();
        foreach (var outfit in outfits)
        {
            string label = string.IsNullOrEmpty(outfit.outfitTitle) ? outfit.thisOutfit.ToString() : outfit.outfitTitle;
            OutfitName outfitName = outfit.thisOutfit;

            if (me.PreviewEnabled && _pendingPreviewOutfit.HasValue && _pendingPreviewOutfit.Value == outfitName)
            {
                float halfWidth = (slotWidth - ButtonSpacing) / 2f;

                if (GUILayout.Button("Apply", style, GUILayout.Height(ButtonHeight), GUILayout.Width(halfWidth)))
                {
                    me.ApplyPreviewOutfit(outfitName);
                    _pendingPreviewOutfit = null;
                    EditorUtility.SetDirty(me);
                }
                if (GUILayout.Button("Cancel", style, GUILayout.Height(ButtonHeight), GUILayout.Width(halfWidth)))
                {
                    me.CancelPreviewOutfit();
                    _pendingPreviewOutfit = null;
                    EditorUtility.SetDirty(me);
                }
            }
            else
            {
                if (GUILayout.Button(label, style, GUILayout.Height(ButtonHeight), GUILayout.Width(slotWidth)))
                {
                    if (me.PreviewEnabled)
                    {
                        me.SetMainOutfitPreview(outfitName);
                        _pendingPreviewOutfit = outfitName;
                    }
                    else
                    {
                        me.ToggleOutfit(outfitName);
                    }
                    EditorUtility.SetDirty(me);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private static float GetButtonWidth(int columnCount)
    {
        float totalWidth = EditorGUIUtility.currentViewWidth - SideMargin;
        float totalSpacing = ButtonSpacing * (columnCount - 1);
        return (totalWidth - totalSpacing) / columnCount;
    }

    private static void DrawButtonRow(NorasWardrobe me, GUIStyle style, params (string Label, System.Action OnClick)[] buttons)
    {
        if (buttons == null || buttons.Length == 0) { return; }

        float buttonWidth = GetButtonWidth(buttons.Length);

        EditorGUILayout.BeginHorizontal();
        foreach (var button in buttons)
        {
            if (GUILayout.Button(button.Label, style, GUILayout.Height(ButtonHeight), GUILayout.Width(buttonWidth)))
            {
                button.OnClick();
                EditorUtility.SetDirty(me);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif


public enum HairName
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


public enum ShoesName
{
    WorkFlats,
    SilentShoes,
    Sandals,
    Boots,
    WhiteFlats,
    ShittyTrainers,
    FMB,
    BlackBowFlats,
    WhiteBowFlats,
    BlackBoots,
    BlackFMB,
    PrimAndProper,
    ShuPlaceholder4,
    None
}

public enum NecklaceName
{
    None,
    Butterfly,
    Pearls,
    Cross,
    NekPlaceholder1,
    NekPlaceholder2,
    NekPlaceholder3,
    NekPlaceholder4
}

public enum GlassesName
{
    None,
    Professional,
    GlaPlaceholder1,
    GlaPlaceholder2,
    GlaPlaceholder3,
    GlaPlaceholder4
}

public enum OutfitStage {
    StageOne,
    StageTwo,
    StageThree,
    Special
}

public enum OutfitType
{
    Work,
    Main,
    Pyjamas,
    NightOut,
    Special,
    Storyline,
    Undergarments
}

public enum OutfitName
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
    Domme,
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
    WorkPantSuit,
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
    FrootCardiganTop,
    CasualShortDress,
    StrappyTopAndShorts,
    StylishHalterTopAndPants,
    LeatherTube,
    CasualTeeAndPants,
    BlazerAndSkirt,
    StrappyDress,
    HalterDressAndTights,
    ShortSleeveDress,
    CropTopAndSkirt,
    StraplessAndPants,
    TurtleneckAndPants,
    FloatyTopAndSkirt,
    TubeDressAndTights,
    TankTopAndSkirt,
    StraplessCardiAndSkirt,
    FitnessAerobics,
    StringyTopAndSkirt,
    Nothinatall,
    DuckTeeAndSkirt,
    StrappyTopAndPants,
    StraplessTopAndSkirt,
    EveningDress,
    CasualTopAndPants,
    TwilightDress,
    AllInOne,
    ModestCardiAndSkirt,
    CasualTopAndSkirt,
    SkimpyTopAndShorts,
    MinidressWithSleeves,
    JezebelleDress,
    AltWork,
    FlutedDressAndTights,
    RockDress,
    ShortsAndTeeShirt,
    SkimpyTopAndMidSkirt,
    RevealingDress,
    FloatyDress,
    LowTopLongSkirt,
    WoolyAndSkirt,
    DressOverLeggings,
    FreeSpiritDress,
    SummerDress,
    SkimpyTopAndSkirt,
    TightTeeAndPants,
    CasualSuit,
    BlousseAndSkirt,
    OffShoulderTopAndSkirt,
    LowBlousseAndSkirt,
    TankTopAndShorts,
    WoolyModestyTwo,
    SweaterAndSkirtTwo,
    CorruptedStrappyDress,
    TurtleneckAndSkirtAlt,
    TankTopAndSkirtAlt,
    LooseBlousseAndSkirt,
    SheerRuffleDress,
    LongStrappyDress,
    CasualTopAndLongSkirt,
    TubeDressAndCardi,
    RuffleBlousseAndPants,
    TankTopAndModestSkirt,
    StrappyTopAndFloatySkirt,
    ShortDressCardiAndTights,
    CutOutDress,
    BlousseAndPants,
    FloatyTopAndTinySkirt,
    HeartDress,
    SkinTightLeo,
    WavyDress,
    RuffleTopAndSkirt,
    RuffleTopWithPants,
    RuffleTopWithMaxiSkirt,
    MicrodressAndStocks,
    ExtremeRuffleDress,
    WorkRuffleAndSkirt,
    WavyDressAndTights,
    CompactDress,
    LightJacketAndPants,
    BlousseAndLongSkirt,
    CasualOveralls,
    BusinessDress,
    CasualTopAndPantsAlt,
    LooseTopAndTinySkirt,
    StrappyTopAndFloatySkirtTwo,
    Romper,
    SplitDress,
    LeopardSuit,
    Traditionalt,
    TiedTopAndSkirt,
    FloatyBlousseAndSkirt,
    TightLittleDress,
    TightCropTopAndSkirt,
    FlirtyDress,
    NewTraditionDress,
    FrilledDress,
    SleevelessBlousseAndSkirt,
    PVCSuit,
    WavyTopAndSkirt,
    ModestSleevelessDress,
    FloatyBlousseAndCutoffs,
    PartyDress,
    ConservativeDressAndCardi,
    FittedDress,
    PrincessDress,
    StraplessWithTights,
    FloatyBlousseAndLongSkirt,
    StraplessTopAndFlares,
    SleevelessShirtAndSkirt,
    StrappyTopAndTrousers,
    StylishSuit,
    ModestEveningDress,
    ConservativeEveningDress,
    FrilledSkirtAndTop,
    BridesmaidDress,
    LiberalDress,
    ShareholderMeetingDress,
    RuffleCollarTopAndSkirt,
    RuffleCollarDress,
    LowRuffleAndSkirt,
    CroppedShirtAndSkirt,
    CroptopSkirtAndCardi,
    TightDress,
    TeardropDress,
    CroppedDressAndTights,
    ClingyDress,
    ClingySkirtAndTop,
    CropTopAndShorts,
    StylishJacketAndSkirt,
    KnottedCardiAndSkirt,
    JumperOverShirtAndSkirt,
    RelaxedShirtAndPants,
    RuffleTopAndRuffleSkirt,
    RipplyTopAndPants,
    LabTechnician,
    LayeredSweaterAndPants,
    ButterflyDress,
    StylishJacketAndLongSkirt,
    LayeredSweaterAndLongSkirt,
    VestAndShorts,
    VestAndSkirt,
    TiedTopSkirtAndStocks,
    ProfessionalTopAndSkirt,
    StretchyDress,
    ShortDressAndJacket,
    PrettyDress,
    DressWithBlousse,
    BlousseDress,
    BlousseTopAndPants,
    CroppedShirtAndTinySkirt,
    RelaxedDressAndBlousse,
    ShortStretchyDressAndTights,
    CasualJeansAndHoodie,
    ModestShortSleeveTopAndSkirt,
    ModestBlousseTopWithSkirtTop,
    ConservativeBlousseTopWithLongSkirt,
    ConservativeLayeredSweaterAndSkirt,
    EhYo
    
}