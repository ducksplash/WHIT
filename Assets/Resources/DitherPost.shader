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
    TEXTURE2D(_BlueNoiseTex);
    SAMPLER(sampler_BlueNoiseTex);
    TEXTURE2D_X(_DitherExcludeMask);

    float4 _BlitScaleBias;
    float  _Intensity;          // 0-2 recommended
    float  _Steps;              // 4-16 recommended
    float  _PixelScale;         // base pixel size
    float2 _BlueNoiseSize;      // width/height of noise texture
    float  _DistanceFadeStart;  // world units (or linear depth) where intensity starts to fall
    float  _DistanceFadeEnd;    // fully faded by this distance

    // Optional: if you have depth available in the custom pass
    // TEXTURE2D_X(_CameraDepthTexture);

    struct Attributes
    {
        uint vertexID : SV_VertexID;
    };

    struct Varyings
    {
        float4 positionCS : SV_Position;
        float2 uv         : TEXCOORD0;
    };

    Varyings Vert(Attributes input)
    {
        Varyings o;
        o.uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
        o.positionCS = float4(o.uv * 2.0 - 1.0, 0.0, 1.0);
        #if UNITY_UV_STARTS_AT_TOP
        // HDRP already handles most of this, but keep consistent
        #endif
        return o;
    }

    // Simple luminance (Rec.709)
    float Luma(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }

    float4 Frag(Varyings i) : SV_Target
    {
        float2 uvFS = i.uv;
        if (_ProjectionParams.x < 0.0)
            uvFS.y = 1.0 - uvFS.y;

        // Full-res sample for the exclude mask (keeps edges crisp)
        float mask = SAMPLE_TEXTURE2D_X(_DitherExcludeMask, s_point_clamp_sampler, uvFS).r;

        if (mask > 0.5)
        {
            float2 uvFull = saturate(uvFS * _BlitScaleBias.xy + _BlitScaleBias.zw);
            float3 crisp = SAMPLE_TEXTURE2D_X(_BlitTexture, s_point_clamp_sampler, uvFull).xyz;
            return float4(crisp, 1.0);
        }

        // ---- Pixelation ----
        float pixelScale = max(_PixelScale, 1.0);
        float2 screenPixel = uvFS * _ScreenSize.xy;
        int2   blockIndex  = int2(floor(screenPixel / pixelScale));
        float2 blockCenter = (float2(blockIndex) + 0.5) * pixelScale;
        float2 uvBlock     = saturate((blockCenter * _ScreenSize.zw) * _BlitScaleBias.xy + _BlitScaleBias.zw);

        float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, s_point_clamp_sampler, uvBlock).xyz;

        // ---- Distance fade (optional but highly recommended) ----
        // If you inject linear depth, use it here.
        // Otherwise a cheap approximation from UV is better than nothing.
        float distFade = 1.0;
        // Example if you have depth:
        // float depth = LinearEyeDepth(SAMPLE_TEXTURE2D_X(_CameraDepthTexture, s_point_clamp_sampler, uvFS).r, _ZBufferParams);
        // distFade = 1.0 - saturate((depth - _DistanceFadeStart) / max(_DistanceFadeEnd - _DistanceFadeStart, 0.001));

        float intensity = _Intensity * distFade;

        // ---- Blue-noise dither (stable, tileable) ----
        float steps = max(_Steps, 2.0);
        float invSteps = 1.0 / (steps - 1.0);

        // Tile the noise correctly and keep it resolution-independent
        float2 noiseUV = (float2(blockIndex) + 0.5) / _BlueNoiseSize;
        float  t = SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, noiseUV).r - 0.5;

        // Slightly stronger dither on darks, weaker on brights (reduces the “goes white” look)
        float luma = Luma(col);
        float adapt = lerp(1.15, 0.75, saturate(luma));

        float3 dithered = col + t * invSteps * intensity * adapt;
        dithered = saturate(dithered);

        // Quantise
        float3 q = round(dithered * (steps - 1.0)) * invSteps;

        return float4(q, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
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