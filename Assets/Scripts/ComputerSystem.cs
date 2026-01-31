using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    public bool IsLoggedIn;
    public TextMeshProUGUI PasswordSource;
    public TMP_InputField PasswordInput;

    public Transform ViewableArea;

    void Start()
    {
        IncorrectPasswordText.gameObject.SetActive(false);
        EventManager.OnStopComputer += OnStopComputer;
    }


    public void OnStartComputer()
    {
        Debug.Log("StartPC");
        NearestChair.gameObject.SetActive(false);
        GameMaster.Instance.INMENU = true;
        GameMaster.Instance.FROZEN = true;
        GameMaster.Instance.ONPC = true;
        GameMaster.Instance.EventManager.StartComputer(ViewableArea);
    }


    public void OnStopComputer()
    {
        Debug.Log("StopPC");
        NearestChair.gameObject.SetActive(true);
        GameMaster.Instance.INMENU = false;
        GameMaster.Instance.FROZEN = false;
        GameMaster.Instance.ONPC = false;
    }


    

    public void LogOn()
    {
        if (PasswordInput.text.Trim().Equals(PasswordSource.text.Trim()))
        {
            ChangeScreen(ComputerScreen.Desktop);
        }
        else
        {
            IncorrectPasswordText.gameObject.SetActive(true);
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