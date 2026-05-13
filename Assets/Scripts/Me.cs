using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Me : MonoBehaviour
{
    [Header("Main Outfits (Mutually Exclusive)")]
    public List<SkinnedMeshRenderer> OutfitForWork;
    public List<SkinnedMeshRenderer> OutfitForCasual;
    public List<SkinnedMeshRenderer> OutfitForDate;
    public List<SkinnedMeshRenderer> OutfitForNightOut;
    public List<SkinnedMeshRenderer> OutfitForPyjamas;
    public List<SkinnedMeshRenderer> OutfitForHomelessness;

    [Header("Independent Layer")]
    public List<SkinnedMeshRenderer> OutfitUnderwearLayer;

    public bool UnressedOnLoad;
    public string ThisCharacterName;

    [Header("Only applies to NPCs")]
    public NPCController npcController;

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
            SetMainOutfit(1); // Start with Work outfit
    }

    // ====================== PUBLIC TOGGLE METHODS ======================
    public void ToggleWorkOutfit(bool? forceOn = null)        => ToggleMainOutfit(1, forceOn);
    public void ToggleCasualOutfit(bool? forceOn = null)      => ToggleMainOutfit(2, forceOn);
    public void ToggleDatingOutfit(bool? forceOn = null)      => ToggleMainOutfit(3, forceOn);
    public void ToggleNightOutOutfit(bool? forceOn = null)    => ToggleMainOutfit(4, forceOn);
    public void TogglePyjamasOutfit(bool? forceOn = null)     => ToggleMainOutfit(5, forceOn);
    public void ToggleHomelessnessOutfit(bool? forceOn = null)=> ToggleMainOutfit(6, forceOn);

    public void ToggleUnderwearLayer(bool? forceOn = null)
    {
        if (forceOn.HasValue)
        {
            SetUnderwearEnabled(forceOn.Value);
        }
        else
        {
            bool currentlyOn = IsUnderwearEnabled();
            SetUnderwearEnabled(!currentlyOn);
        }
    }

    // ====================== MAIN OUTFIT LOGIC ======================
    private void ToggleMainOutfit(int outfitIndex, bool? forceOn = null)
    {
        if (forceOn.HasValue)
        {
            if (forceOn.Value)
                SetMainOutfit(outfitIndex);
            else
                DisableAllMainOutfits();
        }
        else
        {
            // Original toggle behavior
            if (IsOnlyThisMainOutfitActive(outfitIndex))
            {
                DisableAllMainOutfits();
            }
            else
            {
                SetMainOutfit(outfitIndex);
            }
        }
    }

    public void SetMainOutfit(int outfitIndex)
    {
        DisableAllMainOutfits();

        switch (outfitIndex)
        {
            case 1: SetListEnabled(OutfitForWork, true); break;
            case 2: SetListEnabled(OutfitForCasual, true); break;
            case 3: SetListEnabled(OutfitForDate, true); break;
            case 4: SetListEnabled(OutfitForNightOut, true); break;
            case 5: SetListEnabled(OutfitForPyjamas, true); break;
            case 6: SetListEnabled(OutfitForHomelessness, true); break;
        }
    }

    private bool IsOnlyThisMainOutfitActive(int outfitIndex)
    {
        switch (outfitIndex)
        {
            case 1: return AllEnabled(OutfitForWork) && AllMainOutfitsDisabledExcept(OutfitForWork);
            case 2: return AllEnabled(OutfitForCasual) && AllMainOutfitsDisabledExcept(OutfitForCasual);
            case 3: return AllEnabled(OutfitForDate) && AllMainOutfitsDisabledExcept(OutfitForDate);
            case 4: return AllEnabled(OutfitForNightOut) && AllMainOutfitsDisabledExcept(OutfitForNightOut);
            case 5: return AllEnabled(OutfitForPyjamas) && AllMainOutfitsDisabledExcept(OutfitForPyjamas);
            case 6: return AllEnabled(OutfitForHomelessness) && AllMainOutfitsDisabledExcept(OutfitForHomelessness);
        }
        return false;
    }

    // ====================== UNDERWEAR ======================
    public bool IsUnderwearEnabled()
    {
        return AllEnabled(OutfitUnderwearLayer);
    }

    public void SetUnderwearEnabled(bool enabled)
    {
        SetListEnabled(OutfitUnderwearLayer, enabled);
    }

    // ====================== HELPER METHODS ======================
    public void DisableAllMainOutfits()
    {
        SetListEnabled(OutfitForWork, false);
        SetListEnabled(OutfitForCasual, false);
        SetListEnabled(OutfitForDate, false);
        SetListEnabled(OutfitForNightOut, false);
        SetListEnabled(OutfitForPyjamas, false);
        SetListEnabled(OutfitForHomelessness, false);
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

    private bool AllEnabled(List<SkinnedMeshRenderer> list)
    {
        if (list == null || list.Count == 0) return false;
        foreach (var r in list)
            if (r != null && !r.enabled) return false;
        return true;
    }

    private bool AllMainOutfitsDisabledExcept(List<SkinnedMeshRenderer> activeOutfit)
    {
        if (activeOutfit == OutfitForWork)        return AllDisabled(OutfitForCasual, OutfitForDate, OutfitForNightOut, OutfitForPyjamas, OutfitForHomelessness);
        if (activeOutfit == OutfitForCasual)      return AllDisabled(OutfitForWork, OutfitForDate, OutfitForNightOut, OutfitForPyjamas, OutfitForHomelessness);
        if (activeOutfit == OutfitForDate)        return AllDisabled(OutfitForWork, OutfitForCasual, OutfitForNightOut, OutfitForPyjamas, OutfitForHomelessness);
        if (activeOutfit == OutfitForNightOut)    return AllDisabled(OutfitForWork, OutfitForCasual, OutfitForDate, OutfitForPyjamas, OutfitForHomelessness);
        if (activeOutfit == OutfitForPyjamas)     return AllDisabled(OutfitForWork, OutfitForCasual, OutfitForDate, OutfitForNightOut, OutfitForHomelessness);
        if (activeOutfit == OutfitForHomelessness)return AllDisabled(OutfitForWork, OutfitForCasual, OutfitForDate, OutfitForNightOut, OutfitForPyjamas);
        return true;
    }

    private bool AllDisabled(params List<SkinnedMeshRenderer>[] lists)
    {
        foreach (var list in lists)
        {
            if (list == null) continue;
            foreach (var r in list)
                if (r != null && r.enabled) return false;
        }
        return true;
    }
}

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

        if (GUILayout.Button("Disable All Main Outfits", GUILayout.Height(25)))
        {
            me.DisableAllMainOutfits();
            EditorUtility.SetDirty(me);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Work Stuff", GUILayout.Height(35)))       { me.ToggleWorkOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Casuals", GUILayout.Height(35)))         { me.ToggleCasualOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Dating Attire", GUILayout.Height(35)))   { me.ToggleDatingOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Gladrags", GUILayout.Height(35)))        { me.ToggleNightOutOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Pyjamas", GUILayout.Height(35)))         { me.TogglePyjamasOutfit(); EditorUtility.SetDirty(me); }
        if (GUILayout.Button("Skid Row", GUILayout.Height(35)))        { me.ToggleHomelessnessOutfit(); EditorUtility.SetDirty(me); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Underwear Layer (Independent)", EditorStyles.boldLabel);

        if (GUILayout.Button("Toggle Underwear Layer", GUILayout.Height(35)))
        {
            me.ToggleUnderwearLayer();
            EditorUtility.SetDirty(me);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Call ToggleXXXOutfit(true) to force ON\n" +
                              "Call ToggleXXXOutfit(false) to force OFF\n" +
                              "Call without parameter to toggle.", MessageType.Info);
    }
}
#endif