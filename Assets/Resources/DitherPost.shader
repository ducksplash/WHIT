Shader "Hidden/Custom/DitherPost" {
    HLSLINCLUDE
    #pragma target 4.5
    #pragma vertex Vert
    #pragma fragment Frag

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureXR.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D_X(_BlitTexture);
    TEXTURE2D(_BlueNoiseTex);
    SAMPLER(sampler_BlueNoiseTex);

    TEXTURE2D_X(_DitherExcludeMask);
    
    float4 _BlitScaleBias;
    float  _Intensity;
    float  _Steps;
    float  _PixelScale;
    float2 _BlueNoiseSize; // texture width/height in texels, set from C#

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

        // Sample at full resolution so excluded-object edges stay crisp, not blocky
        float mask = SAMPLE_TEXTURE2D_X(_DitherExcludeMask, s_point_clamp_sampler, uvFS).r;

        float  pixelScale = max(_PixelScale, 1.0);
        float2 screenPixel = uvFS * _ScreenSize.xy;

        int2   blockIndex = int2(floor(screenPixel / pixelScale));
        float2 blockCenterPx = (float2(blockIndex) + 0.5) * pixelScale;
        float2 uvBlock = blockCenterPx * _ScreenSize.zw;

        float2 uvBlockSample = saturate(uvBlock * _BlitScaleBias.xy + _BlitScaleBias.zw);

        if (mask > 0.5)
        {
            // Excluded layer: bypass pixelation and dithering entirely
            float2 uvFullSample = saturate(uvFS * _BlitScaleBias.xy + _BlitScaleBias.zw);
            float3 crisp = SAMPLE_TEXTURE2D_X(_BlitTexture, s_point_clamp_sampler, uvFullSample).xyz;
            return float4(crisp, 1.0);
        }

        float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, s_point_clamp_sampler, uvBlockSample).xyz;

        float steps = max(_Steps, 2.0);
        float invStepsMinus1 = 1.0 / (steps - 1.0);

        float2 noiseUV = (float2(blockIndex) + 0.5) / _BlueNoiseSize;
        float  t = SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, noiseUV).r - 0.5;

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