using UnityEngine;

public class Evidence : MonoBehaviour
{
    public bool EvidenceCollected;
    public bool EvidenceFake;
    public bool PlayerSeen;
    public bool PhotographableEvidence;


    public string EvidenceName;
    public string EvidenceBody;
    public Transform EvidenceTransform;
    public Rigidbody EvidenceRigidbody;
    public Renderer EvidenceRenderer;


    private void Start()
    {
        EvidenceTransform = transform;
        EvidenceRigidbody = GetComponent<Rigidbody>();
        EvidenceRenderer = GetComponent<Renderer>();

        EvidenceName = "Purchase Order #2349080";
        EvidenceBody = "Deuterium 24,000 Litres\n";

    }










}
