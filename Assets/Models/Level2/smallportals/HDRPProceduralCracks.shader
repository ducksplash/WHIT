Shader "Custom/HDRPProceduralCracks"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,0.1)
        _CrackColor ("Crack Color", Color) = (0,0,0,1)
        _Density ("Crack Density", Range(1, 50)) = 10
        _CrackWidth ("Crack Width", Range(0.001, 0.1)) = 0.02
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _EmissionStrength ("Crack Emission", Range(0, 10)) = 0   // NEW
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "ForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;

            float4 _BaseColor;
            float4 _CrackColor;
            float _Density;
            float _CrackWidth;
            float _Transparency;
            float _EmissionStrength; // NEW

            // Random helper
            float2 Hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            float CrackVoronoi(float2 uv, float density)
            {
                uv *= density;
                float2 cell = floor(uv);
                float2 fracUV = frac(uv);

                float minDist1 = 10.0;
                float minDist2 = 10.0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 randomOffset = Hash2(cell + neighbor);
                        float2 feature = neighbor + randomOffset - fracUV;
                        float dist = length(feature);

                        if (dist < minDist1)
                        {
                            minDist2 = minDist1;
                            minDist1 = dist;
                        }
                        else if (dist < minDist2)
                        {
                            minDist2 = dist;
                        }
                    }
                }

                float edge = minDist2 - minDist1;
                float cracks = smoothstep(_CrackWidth * 2.0, _CrackWidth, edge);
                return cracks;
            }

            Varyings Vert(Attributes input)
            {
                Varyings o;
                float4 worldPos = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0));
                o.positionCS = mul(unity_MatrixVP, worldPos);
                o.uv = input.uv;
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float cracks = CrackVoronoi(i.uv, _Density);

                // Color and emission
                float3 baseColor = _BaseColor.rgb;
                float3 crackColor = _CrackColor.rgb * (1.0 + _EmissionStrength * cracks); // boost glow
                float3 finalColor = lerp(baseColor, crackColor, cracks);

                float alpha = lerp(_Transparency, 1.0, cracks);
                return float4(finalColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
