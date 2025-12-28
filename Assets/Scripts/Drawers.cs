using UnityEngine;

public class Drawers : MonoBehaviour
{
    private Animator drawerAnimator;
    public bool isOpen = false;
    public bool isLocked = false;

    void Start()
    {
        drawerAnimator = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (isLocked) return;

        if (!isOpen)
        {
            drawerAnimator.SetTrigger("opened");
            drawerAnimator.SetTrigger("idle");
            isOpen = true;
        }
        else
        {
            drawerAnimator.SetTrigger("closed");
            drawerAnimator.SetTrigger("idle");
            isOpen = false;
        }
    }
}