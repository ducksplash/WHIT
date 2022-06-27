using UnityEngine;

public class walls : MonoBehaviour
{

    public Transform defaultparent;




    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == 12 || collision.gameObject.layer == 13 || collision.gameObject.layer == 14)
        {
            //Debug.Log(collision.transform.name);


            collision.transform.SetParent(defaultparent);



            pickup.hasobject = false;
            GameObject.FindGameObjectWithTag("ROTATIONMENU").GetComponent<CanvasGroup>().alpha = 0;

            collision.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            
        }
    }



}
