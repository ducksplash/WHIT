using System.Collections.Generic;
using UnityEngine;

public class MetalGate : MonoBehaviour
{
    public int GateID = -1;

    public string MacAddress;

    public GameObject MetalBarsLeftOpenRend;
    public GameObject MetalBarsRightOpenRend;
    public GameObject MetalBarsLeftClosedRend;
    public GameObject MetalBarsRightClosedRend;

    public Color thisGateColor;

    public List<MeshRenderer> GateMaterials = new List<MeshRenderer>();

    private bool gateLocked = true;

    private static readonly int EmissiveColorProperty = Shader.PropertyToID("_EmissiveColor");

    
    private void Start()
    {
        GateBackbone.Instance.RegisterGate(this);
        EventManager.OnKelliUnlockGate += ToggleGate;

        MacAddress = "29:3A:B1:D8:ED:" + GateID.ToString("00");

        SetGateState(gateLocked);
    }

    private void OnDestroy()
    {
        EventManager.OnKelliUnlockGate -= ToggleGate;
    }

    public void SetGateColour(Color AssignedColour)
    {
        thisGateColor = AssignedColour;

        foreach (var rend in GateMaterials)
        {
            if (rend == null) continue;

            Material mat = rend.material;
            if (mat.HasProperty(EmissiveColorProperty))
            {
                mat.SetColor(EmissiveColorProperty, AssignedColour);
            }
        }
    }

    public void ToggleGate(string SuppliedGateID)
    {
        if (!SuppliedGateID.Equals(MacAddress)) return;

        gateLocked = !gateLocked;
        SetGateState(gateLocked);
    }

    private void SetGateState(bool locked)
    {
        if (locked)
        {
            CloseGate();
        }
        else
        {
            OpenGate();
        }
    }

    private void OpenGate()
    {
        if (MetalBarsLeftOpenRend != null) MetalBarsLeftOpenRend.SetActive(true);
        if (MetalBarsRightOpenRend != null) MetalBarsRightOpenRend.SetActive(true);
        if (MetalBarsLeftClosedRend != null) MetalBarsLeftClosedRend.SetActive(false);
        if (MetalBarsRightClosedRend != null) MetalBarsRightClosedRend.SetActive(false);
    }

    private void CloseGate()
    {
        if (MetalBarsLeftOpenRend != null) MetalBarsLeftOpenRend.SetActive(false);
        if (MetalBarsRightOpenRend != null) MetalBarsRightOpenRend.SetActive(false);
        if (MetalBarsLeftClosedRend != null) MetalBarsLeftClosedRend.SetActive(true);
        if (MetalBarsRightClosedRend != null) MetalBarsRightClosedRend.SetActive(true);
    }
}