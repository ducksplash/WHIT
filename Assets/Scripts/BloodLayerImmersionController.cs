using System;
using UnityEngine;

public class BloodLayerImmersionController : MonoBehaviour
{
    public Renderer BloodRenderer;
    public string CamTag = "MainCamera";

    private static readonly int ZTestModeID = Shader.PropertyToID("_ZTestMode");

    private const float ZTestLessEqual = 4f;
    private const float ZTestAlways = 8f;

    private Material bloodMaterial;

    private void Awake()
    {
        bloodMaterial = BloodRenderer.material;
    }

    private void Start()
    {
        EventManager.OnDeath += StopAllAndReset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(CamTag)) return;
        Debug.Log("camera entered trigger collider");
        SetZTest(ZTestAlways);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(CamTag)) return;
        EventManager.KillPlayer(Death.DrownedInBlood);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(CamTag)) return;
        Debug.Log("camera exited trigger collider");
        SetZTest(ZTestLessEqual);
    }

    private void SetZTest(float zTestValue)
    {
        bloodMaterial.SetFloat(ZTestModeID, zTestValue);
    }
    
    private void StopAllAndReset()
    {
        SetZTest(ZTestLessEqual);
    }

}