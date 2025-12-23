using System.Collections;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class OnboardingManager : MonoBehaviour
{
    // Completed Onboarding Steps
    public bool DEBUGGERY;
    public bool TORCHCOLLECTED;
    public bool NOTEPADCOLLECTED;
    public bool PHONECOLLECTED;
    public bool PHONEACCESSED;
    public bool TESTEVIDENCECOLLECTED;
    public bool ONBOARDINGCOMPLETE;
    
    [Header("Physical Objects in Noras Flat")]
    public GameObject phonePickup;
    public GameObject notepadPickup;
    public GameObject torchPickup;
    
    [Header("UI Elements")]
    public CanvasGroup phoneTick;
    public CanvasGroup notepadTick;
    public CanvasGroup torchTick;
    public CanvasGroup evidenceTick;
    
    
    [Header("Onboarding Dialogues")]
    // nora initially says she needs her stuff
    public DialogueName needMyThings = DialogueName.NoraNeedsHerThings;
    
    // Announces collection of Phone
    public DialogueName pickupPhoneDialogue = DialogueName.NoraCollectedPhone;
    // Announces collection of Torch
    public DialogueName pickupTorchDialogue = DialogueName.NoraCollectedTorch;
    // Announces collection of Notepad
    public DialogueName pickupNotepadDialogue = DialogueName.NoraCollectedNotepad;
    
    // Proclaims need for test evidence (do when trying to leave flat)
    public DialogueName needTestEvidence = DialogueName.NoraNeedsTestEvidence;
    
    // kieron suggests exploring phone when Nora takes it out of pocket for first time
    public DialogueName phoneTutorialGotPhone = DialogueName.KieronToNoraGotPhone;

    // kieron comments on Nora's first evidence
    public DialogueName phoneTutorialFirstEvidence = DialogueName.KieronToNoraFirstEvidence;

    // Nora proclaims she is ready to go.
    public DialogueName readyMessage = DialogueName.NoraReadyToGo;
        
    public Image MyFirstEvidence;
    public TextMeshProUGUI EvidenceDesc;
    void Awake()
    {
        
        
        
        ONBOARDINGCOMPLETE = PlayerPrefs.GetInt("ONBOARDINGCOMPLETE", 0) != 0;
        
        // If onboarding was previously marked as complete, we can go ahead and true the rest as these, canonically, all precede ONBOARDINGCOMPLETE
        if (DEBUGGERY || ONBOARDINGCOMPLETE)
        {
            TORCHCOLLECTED = true;
            NOTEPADCOLLECTED = true;
            PHONECOLLECTED = true;
            TESTEVIDENCECOLLECTED = true;
            PHONEACCESSED = true;
        }

        // 
        TORCHCOLLECTED = PlayerPrefs.GetInt("TORCHCOLLECTED", 0) != 0;
        NOTEPADCOLLECTED = PlayerPrefs.GetInt("NOTEPADCOLLECTED", 0) != 0;
        PHONECOLLECTED = PlayerPrefs.GetInt("PHONECOLLECTED", 0) != 0;
        TESTEVIDENCECOLLECTED = PlayerPrefs.GetInt("TESTEVIDENCECOLLECTED", 0) != 0;
        PHONEACCESSED = PlayerPrefs.GetInt("PHONEACCESSED", 0) != 0;
        
        
    }

    private void Start()
    {
        if (GameMaster.Instance.THISLEVEL == GameMaster.GAMELEVEL.NorasFlat)
        {

            if (PHONECOLLECTED) { Destroy(phonePickup); phoneTick.alpha = 1; } else { phoneTick.alpha = 0; }

            if (TORCHCOLLECTED) { Destroy(torchPickup); torchTick.alpha = 1; } else { torchTick.alpha = 0; }

            if (NOTEPADCOLLECTED) { Destroy(notepadPickup); notepadTick.alpha = 1; } else { notepadTick.alpha = 0; }

            if (TESTEVIDENCECOLLECTED) { evidenceTick.alpha = 1; } else { evidenceTick.alpha = 0; }
            

            // Todo: Refactor evidence levels 
            GameMaster.ExpectedEQThisLevel = GameMaster.ExpectedEQ_Level0;
                
            UpdateChalkboard();
        }
        
    }

    public void CollectTorch()
    {
        Debug.Log("CollectTorch");
        TORCHCOLLECTED = true;
        Destroy(torchPickup); 
        GameMaster.Instance.DialogueManager.NewDialogue(pickupTorchDialogue, 6);
        if (torchTick) torchTick.alpha = 1;
        PlayerPrefs.SetInt("TORCHCOLLECTED", TORCHCOLLECTED ? 1 : 0); PlayerPrefs.Save();
        CheckOnboardingStatus();
    }


    public void CollectNotepad()
    {
        Debug.Log("CollectNotepad");
        NOTEPADCOLLECTED = true;
        Destroy(notepadPickup); 
        GameMaster.Instance.DialogueManager.NewDialogue(pickupNotepadDialogue, 6);
        if (notepadTick) notepadTick.alpha = 1;
        PlayerPrefs.SetInt("NOTEPADCOLLECTED", NOTEPADCOLLECTED ? 1 : 0); PlayerPrefs.Save();
        
        CheckOnboardingStatus();
    }


    public void CollectPhone()
    {
        Debug.Log("CollectPhone");
        PHONECOLLECTED = true;
        Destroy(phonePickup); 
        GameMaster.Instance.DialogueManager.NewDialogue(pickupPhoneDialogue, 6);
        if (phoneTick) phoneTick.alpha = 1;
        PlayerPrefs.SetInt("PHONECOLLECTED", PHONECOLLECTED ? 1 : 0); PlayerPrefs.Save();
        
        CheckOnboardingStatus();
    }


    public void OpenedPhone()
    {
        Debug.Log("OpenedPhone");
        PHONEACCESSED = true;
        GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialGotPhone, 6);
        
        PlayerPrefs.SetInt("PHONEACCESSED", PHONEACCESSED ? 1 : 0); PlayerPrefs.Save();
        CheckOnboardingStatus();
    }



    public void CollectTestEvidence()
    {
        Debug.Log("CollectTestEvidence");
        TESTEVIDENCECOLLECTED = true;
        GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialFirstEvidence, 5);
        
        PlayerPrefs.SetInt("TESTEVIDENCECOLLECTED", TESTEVIDENCECOLLECTED ? 1 : 0); PlayerPrefs.Save();
        
        CheckOnboardingStatus();
    }


    public void EvidenceNotCollected()
    {
        GameMaster.Instance.DialogueManager.NewDialogue(needTestEvidence, 5);
    }

    public void NotReadyYet()
    {
        
        GameMaster.Instance.DialogueManager.NewDialogue(needMyThings, 5);
        
    }


    
    public void CheckOnboardingStatus()
    {
        Debug.Log("CheckOnboardingStatus");
        
        
        if (TORCHCOLLECTED && NOTEPADCOLLECTED && PHONECOLLECTED)
        {

            if (TESTEVIDENCECOLLECTED)
            {
                ONBOARDINGCOMPLETE = true;
                StartCoroutine(NoraReady());
                PlayerPrefs.SetInt("ONBOARDINGCOMPLETE", ONBOARDINGCOMPLETE ? 1 : 0);
                PlayerPrefs.Save();

                Debug.Log("ONBOARDINGCOMPLETE");
            }
        }

        UpdateChalkboard();
    }
    

    public void UpdateChalkboard()
    {
        Debug.Log("UpdateChalkboard");
        if (GameMaster.EvidenceFound.Count > 0)
        {
            Debug.Log("UpdateChalkboard EvidenceFound.Count > 0");
            // lets get the files
            var filepath = Application.persistentDataPath + "/Phone/0/Evidence/";


            DirectoryInfo dir = new DirectoryInfo(filepath);
            if (dir.Exists)
            {
                FileInfo[] info = dir.GetFiles("*.quack");
                var lines = System.IO.File.ReadAllLines(info[0].FullName);

                var photopath = Application.persistentDataPath + "/Phone/0/DCIM/";

                // Read image bytes
                byte[] imageData = File.ReadAllBytes(photopath + lines[1]);

                // Create new Texture2D (size will auto-resize)
                Texture2D tempTexture = new Texture2D(2, 2);
                tempTexture.LoadImage(imageData);

                // Convert Texture2D to Sprite for Image component
                Sprite newSprite = Sprite.Create(
                    tempTexture,
                    new Rect(0, 0, tempTexture.width, tempTexture.height),
                    new Vector2(0.5f, 0.5f)
                );

                // Assign Sprite to Image component
                MyFirstEvidence.GetComponent<Image>().sprite = newSprite;

                // Set description and tick
                EvidenceDesc.text = lines[5];
                evidenceTick.alpha = 1;
            }
        }
    }
    
        



    IEnumerator NoraReady()
    {
        yield return new WaitForSeconds(5);
        var msg = "Ok, I think I'm ready to go now.";

        GameMaster.Instance.DialogueManager.NewDialogue(readyMessage, 5);


    }

}

