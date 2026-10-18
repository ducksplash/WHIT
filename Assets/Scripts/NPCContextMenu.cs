using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCContextMenu : MonoBehaviour
{
    public NPCController SelectedNPC;
    public TextMeshProUGUI npcName;
    [Header("Trigger UI")]
    [SerializeField] private TMP_Dropdown triggerDropdown;

    private readonly List<string> _triggerOptions = new();
    
    private void Start()
    {
        EventManager.OnEngageNPC += EngageNPC;
    }

    void EngageNPC(NPCController npc)
    {
        SelectedNPC = npc;
        npcName.text = SelectedNPC.thisNPC.ToString().Replace("_", " ");
        RefreshTriggerDropdown();
    }

    public void OnClickPrev()
    {
        Debug.Log("click prev");
        EventManager.NPCPrev(SelectedNPC.thisNPC);
    }
    
    public void OnClickNext()
    {
        Debug.Log("click next");
        EventManager.NPCNext(SelectedNPC.thisNPC);
    }
    
    public void OnClickExit()
    {
        Debug.Log("click exit");
        EventManager.NPCExit(SelectedNPC.thisNPC);
    }
    
    
    
    
    
    
    private void RefreshTriggerDropdown()
    {

        _triggerOptions.Clear();

        if (SelectedNPC != null && SelectedNPC.triggerNames != null)
        {
            for (int i = 0; i < SelectedNPC.triggerNames.Count; i++)
            {
                string trig = SelectedNPC.triggerNames[i];
                if (!string.IsNullOrWhiteSpace(trig))
                    _triggerOptions.Add(trig);
            }
        }

        if (triggerDropdown == null) return;

        triggerDropdown.ClearOptions();

        if (_triggerOptions.Count == 0)
        {
            triggerDropdown.AddOptions(new List<string> { "(No triggers)" });
            triggerDropdown.interactable = false;
            return;
        }

        triggerDropdown.AddOptions(_triggerOptions);
        triggerDropdown.interactable = true;

        int idx = (SelectedNPC != null) ? Mathf.Clamp(SelectedNPC.selectedTriggerIndex, 0, _triggerOptions.Count - 1) : 0;
        triggerDropdown.value = idx;
        triggerDropdown.RefreshShownValue();

    }

    public void OnPlayTriggerClicked()
    {
        if (SelectedNPC == null) return;
        if (_triggerOptions.Count == 0) return;
        if (triggerDropdown == null) return;

        int idx = Mathf.Clamp(triggerDropdown.value, 0, _triggerOptions.Count - 1);
        string trig = _triggerOptions[idx];

        SelectedNPC.selectedTriggerIndex = idx;
        SelectedNPC.PlayTrigger(trig);
    }

}
