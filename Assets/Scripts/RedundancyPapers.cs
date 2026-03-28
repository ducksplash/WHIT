using System;
using UnityEngine;

public class RedundancyPapers : MonoBehaviour
{
    public Animator papersAni;
    private static readonly int SlidePapersTrigger = Animator.StringToHash("SlidePapers");


    private void Start()
    {
        DirectorEvents.OnSlidePapers += SlidePapers;
    }


    private void SlidePapers()
    {
        Debug.Log("slide papers");
        
        papersAni.SetTrigger(SlidePapersTrigger);
    }
}
