using UnityEngine;

public class SteamQuitHandler : MonoBehaviour
{
    private void Awake()
    {
        Application.wantsToQuit += WantsToQuit;
    }

    private bool WantsToQuit()
    {
        Debug.Log("Steam Deck Exit Game triggered");

        // Save game
        // Cleanup networking
        // Stop async tasks
        // etc

        Application.Quit();
        
        return true; // allow quit
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= WantsToQuit;
    }
}