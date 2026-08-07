using System;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [Tooltip("Invisible transform placed at the intended lying down position where the middle of the body would be.")]
    public Transform bedFootTransform;
    public Transform bedLyingTransform;

    private Renderer bedFootMarker;
    private Renderer bedBedMarker;
    
    [Tooltip("If true, NPCs must not lie here.")]
    public bool occupied;

    [Tooltip("Optional: tracks who is occupying to avoid races.")]
    [SerializeField] private NPCController occupiedBy;
    
    public bool IsOccupied => occupied || occupiedBy != null;


    private void Awake()
    {
        if (bedFootTransform != null)
        {
            bedFootMarker = bedFootTransform.gameObject.GetComponent<Renderer>();
        }

        bedBedMarker = bedLyingTransform.gameObject.GetComponent<Renderer>();
    }

    private void Start()
    {
        if (bedBedMarker != null) bedBedMarker.enabled = false;
        if (bedFootMarker != null) bedFootMarker.enabled = false;
    }


    public void NoraSleep()
    {
        if (!occupied)
        {
            
            Debug.Log("nora sleep");
            EventManager.NoraSleep(this);
        }
    }


    public bool TryOccupy(NPCController who)
    {
        if (who == null) return false;
        if (IsOccupied) return false;

        occupiedBy = who;
        occupied = true;
        return true;
    }

    public void Release(NPCController who)
    {
        if (occupiedBy != null)
        {
            occupiedBy = null;
            occupied = false;
        }
    }
}