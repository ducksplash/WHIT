using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Type")]
    public DoorType DoorType = DoorType.Standard;

    [Header("State")]
    [SerializeField] private bool isOpen;
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isAnimating;

    [Header("Animation (Optional)")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private float colliderDisableTime = 0.5f;

    [Header("Indicator Lights (Optional)")]
    [SerializeField] private List<Renderer> indicatorLights = new List<Renderer>();

    static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");

    private void Awake()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponentInChildren<Animator>();

        UpdateIndicatorLights();
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
        if (isLocked)
            return;

        ToggleDoor(hitCollider);
    }

    private void HandleTravelDoor()
    {
        // Matches old InnerDoors ExteriorDoor behaviour
        GameMaster.Instance.TravelCompanion.LaunchCompanion();
    }


    private void ToggleDoor(Collider hitCollider)
    {
        isAnimating = true;
        isOpen = !isOpen;

        if (doorAnimator != null)
        {
            doorAnimator.ResetTrigger("opened");
            doorAnimator.ResetTrigger("closed");

            doorAnimator.SetTrigger(isOpen ? "opened" : "closed");
        }

        if (hitCollider != null)
            StartCoroutine(DisableColliderTemporarily(hitCollider));

        StartCoroutine(AnimationCooldown());
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

        if (doorAnimator != null)
            doorAnimator.SetTrigger("idle");
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

    // Launch Travel Companion for this door type
    Travel
}
