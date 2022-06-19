using UnityEngine;

public class walls : MonoBehaviour
{

    public Transform defaultparent;

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == 12)
        {
            pickup.hasobject = false;
            Debug.Log(collision.transform.name);
            collision.transform.SetParent(defaultparent);
            collision.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            
        }
    }



}
