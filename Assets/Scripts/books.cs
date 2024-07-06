using System.Collections;
using UnityEngine;

public class books : MonoBehaviour
{
	public GameObject thisBook;
	private string thisBookName;
	public GameObject thisBookHinge;
	private Animator bookAnimator;
	public bool isOpen = false;
	private Collider theBookCollider;
	public bool PlayerClicked;
	
    void Start()
    {
		bookAnimator = thisBookHinge.GetComponent<Animator>();
		thisBookName = thisBook.name;
		PlayerClicked = false;
    }
	
	
	IEnumerator DisableColliderMomentarily(float aWeeSecond, Collider theCollider)
	{
		theCollider.enabled = false;
		yield return new WaitForSeconds(aWeeSecond);
		PlayerClicked = false;
		theCollider.enabled = true;
	}


	void Update()
	{
		if (!PlayerClicked)
		{
			if (Input.GetMouseButtonUp(1))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 4f))
				{
					if (hit.transform.name.Equals(thisBookName))
					{
						DoBook(hit);
						PlayerClicked = true;
					}
				}
			}
		}
	}
	
		public void DoBook(RaycastHit hit)
		{
			var thisBookCollider = hit.transform.GetComponent<Collider>();

			if (!isOpen)
			{

				bookAnimator.SetTrigger("opened");
				StartCoroutine(DisableColliderMomentarily(0.5f, thisBookCollider));
				isOpen = true;

			}
			else
			{
				bookAnimator.SetTrigger("closed");
				isOpen = false;
				StartCoroutine(DisableColliderMomentarily(0.5f, thisBookCollider));
				bookAnimator.SetTrigger("idle");
			}
		}
	}
