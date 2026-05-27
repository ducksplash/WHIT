using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Concentric Circle Rotate")]
public sealed class ConcentricCircleRotate : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    // One parameter per ring — VolumeParameters don't support float[] directly
    public ClampedFloatParameter ring0 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring1 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring2 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring3 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring4 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring5 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring6 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring7 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring8 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring9 = new ClampedFloatParameter(0f, -360f, 360f);

    Material m_Material;
    readonly float[] m_Rotations = new float[10];

    // Controls whether HDRP runs this effect at all
    public bool IsActive() => m_Material != null;

    // Runs after tone mapping — change to BeforePostProcess or AfterOpaqueAndSky if needed
    public override CustomPostProcessInjectionPoint injectionPoint =>
        CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        m_Material = CoreUtils.CreateEngineMaterial("Hidden/HDRP/ConcentricCircleRotate");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        m_Rotations[0] = ring0.value;
        m_Rotations[1] = ring1.value;
        m_Rotations[2] = ring2.value;
        m_Rotations[3] = ring3.value;
        m_Rotations[4] = ring4.value;
        m_Rotations[5] = ring5.value;
        m_Rotations[6] = ring6.value;
        m_Rotations[7] = ring7.value;
        m_Rotations[8] = ring8.value;
        m_Rotations[9] = ring9.value;

        m_Material.SetFloatArray("_Rotations", m_Rotations);
        m_Material.SetTexture("_InputTexture", source);

        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}