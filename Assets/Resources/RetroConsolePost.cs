using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable, VolumeComponentMenu("Post-processing/Custom/Retro Console Post")]
public sealed class RetroConsolePostProcess : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [SerializeField] private Shader shader;

    // Master effect fade (0..1)
    public ClampedFloatParameter effectIntensity = new ClampedFloatParameter(1f, 0f, 1f);

    // Pixelation
    public ClampedFloatParameter pixelSize = new ClampedFloatParameter(4f, 1f, 16f);
    public ClampedFloatParameter pixelateStrength = new ClampedFloatParameter(1f, 0f, 1f);

    // Console-ish color depth
    public BoolParameter quantize = new BoolParameter(true);
    public ClampedIntParameter consoleSteps = new ClampedIntParameter(32, 2, 64);

    private Material _material;

    public override CustomPostProcessInjectionPoint injectionPoint =>
        CustomPostProcessInjectionPoint.AfterPostProcess;

    // IMPORTANT:
    // - `active` is the built-in enable toggle for the override component.
    // - `effectIntensity.overrideState` is true only if the checkbox is ticked in the Volume UI.
    // - This ensures: no checkbox tick => no effect, and removing the override => no effect.
    public bool IsActive()
    {
        if (!active) return false;
        if (_material == null) return false;

        // Require the override checkbox for effectIntensity to be ON
        if (!effectIntensity.overrideState) return false;

        // If the value is 0, we can skip rendering (gradual works fine above 0)
        return effectIntensity.value > 0f;
    }

    public override bool visibleInSceneView => true;

    public override void Setup()
    {
        if (shader == null)
            shader = Shader.Find("Hidden/Custom/RetroConsolePost");

        if (shader == null)
        {
            Debug.LogError("RetroConsolePostProcess: Shader missing. Assign it in the Volume Profile override UI.");
            return;
        }

        _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(_material);
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (_material == null)
        {
            HDUtils.BlitCameraTexture(cmd, source, destination);
            return;
        }

        _material.SetFloat("_EffectIntensity", effectIntensity.value);
        _material.SetFloat("_PixelSize", pixelSize.value);
        _material.SetFloat("_PixelateStrength", pixelateStrength.value);
        _material.SetFloat("_QuantizeInConsole", quantize.value ? 1f : 0f);
        _material.SetFloat("_ConsoleSteps", consoleSteps.value);

        HDUtils.BlitCameraTexture(cmd, source, destination, _material, 0);
    }
}
