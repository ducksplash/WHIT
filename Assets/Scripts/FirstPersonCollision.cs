using UnityEngine;

public class FirstPersonCollision : MonoBehaviour
{
	
	
	private Collider thePlayerCollider;
	
    public float speed = 5;
    Vector2 velocity;
	private Collision thisCollision;
	private bool didCollide = false;
	
	
	void Start()
	{
		
		thePlayerCollider = gameObject.GetComponent<CapsuleCollider>();
		
	}
	
	
	
	void OnCollisionEnter(Collision thisCollision)
    {
        //Output the Collider's GameObject's name
		didCollide = true;
		
    }	
	
	void OnCollisionExit(Collision thisCollision)
    {
        //Output the Collider's GameObject's name
		didCollide = false;
    }

	
	

    void FixedUpdate()
    {
		
        velocity.y = Input.GetAxis("Vertical") * speed * Time.fixedDeltaTime;
        velocity.x = Input.GetAxis("Horizontal") * speed * Time.fixedDeltaTime;
        transform.Translate(velocity.x, 0, velocity.y);
		
    }
}
