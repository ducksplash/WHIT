using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MENPC : MonoBehaviour
{
    [Header("Character Details")]
    public string ThisCharacterName;
    public NPC thisNPC;

    [Header("NPC Controller")]
    public NPCController npcController;

    [Header("Settings")] 
    
    public bool DebugMode;
    public bool UnressedOnLoad;
    
    [Header("Start up Outfit")]
    public NPCOutfitName startUpOutfit = NPCOutfitName.Dress;
    [Header("Current NPCOutfit")]
    public NPCOutfitName currentOutfit = NPCOutfitName.Casual;
    
    [Header("Character Root")]
    public GameObject SkinnedMeshRendererParentOutfits;
    public GameObject SkinnedMeshRendererParentAccessories;
    public GameObject SkinnedMeshRendererParentBodies;
    public GameObject SkinnedMeshRendererParentEyes;
    
    [Header("Body")]
    public SkinnedMeshRenderer Body;

    [Header("Body Types")]
    public SkinnedMeshRenderer NormalBody;


    [Header("NPCEyes")]
    public SkinnedMeshRenderer DefaultEyes;

    [Header("Accessories")]
    public SkinnedMeshRenderer DefaultHat;
    public SkinnedMeshRenderer HatPlaceholder1;
    public SkinnedMeshRenderer HatPlaceholder2;
    public SkinnedMeshRenderer HatPlaceholder3;
    public SkinnedMeshRenderer HatPlaceholder4;
    public SkinnedMeshRenderer HatPlaceholder5;
    public SkinnedMeshRenderer Choker;
    public SkinnedMeshRenderer WhiteLaceChoker;
    public SkinnedMeshRenderer BlackLaceChoker;
    public SkinnedMeshRenderer ChokerPlaceholder3;
    public SkinnedMeshRenderer ChokerPlaceholder4;
    public SkinnedMeshRenderer ChokerPlaceholder5;
    public SkinnedMeshRenderer Cigarette;
    public SkinnedMeshRenderer VicePlaceholder1;
    public SkinnedMeshRenderer VicePlaceholder2;
    public SkinnedMeshRenderer VicePlaceholder3;
    public SkinnedMeshRenderer VicePlaceholder4;
    public SkinnedMeshRenderer VicePlaceholder5;

    [Header("Shoes")]
    public SkinnedMeshRenderer WorkFlatsShoes;


    [Header("Hair")]
    public SkinnedMeshRenderer DefaultHair;

    
    [Header("Necklace")]
    public SkinnedMeshRenderer DefaultNecklace;

    
    [Header("Glasses")]
    public SkinnedMeshRenderer DefaultGlasses;

    
    [Header("Outfits")]
    public List<NPCOutfit> Outfits = new List<NPCOutfit>();

    [Header("Jigglers")]
    public GameObject JiggleLeftBoob;
    public GameObject JiggleRightBoob;
    public GameObject JiggleLeftButtcheek;
    public GameObject JiggleRightButtcheek;

    [Header("Input")]
    public InputActionReference nextOutfit;
    public InputActionReference previousOutfit;
    public InputActionReference holdToggleDebug;
    public InputActionReference holdToggleUndress;

    private Dictionary<NPCOutfitName, NPCOutfit> _outfitLookup = new Dictionary<NPCOutfitName, NPCOutfit>();

    private static readonly NPCOutfitType[] InspectorSectionOrder =
    {
        NPCOutfitType.Main,
        NPCOutfitType.Storyline,
        NPCOutfitType.Special,
        NPCOutfitType.NightOut
    };

    private List<NPCOutfitName> _inspectorOrderedOutfits;


    private void Awake()
    {
        BuildOutfitLookup();
    }
    

    private void Start()
    {        
        try
        {
            ThisCharacterName = npcController.thisNPC.ToString().Replace("_", " ");
            thisNPC = npcController.thisNPC;
        }
        catch
        {
            ThisCharacterName = "Not Found";
        }
        
        SetupAccessories();
        SetupHair();
        SetupShoes();
        SetupNecklaces();
        SetupGlasses();
        SetupBodies();
        SetupEyes();

        if (!UnressedOnLoad) 
        { SwitchToOutfit(NPCOutfitName.Casual); }
        else { DisableAllMainOutfits(); }

        SetupInput();
        
        SetMainOutfit(startUpOutfit);


        EventManager.OnPrevNPC += PreviousOutfitWithContext;
        EventManager.OnNextNPC += NextOutfitWithContext;
        EventManager.OnExitNPC += ExitConversationWithContext;

    }

 
    
    

    private void OnValidate()
    {
        _outfitLookup = null;
        _inspectorOrderedOutfits = null;
    }

    private void SetupInput()
    {
        Debug.Log("inputs in debug hd");
        // if (holdToggleDebug != null) { holdToggleDebug.action.performed += OnToggleDebug; }
        // if (nextOutfit != null) { nextOutfit.action.performed += OnNextOutfit; }
        // if (previousOutfit != null) { previousOutfit.action.performed += OnPreviousOutfit; }
        // if (holdToggleUndress != null) { holdToggleUndress.action.performed += Undress; }
    }

    private void OnToggleDebug(InputAction.CallbackContext ctx) { ToggleDebug(); }
    private void OnNextOutfit(InputAction.CallbackContext ctx) { NextOutfit(); }
    private void OnPreviousOutfit(InputAction.CallbackContext ctx) { PreviousOutfit(); }

    private void OnDisable()
    {
        if (nextOutfit != null) { nextOutfit.action.performed -= OnNextOutfit; }
        if (previousOutfit != null) { previousOutfit.action.performed -= OnPreviousOutfit; }
        if (holdToggleDebug != null) { holdToggleDebug.action.performed -= OnToggleDebug; }
        if (holdToggleUndress != null) { holdToggleUndress.action.performed -= Undress; }
        
        EventManager.OnPrevNPC -= PreviousOutfitWithContext;
        EventManager.OnNextNPC -= NextOutfitWithContext;
        EventManager.OnExitNPC -= ExitConversationWithContext;
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


    private void SetupGlasses()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allGlasses =
        {
            DefaultGlasses
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
    

    private void SetupBodies()
    {
        if (SkinnedMeshRendererParentBodies == null) { return; }
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;
        foreach (var smr in SkinnedMeshRendererParentBodies.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
            EnsureUniqueBodyMaterials(smr);
        }
        SetBody(NPCBodies.Normal);
    }


    private void SetupEyes()
    {
        if (SkinnedMeshRendererParentEyes == null) { return; }
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;
        foreach (var smr in SkinnedMeshRendererParentEyes.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr == null) { continue; }
            if (rootBone != null) { smr.rootBone = rootBone; }
            if (bones != null) { smr.bones = bones; }
            smr.updateWhenOffscreen = true;
        }
        SetEyes(NPCEyes.Default);
    }


    private void SetupHair()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allHair =
        {
            DefaultHair
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


    private void SetupShoes()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allShoes =
        {
            WorkFlatsShoes
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



    private void SetupNecklaces()
    {
        Transform rootBone = Body != null ? Body.rootBone : null;
        Transform[] bones = Body != null ? Body.bones : null;

        SkinnedMeshRenderer[] allNecklaces =
        {
            DefaultNecklace
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

    
    private void HideAllGlasses()
    {
        if (DefaultGlasses != null) { DefaultGlasses.enabled = false; }
    }


    private void HideAllHair()
    {
        if (DefaultHair != null) { DefaultHair.enabled = false; }
    }

    private void HideAllShoes()
    {
        if (WorkFlatsShoes != null) { WorkFlatsShoes.enabled = false; }
    }

    private void HideAllNecklaces()
    {
        if (DefaultNecklace != null) { DefaultNecklace.enabled = false; }
    }


    private void HideAllBodies()
    {
        if (NormalBody != null) { NormalBody.enabled = false; }
    }

    private void HideAllEyes()
    {
        if (DefaultEyes != null) { DefaultEyes.enabled = false; }
    }


    public void SetHair(NPCHairName hair)
    {
        HideAllHair();
        switch (hair)
        {
            case NPCHairName.DefaultHair: if (DefaultHair != null) { DefaultHair.enabled = true; } break;
        }
    }


    public void SetShoes(NPCShoesName shoes)
    {
        HideAllShoes();
        
        switch (shoes)
        {
            case NPCShoesName.Default:
                if (WorkFlatsShoes != null) { WorkFlatsShoes.enabled = true; }

                break;

            case NPCShoesName.None:

                break;
        }
    }

    
    public void SetNecklace(NPCNecklaceName necklace)
    {
        HideAllNecklaces();
        switch (necklace)
        {
            case NPCNecklaceName.Default: if (DefaultNecklace != null) { DefaultNecklace.enabled = true; } break;
            case NPCNecklaceName.None: break;
        }
    }
    

    public void SetGlasses(NPCGlassesName glasses)
    {
        HideAllGlasses();
        switch (glasses)
        {
            case NPCGlassesName.Default: if (DefaultGlasses != null) { DefaultGlasses.enabled = true; } break;
            case NPCGlassesName.None: break;
        }
    }


    public void SetBody(NPCBodies body)
    {
        HideAllBodies();
        switch (body)
        {
            case NPCBodies.Normal: if (NormalBody != null) { NormalBody.enabled = true; } break;
            default: if (NormalBody != null) { NormalBody.enabled = true; } break;
        }
    }


    public void SetEyes(NPCEyes eyes)
    {
        HideAllEyes();
        switch (eyes)
        {
            case NPCEyes.Default: if (DefaultEyes != null) { DefaultEyes.enabled = true; } break;
            default: if (DefaultEyes != null) { DefaultEyes.enabled = true; } break;
        }
    }

    
    private void HideAllHat()
    {
        if (DefaultHat != null) { DefaultHat.enabled = false; }
        if (HatPlaceholder1 != null) { HatPlaceholder1.enabled = false; }
        if (HatPlaceholder2 != null) { HatPlaceholder2.enabled = false; }
        if (HatPlaceholder3 != null) { HatPlaceholder3.enabled = false; }
        if (HatPlaceholder4 != null) { HatPlaceholder4.enabled = false; }
        if (HatPlaceholder5 != null) { HatPlaceholder5.enabled = false; }
    }


    public void SetHat(NPCHatName hat)
    {
        HideAllHat();
        switch (hat)
        {
            case NPCHatName.Default: if (DefaultHat != null) { DefaultHat.enabled = true; } break;
            case NPCHatName.None: break;
        }
    }



    private void HideAllChoker()
    {
        if (Choker != null) { Choker.enabled = false; }
        if (WhiteLaceChoker != null) { WhiteLaceChoker.enabled = false; }
        if (BlackLaceChoker != null) { BlackLaceChoker.enabled = false; }
        if (ChokerPlaceholder3 != null) { ChokerPlaceholder3.enabled = false; }
        if (ChokerPlaceholder4 != null) { ChokerPlaceholder4.enabled = false; }
        if (ChokerPlaceholder5 != null) { ChokerPlaceholder5.enabled = false; }
    }
    
    public void SetChoker(NPCChokerAccessoryName choker)
    {
        HideAllChoker();
        switch (choker)
        {
            case NPCChokerAccessoryName.Choker: if (Choker != null) { Choker.enabled = true; } break;
            case NPCChokerAccessoryName.None: break;
        }
    }


    private void HideAllVice()
    {
        if (Cigarette != null) { Cigarette.enabled = false; }
        if (VicePlaceholder1 != null) { VicePlaceholder1.enabled = false; }
        if (VicePlaceholder2 != null) { VicePlaceholder2.enabled = false; }
        if (VicePlaceholder3 != null) { VicePlaceholder3.enabled = false; }
        if (VicePlaceholder4 != null) { VicePlaceholder4.enabled = false; }
        if (VicePlaceholder5 != null) { VicePlaceholder5.enabled = false; }
    }

    public void SetVice(NPCViceName vice)
    {
        HideAllVice();
        switch (vice)
        {
            case NPCViceName.Cigarette: if (Cigarette != null) { Cigarette.enabled = true; } break;
            case NPCViceName.None: break;
        }
    }
    
    
    private NPCHairName GetHairForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Hair ?? NPCHairName.DefaultHair;
    }

    private NPCShoesName GetShoesForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Shoes ?? NPCShoesName.Default;
    }
    
    private NPCNecklaceName GetNecklaceForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Necklace ?? NPCNecklaceName.None;
    }
    
    private NPCGlassesName GetGlassesForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Glasses ?? NPCGlassesName.None;
    }

    private NPCBodies GetBodyForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Body ?? NPCBodies.Normal;
    }

    private NPCEyes GetEyesForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.NPCEyes ?? NPCEyes.Default;
    }

    private NPCWingsName GetWingsForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Wings ?? NPCWingsName.None;
    }

    private NPCApronName GetApronForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Apron ?? NPCApronName.None;
    }

    private NPCHatName GetHatForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Hat ?? NPCHatName.None;
    }

    private NPCChokerAccessoryName GetChokerForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Choker ?? NPCChokerAccessoryName.None;
    }

    private NPCViceName GetViceForOutfit(NPCOutfitName outfit)
    {
        return GetOutfit(outfit)?.Vice ?? NPCViceName.None;
    }
    
    private NPCOutfit GetOutfit(NPCOutfitName outfit)
    {
        if (_outfitLookup == null) { BuildOutfitLookup(); }
        return _outfitLookup != null && _outfitLookup.TryGetValue(outfit, out NPCOutfit data) ? data : null;
    }

    private void BuildOutfitLookup()
    {
        _outfitLookup = new Dictionary<NPCOutfitName, NPCOutfit>();
        if (Outfits == null) { return; }
        foreach (var outfit in Outfits)
        {
            if (outfit == null) { continue; }
            _outfitLookup[outfit.thisOutfit] = outfit;
        }
    }

    private List<NPCOutfitName> GetInspectorOrderedOutfits()
    {
        if (_inspectorOrderedOutfits != null) { return _inspectorOrderedOutfits; }

        _inspectorOrderedOutfits = new List<NPCOutfitName>();
        if (Outfits == null) { return _inspectorOrderedOutfits; }

        foreach (var type in InspectorSectionOrder)
        {
            foreach (var outfit in Outfits)
            {
                if (outfit == null) { continue; }
                if (outfit.NPCOutfitType != type) { continue; }
                _inspectorOrderedOutfits.Add(outfit.thisOutfit);
            }
        }

        return _inspectorOrderedOutfits;
    }

    private void InstantiatePrefabs(List<GameObject> prefabs)
    {
        if (prefabs == null || SkinnedMeshRendererParentOutfits == null) { return; }
        if (Body == null) { Debug.LogError("Body SkinnedMeshRenderer is not assigned on MENPC!"); return; }
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


    public bool IsOutfitSpawnable(NPCOutfitName outfit)
    {
        var data = GetOutfit(outfit);
        if (data == null) { return true; }
        if (data.MarkedForDeletion) { return false; }
        return data.SpawnAs;
    }

    public void NextOutfit()
    {
        List<NPCOutfitName> order = GetInspectorOrderedOutfits();
        if (order.Count == 0) { return; }

        int index = order.IndexOf(currentOutfit);
        index = (index + 1) % order.Count;

        SwitchToOutfit(order[index]);
    }

    public void NextOutfitWithContext(NPC npcNAME)
    {
        Debug.Log("context: "+npcNAME);
        if (thisNPC != npcNAME) return;
        List<NPCOutfitName> order = GetInspectorOrderedOutfits();
        if (order.Count == 0) { return; }

        int index = order.IndexOf(currentOutfit);
        index = (index + 1) % order.Count;

        SwitchToOutfit(order[index]);
    }

    public void PreviousOutfit()
    {
        List<NPCOutfitName> order = GetInspectorOrderedOutfits();
        if (order.Count == 0) { return; }

        int index = order.IndexOf(currentOutfit);
        if (index < 0) { index = 0; }
        index = (index - 1 + order.Count) % order.Count;

        SwitchToOutfit(order[index]);
    }
    public void PreviousOutfitWithContext(NPC npcNAME)
    {
        Debug.Log("context: "+npcNAME);
        if (thisNPC != npcNAME) return;
        List<NPCOutfitName> order = GetInspectorOrderedOutfits();
        if (order.Count == 0) { return; }

        int index = order.IndexOf(currentOutfit);
        if (index < 0) { index = 0; }
        index = (index - 1 + order.Count) % order.Count;

        SwitchToOutfit(order[index]);
    }

    public void ExitConversationWithContext(NPC npcNAME)
    {
        Debug.Log("context: "+npcNAME);
        npcController.Talk.EndConversationWithNora();
        EventManager.DisengageNPC();
    }

#if UNITY_EDITOR
    public void StepOutfitFiltered(int direction, Func<NPCOutfit, bool> filter)
    {
        List<NPCOutfitName> order = GetInspectorOrderedOutfits();
        if (order.Count == 0) { return; }

        int index = order.IndexOf(currentOutfit);
        if (index < 0) { index = direction > 0 ? -1 : 0; }

        for (int i = 0; i < order.Count; i++)
        {
            index = ((index + direction) % order.Count + order.Count) % order.Count;
            NPCOutfit candidate = GetOutfit(order[index]);
            if (filter == null || (candidate != null && filter(candidate)))
            {
                SwitchToOutfit(order[index]);
                return;
            }
        }

        Debug.LogWarning("MENPC: No outfits match the current filter.");
    }
#endif

    private void SwitchToOutfit(NPCOutfitName outfit)
    {
        if (outfit == NPCOutfitName.None) { DisableAllMainOutfits(); return; }
        ToggleOutfit(outfit, true);
    }

    public void ToggleOutfit(NPCOutfitName outfit, bool? forceOn = null)
    {
        var data = GetOutfit(outfit);
        JiggleToggle(data != null && data.Jiggle);
        ToggleMainOutfit(outfit, forceOn);
    }


    private void JiggleToggle(bool isJiggly = false)
    {
        if (JiggleLeftBoob != null) { JiggleLeftBoob.SetActive(isJiggly); }
        if (JiggleRightBoob != null) { JiggleRightBoob.SetActive(isJiggly); }
        if (JiggleLeftButtcheek != null) { JiggleLeftButtcheek.SetActive(isJiggly); }
        if (JiggleRightButtcheek != null) { JiggleRightButtcheek.SetActive(isJiggly); }
    }

    private void ToggleMainOutfit(NPCOutfitName outfit, bool? forceOn = null)
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

    public void SetMainOutfit(NPCOutfitName outfit)
    {
        ClearOutfitMeshes();
        currentOutfit = outfit;

        var data = GetOutfit(outfit);
        if (data != null) { InstantiatePrefabs(data.OutfitPrefabs); }

        SetHair(GetHairForOutfit(outfit));
        SetShoes(GetShoesForOutfit(outfit));
        SetNecklace(GetNecklaceForOutfit(outfit));
        SetGlasses(GetGlassesForOutfit(outfit));
        SetBody(GetBodyForOutfit(outfit));
        SetEyes(GetEyesForOutfit(outfit));
        SetHat(GetHatForOutfit(outfit));
        SetChoker(GetChokerForOutfit(outfit));
        SetVice(GetViceForOutfit(outfit));
        ApplyBodyColors(outfit);

        Debug.Log("outfit name passed: "+outfit);
        Debug.Log("wardrobe count when asked: "+_outfitLookup.Count);

        string OutfitDebugText = "Title: "+data.outfitTitle + "\nOID: " + data.thisOutfit+"["+(int)data.thisOutfit+"]\nMarked Deleted: "+data.MarkedForDeletion;
        
        EventManager.OutfitWasChanged(OutfitDebugText);
    }
    




    public void DisableAllMainOutfits()
    {
        ClearOutfitMeshes();
        currentOutfit = NPCOutfitName.None;
        SetHair(NPCHairName.DefaultHair);
        HideAllShoes();
        HideAllNecklaces();
        HideAllGlasses();
        SetBody(NPCBodies.Normal);
        SetEyes(NPCEyes.Default);
        SetHat(NPCHatName.None);
        SetChoker(NPCChokerAccessoryName.None);
        SetVice(NPCViceName.None);
    }

    public void UndressAsIs()
    {
        HideAllHat();
        HideAllChoker();
        HideAllVice();
        ClearOutfitMeshes();
        currentOutfit = NPCOutfitName.None;
    }

    private NPCOutfitName PickRandom(NPCOutfitName[] pool)
    {
        NPCOutfitName[] eligible = pool.Where(o => IsOutfitSpawnable(o) && o != currentOutfit).ToArray();
        if (eligible.Length == 0)
            eligible = pool.Where(o => IsOutfitSpawnable(o)).ToArray();

        if (eligible.Length == 0)
            return currentOutfit;

        return eligible[Random.Range(0, eligible.Length)];
    }

    private NPCOutfitName[] GetOutfitsOfType(NPCOutfitType type)
    {
        if (Outfits == null) { return new NPCOutfitName[0]; }
        return Outfits
            .Where(o => o != null && o.NPCOutfitType == type && o.thisOutfit != NPCOutfitName.None)
            .Select(o => o.thisOutfit)
            .ToArray();
    }

    public void SetRandomOutfitOfType(NPCOutfitType type)
    {
        NPCOutfitName[] pool;

        switch (type)
        {
            case NPCOutfitType.Main:
                pool = GetOutfitsOfType(NPCOutfitType.Main);
                break;
            case NPCOutfitType.NightOut:
                pool = GetOutfitsOfType(NPCOutfitType.NightOut);
                break;
            case NPCOutfitType.Special:
                pool = GetOutfitsOfType(NPCOutfitType.Special);
                break;
            case NPCOutfitType.Storyline:
                pool = GetOutfitsOfType(NPCOutfitType.Storyline);
                break;
            default:
                Debug.LogWarning($"MENPC: No outfit pool defined for type {type}");
                return;
        }

        if (pool.Length == 0)
        {
            Debug.LogWarning($"MENPC: No outfits found for type {type}");
            return;
        }

        SwitchToOutfit(PickRandom(pool));
    }


    private SkinnedMeshRenderer GetBodyRenderer(NPCBodies body)
    {
        switch (body)
        {
            case NPCBodies.Normal: return NormalBody;
            default: return NormalBody;
        }
    }


    private void ApplyBodyColors(NPCOutfitName outfit)
    {
        SkinnedMeshRenderer target = GetBodyRenderer(GetBodyForOutfit(outfit));
        if (target == null) { target = Body; }
        if (target == null) { return; }

        Color lipColor = GetLipColorForOutfit(outfit);
        Color nailColor = GetNailColorForOutfit(outfit);
        Material[] materials = Application.isPlaying ? target.materials : target.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
            {
                continue;
            }
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
        if (Application.isPlaying) { target.materials = materials; }
    }
    
    

    
    private Color GetLipColorForOutfit(NPCOutfitName outfit)
    {
        var data = GetOutfit(outfit);
        return data != null ? data.lipsColor : Color.white;
    }

    private Color GetNailColorForOutfit(NPCOutfitName outfit)
    {
        var data = GetOutfit(outfit);
        return data != null ? data.nailsColor : Color.white;
    }

    public void Undress(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        UndressAsIs();
    }
    
    
    private void EnsureUniqueBodyMaterials(SkinnedMeshRenderer smr)
    {
        if (smr == null) { return; }
        Material[] mats = smr.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null) { mats[i] = new Material(mats[i]); }
        }
        smr.sharedMaterials = mats;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MENPC))]
