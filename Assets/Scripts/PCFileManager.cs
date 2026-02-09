using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class PCFileManager : MonoBehaviour
{
    
    public FileGriddle CurrentFolder = FileGriddle.User;
    public List<CanvasGroup> AllFolderScreens;
    public FileNavver FileManagerNavver;
    public TextMeshProUGUI AddressBarText;
    public string AddressBarRoot;


    void Start()
    {
        AddressBarRoot = AddressBarText.text;
        FileManagerNavver.enabled = false;
        TerminalEventManager.OnFileManagerStarted += FileManagerStarted;
        TerminalEventManager.OnFileManagerClosed += FileManagerClosed;
    }
    
    

    private void FileManagerStarted()
    {
        FileManagerNavver.enabled = true;
        TerminalEventManager.OnFileGridClick += SelectFolderItem;
        ChangeScreen(FolderScreen.User); 
    }


    private void FileManagerClosed()
    {
        FileManagerNavver.enabled = false;
        TerminalEventManager.OnFileGridClick -= SelectFolderItem;
    }

    
    
    public void SelectFolderItem(FileGriddle fileGriddle)
    {
        Debug.Log("GO TO "+fileGriddle);
        
        
        GameMaster.Instance.TerminalEventManager.VideoManagerClosed();
        
        
        SetAddressBar(fileGriddle == FileGriddle.User ? "Nora" : fileGriddle.ToString());
                
        switch (fileGriddle)
        {

            case FileGriddle.User: 
                ChangeScreen(FolderScreen.User);
                break;
            
            case FileGriddle.Documents:
                ChangeScreen(FolderScreen.Documents); 
                break;
            
            case FileGriddle.Photos:
                ChangeScreen(FolderScreen.Photos);
                break;

            case FileGriddle.Videos:
                ChangeScreen(FolderScreen.Videos);
                GameMaster.Instance.TerminalEventManager.VideoManagerStarted();
                FileManagerNavver.enabled = false;
                break;
            
            case FileGriddle.Audio:
                ChangeScreen(FolderScreen.Audio);
                break;

            case FileGriddle.Downloads:
                ChangeScreen(FolderScreen.Downloads);
                break;
            
            case FileGriddle.Evidence:
                ChangeScreen(FolderScreen.Evidence);
                break;

            case FileGriddle.O:
                ChangeScreen(FolderScreen.O);
                break;

            case FileGriddle.Phone:
                ChangeScreen(FolderScreen.Phone);
                break;
            
            case FileGriddle.DCIM:
                ChangeScreen(FolderScreen.DCIM);
                break;

        }
        
    }


    private void SetAddressBar(string folderAddress)
    {
        AddressBarText.text =  Path.Combine(AddressBarRoot,folderAddress);
            

    }

    public void ChangeScreen(FolderScreen ScreenToOpen)
    {
        CurrentFolder = (FileGriddle)ScreenToOpen;

        CloseAllScreens();

        for (var i = 0; i < AllFolderScreens.Count; i++)
        {
            if (AllFolderScreens[i].GetComponent<FileManagerScreen>().ThisFileFolder.Equals(ScreenToOpen))
            {
                CanvasGroup SelectedScreen = AllFolderScreens[i].GetComponent<CanvasGroup>();

                SelectedScreen.alpha = 1;
                SelectedScreen.interactable = true;
                SelectedScreen.blocksRaycasts = true;

                // FileManagerScreen thisScreen = AllFolderScreens[i].GetComponent<FileManagerScreen>();
            }
        }
    }


    public void CloseAllScreens()
    {
        for (var i = 0; i < AllFolderScreens.Count; i++)
        {
            CanvasGroup ThisScreen = AllFolderScreens[i].GetComponent<CanvasGroup>();
            
            ThisScreen.alpha = 0;
            ThisScreen.interactable = false;
            ThisScreen.blocksRaycasts = false;
            
            // FileManagerScreen thisScreen = AllFolderScreens[i].GetComponent<FileManagerScreen>();
            // Debug.Log("Closed: "+AllFolderScreens[i].gameObject.name);

        }
    }


    public void CloseToDesktop()
    {
        GameMaster.Instance.TerminalEventManager.CloseToDesktop();
    }

    

    public void FolderStepBack()
    {
        FileManagerNavver.enabled = true;
        
        if (CurrentFolder.ToString() != FileGriddle.User.ToString())
        {
            ChangeScreen(FolderScreen.User);
        }
        else
        {
            CloseToDesktop();
        }
    }

    
    
}



public enum FileGriddle
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


public enum FolderScreen
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
