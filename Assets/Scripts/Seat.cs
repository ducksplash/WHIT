using System;
using UnityEngine;

public class Seat : MonoBehaviour
{
    [Tooltip("Invisible transform placed at the intended hips/butt position (eg: on a chair).")]
    public Transform seatTransform;

    [Tooltip("If true, NPCs must not sit here.")]
    public bool occupied;

    [Tooltip("Optional: tracks who is occupying to avoid races.")]
    [SerializeField] private NPCController occupiedBy;
    
    private Renderer SeatMarker;
    
    public bool IsValid => seatTransform != null;

    public bool IsOccupied => occupied || occupiedBy != null;


    private void Awake()
    {
        
        SeatMarker = seatTransform.gameObject.GetComponent<Renderer>();
    }

    private void Start()
    {
        if (SeatMarker != null) SeatMarker.enabled = false;
    }

    public bool TryOccupy(NPCController who)
    {
        if (who == null) return false;
        if (!IsValid) return false;
        if (IsOccupied) return false;

        occupiedBy = who;
        occupied = true;
        return true;
    }

    public void Release(NPCController who)
    {
        if (who != null && occupiedBy == who)
        {
            occupiedBy = null;
        }

        // Keep bool as source-of-truth for designers, but clear it if we were the owner.
        if (occupiedBy == null) occupied = false;
    }
}