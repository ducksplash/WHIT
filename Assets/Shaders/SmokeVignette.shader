Shader "UI/SmokeVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Tex0 ("Layer 0", 2D) = "black" {}
        _Tex1 ("Layer 1", 2D) = "black" {}
        _Tex2 ("Layer 2", 2D) = "black" {}
        _Tex3 ("Layer 3", 2D) = "black" {}
        _Tex4 ("Layer 4", 2D) = "black" {}
        _Tex5 ("Layer 5", 2D) = "black" {}
        _Tex6 ("Layer 6", 2D) = "black" {}
        _Tex7 ("Layer 7", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Must be present for reliable Material.Set* / MaterialPropertyBlock on Vulkan/HDRP
        [HideInInspector] _LayerCount ("Layer Count", Float) = 0

        // Stencil / UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "SmokeVignetteUI"
            HLSLPROGRAM
            #pragma vertex VertSmoke
            #pragma fragment FragSmoke
            #pragma target 4.5
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #define MAX_LAYERS 8

            sampler2D _MainTex;
            sampler2D _Tex0, _Tex1, _Tex2, _Tex3, _Tex4, _Tex5, _Tex6, _Tex7;
            float4 _Color;

            // Keep everything inside UnityPerMaterial so SetFloatArray / SetVectorArray bind correctly
            CBUFFER_START(UnityPerMaterial)
                float _LayerCount;
                float _LayerAlpha[MAX_LAYERS];
                float _RotSpeed[MAX_LAYERS];
                float _RotDir[MAX_LAYERS];
                float4 _StartColor[MAX_LAYERS];
                float4 _EndColor[MAX_LAYERS];
                float _LerpEnabled[MAX_LAYERS];
                float _LerpSpeed[MAX_LAYERS];
                float _VignetteRadius[MAX_LAYERS];
                float _VignetteSoftness[MAX_LAYERS];
                float _VignetteIntensity[MAX_LAYERS];
            CBUFFER_END

            float4 _ClipRect;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            v2f VertSmoke(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.worldPos = IN.vertex;
                OUT.uv       = IN.uv;
                OUT.color    = IN.color * _Color;
                return OUT;
            }

            float2 RotateUV(float2 uv, float angle)
            {
                uv -= 0.5;
                float s = sin(angle);
                float c = cos(angle);
                uv = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
                return uv + 0.5;
            }

            // Constant-index after unroll – safe on Vulkan
            float4 SampleLayer(int index, float2 uv)
            {
                if (index == 0) return tex2D(_Tex0, uv);
                if (index == 1) return tex2D(_Tex1, uv);
                if (index == 2) return tex2D(_Tex2, uv);
                if (index == 3) return tex2D(_Tex3, uv);
                if (index == 4) return tex2D(_Tex4, uv);
                if (index == 5) return tex2D(_Tex5, uv);
                if (index == 6) return tex2D(_Tex6, uv);
                return tex2D(_Tex7, uv);
            }

            float4 FragSmoke(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;
                float4 result = float4(0, 0, 0, 0);
                float dist = distance(uv, float2(0.5, 0.5));

                // Constant trip count + no early-out – required for Steam Deck / Vulkan
                [unroll]
                for (int i = 0; i < MAX_LAYERS; i++)
                {
                    float active = (i < _LayerCount) ? 1.0 : 0.0;

                    float vignette = smoothstep(
                        _VignetteRadius[i],
                        _VignetteRadius[i] + _VignetteSoftness[i],
                        dist) * _VignetteIntensity[i];

                    float angle = _Time.y * _RotSpeed[i] * _RotDir[i];
                    float2 rotUV = RotateUV(uv, angle);
                    float4 tex = SampleLayer(i, rotUV);

                    float t = 0;
                    if (_LerpEnabled[i] > 0.5)
                        t = abs(frac(_Time.y * _LerpSpeed[i] * 0.5) * 2.0 - 1.0);

                    float4 tint = lerp(_StartColor[i], _EndColor[i], t);
                    float4 layer = tex * tint;

                    layer.a *= vignette * _LayerAlpha[i] * active;

                    // Standard over
                    result.rgb = lerp(result.rgb, layer.rgb, layer.a);
                    result.a   = layer.a + result.a * (1.0 - layer.a);
                }

                result *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}