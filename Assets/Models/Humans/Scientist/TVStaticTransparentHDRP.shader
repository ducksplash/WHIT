Shader "Custom/TVStaticTransparent_NoIncludes"
{
    Properties
    {
        _BaseColor("Template Color (Tint)", Color) = (1,1,1,1)
        _TemplateTex("Template Texture (RGB)", 2D) = "white" {}
        _UseTemplateTex("Use Template Texture (0/1)", Float) = 0

        _NoiseScale("Noise Scale", Float) = 200
        _Speed("Speed", Float) = 2
        _Contrast("Contrast", Float) = 2

        _Opacity("Opacity", Range(0,1)) = 1
        _BlackTransparentThreshold("Black -> Transparent Threshold", Range(0,1)) = 0.08

        _UseScreenSpace("Use Screen Space UV (0/1)", Float) = 1
        _FlipScreenY("Flip Screen Y (0/1)", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="ForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            // ---- Minimal built-in globals (declared manually; no includes) ----
            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;
            float4   _Time;

            sampler2D _TemplateTex;

            float4 _BaseColor;
            float  _UseTemplateTex;

            float  _NoiseScale;
            float  _Speed;
            float  _Contrast;

            float  _Opacity;
            float  _BlackTransparentThreshold;

            float  _UseScreenSpace;
            float  _FlipScreenY;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 clipPos    : TEXCOORD1;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float TVStatic(float2 uv)
            {
                float2 p = uv * _NoiseScale;

                float t = _Time.y * _Speed;
                float frameJitter = floor(t * 24.0) * 0.1234;

                float n1 = Hash21(p + frameJitter);
                float n2 = Hash21(p * 1.37 + frameJitter + 3.1);

                float n = n1 * 0.65 + n2 * 0.35;
                n = pow(saturate(n), max(0.001, _Contrast));
                return n;
            }

            float Luminance(float3 c)
            {
                return dot(c, float3(0.299, 0.587, 0.114));
            }

            float4 ObjectToClip(float3 positionOS)
            {
                float4 world = mul(unity_ObjectToWorld, float4(positionOS, 1.0));
                return mul(unity_MatrixVP, world);
            }

            float2 ClipToScreenUV(float4 clipPos)
            {
                float2 uv = clipPos.xy / max(1e-5, clipPos.w); // -1..1
                uv = uv * 0.5 + 0.5;                            //  0..1
                uv.y = lerp(uv.y, 1.0 - uv.y, step(0.5, _FlipScreenY));
                return uv;
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                float4 clip = ObjectToClip(IN.positionOS);
                OUT.positionCS = clip;
                OUT.clipPos = clip;
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 screenUV = ClipToScreenUV(IN.clipPos);
                float2 uv = lerp(IN.uv, screenUV, saturate(_UseScreenSpace));

                float templateColorLum = Luminance(_BaseColor.rgb);
                float templateTexLum   = Luminance(tex2D(_TemplateTex, uv).rgb);

                float templateValue = lerp(templateColorLum, templateTexLum, step(0.5, _UseTemplateTex));

                float staticVal = saturate(TVStatic(uv) * templateValue);

                // Black -> transparent
                float alpha = saturate((staticVal - _BlackTransparentThreshold) /
                                       max(1e-5, (1.0 - _BlackTransparentThreshold)));
                alpha *= _Opacity;

                float3 col = _BaseColor.rgb * staticVal;

                return float4(col, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}