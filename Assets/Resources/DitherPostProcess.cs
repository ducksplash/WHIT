using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable, VolumeComponentMenu("Post-processing/Custom/Dither Post")]
public sealed class DitherPostProcess : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    // Assign in the Volume Profile override UI (recommended for builds)
    [SerializeField] private Shader shader;

    public ClampedFloatParameter intensity = new ClampedFloatParameter(1f, 0f, 1f);
    public ClampedIntParameter steps = new ClampedIntParameter(16, 2, 64);
    public ClampedFloatParameter pixelScale = new ClampedFloatParameter(1f, 1f, 8f);

    private Material _material;

    public override CustomPostProcessInjectionPoint injectionPoint =>
        CustomPostProcessInjectionPoint.AfterPostProcess;

    public bool IsActive() => _material != null && intensity.value > 0f;
    public override bool visibleInSceneView => true;

    public override void Setup()
    {
        // Fallback for editor convenience only (don’t rely on this for builds)
        if (shader == null)
            shader = Shader.Find("Hidden/Custom/DitherPost");

        if (shader == null)
        {
            Debug.LogError("DitherPostProcess: Shader missing. Assign it in the Volume Profile override UI.");
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

        _material.SetFloat("_Intensity", intensity.value);
        _material.SetFloat("_Steps", steps.value);
        _material.SetFloat("_PixelScale", pixelScale.value);

        HDUtils.BlitCameraTexture(cmd, source, destination, _material, 0);
    }
}