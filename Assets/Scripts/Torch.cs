using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VLB;
using UnityEngine.Animations.Rigging;

public class Torch : MonoBehaviour
{
    [Header("Torch Objects")]
    public Light lightBeam;
    public GameObject theTorch;
    public VolumetricLightBeamSD SpotlightBeam;

    [Header("UI")]
    public Image torchimg;
    public Sprite litsprite;
    public Sprite unlitsprite;

    [Header("Animation Rigging")]
    [Tooltip("Assign the UpperBody Rig that controls the torch arm IK.")]
    public Rig torchRig;

    [Tooltip("How fast the arm raises/lowers.")]
    public float rigBlendSpeed = 6f;

    [Header("Input")]
    public InputActionReference swingAction;
    public InputActionReference toggleAction;

    public static bool torchToggle = true;
    public bool isSwinging;

    private Animator torchAnimator;
    private float targetRigWeight;

    void Start()
    {
        torchimg.sprite = litsprite;
        torchimg.GetComponent<CanvasGroup>().alpha = 0f;

        if (!GameMaster.Instance.OnboardingManager.TORCHCOLLECTED)
            theTorch.SetActive(false);

        StartCoroutine(StartHands());

    }

    void LateStart()
    {
        var rb = GetComponent<RigBuilder>();
        if (rb != null)
        {
            rb.Clear();
            rb.Build();
        }
    }

    IEnumerator StartHands()
    {
        yield return new WaitForSeconds(1);
        LateStart();
    }

    
    void Update()
    {
        // Smoothly blend IK weight
        if (torchRig != null)
        {
            torchRig.weight = Mathf.Lerp(
                torchRig.weight,
                targetRigWeight,
                Time.deltaTime * rigBlendSpeed
            );
        }
    }

    void OnEnable()
    {
        EventManager.OnTorchCollected += OnTorchCollected;

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
        EventManager.OnTorchCollected -= OnTorchCollected;

        if (swingAction != null)
            swingAction.action.performed -= OnSwing;

        if (toggleAction != null)
            toggleAction.action.performed -= OnToggle;
    }

    public void OnTorchCollected()
    {
        if (!theTorch.activeSelf)
            theTorch.SetActive(true);

        torchAnimator = theTorch.GetComponentInChildren<Animator>();
        lightBeam = theTorch.GetComponentInChildren<Light>();

        torchimg.GetComponent<CanvasGroup>().alpha = 1f;

        // Raise arm when first collected
        targetRigWeight = 1f;
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.OnboardingManager.TORCHCOLLECTED) return;
        if (GameMaster.Instance.PLAYERBUSY) return;

        torchToggle = !torchToggle;

        if (lightBeam != null)
            lightBeam.enabled = torchToggle;

        if (SpotlightBeam != null)
            SpotlightBeam.enabled = torchToggle;

        if (torchimg != null)
            torchimg.sprite = torchToggle ? litsprite : unlitsprite;

        // Lower arm when torch off
        targetRigWeight = torchToggle ? 1f : 0f;
    }

    private void OnSwing(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.OnboardingManager.TORCHCOLLECTED) return;
        if (GameMaster.Instance.PLAYERBUSY || isSwinging || GameMaster.Instance.PauseManager.IsPaused) return;

        isSwinging = true;

        if (torchAnimator != null)
        {
            torchAnimator.SetTrigger("swing");

            float swingDuration = 0.5f;
            StartCoroutine(ResetIdleAfter(swingDuration));
        }
    }

    private IEnumerator ResetIdleAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (torchAnimator != null)
        {
            torchAnimator.ResetTrigger("swing");
            torchAnimator.SetTrigger("idle");
            isSwinging = false;
        }
    }
}
