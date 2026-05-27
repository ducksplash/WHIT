Shader "Hidden/HDRP/ConcentricCircleRotate"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D_X(_InputTexture);

    float4 _TintColor;

    
    // Ring rotations (20 rings)
    float _Rotations[20];

    // Horizontal slice offsets (20 slices)
    float _SliceOffsets[20];

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord   = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    float Hash21(float2 p)
    {
        p = frac(p * float2(123.34, 456.21));
        p += dot(p, p + 45.32);
        return frac(p.x * p.y);
    }

    float4 Frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = input.texcoord;

        float aspect = _ScreenParams.x / _ScreenParams.y;

        // ── Ring Rotation ─────────────────────────────
        float2 centered = uv - 0.5;
        centered.x *= aspect;

        float radius = length(centered);
        float angle  = atan2(centered.y, centered.x);

        // Updated for 20 rings
        int ring = clamp((int)(radius * 20.0), 0, 19);
        angle += radians(_Rotations[ring]);

        float2 rotated;
        sincos(angle, rotated.y, rotated.x);
        rotated *= radius;
        rotated.x /= aspect;

        float2 ringUV = rotated + 0.5;

        // ── Slice Offset ──────────────────────────────
        int slice = clamp((int)(uv.y * 20.0), 0, 19);

        float2 finalUV = ringUV;
        finalUV.x += _SliceOffsets[slice];

        // WRAP FIX
        finalUV = frac(finalUV);

        // HDRP scale
        finalUV *= _RTHandleScale.xy;

        float4 col = SAMPLE_TEXTURE2D_X(_InputTexture, s_linear_clamp_sampler, finalUV);
        return lerp(col, col * _TintColor, _TintColor.a);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            Name "ConcentricCircleRotate"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }

    Fallback Off
}