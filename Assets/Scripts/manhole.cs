using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class manhole : MonoBehaviour
{
    public GameObject manholeobject;
    public CanvasGroup actioncanvas;
    public GameObject actionobject;
    private Transform playerTransform;
    public float detectionDistance = 10.0f;
    public float visibilityDelay = 0.5f;
    public TextMeshProUGUI actiontext;
    public Image actionprogress;
    public string NextScene;

	public CanvasGroup loadingpanel;
	public Image loadingbar;
	public TextMeshProUGUI loadingclock;

    public float holdTime = 1.0f;

    private bool isHolding = false;
    private float holdTimer = 0.0f;
    private bool playerinrange;






    private void Start()
    {
        manholeobject = gameObject;
        Debug.Log(manholeobject.name);

        actioncanvas.alpha = 0;
        actionprogress.fillAmount = 0.0f;


    }








    private void Update()
    {
        playerTransform = Camera.main.transform.parent.transform;
        actionobject.transform.LookAt(playerTransform);



        if (playerinrange)
        {

            if (InputManager.GetKey("special"))
            {
                if (!isHolding)
                {
                    // Key is just pressed
                    isHolding = true;
                    holdTimer = 0.0f;
                    Debug.Log("Key is being held down");
                }
                else
                {
                    // Key is being held down
                    holdTimer += Time.deltaTime;
                    float progress = Mathf.Clamp01(holdTimer / holdTime);
                    actionprogress.fillAmount = progress;
                    if (holdTimer >= holdTime)
                    {
                        
                        Debug.Log("Key has been held down for " + holdTime + " seconds");
                        ChangeScene(NextScene);
                    }
                }
            }
            else
            {
                if (isHolding)
                {
                    // Key is just released
                    isHolding = false;
                    actionprogress.fillAmount = 0.0f;
                }
            }

        }




    }



    private void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {


                Invoke("ShowSign", visibilityDelay);
                
            
        }
    }

    private void OnTriggerStay(Collider hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {


                Invoke("ShowSign", visibilityDelay);
                
            
        }
    }

    private void OnTriggerExit(Collider hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {


                Invoke("HideSign", visibilityDelay);
                
            
        }
    }

    private void ShowSign()
    {

        actiontext.text = "Hold "+InputManager.GetKeyName("special")+"\nTo Enter";
        actioncanvas.alpha = 1;
        playerinrange = true;
    }

    private void HideSign()
    {
        actioncanvas.alpha = 0;
        playerinrange = false;
    }





    public void ChangeScene(string SceneName)
	{
		GameMaster.INMENU = false;
		GameMaster.FROZEN = false;
		StartCoroutine(ChangeSceneAsync(SceneName));
	}




	IEnumerator ChangeSceneAsync(string levelName)
	{

		loadingpanel.alpha = 1;

		AsyncOperation op = SceneManager.LoadSceneAsync(levelName);


		var buildDate = "";

		buildDate += System.DateTime.Now.ToString("dddd");
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("MMMM d");
		buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("yyyy");


		loadingclock.text = buildDate;


		while (!op.isDone)
		{
			loadingbar.fillAmount = Mathf.Clamp01(op.progress / .9f);


			yield return null;
		}
	}





	public string MonthDay(string day)
	{
		string nuNum = "th";
		if (int.Parse(day) < 11 || int.Parse(day) > 20)
		{
			day = day.ToCharArray()[^1].ToString();
			switch (day)
			{
				case "1":
					nuNum = "st";
					break;
				case "2":
					nuNum = "nd";
					break;
				case "3":
					nuNum = "rd";
					break;
			}
		}
		return nuNum;
	}

}
