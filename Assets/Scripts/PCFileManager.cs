using System;
using System.Collections.Generic;
using UnityEngine;

public class PCFileManager : MonoBehaviour
{
    
    public FileGriddle CurrentFolder = FileGriddle.User;
    public List<CanvasGroup> AllFolderScreens;
    public List<FileNavver> FileNavvers = new List<FileNavver>();


    private void Start()
    {
        DisableNavvers();
    }

    private void OnEnable()
    {
        
    }
    
    
    
    
    public void SelectFolderItem(FileGriddle fileGriddle)
    {
        Debug.Log("GO TO "+fileGriddle);
        
        switch (fileGriddle)
        {

            case FileGriddle.User: 
                // do nothing
                break;
            case FileGriddle.Documents: break;
            
            
            case FileGriddle.Photos:
                //ChangeScreen(ComputerScreen.Files);
                break;

            case FileGriddle.Videos:

                break;
            
            case FileGriddle.Audio:

                break;

            case FileGriddle.Downloads:

                break;
            

            case FileGriddle.Evidence:

                break;
            

            case FileGriddle.O:

                break;

            case FileGriddle.Phone:

                break;
            case FileGriddle.DCIM:

                break;

        }
        
    }


    public void ChangeScreen(FolderScreen ScreenToOpen)
    {
        // ✅ keep CurrentFolder in sync with the visible folder screen
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

                FileManagerScreen thisScreen = AllFolderScreens[i].GetComponent<FileManagerScreen>();
                thisScreen.FileNavver.enabled = true;
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
            
            FileManagerScreen thisScreen = AllFolderScreens[i].GetComponent<FileManagerScreen>();
            
            thisScreen.FileNavver.enabled = false;

            Debug.Log("Closed: "+AllFolderScreens[i].gameObject.name);

        }
    }

    
    
    private void DisableNavvers()
    {
        for (var i = 0; i < FileNavvers.Count; i++)
        {
            FileNavvers[i].enabled = false;
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
