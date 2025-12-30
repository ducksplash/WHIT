using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls; 

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject MainPanel;
    public GameObject OptionsPanel;
    public GameObject ControlsPanel;
    public GameObject KeybindPanel;

    [Header("Buttons")]
    public Button PlayButton;
    public Button QuitButton;
    public Button OptionsButton;
    public Button BackButton;

    [Header("Key Binding UI")]
    public TextMeshProUGUI KeybindLabel;
    public GameObject WaitingForKeyPanel;

    // state
    private bool WaitingForKey = false;
    private string KeySetFunction = "";

    private void Start()
    {
        if (MainPanel != null)
            MainPanel.SetActive(true);

        if (OptionsPanel != null)
            OptionsPanel.SetActive(false);

        if (ControlsPanel != null)
            ControlsPanel.SetActive(false);

        if (KeybindPanel != null)
            KeybindPanel.SetActive(false);

        if (WaitingForKeyPanel != null)
            WaitingForKeyPanel.SetActive(false);

        if (PlayButton != null)
            PlayButton.onClick.AddListener(StartGame);

        if (QuitButton != null)
            QuitButton.onClick.AddListener(QuitGame);

        if (OptionsButton != null)
            OptionsButton.onClick.AddListener(OpenOptions);

        if (BackButton != null)
            BackButton.onClick.AddListener(ExitToMain);
    }

    private void Update()
    {
        HandleEscape();
        HandleKeyRebinding();
    }

    // ==============================
    // ESCAPE → return to main menu
    // ==============================
    private void HandleEscape()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.escapeKey.wasReleasedThisFrame)
        {
            ExitToMain();
        }
    }

    // ======================================================
    // KEY REBINDING – replaces Input.anyKey + Input.GetKey()
    // ======================================================
    private void HandleKeyRebinding()
    {
        if (!WaitingForKey)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // Allow cancel with Escape
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelRebind();
            return;
        }

        // Any key pressed?
        if (!keyboard.anyKey.wasPressedThisFrame)
            return;

        foreach (KeyControl key in keyboard.allKeys)
        {
            if (!key.wasPressedThisFrame)
                continue;

            TrySaveKey(key);
            break;
        }
    }

    private void TrySaveKey(KeyControl key)
    {
        // Convert input-system key → KeyCode string
        string keyName = key.keyCode.ToString();

        if (!Enum.TryParse(key.keyCode.ToString(), out KeyCode unityKey))
        {
            Debug.Log($"Cannot convert key: {keyName}");
            return;
        }

        SaveKey(unityKey, KeySetFunction);
    }

    // ==============================
    // KEY SAVE + UI UPDATE
    // ==============================
    public void BeginRebind(string functionName)
    {
        KeySetFunction = functionName;
        WaitingForKey = true;

        if (WaitingForKeyPanel != null)
            WaitingForKeyPanel.SetActive(true);
    }

    private void CancelRebind()
    {
        WaitingForKey = false;

        if (WaitingForKeyPanel != null)
            WaitingForKeyPanel.SetActive(false);
    }

    private void SaveKey(KeyCode key, string functionName)
    {
        StoredPrefs.SetString(functionName, key.ToString());
        StoredPrefs.Save();

        WaitingForKey = false;

        if (WaitingForKeyPanel != null)
            WaitingForKeyPanel.SetActive(false);

        RefreshKeyUI();
    }

    private void RefreshKeyUI()
    {
        if (KeybindLabel == null)
            return;

        string value = StoredPrefs.GetString(KeySetFunction, "None");
        KeybindLabel.text = $"{KeySetFunction}: {value}";
    }

    // ==============================
    // PANEL NAVIGATION
    // ==============================
    public void OpenOptions()
    {
        MainPanel?.SetActive(false);
        OptionsPanel?.SetActive(true);
    }

    public void OpenControls()
    {
        OptionsPanel?.SetActive(false);
        ControlsPanel?.SetActive(true);
    }

    public void OpenKeybinds()
    {
        ControlsPanel?.SetActive(false);
        KeybindPanel?.SetActive(true);
        RefreshKeyUI();
    }

    public void ExitToMain()
    {
        MainPanel?.SetActive(true);
        OptionsPanel?.SetActive(false);
        ControlsPanel?.SetActive(false);
        KeybindPanel?.SetActive(false);
        WaitingForKey = false;

        if (WaitingForKeyPanel != null)
            WaitingForKeyPanel.SetActive(false);
    }

    // ==============================
    // GAME FLOW
    // ==============================
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
