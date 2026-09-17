using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class LevelVolumeProfiles : MonoBehaviour
{
    [Header("References")]
    
    public Volume PostProcessVolume;

    public Color FogColorTawleyMeats;
    public Color FogColorTawleyMeatsMaze;
    
    private Fog fog;

    private void Start()
    {
        EventManager.OnLevelLoaded += LoadLevelVolume;

    }


    // private void CacheVolumeComponents()
    // {
    //     if (PostProcessVolume == null)
    //     {
    //         Debug.LogError("PostProcessVolume is not assigned!");
    //         return;
    //     }
    //
    //     VolumeProfile profile = PostProcessVolume.profile;
    //     if (profile == null)
    //     {
    //         Debug.LogError("Volume has no profile assigned!");
    //         return;
    //     }
    //
    //     if (!profile.TryGet(out fog))
    //     {
    //         
    //         Debug.Log("foggone");
    //     }
    //     else
    //     {
    //         // Force the override to be active
    //         fog.active = true;
    //     }
    // }

    private void LoadLevelVolume()
    {
        if (PostProcessVolume.profile.TryGet(out fog)) { Debug.Log("gotfogs"); }
        
        if (fog == null) return;

        bool enableFog = false;
        Color fogColor = Color.clear;
        
        
        switch (GameMaster.Instance.THISLEVEL)
        {
            case GAMELEVEL.ETV: enableFog = false; fogColor = Color.clear; break;

            case GAMELEVEL.MainMenu: enableFog = false; fogColor = Color.clear; break;
                
            case GAMELEVEL.NorasOldFlat: enableFog = false; fogColor = Color.clear; break;

            case GAMELEVEL.FarsetCentralStation: enableFog = false; fogColor = Color.clear; break;
            
            case GAMELEVEL.EnteringTawley: enableFog = false; fogColor = Color.clear; break;
            
            case GAMELEVEL.NorasFlat: enableFog = false; fogColor = Color.clear; break;
            
            case GAMELEVEL.TawleyMeats: enableFog = true; fogColor = FogColorTawleyMeats; break;
            
            case GAMELEVEL.TawleyMeatsMaze: enableFog = true; fogColor = FogColorTawleyMeatsMaze; break;

            case GAMELEVEL.RoarkOutside: enableFog = false; fogColor = Color.clear; break;

            case GAMELEVEL.RoarkInside: enableFog = false; fogColor = Color.clear; break;

            default: enableFog = false; fogColor = Color.clear; break;
        }
        
        
        
        fog.active = enableFog;

        fog.enabled.overrideState = enableFog;
        fog.enabled.value = enableFog;
        fog.color.value = fogColor;
        
        


        //PostProcessVolume.profile = PostProcessVolume.profile;
    }
}