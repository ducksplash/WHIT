using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ComputerSystem : MonoBehaviour
{
    public ComputerScreen CurrentScreen = ComputerScreen.LockScreen;

    public CanvasGroup LockScreen;
    public CanvasGroup DesktopScreen;
    public List<CanvasGroup> AllProgrammeScreens;

    [Header("Furniture")] 
    public GameObject NearestChair;
    
    [Header("Lock Screen Texts")] 
    public TextMeshProUGUI IncorrectPasswordText;
    
    [Header("Desktop Texts")] 
    public TextMeshProUGUI FilesText;
    public TextMeshProUGUI EmailText;
    public TextMeshProUGUI PhoneText;
    public TextMeshProUGUI WebText;
    public TextMeshProUGUI HackingText;
    public TextMeshProUGUI GamesText;
    public TextMeshProUGUI SettingsText;

    [Header("File Manager Texts")] 
    public TextMeshProUGUI AddressBarText;
    
    public TextMeshProUGUI FavouritesText;
    public List<TextMeshProUGUI> DocumentsText;
    public List<TextMeshProUGUI> DownloadsText;
    public List<TextMeshProUGUI> PhotosText;
    public List<TextMeshProUGUI> VideosText;
    public List<TextMeshProUGUI> AudioText;
    
    public TextMeshProUGUI SystemText;
    public CanvasGroup CrosshairCanvas;

    public bool IsLoggedIn;
    public TextMeshProUGUI PasswordSource;
    public TMP_InputField PasswordInput;

    public Transform ViewableArea;

    public Collider PCCollider;
    
    public InputActionReference ClosePCInput;
    public InputActionReference goBack;
    
    void Start()
    {
        IncorrectPasswordText.gameObject.SetActive(false);
        EventManager.OnStopComputer += OnStopComputer;
        ClosePCInput.action.performed += InputStopComputer;
        goBack.action.performed += InputStopComputer;
        TerminalEventManager.OnPCGridClick += SelectMenuItem;
    }


    public void OnStartComputer()
    {
        if (GameMaster.Instance.PHONEOUT) return;
        if (GameMaster.Instance.ONPC) return;
        
        Debug.Log("StartPC");
        PCCollider.enabled = false;
        NearestChair.gameObject.SetActive(false);
        GameMaster.Instance.INMENU = true;
        GameMaster.Instance.FROZEN = true;
        GameMaster.Instance.ONPC = true;
        GameMaster.Instance.EventManager.StartComputer(ViewableArea);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
    }


    public void OnStopComputer()
    {
        if (GameMaster.Instance.PHONEOUT) return;
        if (!GameMaster.Instance.ONPC) return;
        
        Debug.Log("StopPC");
        PCCollider.enabled = true;
        NearestChair.gameObject.SetActive(true);
        GameMaster.Instance.INMENU = false;
        GameMaster.Instance.FROZEN = false;
        GameMaster.Instance.ONPC = false;
        IncorrectPasswordText.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    public void InputStopComputer(InputAction.CallbackContext callbackContext)
    {
        GameMaster.Instance.EventManager.StopComputer();
    }
    

    public void LogOn()
    {
        Debug.Log("log on");
        Debug.Log(PasswordInput.text);
        Debug.Log(PasswordSource.text);
        if (PasswordInput.text.ToString().Trim().Equals(PasswordSource.text.ToString().Trim()))
        {
            Debug.Log("in");
            ChangeScreen(ComputerScreen.Desktop);
            TogglePCLock(false);
        }
        else
        {
            Debug.Log("no");
            TogglePCLock(true);
            IncorrectPasswordText.gameObject.SetActive(true);
        }
    }



    public void SelectMenuItem(PCGriddle pcGriddle)
    {
        Debug.Log("GO TO "+pcGriddle);
        
        switch (pcGriddle)
        {

            case PCGriddle.None: 
                // do nothing
                break;
            case PCGriddle.LogOn: LogOn(); break;

        }
        
    }
    
    
    
    


    public void ChangeScreen(ComputerScreen ScreenToOpen)
    {
        CloseAllScreens();
        
        for (var i = 0; i < AllProgrammeScreens.Count; i++)
        {
            if (AllProgrammeScreens[i].GetComponent<PCScreen>().ThisPCScreen.Equals(ScreenToOpen))
            {            
                CanvasGroup SelectedScreen = AllProgrammeScreens[i].GetComponent<CanvasGroup>();

                SelectedScreen.alpha = 0;
                SelectedScreen.interactable = false;
                SelectedScreen.blocksRaycasts = false;
            }
        }
    }

    public void CloseAllScreens()
    {
        for (var i = 0; i < AllProgrammeScreens.Count; i++)
        {
            CanvasGroup ThisScreen = AllProgrammeScreens[i].GetComponent<CanvasGroup>();
            
            ThisScreen.alpha = 0;
            ThisScreen.interactable = false;
            ThisScreen.blocksRaycasts = false;
            
        }
    }
    public void TogglePCLock(bool isLocked)
    {
        LockScreen.alpha = isLocked ? 1 : 0;
        LockScreen.interactable = isLocked;
        LockScreen.blocksRaycasts = isLocked;
        
        DesktopScreen.alpha = isLocked ? 0 : 1;
        DesktopScreen.interactable = !isLocked;
        DesktopScreen.blocksRaycasts = !isLocked;
    }
    
}



public enum ComputerScreen
{
    LockScreen = 1000,
    Desktop = 1001,
    Files = 1002,
    Phone = 1003,
    Emails = 1004,
    Web = 1005,
    Hacking = 1006,
    Games = 1007,
    Settings = 1008
    
}

public enum FileFolder
{
    User = 2000,
    Documents = 2001,
    Downloads = 2002,
    Photos = 2003,
    Videos = 2004,
    Audio = 2005,
    Phone = 2006,
    O = 2007,
    DCIM = 2008,
    Evidence = 2009
}


public enum PCGriddle
{
    None = 4000,
    LogOn = 4001,
    
    

    LockScreen = 10001,
    Desktop = 10002,
    Files = 10002,
    Phone = 10003,
    Emails = 10004,
    Web = 10005,
    Hacking = 10006,
    Games = 10007,
    Settings = 10008

}
