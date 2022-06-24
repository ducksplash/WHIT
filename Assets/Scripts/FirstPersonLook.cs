using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    Vector2 currentMouseLook;
    Vector2 appliedMouseDelta;
    public phone thephonescript;
    public FirstPersonCollision playerhandler;
    public float sensitivity = 1;
    public float smoothing = 2;


    void Reset()
    {
        character = GetComponentInParent<FirstPersonCollision>().transform;
    }

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
		
		if (!thephonescript.PT && !playerhandler.INMENU)
        {
		    Cursor.lockState = CursorLockMode.Locked;
		    Cursor.visible = false;
        }
		
		

        if (!GameMaster.FROZEN)
        {


        // Rotate camera and controller.
        transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
        }
    }

    private void LateUpdate()
    {
        if (!GameMaster.FROZEN)
        {
            // Get smooth mouse look.
            Vector2 smoothMouseDelta = Vector2.Scale(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")), Vector2.one * sensitivity * smoothing);
            appliedMouseDelta = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1 / smoothing);
            currentMouseLook += appliedMouseDelta;
            currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -50, 50);
        }
    }




}