public class MENPCEditor : Editor
{
    private const float ButtonHeight = 28f;
    private const float ButtonSpacing = 4f;
    private const float SideMargin = 40f;
    private const int DefaultColumns = 3;
    private const string FilterSessionKey = "NorasWardrobeEditor.NPCOutfitFilter";

    private enum NPCOutfitFilter
    {
        All,
        StageOne,
        StageTwo,
        StageThree,
        StageSpecial,
        TypeWork,
        TypeMain,
        TypePyjamas,
        TypeNightOut,
        TypeSpecial,
        TypeStoryline,
        TypeUndergarments,
        MarkedForDeletion
    }

    private NPCOutfitFilter _filter = NPCOutfitFilter.All;

    private string _outfitSearchText = string.Empty;
    private Rect _searchAreaRect;

    private void OnEnable()
    {
        _outfitSearchText = string.Empty;
        _filter = (NPCOutfitFilter)SessionState.GetInt(FilterSessionKey, (int)NPCOutfitFilter.All);
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

    private static string GetFilterLabel(NPCOutfitFilter filter)
    {
        switch (filter)
        {
            case NPCOutfitFilter.All: return "All";
            case NPCOutfitFilter.StageOne: return "Only Stage: One";
            case NPCOutfitFilter.StageTwo: return "Only Stage: Two";
            case NPCOutfitFilter.StageThree: return "Only Stage: Three";
            case NPCOutfitFilter.StageSpecial: return "Only Stage: Special";
            case NPCOutfitFilter.TypeWork: return "Only Type: Work";
            case NPCOutfitFilter.TypeMain: return "Only Type: Main";
            case NPCOutfitFilter.TypePyjamas: return "Only Type: Pyjamas";
            case NPCOutfitFilter.TypeNightOut: return "Only Type: NightOut";
            case NPCOutfitFilter.TypeSpecial: return "Only Type: Special";
            case NPCOutfitFilter.TypeStoryline: return "Only Type: Storyline";
            case NPCOutfitFilter.TypeUndergarments: return "Only Type: Undergarments";
            case NPCOutfitFilter.MarkedForDeletion: return "Only Marked For Deletion";
            default: return "All";
        }
    }


    private static Func<NPCOutfit, bool> GetFilterPredicate(NPCOutfitFilter filter)
    {
        switch (filter)
        {
            case NPCOutfitFilter.TypeMain: return o => o.NPCOutfitType == NPCOutfitType.Main;
            case NPCOutfitFilter.TypeNightOut: return o => o.NPCOutfitType == NPCOutfitType.NightOut;
            case NPCOutfitFilter.TypeSpecial: return o => o.NPCOutfitType == NPCOutfitType.Special;
            case NPCOutfitFilter.TypeStoryline: return o => o.NPCOutfitType == NPCOutfitType.Storyline;
            case NPCOutfitFilter.MarkedForDeletion: return o => o.MarkedForDeletion;
            default: return null;
        }
    }

    private void AddFilterMenuItem(GenericMenu menu, NPCOutfitFilter filter)
    {
        menu.AddItem(new GUIContent(GetFilterLabel(filter)), _filter == filter, () =>
        {
            _filter = filter;
            SessionState.SetInt(FilterSessionKey, (int)filter);
            Repaint();
        });
    }

    private void DrawFilterDropdown(GUIStyle style)
    {
        if (GUILayout.Button("Filter: " + GetFilterLabel(_filter) + "  \u25BE", style, GUILayout.Height(ButtonHeight)))
        {
            GenericMenu menu = new GenericMenu();
            AddFilterMenuItem(menu, NPCOutfitFilter.All);
            AddFilterMenuItem(menu, NPCOutfitFilter.StageOne);
            AddFilterMenuItem(menu, NPCOutfitFilter.StageTwo);
            AddFilterMenuItem(menu, NPCOutfitFilter.StageThree);
            AddFilterMenuItem(menu, NPCOutfitFilter.StageSpecial);
            menu.AddSeparator("");
            AddFilterMenuItem(menu, NPCOutfitFilter.TypeWork);
            AddFilterMenuItem(menu, NPCOutfitFilter.TypeMain);
            AddFilterMenuItem(menu, NPCOutfitFilter.TypePyjamas);
            AddFilterMenuItem(menu, NPCOutfitFilter.TypeNightOut);
            AddFilterMenuItem(menu, NPCOutfitFilter.TypeSpecial);
            AddFilterMenuItem(menu, NPCOutfitFilter.TypeStoryline);
            AddFilterMenuItem(menu, NPCOutfitFilter.TypeUndergarments);
            menu.AddSeparator("");
            AddFilterMenuItem(menu, NPCOutfitFilter.MarkedForDeletion);
            menu.ShowAsContext();
        }
    }

    public override void OnInspectorGUI()
    {
        MENPC me = (MENPC)target;
        
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
            "Assets/Editor/butterflyeditornpc.png"
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



        var resultsbuttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.7f, 0.4f, 1f) },
            padding = new RectOffset(3, 3, 3, 3)
        };


        
        EditorGUILayout.BeginVertical(boxStyle);
        
        EditorGUILayout.Space();
        

        GUI.backgroundColor = Color.red;
        DrawButtonRow(me, redButton,
            ("Previous NPCOutfit", () => { me.StepOutfitFiltered(-1, GetFilterPredicate(_filter)); SelectOutfitAsset(me); }),
            ("Next NPCOutfit", () => { me.StepOutfitFiltered(1, GetFilterPredicate(_filter)); SelectOutfitAsset(me); }));

        DrawFilterDropdown(redButton);
        
        
        EditorGUILayout.Space();
        
        
        DrawOutfitSearch(me, statsTitle, resultsbuttonStyle);
        
        EditorGUILayout.Space();
        DrawNPCOutfitTypeSection(me, clearAllStyle, "Main Outfits", NPCOutfitType.Main, Color.green);
        
        EditorGUILayout.Space();
        DrawNPCOutfitTypeSection(me, clearAllStyle, "Night Out", NPCOutfitType.NightOut, new Color(1f, 0.6f, 0.8f));

        EditorGUILayout.Space();
        DrawNPCOutfitTypeSection(me, clearAllStyle, "Special Outfits", NPCOutfitType.Special, new Color(0.7f, 0.4f, 1f));

        EditorGUILayout.Space();
        DrawNPCOutfitTypeSection(me, clearAllStyle, "Storyline Outfits", NPCOutfitType.Storyline, new Color(0.1f, 0.1f, 0.5f));
        
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("DefaultHat", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("DefaultHat", () => me.SetHat(NPCHatName.Default)));
        DrawButtonRow(me, clearAllStyle, ("None", () => me.SetHat(NPCHatName.None)));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Choker (Accessory)", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Choker", () => me.SetChoker(NPCChokerAccessoryName.Choker)));
        DrawButtonRow(me, clearAllStyle, ("None", () => me.SetChoker(NPCChokerAccessoryName.None)));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Vice", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Cigarette", () => me.SetVice(NPCViceName.Cigarette)));
        DrawButtonRow(me, clearAllStyle, ("None", () => me.SetVice(NPCViceName.None)));

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.5f);

        EditorGUILayout.LabelField("Shoes", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("WorkFlats", () => me.SetShoes(NPCShoesName.Default)));
        DrawButtonRow(me, clearAllStyle, ("None", () => me.SetShoes(NPCShoesName.None)));

        
        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.8f, 0.7f, 0.9f);

        EditorGUILayout.LabelField("Necklace", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Butterfly", () => me.SetNecklace(NPCNecklaceName.Default)));
        DrawButtonRow(me, clearAllStyle, ("None", () => me.SetNecklace(NPCNecklaceName.None)));
        
        
        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.7f, 0.85f, 0.95f);

        EditorGUILayout.LabelField("Glasses", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Professional", () => me.SetGlasses(NPCGlassesName.Default)));
        DrawButtonRow(me, clearAllStyle, ("None", () => me.SetGlasses(NPCGlassesName.None)));

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.6f, 0.75f, 0.6f);

        EditorGUILayout.LabelField("Body", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Normal", () => me.SetBody(NPCBodies.Normal)));
        

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.6f, 0.8f, 0.85f);

        EditorGUILayout.LabelField("NPCEyes", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Default", () => me.SetEyes(NPCEyes.Default)));
        
        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);

        EditorGUILayout.LabelField("Hair", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Default Hair", () => me.SetHair(NPCHairName.DefaultHair)));

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Random Outfits", EditorStyles.boldLabel);
        DrawButtonRow(me, clearAllStyle, ("Random Work", () => me.SetRandomOutfitOfType(NPCOutfitType.Main)));
        DrawButtonRow(me, clearAllStyle, ("Random Night Out", () => me.SetRandomOutfitOfType(NPCOutfitType.NightOut)));
        DrawButtonRow(me, clearAllStyle, ("Random Storyline", () => me.SetRandomOutfitOfType(NPCOutfitType.Storyline)));
        DrawButtonRow(me, clearAllStyle, ("Random Special NPCOutfit", () => me.SetRandomOutfitOfType(NPCOutfitType.Special)));
        EditorGUILayout.EndVertical();
        DrawDefaultInspector();
    }

    private void DrawOutfitSearch(MENPC me, GUIStyle titleStyle, GUIStyle labelStyle)
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
            string query = _outfitSearchText.ToLowerInvariant().Trim();
            bool queryIsNumeric = int.TryParse(query, out int numericQuery);

            List<NPCOutfitName> matches = System.Enum.GetValues(typeof(NPCOutfitName))
                .Cast<NPCOutfitName>()
                .Where(o => o != NPCOutfitName.None &&
                    (o.ToString().ToLowerInvariant().Contains(query) ||
                     (queryIsNumeric && (int)o == numericQuery)))
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
                    if (GUILayout.Button($"{match} [{(int)match}]", labelStyle))
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

    

    private static void SelectOutfitAsset(MENPC me)
    {
        NPCOutfit matchingOutfit = (me.Outfits ?? new List<NPCOutfit>()).FirstOrDefault(o => o != null && o.thisOutfit == me.currentOutfit);

        if (matchingOutfit == null)
        {
            Debug.LogWarning($"NorasWardrobeEditor: No NPCOutfit asset found matching {me.currentOutfit}.");
            return;
        }

        Selection.activeObject = matchingOutfit;
        EditorGUIUtility.PingObject(matchingOutfit);
    }

    private void DrawNPCOutfitTypeSection(MENPC me, GUIStyle style, string sectionLabel, NPCOutfitType type, Color color)
    {
        GUI.backgroundColor = color;
        EditorGUILayout.LabelField(sectionLabel, EditorStyles.boldLabel);

        var outfits = (me.Outfits ?? new List<NPCOutfit>())
            .Where(o => o != null && o.NPCOutfitType == type)
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

    private void DrawOutfitButtonRow(MENPC me, GUIStyle style, NPCOutfit[] outfits)
    {
        if (outfits == null || outfits.Length == 0) { return; }

        float slotWidth = GetButtonWidth(outfits.Length);

        EditorGUILayout.BeginHorizontal();
        foreach (var outfit in outfits)
        {
            string label = string.IsNullOrEmpty(outfit.outfitTitle) ? outfit.thisOutfit.ToString() : outfit.outfitTitle;
            NPCOutfitName NPCOutfitName = outfit.thisOutfit;

            if (GUILayout.Button(label, style, GUILayout.Height(ButtonHeight), GUILayout.Width(slotWidth)))
            {

                me.ToggleOutfit(NPCOutfitName);
                SelectOutfitAsset(me);
                
                EditorUtility.SetDirty(me);
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

    private static void DrawButtonRow(MENPC me, GUIStyle style, params (string Label, System.Action OnClick)[] buttons)
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


public enum NPCHairName
{
    DefaultHair
}


public enum NPCShoesName
{
    None,
    Default
}

public enum NPCNecklaceName
{
    None,
    Default
}

public enum NPCGlassesName
{
    None,
    Default
}

public enum NPCBodies
{
    Normal
}

public enum NPCEyes
{
    Default
}

public enum NPCWingsName
{
    None,
    Default
}

public enum NPCApronName
{
    None,
    Default
}

public enum NPCHatName
{
    None,
    Default
}

public enum NPCChokerAccessoryName
{
    None,
    Choker
}

public enum NPCViceName
{
    None,
    Cigarette
}


public enum NPCOutfitType
{
    Main,
    NightOut,
    Storyline,
    Special
}

public enum NPCOutfitName
{
    None,
    Casual,
    Dress,
    FlutterDress
    
}