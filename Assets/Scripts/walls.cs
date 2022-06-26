using UnityEngine;

public class walls : MonoBehaviour
{

    public Transform defaultparent;




    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == 12 || collision.gameObject.layer == 13)
        {
            //Debug.Log(collision.transform.name);
            collision.transform.SetParent(defaultparent);
            if (collision.transform.parent == default)
            {
                pickup.hasobject = false;
            }
            collision.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            
        }
    }



}
