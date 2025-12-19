using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public List<WorldLight> LightList = new List<WorldLight>();
    [SerializeField] private Material[] switchMats;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        switchMats = rend.materials; // instance materials
    }

    public void ToggleLightswitch()
    {
        foreach (WorldLight LightBank in LightList)
        {
            LightBank.ToggleLight();
        }

        // Pass the on/off state of the first light to change the switch visuals
        changeSwitch(LightList[0].gameObject.GetComponent<WorldLight>().lightOn);
    }

    void changeSwitch(bool onState)
    {
        Color lightEmissionColour = onState ? new Color(0, 0.7f, 0, 1) : new Color(0.5f, 0, 0, 1);

        for (int i = 0; i < switchMats.Length; i++)
        {
            Material mat = switchMats[i];
            string matName = mat.name.Replace(" (Instance)", ""); // remove instance suffix

            if (matName.Contains("neon") || matName.Contains("led") || matName.Contains("bulb") || matName.Contains("diffuser"))
            {
                if (mat.HasProperty("_EmissiveColor"))
                {
                    mat.SetColor("_EmissiveColor", lightEmissionColour * 5f); // emissive intensity
                    mat.EnableKeyword("_EMISSION");
                    mat.EnableKeyword("_EMISSIVE_COLOR");
                }

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", lightEmissionColour);
            }
        }

        // Reassign materials back to renderer to apply changes in HDRP
        rend.materials = switchMats;
        RotateSwitch();
    }
    
    	public void RotateSwitch()
        {
		    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);

	    }
}