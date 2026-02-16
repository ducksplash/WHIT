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

    private bool _unhooked;

    private void OnEnable()
    {
        HookEvents();

        if (AddressBarText != null)
            AddressBarRoot = AddressBarText.text;

        if (FileManagerNavver != null)
            FileManagerNavver.enabled = false;
    }

    private void OnDisable()
    {
        UnhookEvents();
    }

    private void OnDestroy()
    {
        UnhookEvents();
    }

    private void HookEvents()
    {
        UnhookEvents();
        _unhooked = false;

        TerminalEventManager.OnFileManagerStarted += FileManagerStarted;
        TerminalEventManager.OnFileManagerClosed += FileManagerClosed;
        TerminalEventManager.OnPhotoManagerClosed += PhotoManagerClosed;

        // Don’t subscribe to OnFileGridClick until FileManagerStarted
        TerminalEventManager.OnFileGridClick -= SelectFolderItem;
    }

    private void UnhookEvents()
    {
        if (_unhooked) return;
        _unhooked = true;

        TerminalEventManager.OnFileManagerStarted -= FileManagerStarted;
        TerminalEventManager.OnFileManagerClosed -= FileManagerClosed;
        TerminalEventManager.OnPhotoManagerClosed -= PhotoManagerClosed;

        TerminalEventManager.OnFileGridClick -= SelectFolderItem;
    }

    private void FileManagerStarted()
    {
        TerminalEventManager.OnFileGridClick -= SelectFolderItem;
        TerminalEventManager.OnFileGridClick += SelectFolderItem;

        ChangeScreen(FolderScreen.User);

        if (FileManagerNavver != null)
            FileManagerNavver.enabled = true;

        SetAddressBar("Nora");
    }

    private void FileManagerClosed()
    {
        if (FileManagerNavver != null)
            FileManagerNavver.enabled = false;

        TerminalEventManager.OnFileGridClick -= SelectFolderItem;
    }

    private void PhotoManagerClosed()
    {
        // Returning from Photos viewer/manager back to Files:
        if (FileManagerNavver != null)
            FileManagerNavver.enabled = true;

        // ✅ This was “-=” in your original; that means clicks stop working after returning.
        TerminalEventManager.OnFileGridClick -= SelectFolderItem;
        TerminalEventManager.OnFileGridClick += SelectFolderItem;
    }

    public void SelectFolderItem(FileGriddle fileGriddle)
    {
        Debug.Log("GO TO " + fileGriddle);

        if (GameMaster.Instance != null && GameMaster.Instance.TerminalEventManager != null)
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
                if (FileManagerNavver != null) FileManagerNavver.enabled = false;
                GameMaster.Instance.TerminalEventManager.PhotoManagerStarted();
                break;

            case FileGriddle.Videos:
                ChangeScreen(FolderScreen.Videos);
                GameMaster.Instance.TerminalEventManager.VideoManagerStarted();
                if (FileManagerNavver != null) FileManagerNavver.enabled = false;
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
        if (AddressBarText == null) return;

        // Path.Combine uses backslashes on Windows; if you want UI-friendly always-forward-slash,
        // replace with $"{AddressBarRoot}/{folderAddress}".
        AddressBarText.text = Path.Combine(AddressBarRoot, folderAddress);
    }

    public void ChangeScreen(FolderScreen ScreenToOpen)
    {
        CurrentFolder = (FileGriddle)ScreenToOpen;

        CloseAllScreens();

        if (AllFolderScreens == null) return;

        // ✅ Remove destroyed entries after scene reload
        AllFolderScreens.RemoveAll(cg => cg == null);

        for (int i = 0; i < AllFolderScreens.Count; i++)
        {
            var cg = AllFolderScreens[i];
            if (cg == null) continue;

            var screen = cg.GetComponent<FileManagerScreen>();
            if (screen != null && screen.ThisFileFolder.Equals(ScreenToOpen))
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                break;
            }
        }
    }

    public void CloseAllScreens()
    {
        if (AllFolderScreens == null) return;

        // ✅ Remove destroyed entries after scene reload
        AllFolderScreens.RemoveAll(cg => cg == null);

        for (int i = 0; i < AllFolderScreens.Count; i++)
        {
            var cg = AllFolderScreens[i];
            if (cg == null) continue;

            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    public void CloseToDesktop()
    {
        if (GameMaster.Instance != null && GameMaster.Instance.TerminalEventManager != null)
            GameMaster.Instance.TerminalEventManager.CloseToDesktop();
    }

    public void FolderStepBack()
    {
        if (FileManagerNavver != null)
            FileManagerNavver.enabled = true;

        if (CurrentFolder != FileGriddle.User)
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
