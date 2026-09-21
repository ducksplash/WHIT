using System;
using System.Collections.Generic;
using UnityEngine;

public class GateBackbone : MonoBehaviour
{
    public static GateBackbone Instance;

    [Header("Gates")]
    public List<int> GateIDs = new List<int>();
    public List<Color> UsedColours = new List<Color>();
    public List<int> AllocatedGateIDs = new List<int>();

    private Dictionary<int, MetalGate> gates = new Dictionary<int, MetalGate>();
    private int nextGateID = 0;

    private static readonly Dictionary<GateColours, Color> ColourMap = new Dictionary<GateColours, Color>
    {
        { GateColours.Red, Color.red },
        { GateColours.Green, Color.green },
        { GateColours.Blue, Color.blue },
        { GateColours.Yellow, Color.yellow },
        { GateColours.Purple, new Color(0.5f, 0f, 0.5f) },
        { GateColours.Cyan, Color.cyan },
        { GateColours.LightBlue, new Color(0.68f, 0.85f, 0.9f) },
        { GateColours.DarkGreen, new Color(0f, 0.39f, 0f) },
        { GateColours.Maroon, new Color(0.5f, 0f, 0f) },
        { GateColours.White, Color.white }
    };

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterGate(MetalGate gate)
    {
        Debug.Log("RegisterGate");
        if (gate == null) return;

        int id = nextGateID++;

        gate.GateID = id;

        GateIDs.Add(id);
        gates.Add(id, gate);

        Color assignedColour = GetNextAvailableColour();
        UsedColours.Add(assignedColour);

        gate.SetGateColour(assignedColour);
    }

    public MetalGate RegisterAccessPanel()
    {
        Debug.Log("RegisterAccessPanel");
        foreach (int id in GateIDs)
        {
            if (!AllocatedGateIDs.Contains(id))
            {
                AllocatedGateIDs.Add(id);
                return gates[id];
            }
        }

        return null;
    }

    private Color GetNextAvailableColour()
    {
        foreach (GateColours colourEnum in Enum.GetValues(typeof(GateColours)))
        {
            Color candidate = ColourMap[colourEnum];
            if (!UsedColours.Contains(candidate))
            {
                return candidate;
            }
        }

        return Color.black;
    }
}

public enum GateColours
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Cyan,
    LightBlue,
    DarkGreen,
    Maroon,
    White
}