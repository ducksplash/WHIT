using UnityEngine;

public class ParticleProxima : MonoBehaviour
{
    public ParticleSystem PGenerator;
    public Renderer PGen;
    private void Start()
    {
        PGenerator = gameObject.GetComponent<ParticleSystem>();

        PGen = PGenerator.GetComponent<Renderer>();
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.name.Contains("Player"))
        {
            PGen.enabled = false;

        }
    }


    private void OnTriggerExit(Collider other)
    {

        if (other.name.Contains("Player"))
        {
            PGen.enabled = true;

        }
    }



}
