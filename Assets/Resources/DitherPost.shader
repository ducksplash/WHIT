Shader "Hidden/Custom/DitherPost"
{
    HLSLINCLUDE
    #pragma target 4.5
    #pragma vertex Vert
    #pragma fragment Frag

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureXR.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D_X(_BlitTexture);

    float4 _BlitScaleBias;
    float _Intensity;
    float _Steps;
    float _PixelScale;

    static const float bayer4x4[16] = {
         0,  8,  2, 10,
        12,  4, 14,  6,
         3, 11,  1,  9,
        15,  7, 13,  5
    };

    float BayerThreshold4x4(int2 p)
    {
        int x = p.x & 3;
        int y = p.y & 3;
        return (bayer4x4[y * 4 + x] / 16.0) - 0.5;
    }

    struct Attributes { uint vertexID : SV_VertexID; };
    struct Varyings  { float4 positionCS : SV_Position; float2 uv : TEXCOORD0; };

    Varyings Vert(Attributes input)
    {
        Varyings o;
        o.uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
        o.positionCS = float4(o.uv * 2.0 - 1.0, 0.0, 1.0);
        return o;
    }

    float3 Quantize(float3 c, float steps)
    {
        steps = max(steps, 2.0);
        return round(c * (steps - 1.0)) / (steps - 1.0);
    }

    float4 Frag(Varyings i) : SV_Target
    {
        float2 uvFS = i.uv;
        if (_ProjectionParams.x < 0.0)
            uvFS.y = 1.0 - uvFS.y;

        float2 uvSample = saturate(uvFS * _BlitScaleBias.xy + _BlitScaleBias.zw);

        // AfterPostProcess: this is already post-tonemap/exposure, so treat as LDR 0..1
        float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, s_linear_clamp_sampler, uvSample).xyz;

        float2 pixel = uvFS * _ScreenSize.xy;
        int2 p = int2(floor(pixel / max(_PixelScale, 1.0)));
        float t = BayerThreshold4x4(p);

        float steps = max(_Steps, 2.0);
        float stepSize = 1.0 / (steps - 1.0);

        float3 dither = (t * stepSize) * _Intensity;
        float3 q = Quantize(saturate(col + dither), steps);

        return float4(q, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        Pass
        {
            Name "DitherPost"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off
            HLSLPROGRAM
            ENDHLSL
        }
    }

    Fallback Off
}
