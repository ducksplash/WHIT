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
    public Renderer bulbRenderer;
    private Material[] mats;


    [Header("UI")]
    public Image torchimg;
    public Sprite litsprite;
    public Sprite unlitsprite;
    

    [Header("Input")]
    public InputActionReference toggleAction;

    public static bool torchToggle = true;
    private static readonly int EmissiveColour = Shader.PropertyToID("_EmissiveColor");


    void Start()
    {
        torchimg.sprite = litsprite;
        torchimg.GetComponent<CanvasGroup>().alpha = 0f;

        if (!GameMaster.Instance.OnboardingManager.TORCHCOLLECTED) theTorch.SetActive(false);
        
        if (bulbRenderer == null) bulbRenderer = GetComponentInChildren<Renderer>();
        
        
        SetEmissives(torchToggle);
    }


    void OnEnable()
    {
        EventManager.OnTorchCollected += OnTorchCollected;

        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnToggle;
        }
    }

    void OnDisable()
    {
        EventManager.OnTorchCollected -= OnTorchCollected;

        if (toggleAction != null)
            toggleAction.action.performed -= OnToggle;
    }

    public void OnTorchCollected()
    {
        if (!theTorch.activeSelf) theTorch.SetActive(true);

        lightBeam = theTorch.GetComponentInChildren<Light>();

        torchimg.GetComponent<CanvasGroup>().alpha = 1f;
        
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.OnboardingManager.TORCHCOLLECTED) return;
        if (Player.Instance.PlayerPhone.CameraOpen) return;
        if (GameMaster.Instance.PLAYERBUSY) return;
        if (GameMaster.Instance.PauseManager != null && GameMaster.Instance.PauseManager.IsPaused) return;

        torchToggle = !torchToggle;

        if (lightBeam != null) lightBeam.enabled = torchToggle;

        if (SpotlightBeam != null) SpotlightBeam.enabled = torchToggle;

        if (torchimg != null) torchimg.sprite = torchToggle ? litsprite : unlitsprite;
        
        SetEmissives(torchToggle);

    }

    private void SetEmissives(bool torchOn)
    {
        if (bulbRenderer != null)
        {
            // Instantiate material instances for runtime changes
            mats = bulbRenderer.materials;

            // Ensure HDRP emission is enabled
            foreach (var m in mats)
            {
                if (m == null) continue;
                
                m.SetColor(EmissiveColour, torchOn ? Color.white * 100 : Color.black * 100); 
                
            }
        }
    }
}