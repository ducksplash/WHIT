using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Concentric Circle Rotate")]
public sealed class ConcentricCircleRotate : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    // ── Ring Rotations (20 rings) ───────────────────────────────────────
    public ClampedFloatParameter ring0  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring1  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring2  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring3  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring4  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring5  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring6  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring7  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring8  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring9  = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring10 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring11 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring12 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring13 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring14 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring15 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring16 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring17 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring18 = new ClampedFloatParameter(0f, -360f, 360f);
    public ClampedFloatParameter ring19 = new ClampedFloatParameter(0f, -360f, 360f);

    // ── Horizontal Slice Offsets (20 slices) ───────────────────────────
    public ClampedFloatParameter slice0  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice1  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice2  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice3  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice4  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice5  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice6  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice7  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice8  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice9  = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice10 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice11 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice12 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice13 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice14 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice15 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice16 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice17 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice18 = new ClampedFloatParameter(0f, -2f, 2f);
    public ClampedFloatParameter slice19 = new ClampedFloatParameter(0f, -2f, 2f);

    // ── Dissolve (keep 100 cells for visibility) ───────────────────────
    public BoolParameter dissolveEnabled = new BoolParameter(false);
    public ClampedFloatParameter[] dissolveProgress = new ClampedFloatParameter[100];

    private Material m_Material;

    private readonly float[] m_Rotations = new float[20];
    private readonly float[] m_SliceOffsets = new float[20];
    private readonly float[] m_DissolveArray = new float[100];

    public ColorParameter tintColor = new ColorParameter(Color.white, true);
    
    public bool IsActive() => m_Material != null;

    public override CustomPostProcessInjectionPoint injectionPoint =>
        CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        m_Material = CoreUtils.CreateEngineMaterial("Hidden/HDRP/ConcentricCircleRotate");

        for (int i = 0; i < 100; i++)
        {
            dissolveProgress[i] = new ClampedFloatParameter(0f, 0f, 1f);
        }
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null) return;

        // Rings (20)
        m_Rotations[0]  = ring0.value;  m_Rotations[1]  = ring1.value;
        m_Rotations[2]  = ring2.value;  m_Rotations[3]  = ring3.value;
        m_Rotations[4]  = ring4.value;  m_Rotations[5]  = ring5.value;
        m_Rotations[6]  = ring6.value;  m_Rotations[7]  = ring7.value;
        m_Rotations[8]  = ring8.value;  m_Rotations[9]  = ring9.value;
        m_Rotations[10] = ring10.value; m_Rotations[11] = ring11.value;
        m_Rotations[12] = ring12.value; m_Rotations[13] = ring13.value;
        m_Rotations[14] = ring14.value; m_Rotations[15] = ring15.value;
        m_Rotations[16] = ring16.value; m_Rotations[17] = ring17.value;
        m_Rotations[18] = ring18.value; m_Rotations[19] = ring19.value;

        // Slices (20)
        m_SliceOffsets[0]  = slice0.value;  m_SliceOffsets[1]  = slice1.value;
        m_SliceOffsets[2]  = slice2.value;  m_SliceOffsets[3]  = slice3.value;
        m_SliceOffsets[4]  = slice4.value;  m_SliceOffsets[5]  = slice5.value;
        m_SliceOffsets[6]  = slice6.value;  m_SliceOffsets[7]  = slice7.value;
        m_SliceOffsets[8]  = slice8.value;  m_SliceOffsets[9]  = slice9.value;
        m_SliceOffsets[10] = slice10.value; m_SliceOffsets[11] = slice11.value;
        m_SliceOffsets[12] = slice12.value; m_SliceOffsets[13] = slice13.value;
        m_SliceOffsets[14] = slice14.value; m_SliceOffsets[15] = slice15.value;
        m_SliceOffsets[16] = slice16.value; m_SliceOffsets[17] = slice17.value;
        m_SliceOffsets[18] = slice18.value; m_SliceOffsets[19] = slice19.value;

        // Dissolve (100 cells)
        for (int i = 0; i < 100; i++)
            m_DissolveArray[i] = dissolveProgress[i].value;

        m_Material.SetFloatArray("_Rotations", m_Rotations);
        m_Material.SetFloatArray("_SliceOffsets", m_SliceOffsets);
        m_Material.SetFloatArray("_DissolveProgress", m_DissolveArray);
        m_Material.SetInt("_DissolveEnabled", dissolveEnabled.value ? 1 : 0);

        m_Material.SetTexture("_InputTexture", source);
        m_Material.SetColor("_TintColor", tintColor.value);

        HDUtils.DrawFullScreen(cmd, m_Material, destination);
        
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}