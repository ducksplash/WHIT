Shader "Hidden/HDRP/ConcentricCircleRotate"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D_X(_InputTexture);         // XR-compatible texture type
    float _Rotations[10];

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
        // HDRP built-ins: produce a correct full-screen triangle
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord   = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    float4 Frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // texcoord is [0,1] in viewport space — map it into actual RTHandle content space
        float2 uv = input.texcoord * _RTHandleScale.xy;

        float  aspect   = _ScreenParams.x / _ScreenParams.y;

        // Work in [0,1] viewport space for ring/rotation logic, NOT RTHandle space
        float2 viewUV   = input.texcoord;
        float2 centered = viewUV - 0.5;
        centered.x *= aspect;

        float radius = length(centered);
        float angle  = atan2(centered.y, centered.x);

        int ring = clamp((int)(radius * 10.0), 0, 9);
        angle += radians(_Rotations[ring]);

        float2 rotated;
        sincos(angle, rotated.y, rotated.x);
        rotated   *= radius;
        rotated.x /= aspect;

        // finalUV is in [0,1] viewport space — scale it into RTHandle content space before sampling
        float2 finalUV = (rotated + 0.5) * _RTHandleScale.xy;

        return SAMPLE_TEXTURE2D_X(_InputTexture, s_linear_clamp_sampler, finalUV);
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