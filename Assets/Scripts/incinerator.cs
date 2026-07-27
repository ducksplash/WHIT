using System.Collections;
using UnityEngine;

public class incinerator : MonoBehaviour
{
    
    // incinerator uses old Doors script for lock/unlock
    // refactor to use new Door script
    
    public ParticleSystem[] flames;
    public Collider doorcollider;
    private float flamesize = 20f;
    public bool flametriggered;
    public Renderer stopbuttonrim;
    public Renderer gobuttonrim;
    public Light[] redlights;
    public UnityEngine.ParticleSystem.MinMaxCurve origsize;

    void Start()
    {
        foreach (ParticleSystem flame in flames)
        {
            origsize = flame.main.startSize;

            Debug.Log("todo: Refactor incinerator to use new Door script");
        }

        return;

        // if (GameMaster.Instance.POWER_SUPPLY_ENABLED && GameMaster.Instance.INCINERATOR_ENABLED)
        // {
        //
        //     FlameControl(true);
        //     
        // }
        // else
        // {
        //     FlameControl(false);
        // }
        
    }


    private void Update()
    {
        if (flametriggered)
        {
            foreach (ParticleSystem flame in flames)
            {
                var main = flame.main;
                main.startSize = flamesize;

            }
        }


        return;

        if (GameMaster.Instance.POWER_SUPPLY_ENABLED && GameMaster.Instance.INCINERATOR_ENABLED)
        {

            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
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





    public void FlameControl(bool command)
    {

        foreach (ParticleSystem flame in flames)
        {
            var em = flame.emission;
            em.enabled = command;

            if (!command)
            {
                var main = flame.main;
                main.startSize = origsize;
            }

        }

    }



    public void UseFurnace()
    {
        Debug.Log("button use happen");

            FlameControl(true);
        

        var emitColor = new Color(0f, 0.8f, 0, 1f);
        gobuttonrim.material.SetColor("_Color", emitColor);
        gobuttonrim.material.SetColor("_EmissiveColor", emitColor * 5);

        var emiColor = new Color(1f, 1f, 1, 1f);
        stopbuttonrim.material.SetColor("_Color", emiColor);
        stopbuttonrim.material.SetColor("_EmissiveColor", emiColor * 5);


        foreach (Light singlight in redlights)
        {
            if (singlight) { singlight.enabled = true;}
        }

    }


    public void StopFurnace()
    {
        Debug.Log("button stop happen");


            FlameControl( false);
        

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

        if (GameMaster.Instance.POWER_SUPPLY_ENABLED)
        {

            if (GameMaster.Instance.INCINERATOR_ENABLED)
            {


                if (other.CompareTag("Player"))
                {
                    // GetComponentInChildren<innerDoors>().isOpen = false;
                    // GetComponentInChildren<innerDoors>().isLocked = true;

                    GetComponentInChildren<Animator>().SetTrigger("closed");
                    StartCoroutine(weeWait(1, doorcollider));
                    GetComponentInChildren<Animator>().SetTrigger("idle");

                    GameMaster.Instance.PLAYERBUSY = true;


                        FlameControl(true);
                    


                    foreach (Light singlight in redlights)
                    {

                        singlight.enabled = true;

                    }
                    StartCoroutine(BurnPlayer());
                }
            }

        }




    }


    IEnumerator BurnPlayer()
    {

        flametriggered = true;
        yield return new WaitForSeconds(0.01f);
        flametriggered = false;

        yield return new WaitForSeconds(1f);



        flamesize = 10;


        yield return new WaitForSeconds(3f);

        Player.Instance.CauseDeath("incineration");

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
        flametriggered = false;

            FlameControl(false);


        
        // GetComponentInChildren<innerDoors>().isLocked = false;

        foreach (Light singlight in redlights)
        {

            singlight.enabled = false;

        }
    }


}
