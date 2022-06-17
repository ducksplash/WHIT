using UnityEngine;

public class clickable : MonoBehaviour
{
    public CanvasGroup selectcursor;



    void Awake()
    {
        selectcursor = gameObject.GetComponent<CanvasGroup>();
    }

    void FixedUpdate()
    {

        int layer_mask = LayerMask.GetMask("clickable");

        //Actually you can add any layer name you want, for example:
        //int layer_mask = LayerMask.GetMask("Ground","Enemy","Boxes");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //do the raycast specifying the mask
        if (Physics.Raycast(ray, out hit, 5.5f, layer_mask))
        {
            Debug.Log("clickable");
            selectcursor.alpha = 1f;
        }
        else
        {
            selectcursor.alpha = 0f;
        }


    }
}
