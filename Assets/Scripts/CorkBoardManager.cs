using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class CorkBoardManager : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup phoneTick;
    public CanvasGroup notepadTick;
    public CanvasGroup torchTick;
    public CanvasGroup evidenceTick;
    
    public Image MyFirstEvidence;
    public TextMeshProUGUI EvidenceDesc;
    
    
    
    void Awake()
    {
        EventManager.OnUpdateCorkboard += UpdateCorkboard;
    }

    public void UpdateCorkboard()
    {

        Debug.Log("UPDATE CORKBOARD!!");
        
        if (phoneTick != null) phoneTick.alpha = GameMaster.Instance.OnboardingManager.PHONECOLLECTED ? 1 : 0;

        if (torchTick != null) torchTick.alpha = GameMaster.Instance.OnboardingManager.TORCHCOLLECTED ? 1 : 0;

        if (notepadTick != null) notepadTick.alpha = GameMaster.Instance.OnboardingManager.NOTEPADCOLLECTED ? 1 : 0;

        if (evidenceTick != null) evidenceTick.alpha = GameMaster.Instance.OnboardingManager.TESTEVIDENCECOLLECTED ? 1 : 0;
        
        
        EvidenceName evidenceName = GameMaster.Instance.OnboardingManager.FirstOnboardingEvidence;

        if (!GameMaster.Instance.EvidenceManager.EvidenceFound.TryGetValue(evidenceName, out string quackPath))
        {
            //Debug.Log($"[Onboarding] Evidence not collected yet: {evidenceName}");
            return;
        }

        if (!File.Exists(quackPath))
        {
            Debug.LogError($"[Onboarding] Evidence file missing: {quackPath}");
            return;
        }

        string[] lines = File.ReadAllLines(quackPath);

        if (lines.Length < 6)
        {
            Debug.LogError($"[Onboarding] Malformed .quack file: {quackPath}");
            return;
        }

        EvidenceRecord record = new EvidenceRecord
        {
            Name = lines[0],
            Photo = lines[1],
            DateFound = lines[2],
            IsFake = bool.TryParse(lines[3], out var fake) && fake,
            Quality = int.TryParse(lines[4], out var q) ? q : 0,
            Details = lines[5]
        };

        string photoPath = Path.Combine(
            Application.persistentDataPath,
            "Phone/0/DCIM",
            record.Photo
        );

        //Debug.Log($"[Onboarding] Loading image: {photoPath}");

        if (!File.Exists(photoPath))
        {
            Debug.LogError($"[Onboarding] Evidence image missing: {photoPath}");
            return;
        }

        byte[] bytes = File.ReadAllBytes(photoPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(bytes);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        MyFirstEvidence.sprite = sprite;
        MyFirstEvidence.color = Color.white;
        MyFirstEvidence.enabled = true;

        EvidenceDesc.text = record.Details;
        evidenceTick.alpha = 1;

        //Debug.Log("[Onboarding] Chalkboard updated successfully");
    }


}
