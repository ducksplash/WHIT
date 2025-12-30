using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyEntryUI : MonoBehaviour
{
    public TMP_InputField keyInput;
    public TMP_InputField valueInput;
    public Button deleteButton;

    private string key;
    private Action<string, string> onValueChanged;
    private Action<string> onDeleted;

    public void Setup(string key, string value, Action<string, string> valueChangedCallback, Action<string> deleteCallback)
    {
        this.key = key;
        this.onValueChanged = valueChangedCallback;
        this.onDeleted = deleteCallback;

        // Assign values
        if (keyInput != null)
        {
            keyInput.text = key;
            keyInput.interactable = false; // Make key read-only
        }

        if (valueInput != null)
        {
            valueInput.text = value;
            valueInput.onEndEdit.RemoveAllListeners();
            valueInput.onEndEdit.AddListener(OnValueEdited);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }
    }

    private void OnValueEdited(string newValue)
    {
        onValueChanged?.Invoke(key, newValue);
    }

    private void OnDeleteClicked()
    {
        onDeleted?.Invoke(key);
        Destroy(gameObject);
    }
}