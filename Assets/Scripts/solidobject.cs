using UnityEngine;

public class solidobject : MonoBehaviour
{

    public Transform defaultparent;

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == 0 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7 || collision.gameObject.layer == 8)
        {

            transform.SetParent(defaultparent);
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            pickup.hasobject = false;
            GameMaster.HASITEM = false;
            GameObject.FindGameObjectWithTag("ROTATIONMENU").GetComponent<CanvasGroup>().alpha = 0;
                        
        }
    }


}
