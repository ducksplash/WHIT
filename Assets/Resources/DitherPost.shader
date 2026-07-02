Shader "Hidden/Custom/DitherPost" {
    HLSLINCLUDE
    #pragma target 4.5
    #pragma vertex Vert
    #pragma fragment Frag

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureXR.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D_X(_BlitTexture);

    float4 _BlitScaleBias;
    float  _Intensity;
    float  _Steps;
    float  _PixelScale;

    // Pre-normalized to -0.5..0.5 so Frag never divides
    static const float bayer4x4[16] = {
         0.0/16.0-0.5,  8.0/16.0-0.5,  2.0/16.0-0.5, 10.0/16.0-0.5,
        12.0/16.0-0.5,  4.0/16.0-0.5, 14.0/16.0-0.5,  6.0/16.0-0.5,
         3.0/16.0-0.5, 11.0/16.0-0.5,  1.0/16.0-0.5,  9.0/16.0-0.5,
        15.0/16.0-0.5,  7.0/16.0-0.5, 13.0/16.0-0.5,  5.0/16.0-0.5
    };

    float BayerThreshold4x4(int2 p)
    {
        return bayer4x4[(p.y & 3) * 4 + (p.x & 3)];
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

    float4 Frag(Varyings i) : SV_Target
    {
        float2 uvFS = i.uv;
        if (_ProjectionParams.x < 0.0)
            uvFS.y = 1.0 - uvFS.y;

        float  pixelScale = max(_PixelScale, 1.0);
        float2 screenPixel = uvFS * _ScreenSize.xy;

        // Snap to a chunky-pixel block grid (this is what actually creates the 8-bit look)
        int2   blockIndex = int2(floor(screenPixel / pixelScale));
        float2 blockCenterPx = (float2(blockIndex) + 0.5) * pixelScale;
        float2 uvBlock = blockCenterPx * _ScreenSize.zw;

        float2 uvSample = saturate(uvBlock * _BlitScaleBias.xy + _BlitScaleBias.zw);

        // Point sample: no bilinear blur between blocks
        float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, s_point_clamp_sampler, uvSample).xyz;

        float steps = max(_Steps, 2.0);
        float invStepsMinus1 = 1.0 / (steps - 1.0);

        float t = BayerThreshold4x4(blockIndex);
        float3 dithered = saturate(col + t * invStepsMinus1 * _Intensity);

        float3 q = round(dithered * (steps - 1.0)) * invStepsMinus1;

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