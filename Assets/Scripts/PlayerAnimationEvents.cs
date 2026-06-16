using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private Player playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<Player>();
    }

    public void OnJumpTakeoff()
    {
        playerMovement?.OnJumpTakeoff();
    }
}