using System;
using TMPro;
using UnityEngine;

public class PCPasswordHelperMonkey : MonoBehaviour
{

    public TextMeshProUGUI thisPCPassword;


    private void Start()
    {
        thisPCPassword.text = GameMaster.Instance.NORASPCPASSWORD;
    }
}


