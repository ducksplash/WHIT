using UnityEngine;

public class walls : MonoBehaviour
{

    public Transform defaultparent;



    private void OnCollisionEnter(Collision collision)
    {



        if (collision.gameObject.layer == 12 || collision.gameObject.layer == 13 || collision.gameObject.layer == 14)
        {

            if (gameObject.layer == 0)
            {

                collision.transform.SetParent(defaultparent);

                collision.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                pickup.hasobject = false;
                GameMaster.HASITEM = false;
                GameObject.FindGameObjectWithTag("ROTATIONMENU").GetComponent<CanvasGroup>().alpha = 0;
                
            }
        }
    }



}
