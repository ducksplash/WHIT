using System.Collections;
using UnityEngine;

public class incinerator : MonoBehaviour
{
    public Collider doorcollider;
    public Renderer stopbuttonrim;
    public Renderer gobuttonrim;
    public Light[] redlights;
    public UnityEngine.ParticleSystem.MinMaxCurve origsize;
    private static readonly int Closed = Animator.StringToHash("closed");
    private static readonly int Idle = Animator.StringToHash("idle");

    
    private void Update()
    {
        if (GameMaster.POWER_SUPPLY_ENABLED && GameMaster.INCINERATOR_ENABLED)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.distance <= 5.5f)
                    {

                        if (hit.transform.name.Contains("incstop"))
                        {
                            StopFurnace();
                        }

                        if (hit.transform.name.Contains("incgo"))
                        {
                            UseFurnace();
                        }
                    }
                }
            }
        }
    }

    

    public void UseFurnace()
    {
        Debug.Log("button use happen");
        

        var emitColor = new Color(0f, 0.8f, 0, 1f);
        gobuttonrim.material.SetColor("_Color", emitColor);
        gobuttonrim.material.SetColor("_EmissiveColor", emitColor * 5);

        var emiColor = new Color(1f, 1f, 1, 1f);
        stopbuttonrim.material.SetColor("_Color", emiColor);
        stopbuttonrim.material.SetColor("_EmissiveColor", emiColor * 5);


        foreach (Light singlight in redlights)
        {
            singlight.enabled = true;
        }

    }


    public void StopFurnace()
    {
        Debug.Log("button stop happen");
        
        foreach (Light singlight in redlights)
        {
            singlight.enabled = false;
        }

        var emitColor = new Color(0.8f, 0, 0, 1f);
        stopbuttonrim.material.SetColor("_Color", emitColor);
        stopbuttonrim.material.SetColor("_EmissiveColor", emitColor * 5);

        var emiColor = new Color(1f, 1, 1, 1f);
        gobuttonrim.material.SetColor("_Color", emiColor);
        gobuttonrim.material.SetColor("_EmissiveColor", emiColor * 5);

    }





    private void OnTriggerEnter(Collider other)
    {
        if (GameMaster.POWER_SUPPLY_ENABLED)
        {
            if (GameMaster.INCINERATOR_ENABLED)
            {
                if (other.name.Contains("Player"))
                {
                    GetComponentInChildren<innerDoors>().isOpen = false;
                    GetComponentInChildren<innerDoors>().isLocked = true;

                    GetComponentInChildren<Animator>().SetTrigger(Closed);
                    StartCoroutine(weeWait(1, doorcollider));
                    GetComponentInChildren<Animator>().SetTrigger(Idle);

                    GameMaster.FROZEN = true;
                    
                    foreach (Light singlight in redlights)
                    {
                        singlight.enabled = true;
                    }
                    
                    StartCoroutine(BurnPlayer(other.gameObject));
                }
            }
        }
    }


    IEnumerator BurnPlayer(GameObject Player)
    {
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(3f);
        Player.GetComponent<FirstPersonCollision>().CauseDeath("incineration");
        StartCoroutine(cleanup());
    }


    IEnumerator weeWait(float aWeeSecond, Collider theCollider)
    {

        theCollider.enabled = false;
        yield return new WaitForSeconds(aWeeSecond);
        theCollider.enabled = true;

    }

    IEnumerator cleanup()
    {
        yield return new WaitForSeconds(0.2f);
        
        GetComponentInChildren<innerDoors>().isLocked = false;

        foreach (Light singlight in redlights)
        {
            singlight.enabled = false;
        }
    }


}
