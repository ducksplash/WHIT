using UnityEngine;

public class Drawers : MonoBehaviour
{
	private GameObject thisDrawer;
	private string thisDrawerName;
	private Animator drawerAnimator;
	private bool isOpen = false;
	public bool isLocked = false;

	void Start()
    {
		thisDrawer = this.gameObject;
		drawerAnimator = thisDrawer.GetComponent<Animator>();
		thisDrawerName = thisDrawer.name;
	
    }

    void Update()
    {
  //       if (Input.GetMouseButtonDown(1))
		// {  
		// 	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
		// 	RaycastHit hit;  
		// 	if (Physics.Raycast(ray, out hit)) 
		// 	{  
		// 		if (hit.distance <= 3f)
		// 		{			
		// 			
		// 		if (hit.transform.name.Equals(thisDrawerName))
		// 			{  
		// 					
		// 				if (!isOpen)
		// 				{
		// 					drawerAnimator.SetTrigger("opened");
		// 					Debug.Log("open");
		// 					isOpen = true;
		// 				}
		// 				else
		// 				{
		// 					drawerAnimator.SetTrigger("closed");
		// 					Debug.Log("close");
		// 					isOpen = false;
		// 					drawerAnimator.SetTrigger("idle");
		// 				}
		// 			}					
		// 		}  				
		// 	}  
		// } 
    }
}
