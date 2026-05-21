Shader "Custom/GlowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowStrength ("Glow Strength", Range(0,10)) = 1
        _GlowSpeed ("Glow Speed", Range(0,10)) = 1

        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1

        _Alpha ("Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _MainTex;

            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpeed;

            float4 _EmissionColor;
            float _EmissionStrength;

            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;

                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Base texture
                half4 tex = tex2D(_MainTex, i.uv);

                // Pulsing glow
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;

                // Glow contribution
                half3 glow =
                    tex.rgb *
                    _GlowColor.rgb *
                    pulse *
                    _GlowStrength;

                // Emission contribution
                half3 emission =
                    tex.rgb *
                    _EmissionColor.rgb *
                    _EmissionStrength;

                // Final color
                half3 finalColor =
                    tex.rgb +
                    glow +
                    emission;

                return half4(finalColor, tex.a * _Alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Transparent/Diffuse"
}