using System.Collections;
using UnityEngine;

public class Disposer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DisposeExplode());
    }


    private IEnumerator DisposeExplode()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
