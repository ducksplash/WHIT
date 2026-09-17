using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class MazeDoor : MonoBehaviour
{
    public DoorCode ThisDoorCode;
    public DoorCode TargetDoorCode;
    public Vector3 TargetCoordinates;
    public bool ExitDoor = false;
    public GAMELEVEL ExitTo = GAMELEVEL.NorasFlat;
    public MeshRenderer DoorMeshRenderer;
    public MeshRenderer DoorLightMeshRenderer;
    public Light DoorLight;
    

    public void UseMazeDoor()
    {

        if (ExitDoor)
        {
            Debug.Log("Exit Door Scene Change");
            
            if (DungeonGenerator.Instance != null)
            {
                GameMaster.Instance.TravelCompanion.ChangeScene(ExitTo);
            }
            
        }
        else
        {
            StartCoroutine(TeleportInternally());
        }
        
        
    }
    
    
    private IEnumerator TeleportInternally()
    {
        Vector3 target = TargetCoordinates;

        Debug.Log($"Telepad '{name}' teleporting player " + $"from {Player.Instance.transform.position} to {target}");
        
        Player.Instance.SpawnOverride(target);

        Player.Instance.CanTeleport = false;

        Debug.Log($"Teleport complete. " + $"Player position: {Player.Instance.transform.position}. " + $"CanTeleport = {Player.Instance.CanTeleport}");

        yield return new WaitForSeconds(2);
        
        Player.Instance.CanTeleport = true;
        
        
        Debug.Log($"Telepad '{name}' reactivated. " + $"CanTeleport = {Player.Instance.CanTeleport}");
    }
    
    
    
    
}