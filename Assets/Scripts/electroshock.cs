using System.Collections;
using UnityEngine;

public class electroshock : MonoBehaviour
{
    public Transform cameraTransform;
    private Vector3 orignalCameraPos;


    public GameObject shockParticles;

    public float ShakeDuration = 2f;
    public float ShakeIntensity = 1.4f;

    private bool canShake = false;
    private float ShakinTime;

    public bool WaitingForPower;


    void Start()
    {
        cameraTransform = Camera.main.transform;

        orignalCameraPos = cameraTransform.localPosition;

        shockParticles = transform.GetChild(0).gameObject;


        if (!GameMaster.POWER_SUPPLY_ENABLED)
        {
            shockParticles.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (canShake)
        {
            StartCameraShakeEffect();
        }

        if (!WaitingForPower)
        {
            if (GameMaster.POWER_SUPPLY_ENABLED)
            {
                shockParticles.SetActive(true);
                WaitingForPower = true;
            }
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
            ShakeCamera();
            StartCoroutine(DoDeath(other));
        }
    }



    IEnumerator DoDeath(Collider theplayer)
    {
        yield return new WaitForSeconds(2f);

     //      theplayer.GetComponent<FirstPersonCollision>().CauseDeath("electrocution");


    }


}
