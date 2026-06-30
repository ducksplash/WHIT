using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NorasWardrobe : MonoBehaviour
{
    [Header("Settings")]
    public bool UnressedOnLoad;
    
    [Header("Current Outfit")]
    public OutfitName currentOutfit = OutfitName.Work;

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
    public InputActionReference holdToUnlock;
    public InputActionReference holdToUnlockAll;

    private bool _wingsOverride = false;
    private bool _overallOverride = false;
    private bool _hatOverride = false;
    private bool _chokerOverride = false;

    private Dictionary<OutfitName, Outfit> _outfitLookup;

    private void Start()
    {
        SetupAccessories();
        SetupHair();

        if (!UnressedOnLoad) 
        { SwitchToOutfit(OutfitName.Work); }
        else { DisableAllMainOutfits(); }

        SetupInput(); 
    }

    private void OnValidate()
    {
        _outfitLookup = null;
    }

    private void SetupInput()
    {
        if (holdToUnlock != null) { holdToUnlock.action.performed += OnToggleUndergarments; }
        if (holdToUnlockAll != null) { holdToUnlockAll.action.performed += OnToggleNothinatall; }
        if (nextOutfit != null) { nextOutfit.action.performed += OnNextOutfit; }
        if (previousOutfit != null) { previousOutfit.action.performed += OnPreviousOutfit; }
    }

    private void OnToggleUndergarments(InputAction.CallbackContext ctx) { ToggleUndergarments(); }
    private void OnToggleNothinatall(InputAction.CallbackContext ctx) { Undress(); }
    private void OnNextOutfit(InputAction.CallbackContext ctx) { NextOutfit(); }
    private void OnPreviousOutfit(InputAction.CallbackContext ctx) { PreviousOutfit(); }

    private void OnDisable()
    {
        if (nextOutfit != null) { nextOutfit.action.performed -= OnNextOutfit; }
        if (previousOutfit != null) { previousOutfit.action.performed -= OnPreviousOutfit; }
        if (holdToUnlock != null) { holdToUnlock.action.performed -= OnToggleUndergarments; }
        if (holdToUnlockAll != null) { holdToUnlockAll.action.performed -= OnToggleNothinatall; }
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
        Transform[] bones = Body != null ? Body.bones : null;
        foreach (var smr in SkinnedMeshRendererParentAccessories.GetComponentsInChildren<SkinnedMeshRenderer>(true))
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
            PyjamaHair, DatingHair, DatingHairTwo, OutHair, UpHair, HomelessHair, TwinTailsHair, MessyHair, UpDo
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
        if (MessyHair != null) { MessyHair.enabled = false; }
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
            case HairName.MessyHair: if (MessyHair != null) { MessyHair.enabled = true; } break;
        }
    }

    private HairName GetHairForOutfit(OutfitName outfit)
    {
        return GetOutfit(outfit)?.Hair ?? HairName.DefaultHair;
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

    public bool IsOutfitEnabled(OutfitName outfit)
    {
        var data = GetOutfit(outfit);
        return data == null || data.OutfitEnabled;
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
        _wingsOverride = false;
        _overallOverride = false;
        _hatOverride = false;
        _chokerOverride = false;
    }

    private void ApplyAccessories()
    {
        var data = GetOutfit(currentOutfit);
        bool w = data != null && data.Wings;
        bool o = data != null && data.Apron;
        bool h = data != null && data.Hat;
        bool c = data != null && data.Choker;
        if (ButterflyWings != null) { ButterflyWings.enabled = w || _wingsOverride; }
        if (Overall != null) { Overall.enabled = o || _overallOverride; }
        if (Hat != null) { Hat.enabled = h || _hatOverride; }
        if (Choker != null) { Choker.enabled = c || _chokerOverride; }
    }

    private void HideAllAccessories()
    {
        if (ButterflyWings != null) { ButterflyWings.enabled = false; }
        if (Overall != null) { Overall.enabled = false; }
        if (Hat != null) { Hat.enabled = false; }
        if (Choker != null) { Choker.enabled = false; }
    }

    private static bool IsUndergarmentOutfit(int index)
    {
        return index == (int)OutfitName.None
            // || index == (int)OutfitName.Undergarments
            // || index == (int)OutfitName.Lingerie
            // || index == (int)OutfitName.Fae
            // || index == (int)OutfitName.RisqueNightie
            // || index == (int)OutfitName.KnottedAndShorts
            // || index == (int)OutfitName.Domme
            ;
    }

    public void NextOutfit()
    {
        int total = System.Enum.GetValues(typeof(OutfitName)).Length;
        int index = (int)currentOutfit;
        do { index = (index + 1) % total; }
        while (IsUndergarmentOutfit(index) || !IsOutfitEnabled((OutfitName)index));
        SwitchToOutfit((OutfitName)index);
    }

    public void PreviousOutfit()
    {
        int total = System.Enum.GetValues(typeof(OutfitName)).Length;
        int index = (int)currentOutfit;
        do { index = (index - 1 + total) % total; }
        while (IsUndergarmentOutfit(index) || !IsOutfitEnabled((OutfitName)index));
        SwitchToOutfit((OutfitName)index);
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

    public void ToggleWorkOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Work, forceOn); }
    public void ToggleWorkTwoOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.WorkTwo, forceOn); }
    public void ToggleWorkThreeOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.WorkThree, forceOn); }
    public void ToggleWorkFourOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.WorkFour, forceOn); }
    public void ToggleWorkSuitThreeOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.WorkSuitThree, forceOn); }
    public void ToggleCasualOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Casual, forceOn); }
    public void ToggleShortsAndTightsOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.ShortsAndTights, forceOn); }
    public void ToggleCleanBanditOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.CleanBandit, forceOn); }
    public void ToggleChurchDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.ChurchDress, forceOn); }
    public void ToggleFitnessOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Fitness, forceOn); }
    public void TogglePyjamasOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Pyjamas, forceOn); }
    public void ToggleHousecoatOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Housecoat, forceOn); }
    public void ToggleNightie(bool? forceOn = null) { ToggleOutfit(OutfitName.Nightie, forceOn); }
    public void ToggleRisqueNightie(bool? forceOn = null) { ToggleOutfit(OutfitName.RisqueNightie, forceOn); }
    public void ToggleStealthSuit(bool? forceOn = null) { ToggleOutfit(OutfitName.StealthSuit, forceOn); }
    public void ToggleFirstDateOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Date1, forceOn); }
    public void ToggleSecondDateOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Date2, forceOn); }
    public void ToggleDommeOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Domme, forceOn); }
    public void ToggleGreyCheckHalterDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.GreyCheckHalterDress, forceOn); }
    public void ToggleSweaterAndSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.SweaterAndSkirt, forceOn); }
    public void ToggleTurtleneckAndSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.TurtleneckAndSkirt, forceOn); }
    public void ToggleCheckTopAndJeansOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.CheckTopAndJeans, forceOn); }
    public void ToggleKnottedBlousseAndSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.KnottedBlousseAndSkirt, forceOn); }
    public void ToggleRuffleBlousseAndSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.RuffleBlousseAndSkirt, forceOn); }
    public void ToggleLooseTopAndLongSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.LooseTopAndLongSkirt, forceOn); }
    public void ToggleTurtleneckAndMediumSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.TurtleneckAndMediumSkirt, forceOn); }
    public void ToggleWoolenJumperOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.WoolenJumper, forceOn); }
    public void ToggleEdeaOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Edea, forceOn); }
    public void ToggleDitzyDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.DitzyDress, forceOn); }
    public void ToggleLittleBlackDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.LittleBlackDress, forceOn); }
    public void ToggleCasualPantsuitOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.CasualPantSuit, forceOn); }
    public void ToggleConservativeOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Conservative, forceOn); }
    public void ToggleFrootDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Casual3, forceOn); }
    public void ToggleStraplessRuffleDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.StraplessRuffleDress, forceOn); }
    public void ToggleCheckBodySuitOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.CheckBodySuit, forceOn); }
    public void ToggleElegantDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.ElegantDress, forceOn); }
    public void ToggleNightOutRuffleOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.NightOutRuffle, forceOn); }
    public void ToggleHalterSkirterOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.HalterSkirter, forceOn); }
    public void ToggleWeddingOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Wedding, forceOn); }
    public void ToggleFuneralOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Funeral, forceOn); }
    public void ToggleHomelessnessOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Homelessness, forceOn); }
    public void ToggleUndergarments(bool? forceOn = null) { ToggleOutfit(OutfitName.Undergarments, forceOn); }
    public void ToggleLingerieOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Lingerie, forceOn); }
    public void ToggleFaeOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Fae, forceOn); }
    public void ToggleButtonDressOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.ButtonDress, forceOn); }
    public void ToggleTraditionalOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Traditional, forceOn); }
    public void ToggleWoolyAndJeansOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.WoolyAndJeans, forceOn); }
    public void ToggleKnottedAndShortsOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.KnottedAndShorts, forceOn); }
    public void ToggleModestOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Modest, forceOn); }
    public void ToggleStrappyTopAndSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.StrappyTopAndSkirt, forceOn); }
    public void ToggleShortieOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.Shortie, forceOn); }
    public void ToggleTopWithSkirtOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.TopWithSkirt, forceOn); }
    public void ToggleWoolyModestyOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.WoolyModesty, forceOn); }
    public void ToggleFrootCardiganTopOutfit(bool? forceOn = null) { ToggleOutfit(OutfitName.FrootCardiganTop, forceOn); }

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

        EventManager.OutfitWasChanged(outfit);
    }

    public void DisableAllMainOutfits()
    {
        HideAllAccessories();
        ClearOutfitMeshes();
        currentOutfit = OutfitName.None;
        SetHair(HairName.DefaultHair);
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

    public void SetRandomWorkOutfit()
    {
        OutfitName[] pool =
        {
            OutfitName.Work,
            OutfitName.WorkTwo,
            OutfitName.WorkThree,
            OutfitName.WorkFour,
            OutfitName.WorkSuitThree
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomMainOutfit()
    {
        OutfitName[] pool =
        {
            OutfitName.Casual,
            OutfitName.ShortsAndTights,
            OutfitName.GreyCheckHalterDress,
            OutfitName.SweaterAndSkirt,
            OutfitName.TurtleneckAndSkirt,
            OutfitName.CheckTopAndJeans,
            OutfitName.KnottedBlousseAndSkirt,
            OutfitName.RuffleBlousseAndSkirt,
            OutfitName.LooseTopAndLongSkirt,
            OutfitName.TurtleneckAndMediumSkirt,
            OutfitName.WoolenJumper,
            OutfitName.CasualPantSuit,
            OutfitName.Conservative,
            OutfitName.ButtonDress,
            OutfitName.WoolyAndJeans,
            OutfitName.Modest,
            OutfitName.StrappyTopAndSkirt,
            OutfitName.TopWithSkirt,
            OutfitName.WoolyModesty,
            OutfitName.FrootCardiganTop,
            OutfitName.Date1,
            OutfitName.Date2
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomNightOutOutfit()
    {
        OutfitName[] pool =
        {
            OutfitName.HalterSkirter,
            OutfitName.DitzyDress,
            OutfitName.LittleBlackDress,
            OutfitName.Casual3,
            OutfitName.StraplessRuffleDress,
            OutfitName.CheckBodySuit,
            OutfitName.ElegantDress,
            OutfitName.NightOutRuffle
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomPyjamasOutfit()
    {
        OutfitName[] pool =
        {
            OutfitName.Pyjamas,
            OutfitName.Housecoat,
            OutfitName.Nightie,
            OutfitName.Shortie
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomSpecialOutfit()
    {
        OutfitName[] pool =
        {
            OutfitName.Fitness,
            OutfitName.Edea,
            OutfitName.Traditional,
            OutfitName.FrootCardiganTop,
            OutfitName.ChurchDress,
            OutfitName.CleanBandit
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomRisqueOutfit()
    {
        OutfitName[] pool =
        {
            OutfitName.RisqueNightie,
            OutfitName.Domme,
            OutfitName.Lingerie,
            OutfitName.Fae,
            OutfitName.KnottedAndShorts
        };
        SwitchToOutfit(PickRandom(pool));
    }

    public void SetRandomStorylineOutfit()
    {
        OutfitName[] pool =
        {
            OutfitName.Wedding,
            OutfitName.Funeral,
            OutfitName.Homelessness,
            OutfitName.StealthSuit
        };
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

    public override void OnInspectorGUI()
    {
        NorasWardrobe me = (NorasWardrobe)target;

        var clearAllStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        EditorGUILayout.LabelField("Outfit Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawButtonRow(me, clearAllStyle,
            ("Previous Outfit", () => me.PreviousOutfit()),
            ("Next Outfit", () => me.NextOutfit()));

        GUI.backgroundColor = Color.cyan;

        EditorGUILayout.LabelField("Work Outfits", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Classic Nora Dress", () => me.ToggleWorkOutfit()),
            ("Black Skirt & Cream Shirt", () => me.ToggleWorkThreeOutfit()),
            ("Tartan Dress", () => me.ToggleWorkTwoOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Black Suit & Shirt", () => me.ToggleWorkFourOutfit()),
            ("Suit Jacket & Trousers", () => me.ToggleWorkSuitThreeOutfit()));

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;

        EditorGUILayout.LabelField("Main Outfits", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Uni Sweater & Jeans", () => me.ToggleCasualOutfit()),
            ("Shorts & Tights", () => me.ToggleShortsAndTightsOutfit()),
            ("Check Body Suit", () => me.ToggleCheckBodySuitOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("First Date", () => me.ToggleFirstDateOutfit()),
            ("Second Date", () => me.ToggleSecondDateOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Wooly Jumper & Jeans", () => me.ToggleWoolyAndJeansOutfit()),
            ("Grey Check Halter Dress", () => me.ToggleGreyCheckHalterDressOutfit()),
            ("Sweater & Skirt", () => me.ToggleSweaterAndSkirtOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Turtleneck & Skirt", () => me.ToggleTurtleneckAndSkirtOutfit()),
            ("Check Top & Jeans", () => me.ToggleCheckTopAndJeansOutfit()),
            ("Knotted Blousse & Skirt", () => me.ToggleKnottedBlousseAndSkirtOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Ruffle Blousse & Skirt", () => me.ToggleRuffleBlousseAndSkirtOutfit()),
            ("Loose Top & Long Skirt", () => me.ToggleLooseTopAndLongSkirtOutfit()),
            ("Turtleneck & Medium Skirt", () => me.ToggleTurtleneckAndMediumSkirtOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Wooly Jumper & Tights", () => me.ToggleWoolenJumperOutfit()),
            ("Top With Skirt", () => me.ToggleTopWithSkirtOutfit()),
            ("Wooly Modesty", () => me.ToggleWoolyModestyOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Casual Pantsuit", () => me.ToggleCasualPantsuitOutfit()),
            ("Conservative Jumper & Skirt", () => me.ToggleConservativeOutfit()),
            ("Button Dress", () => me.ToggleButtonDressOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Modest Top & Skirt", () => me.ToggleModestOutfit()),
            ("Strappy Top & Skirt", () => me.ToggleStrappyTopAndSkirtOutfit()));

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);

        EditorGUILayout.LabelField("PJs", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Pyjamas", () => me.TogglePyjamasOutfit()),
            ("Housecoat", () => me.ToggleHousecoatOutfit()),
            ("Nightie", () => me.ToggleNightie()),
            ("Shortie", () => me.ToggleShortieOutfit()));

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(1f, 0.6f, 0.8f);

        EditorGUILayout.LabelField("Night Out", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Ditzy Dress", () => me.ToggleDitzyDressOutfit()),
            ("Little Black Dress", () => me.ToggleLittleBlackDressOutfit()),
            ("Strapless Ruffle Dress", () => me.ToggleStraplessRuffleDressOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Layered Ruffle Dress", () => me.ToggleNightOutRuffleOutfit()),
            ("Elegant Dress", () => me.ToggleElegantDressOutfit()),
            ("Halter Top & Skirt", () => me.ToggleHalterSkirterOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Froot Dress", () => me.ToggleFrootDressOutfit()));

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.7f, 0.4f, 1f);

        EditorGUILayout.LabelField("Special Outfits", EditorStyles.boldLabel);

        DrawButtonRow(me, clearAllStyle,
            ("Traditional", () => me.ToggleTraditionalOutfit()),
            ("Froot Cardi & Dress", () => me.ToggleFrootCardiganTopOutfit()),
            ("Clean Bandit", () => me.ToggleCleanBanditOutfit()));

        float specialRowWidth = GetButtonWidth(3);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Church Dress", clearAllStyle, GUILayout.Height(ButtonHeight), GUILayout.Width(specialRowWidth))) { me.ToggleChurchDressOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Fitness", clearAllStyle, GUILayout.Height(ButtonHeight), GUILayout.Width(specialRowWidth))) { me.ToggleFitnessOutfit(); EditorUtility.SetDirty(me); }
        GUI.backgroundColor = new Color(1f, 0.84f, 0f);
        if (GUILayout.Button("Edea's Dress", clearAllStyle, GUILayout.Height(ButtonHeight), GUILayout.Width(specialRowWidth))) { me.ToggleEdeaOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.5f);

        EditorGUILayout.LabelField("Storyline Outfits", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Wedding", () => me.ToggleWeddingOutfit()),
            ("Funeral", () => me.ToggleFuneralOutfit()),
            ("Skid Row", () => me.ToggleHomelessnessOutfit()),
            ("Stealth Suit", () => me.ToggleStealthSuit()));

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.red;

        EditorGUILayout.LabelField("Undergarments", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Lingerie", () => me.ToggleLingerieOutfit()),
            ("Underwear", () => me.ToggleUndergarments()),
            ("Fae", () => me.ToggleFaeOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Risque Nightie", () => me.ToggleRisqueNightie()),
            ("Knotted Top & Shorts", () => me.ToggleKnottedAndShortsOutfit()),
            ("Domme", () => me.ToggleDommeOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Nothin' At All", () => me.Undress()));

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.yellow;

        EditorGUILayout.LabelField("Accessories (toggle independently — any combination)", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Toggle Wings", () => me.ToggleWings()),
            ("Toggle Overall", () => me.ToggleOverall()),
            ("Toggle Hat", () => me.ToggleHat()),
            ("Toggle Choker", () => me.ToggleChoker()));

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);

        EditorGUILayout.LabelField("Hair (exclusive — only one at a time)", EditorStyles.boldLabel);
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
            ("Up Hair", () => me.SetHair(HairName.UpHair)),
            ("Messy Hair", () => me.SetHair(HairName.MessyHair)));
        DrawButtonRow(me, clearAllStyle,
            ("Updo", () => me.SetHair(HairName.UpDo)));

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle,
            ("Random Work", () => me.SetRandomWorkOutfit()),
            ("Random Main", () => me.SetRandomMainOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Random Night Out", () => me.SetRandomNightOutOutfit()),
            ("Random Pyjamas", () => me.SetRandomPyjamasOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Random Storyline", () => me.SetRandomStorylineOutfit()),
            ("Random Risque Outfit", () => me.SetRandomRisqueOutfit()));
        DrawButtonRow(me, clearAllStyle,
            ("Random Special Outfit", () => me.SetRandomSpecialOutfit()));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("---------------------", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        DrawDefaultInspector();
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