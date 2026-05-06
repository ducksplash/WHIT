using System.Collections;
using UnityEngine;

public class Flash : MonoBehaviour
{

    [SerializeField] private CanvasGroup canvass;

    private Coroutine flashCo;
    

    void Start()
    {
        if (flashCo != null)
        {
            StopCoroutine(flashCo);
            flashCo = null;
        }

        flashCo = StartCoroutine(flashCoroutine());
    }


    private IEnumerator flashCoroutine()
    {

        while (gameObject.activeSelf)
        {
            canvass.alpha = 1;
            yield return new WaitForSeconds(1);
            canvass.alpha = 0;
            yield return new WaitForSeconds(1);
        }
        
        
    }
    
}
