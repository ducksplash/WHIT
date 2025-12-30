using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoredPrefsPanel : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;        // parent object to hold key entries
    public GameObject keyEntryPrefab;      // prefab with 2 TMP InputFields + Delete button
    public TMP_InputField newKeyInput;
    public TMP_InputField newValueInput;
    public Button addKeyButton;
    public Button resetAllButton;
    public CanvasGroup canvasGroup;
    
    private List<KeyEntryUI> keyEntries = new List<KeyEntryUI>();


    private void OnEnable()
    {
        EventManager.OnPaused += PanelToggle;
    }

    private void PanelToggle(bool isPaused)
    {
        if (isPaused)
        {
            PanelEnable();
        }
        else
        {
            PanelDisable();
        }
    }


    private void PanelEnable()
    {
        StoredPrefs.OnPrefsSaved += RefreshKeys;

        if (addKeyButton != null) addKeyButton.onClick.AddListener(AddNewKey);

        if (resetAllButton != null) resetAllButton.onClick.AddListener(ResetAllPrefs);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;

        RefreshKeys();
    }

    private void PanelDisable()
    {
        StoredPrefs.OnPrefsSaved -= RefreshKeys;

        if (addKeyButton != null) addKeyButton.onClick.RemoveListener(AddNewKey);

        if (resetAllButton != null) resetAllButton.onClick.RemoveListener(ResetAllPrefs);
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
    }

    public void RefreshKeys()
    {
        // Clear existing UI entries
        foreach (var entry in keyEntries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }
        keyEntries.Clear();

        // Get all keys
        var allKeys = StoredPrefs.GetAllKeys();
        allKeys.Sort(); // Optional: sort alphabetically

        foreach (var key in allKeys)
        {
            string value = StoredPrefs.GetString(key);
            GameObject go = Instantiate(keyEntryPrefab, contentParent);
            var ui = go.GetComponent<KeyEntryUI>();
            if (ui != null)
            {
                ui.Setup(key, value, OnKeyChanged, OnKeyDeleted);
                keyEntries.Add(ui);
            }
        }
    }

    private void OnKeyChanged(string key, string newValue)
    {
        StoredPrefs.SetString(key, newValue);
        StoredPrefs.Save();
    }

    private void OnKeyDeleted(string key)
    {
        StoredPrefs.DeleteKey(key);
        // No need to call RefreshKeys; the OnPrefsSaved event will trigger it
    }

    private void AddNewKey()
    {
        string key = newKeyInput.text.Trim();
        string value = newValueInput.text;

        if (string.IsNullOrEmpty(key))
            return;

        StoredPrefs.SetString(key, value);
        StoredPrefs.Save();

        newKeyInput.text = "";
        newValueInput.text = "";
    }

    private void ResetAllPrefs()
    {
        StoredPrefs.ResetAll();
    }
}
