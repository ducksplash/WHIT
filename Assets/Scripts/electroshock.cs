using System.Collections;
using UnityEngine;

public class electroshock : MonoBehaviour
{
    public Transform cameraTransform;
    private Vector3 orignalCameraPos;


    public float ShakeDuration = 2f;
    public float ShakeIntensity = 1.4f;

    private bool canShake = false;
    private float ShakinTime;


    void Start()
    {
        cameraTransform = Camera.main.transform;

        orignalCameraPos = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShakeCamera();
        }

        if (canShake)
        {
            StartCameraShakeEffect();
        }
    }

    public void ShakeCamera()
    {
        canShake = true;
        ShakinTime = ShakeDuration;
    }

    public void StartCameraShakeEffect()
    {
        GameMaster.FROZEN = true;


        if (ShakinTime > 0)
        {
            cameraTransform.localPosition = orignalCameraPos + Random.insideUnitSphere * ShakeIntensity;
            ShakinTime -= Time.deltaTime;
        }
        else
        {
            ShakinTime = 0f;
            cameraTransform.localPosition = orignalCameraPos;
            canShake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {


        if (other.name.Contains("Player"))
        {

            Debug.Log(other.name);


            ShakeCamera();

            StartCoroutine(DoDeath(other));




        }


    }



    IEnumerator DoDeath(Collider theplayer)
    {
        yield return new WaitForSeconds(2f);

            theplayer.GetComponent<FirstPersonCollision>().CauseDeath("electrocution");


    }






}
