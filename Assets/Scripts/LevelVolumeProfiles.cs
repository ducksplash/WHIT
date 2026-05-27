using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class LevelVolumeProfiles : MonoBehaviour
{
    [Header("References")]
    public Volume PostProcessVolume;

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
        if (PostProcessVolume.profile.TryGet(out fog))
        {
            Debug.Log("foggone");
        }
        
        if (fog == null) return;

        bool enableFog = GameMaster.Instance.THISLEVEL == GAMELEVEL.TawleyMeats;

        fog.active = enableFog;

        fog.enabled.overrideState = enableFog;
        fog.enabled.value = enableFog;



        //PostProcessVolume.profile = PostProcessVolume.profile;
    }
}