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
		if (thisCollision.transform.name.Contains("abbotoir"))
		{
			didCollide = true;
		}
		
    }	
	
	void OnCollisionExit(Collision thisCollision)
    {
		didCollide = false;
    }

	
	

    void FixedUpdate()
    {
		if (!didCollide)
		{
			velocity.y = Input.GetAxis("Vertical") * speed * Time.fixedDeltaTime;
			velocity.x = Input.GetAxis("Horizontal") * speed * Time.fixedDeltaTime;
			transform.Translate(velocity.x, 0, velocity.y);
		}		
    }
}
