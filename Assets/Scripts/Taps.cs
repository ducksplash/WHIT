using UnityEngine;

public class Taps : MonoBehaviour
{
    [SerializeField] private GameObject TapWaterParticles;

    [SerializeField] private bool StartRunning;


    void Start()
    {
        TapWaterParticles.SetActive(StartRunning);
    }
    
    
    public void ToggleTaps()
    {
        TapWaterParticles.SetActive(!TapWaterParticles.activeSelf);
    }
    
    
}
