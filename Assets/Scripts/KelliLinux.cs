using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KelliLinux : MonoBehaviour
{

    
    [Header("GRUB Loader")]
    public GameObject grubScreen; // grub loader 
    // has an image which does not need to be touched.
    
    [Header("Boot Screen")]
    public GameObject bootScreen; // boot screen
    public TextMeshProUGUI bootSequenceText;
    public List<string> bootSequenceStrings = new List<string>();
    
    [Header("Start Screen")]
    public GameObject startScreen; // launching Kelli Linux

    
    // Scrolls some text upward; three lines, we'll just inject them and align with bottom to simulate a 'terminal' like experience.
    
    [Header("Scanning Screen")]
    public GameObject connectingScreen; // search for device
    public Button hackButton;
    public List<string> scanningSequenceStrings = new List<string>();
    public TextMeshProUGUI scanSequenceText;
    public TextMeshProUGUI scanDeviceFinding;
    public TextMeshProUGUI scanDeviceMac;
    
    [Header("Challenge Screen")]
    public GameObject challengeScreen; // hacking minigame takes place proper

    
    public TextMeshProUGUI foundDeviceResultText;
    public string UnlockedResult;
    public string ErrorResult;
    
    
    private Coroutine StartUpCo;
    private Coroutine FinishUpCo;


    [SerializeField] private DoorAccessPanel FoundDevice;
    
    

    private void Start()
    {
        EventManager.OnKelliStarted += KelliEnable;
        EventManager.OnKelliStopped += KelliDisable;
        EventManager.OnKelliFoundDevice += RegisterDevice;
        EventManager.OnKelliLostDevice += UnRegisterDevice;
        
    }


    private void RegisterDevice(DoorAccessPanel deviceFound)
    {
        FoundDevice = deviceFound;
        Debug.Log("DeviceFound");
    }

    private void UnRegisterDevice()
    {
        FoundDevice = null;
    }

    

    private void KelliEnable()
    {
        grubScreen.SetActive(true);
        StartKelli();
        hackButton.enabled = false;
        hackButton.onClick.AddListener(StartHack);
    }

    private void KelliDisable()
    {
        grubScreen.SetActive(true);
        
        if (StartUpCo != null)
        {
            StopCoroutine(StartUpCo);
            StartUpCo = null;
        }            
        
        hackButton.enabled = false;
        hackButton.onClick.RemoveListener(StartHack);
    }


    private void StartKelli()
    {
        if (StartUpCo != null)
        {
            StopCoroutine(StartUpCo);
            StartUpCo = null;
        }

        StartUpCo = StartCoroutine(StartingSequence());
    }
    
    
    private IEnumerator StartingSequence()
    {
        scanDeviceMac.text = "...";
        yield return new WaitForSecondsRealtime(1);
        grubScreen.SetActive(false);
        bootScreen.SetActive(true);
        
        yield return new WaitForSecondsRealtime(0.1f);
        
        foreach (string bootString in bootSequenceStrings)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            bootSequenceText.text += "\n" + bootString;
        }
        
        yield return new WaitForSecondsRealtime(1);
        
        bootScreen.SetActive(false);
        startScreen.SetActive(true);
        
        yield return new WaitForSecondsRealtime(2);
        
        startScreen.SetActive(false);
        connectingScreen.SetActive(true);

        yield return new WaitForSecondsRealtime(1);

        scanDeviceFinding.text = "Please Wait";
        
        foreach (string scanString in scanningSequenceStrings)
        {
            scanSequenceText.text = scanString;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        
        yield return new WaitForSecondsRealtime(0.5f);


        if (FoundDevice != null)
        {
            scanDeviceFinding.text = "Device Found!";
            scanDeviceMac.text = FoundDevice.MacAddress;
            hackButton.enabled = true;
        }
        else
        {
            scanDeviceFinding.text = "No Devices Found";
            scanDeviceMac.text = "...";
            hackButton.enabled = false;
        }
        
        scanSequenceText.text = "";
    }

    public void ResetKelli()
    {
        scanDeviceMac.text = "...";
        scanDeviceFinding.text = "Please Wait";
        bootSequenceText.text = "";
        scanSequenceText.text = "";
        
        grubScreen.SetActive(true);
        bootScreen.SetActive(false);
        startScreen.SetActive(false);
        connectingScreen.SetActive(false);
        challengeScreen.SetActive(false);
    }


    private void StartHack()
    {


        
        connectingScreen.SetActive(false);
        challengeScreen.SetActive(true);
        
        // start hacking minigame - later fs
        if (!FoundDevice.isBroken)
        {
            FoundDevice.UnlockDoor();
            foundDeviceResultText.text = UnlockedResult;
        }
        else
        {
            foundDeviceResultText.text = ErrorResult;
        }
        
        if (FinishUpCo != null)
        {
            StopCoroutine(FinishUpCo);
            FinishUpCo = null;
        }

        FinishUpCo = StartCoroutine(FinishUp());

    }


    private IEnumerator FinishUp()
    {
        yield return new WaitForSeconds(1f);
        Player.Instance.PlayerPhone.PutAwayPhone();
    }
    
    
    
    
    
    
    
    
    

}
