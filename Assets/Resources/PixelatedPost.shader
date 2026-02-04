Shader "Hidden/Custom/RetroConsolePost"
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

    // Master intensity (0..1)
    float _EffectIntensity;

    // Pixelation
    float _PixelSize;
    float _PixelateStrength;

    // Quantization
    float _QuantizeInConsole; // 0/1
    float _ConsoleSteps;      // target steps at full intensity

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

    float2 GetFullscreenUV(float2 uv)
    {
        if (_ProjectionParams.x < 0.0)
            uv.y = 1.0 - uv.y;
        return uv;
    }

    float2 ToSampleUV(float2 uvFS)
    {
        float2 uvSample = uvFS * _BlitScaleBias.xy + _BlitScaleBias.zw;
        return saturate(uvSample);
    }

    float3 SampleScene(float2 uvSample)
    {
        return SAMPLE_TEXTURE2D_X(_BlitTexture, s_linear_clamp_sampler, uvSample).xyz;
    }

    // simple gamma-ish conversions (good enough for feel; avoids needing HDRP color utils)
    float3 ToGamma(float3 lin)   { return pow(saturate(lin), 1.0 / 2.2); }
    float3 ToLinear(float3 gam)  { return pow(saturate(gam), 2.2); }

    float4 Frag(Varyings i) : SV_Target
    {
        float2 uvFS = GetFullscreenUV(i.uv);

        float3 baseLin = SampleScene(ToSampleUV(uvFS));

        // Pixelation snap
        float px = max(_PixelSize, 1.0);
        float2 screenPx = uvFS * _ScreenSize.xy;
        float2 snappedPx = (floor(screenPx / px) + 0.5) * px;
        float2 snappedUVFS = snappedPx / _ScreenSize.xy;

        float3 pixLin = SampleScene(ToSampleUV(snappedUVFS));

        float master = saturate(_EffectIntensity);

        // --- KEY CHANGE: steps become intensity-dependent ---
        // At master=0 -> very high steps (almost no quantization)
        // At master=1 -> target steps (_ConsoleSteps)
        // 256 is a good "nearly original" value for subtle end
        float targetSteps = max(_ConsoleSteps, 2.0);
        float stepsAtLow = 256.0;
        float dynSteps = lerp(stepsAtLow, targetSteps, master);

        // Quantize only if enabled
        float3 quantLin = Quantize(pixLin, dynSteps);
        float3 consoleLin = lerp(pixLin, quantLin, saturate(_QuantizeInConsole));

        // Pixelation strength (can also be intensity-shaped if you want)
        float3 modeLin = lerp(baseLin, consoleLin, saturate(_PixelateStrength));

        // --- KEY CHANGE: blend in gamma space for nicer perceived ramp ---
        float3 baseGam = ToGamma(baseLin);
        float3 modeGam = ToGamma(modeLin);
        float3 finalGam = lerp(baseGam, modeGam, master);

        float3 finalLin = ToLinear(finalGam);
        return float4(finalLin, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        Pass
        {
            Name "RetroConsolePost"
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
