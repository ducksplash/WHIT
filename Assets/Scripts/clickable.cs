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
    public Sprite idlesprite;
    public Sprite enemysprite;
    public Sprite unknownsprite;
    public Sprite doorsprite;
    public Sprite pickupsprite;
    public Sprite evidencesprite;


    void Awake()
    {
        selectcursor = gameObject.GetComponent<Image>();

        selectcursor.sprite = idlesprite;

    }

    void FixedUpdate()
    {

        int doorlayer = LayerMask.GetMask("door");
        int clickablelayer = LayerMask.GetMask("clickable");
        int enemylayer = LayerMask.GetMask("enemy");
        int idlelayer = LayerMask.GetMask("Default");
        int floorlayer = LayerMask.GetMask("ground");
        int unknownlayer = LayerMask.GetMask("unknown");
        int pickuplayer = LayerMask.GetMask("pickupable");
        int evidencelayer = LayerMask.GetMask("evidence");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;



        if (Physics.Raycast(ray, out hit, 20f, idlelayer))
        {
            //Debug.Log("idle");
            selectcursor.sprite = idlesprite;
        }

        if (Physics.Raycast(ray, out hit, 20f, floorlayer))
        {
            //Debug.Log("idle");
            selectcursor.sprite = idlesprite;
        }

        if (Physics.Raycast(ray, out hit, 5.5f, clickablelayer))
        {
            //Debug.Log("clickable");
            selectcursor.sprite = clickablesprite;
        }

        if (Physics.Raycast(ray, out hit, 5.5f, enemylayer))
        {
            //Debug.Log("enemy");
            selectcursor.sprite = enemysprite;
        }

        if (Physics.Raycast(ray, out hit, 5.5f, unknownlayer))
        {
            //Debug.Log("enemy");
            selectcursor.sprite = unknownsprite;
        }

        if (Physics.Raycast(ray, out hit, 5.5f, doorlayer))
        {
            //Debug.Log("door");
            selectcursor.sprite = doorsprite;
        }

        if (Physics.Raycast(ray, out hit, 5.5f, pickuplayer))
        {
            //Debug.Log("door");
            selectcursor.sprite = pickupsprite;
        }

        if (Physics.Raycast(ray, out hit, 5.5f, evidencelayer))
        {
            //Debug.Log("door");
            selectcursor.sprite = evidencesprite;
        }
    }
}
