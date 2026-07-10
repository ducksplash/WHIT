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

    private Dictionary<OutfitName, Outfit> _outfitLookup;

    private void Start()
    {
        SetupAccessories();
        SetupHair();
        SetupAccessoriesPreview();
        SetupHairPreview();

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

    private void ApplyAccessoriesPreview(OutfitName outfit)
    {
        var data = GetOutfit(outfit);
        bool w = data != null && data.Wings;
        bool o = data != null && data.Apron;
        bool h = data != null && data.Hat;
        bool c = data != null && data.Choker;
        if (ButterflyWingsPreview != null) { ButterflyWingsPreview.enabled = w; }
        if (OverallPreview != null) { OverallPreview.enabled = o; }
        if (HatPreview != null) { HatPreview.enabled = h; }
        if (ChokerPreview != null) { ChokerPreview.enabled = c; }
    }

    private void HideAllAccessories()
    {
        if (ButterflyWings != null) { ButterflyWings.enabled = false; }
        if (Overall != null) { Overall.enabled = false; }
        if (Hat != null) { Hat.enabled = false; }
        if (Choker != null) { Choker.enabled = false; }
    }

    private void HideAllAccessoriesPreview()
    {
        if (ButterflyWingsPreview != null) { ButterflyWingsPreview.enabled = false; }
        if (OverallPreview != null) { OverallPreview.enabled = false; }
        if (HatPreview != null) { HatPreview.enabled = false; }
        if (ChokerPreview != null) { ChokerPreview.enabled = false; }
    }

    private static bool IsUndergarmentOutfit(int index)
    {
        return index == (int)OutfitName.None;
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
        // HideAllAccessoriesPreview();
        // ClearOutfitMeshesPreview();
        // HideAllHairPreview();
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
    private const int SpawnableTarget = 128;

    private OutfitName? _pendingPreviewOutfit;

    private void OnEnable()
    {
        _pendingPreviewOutfit = null;
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

        DrawOutfitStats(me);

        EditorGUILayout.LabelField("Outfit Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawButtonRow(me, clearAllStyle,
            ("Previous Outfit", () => me.PreviousOutfit()),
            ("Next Outfit", () => me.NextOutfit()));

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
        DrawDefaultInspector();
    }

    private static int CountCanSpawn(List<Outfit> outfits, OutfitType? type = null)
    {
        return outfits.Count(o => o.SpawnAs && (type == null || o.outfitType == type));
    }

    private static void DrawOutfitStats(NorasWardrobe me)
    {
        var outfits = (me.Outfits ?? new List<Outfit>()).Where(o => o != null && o.thisOutfit != OutfitName.None).ToList();

        int canSpawnCount = CountCanSpawn(outfits);

        GUI.backgroundColor = Color.blue;
        EditorGUILayout.LabelField("Outfit Stats", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Number of outfits: ", EditorStyles.boldLabel);
        
        foreach (OutfitType type in System.Enum.GetValues(typeof(OutfitType)))
        {
            int count = outfits.Count(o => o.outfitType == type);
            int canSpawn = CountCanSpawn(outfits, type);
            EditorGUILayout.LabelField($"{count} {type}, of which {(canSpawn == 0 ? "none" : $"{canSpawn}")} are spawnable.", EditorStyles.boldLabel); }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{canSpawnCount} Can be used when spawning Nora", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"{SpawnableTarget - canSpawnCount} left to hit target of {SpawnableTarget}", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Total {outfits.Count} Outfits", EditorStyles.boldLabel);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
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
    ModestCardiAndSkirt
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