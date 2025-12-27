using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VLB;

public class Torch : MonoBehaviour
{
    public Light lightBeam;
    public GameObject theTorch;
    public static bool torchToggle = true;

    private Animator torchAnimator;

    public Image torchimg;
    public Sprite litsprite;
    public Sprite unlitsprite;
    public VolumetricLightBeamSD SpotlightBeam;

    public InputActionReference swingAction;
    public InputActionReference toggleAction;
    public bool isSwinging;
    public bool WaitingForTorch = true;

    void Awake()
    {
        EventManager.OnTorchCollected += OnTorchCollected;
    }

    void Start()
    {
        torchimg.sprite = litsprite;
        torchimg.GetComponent<CanvasGroup>().alpha = 0f;
        if (!GameMaster.Instance.OnboardingManager.TORCHCOLLECTED) theTorch.SetActive(false);
    }

    void OnEnable()
    {
        if (swingAction != null)
        {
            swingAction.action.Enable();
            swingAction.action.performed += OnSwing;
        }

        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnToggle;
        }
    }

    void OnDisable()
    {
        if (swingAction != null)
            swingAction.action.performed -= OnSwing;

        if (toggleAction != null)
            toggleAction.action.performed -= OnToggle;
    }

    public void OnTorchCollected()
    {
        Debug.Log("Torch collected, activating torch");
        theTorch.SetActive(true);
        torchAnimator = theTorch.GetComponentInChildren<Animator>();
        lightBeam = theTorch.GetComponentInChildren<Light>();
        WaitingForTorch = false;
        torchimg.GetComponent<CanvasGroup>().alpha = 1f;
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (WaitingForTorch || GameMaster.INMENU || GameMaster.FROZEN) return;

        torchToggle = !torchToggle;

        if (lightBeam != null)
            lightBeam.enabled = torchToggle;
        if (SpotlightBeam != null)
            SpotlightBeam.enabled = torchToggle;

        if (torchimg != null)
            torchimg.sprite = torchToggle ? litsprite : unlitsprite;

        Debug.Log("Torch toggled: " + torchToggle);
    }

    private void OnSwing(InputAction.CallbackContext ctx)
    {
        if (WaitingForTorch || GameMaster.INMENU || GameMaster.FROZEN) return;
        if (Pickup.hasobject) return;
        if (isSwinging) return;
        isSwinging = true;

        if (torchAnimator != null)
        {
            torchAnimator.SetTrigger("swing");
            Debug.Log("SwingTorch triggered");

            // Optional: reset to idle after animation duration
            float swingDuration = 0.5f; // match the swing clip length
            StartCoroutine(ResetIdleAfter(swingDuration));
        }
    }

    private IEnumerator ResetIdleAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (torchAnimator != null)
            torchAnimator.ResetTrigger("swing");
            torchAnimator.SetTrigger("idle");
            isSwinging = false;
    }
    
}
