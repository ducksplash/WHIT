using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class clickable : MonoBehaviour
{
    public Image selectcursor;
    public Sprite clickablesprite;
    public Sprite clickablespritegreen;
    public Sprite clickablespritered;
    public Sprite idlesprite;
    public Sprite enemysprite;
    public Sprite unknownsprite;
    public Sprite doorsprite;
    public Sprite doorspritegreen;
    public Sprite drawersprite;
    public Sprite drawerspritegreen;
    public Sprite lockeddrawersprite;
    public Sprite lockeddoorsprite; 
    public Sprite pickupsprite;
    public Sprite evidencesprite;
    public Sprite pickupcloseenoughsprite;
    public TextMeshProUGUI infotext;
    public TextMeshProUGUI infotextbg;
    public Transform infotextbgimg;
    public int raycost;

    public int RaycastInterval;
    public int StartRaycastInterval;

    void Awake()
    {
        selectcursor = gameObject.GetComponent<Image>();

        selectcursor.sprite = idlesprite;
        INFOTEXT("");

    }

    private void Start()
    {
        raycost = 0;
        RaycastInterval = StartRaycastInterval;
    }

    void FixedUpdate()
    {


        if (RaycastInterval < 1)
        {

            int doorlayer = LayerMask.NameToLayer("door");
            int cupboardlayer = LayerMask.NameToLayer("cupboard");
            int drawerlayer = LayerMask.NameToLayer("drawer");
            int clickablelayer = LayerMask.NameToLayer("clickable");
            int enemylayer = LayerMask.NameToLayer("enemy");
            int idlelayer = LayerMask.NameToLayer("Default");
            int floorlayer = LayerMask.NameToLayer("ground");
            int unknownlayer = LayerMask.NameToLayer("unknown");
            int pickuplayer = LayerMask.NameToLayer("pickupable");
            int evidencelayer = LayerMask.NameToLayer("evidence");
            int staticevidencelayer = LayerMask.NameToLayer("staticevidence");
            int slidingdoorlayer = LayerMask.NameToLayer("slidingdoor");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            raycost++;




            if (Physics.Raycast(ray, out hit, 5.5f))
            {

                if (hit.transform.gameObject.layer == idlelayer)
                {
                    //Debug.Log("idle");
                    selectcursor.sprite = idlesprite;
                    INFOTEXT("");
                }


                if (hit.transform.gameObject.layer == floorlayer)
                {
                    //Debug.Log("idle");
                    selectcursor.sprite = idlesprite;
                    INFOTEXT("");
                }




                if (hit.transform.gameObject.layer == clickablelayer)
                {


                selectcursor.sprite = clickablesprite;
                INFOTEXT("");


                    if (hit.distance <= 3f)
                    {
                        selectcursor.sprite = clickablespritegreen;
                        INFOTEXT("");
                    }


                    if (hit.transform.tag.Contains("LIGHTSWITCHES"))
                    {

                        if (hit.distance <= 3f)
                        {

                            if (GameMaster.POWER_SUPPLY_ENABLED)
                            {
                                INFOTEXT("Lights", "green");
                                selectcursor.sprite = clickablespritegreen;
                            }
                            else
                            {
                                INFOTEXT("Lights (Power Disabled)", "red");
                                selectcursor.sprite = clickablespritered;
                            }

                        }
                        else
                        {
                            INFOTEXT("Lights", "red");
                        }
                    }


                    if (hit.transform.tag.Contains("POWERSWITCH"))
                    {
                        if (hit.distance <= 3f)
                        {

                            if (GameMaster.POWER_SUPPLY_ENABLED)
                            {
                                INFOTEXT("Power (on)", "green");
                                selectcursor.sprite = clickablespritegreen;
                            }
                            else
                            {
                                INFOTEXT("Power (off)", "red");
                                selectcursor.sprite = clickablespritered;
                            }

                        }
                        else
                        {
                            INFOTEXT("Power", "red");
                        }
                    }


                    if (hit.transform.tag.Contains("INCINERATOR"))
                    {

                        if (GameMaster.POWER_SUPPLY_ENABLED && GameMaster.INCINERATOR_ENABLED)
                        {
                            INFOTEXT("Start Incinerator", "green");
                        }
                        else
                        {
                                INFOTEXT("Start Incinerator (Power Disabled)", "red");
                        }
                        

                        if (hit.transform.tag.Contains("INCINERATOROFF"))
                        {

                            if (GameMaster.POWER_SUPPLY_ENABLED && GameMaster.INCINERATOR_ENABLED)
                            {
                                INFOTEXT("Stop Incinerator", "green");
                            }
                            else
                            {
                                INFOTEXT("Stop Incinerator (Power Disabled)", "red");
                            }
                        }
                    }
                }
                



                if (hit.transform.gameObject.layer == enemylayer)
                {
                    //Debug.Log("enemy");
                    selectcursor.sprite = enemysprite;
                }







                if (hit.transform.gameObject.layer == unknownlayer)
                {
                    //Debug.Log("enemy");
                    selectcursor.sprite = unknownsprite;
                }


                if (hit.transform.gameObject.layer == doorlayer)
                {
                    //Debug.Log("door");
                    if (hit.distance <= 3f)
                    {
                        if (hit.transform.parent.parent.GetComponent<innerDoors>().isLocked)
                        {
                            selectcursor.sprite = lockeddoorsprite;
                            INFOTEXT("locked", "red");
                        }
                        else
                        {
                            selectcursor.sprite = doorspritegreen;
                            INFOTEXT("");
                        }
                    }
                    else
                    {
                        selectcursor.sprite = doorsprite;
                        INFOTEXT("");
                    }



                }



                if (hit.transform.gameObject.layer == slidingdoorlayer)
                {
                    //Debug.Log("door");
                    if (hit.distance <= 3f)
                    {
                        if (hit.transform.GetComponent<SlidingDoors>().isLocked)
                        {
                            selectcursor.sprite = lockeddoorsprite;
                            INFOTEXT("locked", "red");
                        }
                        else
                        {
                            selectcursor.sprite = doorspritegreen;
                            INFOTEXT("");
                        }
                    }
                    else
                    {
                        selectcursor.sprite = doorsprite;
                        INFOTEXT("");
                    }



                }



                if (hit.transform.gameObject.layer == drawerlayer)
                {


                    if (hit.distance <= 3f)
                    {
                        if (hit.transform.GetComponent<Drawers>().isLocked)
                        {
                            selectcursor.sprite = lockeddrawersprite;
                            INFOTEXT("locked", "red");
                        }
                        else
                        {
                            selectcursor.sprite = drawerspritegreen;
                            INFOTEXT("");
                        }
                    }
                    else
                    {
                        selectcursor.sprite = drawersprite;
                        INFOTEXT("");
                    }



                    if (hit.transform.GetComponentInParent<Drawers>().isLocked)
                    {
                        selectcursor.sprite = lockeddrawersprite;
                        INFOTEXT("locked", "red");
                    }
                    else
                    {
                        selectcursor.sprite = drawersprite;
                        INFOTEXT("");
                    }

                }



                if (hit.transform.gameObject.layer == cupboardlayer)
                {
                    //Debug.Log("door");

                    selectcursor.sprite = doorsprite;
                    INFOTEXT("");

                    if (hit.transform.GetComponent<TravelCompanion>() != null)
                    {

                        if (hit.distance < 3f && hit.distance > 1.2f)
                        {
                            INFOTEXT("Move to a new location");
                            selectcursor.sprite = doorspritegreen;
                        }
                    }
                }


                if (hit.transform.gameObject.layer == pickuplayer)
                {
                    //Debug.Log("door");
                    selectcursor.sprite = pickupsprite;

                    if (hit.distance <= 3f)
                    {
                        selectcursor.sprite = pickupcloseenoughsprite;
                    }

                }





                if (hit.transform.gameObject.layer == evidencelayer)
                {
                    //Debug.Log("door");
                    selectcursor.sprite = evidencesprite;
                }

                if (hit.transform.gameObject.layer == staticevidencelayer)
                {
                    //Debug.Log("door");
                    selectcursor.sprite = evidencesprite;
                }

            }
            else
            {
                //Debug.Log("idle");
                selectcursor.sprite = idlesprite;
                INFOTEXT("");
            }



            RaycastInterval = StartRaycastInterval;
        }
        else
        {
            RaycastInterval--;
        }
    


    }





    public void INFOTEXT(string text, string textcolor = "white")
    {

        infotext.text = text;
        infotextbg.text = text;

        if (textcolor == "white")
        {
            infotext.color = new Color(1f, 1f, 1f, 1f);
        }

        if (textcolor == "red")
        {
            infotext.color = new Color(1f, 0.2f, 0.2f, 1f);
        }

        if (textcolor == "green")
        {
            infotext.color = new Color(0f, 0.9f, 0f, 1f);
        }


        var stringlen = text.Length;

        var stringmaffs = stringlen * 18;



        var theBarRectTransform = infotextbgimg as RectTransform;
        theBarRectTransform.sizeDelta = new Vector2(stringmaffs, 35);




    }




}
