using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Type")]
    public DoorType DoorType = DoorType.Standard;

    [Header("State")]
    public bool isOpen;
    public bool isLocked;
    [SerializeField] private bool isAnimating;

    [Header("Animation (Optional)")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private float colliderDisableTime = 0.5f;

    [Header("Indicator Lights (Optional)")]
    [SerializeField] private List<Renderer> indicatorLights = new List<Renderer>();

    static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");

    [Header("optional - use with InstantTravel")]
    public GAMELEVEL DestinationLevel;
    
    public bool openOnLoad;
    
    private void Awake()
    {
        if (doorAnimator == null) doorAnimator = GetComponentInChildren<Animator>();
        UpdateIndicatorLights();
        
        if (openOnLoad) ToggleDoor();
    }
    
    public void TryUseDoor(Collider hitCollider = null)
    {
        if (isAnimating)
            return;

        switch (DoorType)
        {
            case DoorType.Travel:
                HandleTravelDoor();
                break;

            case DoorType.Standard:
            default:
                HandleStandardDoor(hitCollider);
                break;

            case DoorType.Sliding:
                // TODO: Sliding door logic
                break;

            case DoorType.Lift:
                // TODO: Lift / elevator logic
                break;

            case DoorType.Cupboard:
                HandleStandardDoor(hitCollider);
                break;
            case DoorType.InstantTravel:
                Debug.Log("inst?");
                GameMaster.Instance.LoadingManager.LoadLevel(DestinationLevel);
                break;
        }
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateIndicatorLights();
    }

    public bool IsLocked() => isLocked;
    public bool IsOpen() => isOpen;
    

    private void HandleStandardDoor(Collider hitCollider)
    {
        
        
        if (isLocked) return;
        ToggleDoor(hitCollider);
    }

    private void HandleTravelDoor()
    {
        // Matches old InnerDoors ExteriorDoor behaviour
        GameMaster.Instance.TravelCompanion.LaunchCompanion();
    }


    private void ToggleDoor(Collider hitCollider = null)
    {
        isAnimating = true;
        isOpen = !isOpen;

        if (doorAnimator != null)
        {
            doorAnimator.ResetTrigger("opened");
            doorAnimator.ResetTrigger("closed");

            
            MakeDoorNoises(isOpen);
            
            doorAnimator.SetTrigger(isOpen ? "opened" : "closed");
            
            
        }

        if (hitCollider != null) StartCoroutine(DisableColliderTemporarily(hitCollider));

        StartCoroutine(AnimationCooldown());
    }



    private void MakeDoorNoises(bool openValue)
    {
        switch (DoorType)
        {
            // assuming standard door is wooden
            case DoorType.Standard:
                GameMaster.Instance.AudioSlave.PlaySFX(openValue ? SFXResource.DoorOpen : SFXResource.DoorClosed);
                break;

            case DoorType.Sliding:
                Debug.Log("dont have a Sliding Door noise yet");
                break;

            case DoorType.Lift:
                Debug.Log("dont have a Lift Door noise yet");
                break;
            
            case DoorType.Fridge:
                Debug.Log("dont have a Fridge Door noise yet");
                break;
            
            case DoorType.Metal:
                GameMaster.Instance.AudioSlave.PlaySFX(openValue ? SFXResource.MetalDoorOpen : SFXResource.MetalDoorClosed);
                break;
            

        }
    }
    
    private IEnumerator DisableColliderTemporarily(Collider col)
    {
        col.enabled = false;
        yield return new WaitForSeconds(colliderDisableTime);
        col.enabled = true;
    }

    private IEnumerator AnimationCooldown()
    {
        yield return new WaitForSeconds(colliderDisableTime);
        isAnimating = false;

        if (doorAnimator != null) doorAnimator.SetTrigger("idle");
    }


    private void UpdateIndicatorLights()
    {
        if (indicatorLights == null || indicatorLights.Count == 0)
            return;

        Color color = isLocked
            ? new Color(0.5f, 0f, 0f, 1f)   // red
            : new Color(0f, 0.7f, 0f, 1f); // green

        foreach (var rend in indicatorLights)
        {
            if (!rend) continue;

            foreach (var mat in rend.materials)
            {
                if (!mat) continue;

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);

                if (mat.HasProperty(EmissiveColor))
                {
                    mat.SetColor(EmissiveColor, color * 20f);
                    mat.EnableKeyword("_EMISSIVE_COLOR");
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
    }
}

public enum DoorType
{
    Standard,
    Sliding,
    Lift,
    Cupboard,
    Fridge,
    Metal,

    // Launch Travel Companion for this door type
    Travel,
    
    // Load immediately
    InstantTravel
}