// File: OnboardingManagerEditor.cs
// Place in folder named "Editor"
#if UNITY_EDITOR

[CustomEditor(typeof(OnboardingManager))]
public class OnboardingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        OnboardingManager mgr = (OnboardingManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("::Onboarding Debug Tools", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Current Status", EditorStyles.miniBoldLabel);
        EditorGUILayout.Toggle("Torch Collected", mgr.TORCHCOLLECTED);
        EditorGUILayout.Toggle("Notepad Collected", mgr.NOTEPADCOLLECTED);
        EditorGUILayout.Toggle("Phone Collected", mgr.PHONECOLLECTED);
        EditorGUILayout.Toggle("Phone Accessed", mgr.PHONEACCESSED);
        EditorGUILayout.Toggle("Test Evidence Collected", mgr.TESTEVIDENCECOLLECTED);
        EditorGUILayout.Toggle("Onboarding Complete", mgr.ONBOARDINGCOMPLETE);

        EditorGUILayout.Space();

        if (GUILayout.Button("Test Step: Collect Torch"))
        {
            mgr.CollectTorch();
        }
        if (GUILayout.Button("Test Step: Collect Notepad"))
        {
            mgr.CollectNotepad();
        }
        if (GUILayout.Button("Test Step: Collect Phone"))
        {
            mgr.CollectPhone();
        }
        if (GUILayout.Button("Test Step: Open Phone"))
        {
            mgr.OpenedPhone();
        }
        if (GUILayout.Button("Test Step: Collect Test Evidence"))
        {
            mgr.CollectTestEvidence();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Reset Torch"))
        {
            PlayerPrefs.SetInt("TORCHCOLLECTED", 0);
        }
        if (GUILayout.Button("Reset Notepad"))
        {
            PlayerPrefs.SetInt("NOTEPADCOLLECTED", 0);
        }
        if (GUILayout.Button("Reset Phone"))
        {
            PlayerPrefs.SetInt("PHONECOLLECTED", 0);
        }
        if (GUILayout.Button("Reset Phone Accessed"))
        {
            PlayerPrefs.SetInt("PHONEACCESSED", 0);
        }
        if (GUILayout.Button("Reset Test Evidence"))
        {
            PlayerPrefs.SetInt("TESTEVIDENCECOLLECTED", 0);
        }
        if (GUILayout.Button("Reset Onboarding Complete"))
        {
            PlayerPrefs.SetInt("ONBOARDINGCOMPLETE", 0);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Reset All Onboarding PlayerPrefs"))
        {
            PlayerPrefs.DeleteKey("TORCHCOLLECTED");
            PlayerPrefs.DeleteKey("NOTEPADCOLLECTED");
            PlayerPrefs.DeleteKey("PHONECOLLECTED");
            PlayerPrefs.DeleteKey("PHONEACCESSED");
            PlayerPrefs.DeleteKey("TESTEVIDENCECOLLECTED");
            PlayerPrefs.DeleteKey("ONBOARDINGCOMPLETE");
            PlayerPrefs.Save();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
