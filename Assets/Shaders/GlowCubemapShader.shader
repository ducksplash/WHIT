Shader "Custom/GlowCubemapShader"
{
    Properties
    {
        _CubeTex ("Cubemap (6 Faces)", CUBE) = "" {}

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            samplerCUBE _CubeTex;

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

                // World direction for cubemap lookup
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldDir = worldPos - _WorldSpaceCameraPos;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldDir);

                // Sample cubemap
                half4 tex = texCUBE(_CubeTex, dir);

                // Pulse
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;

                // Glow
                half3 glow =
                    tex.rgb *
                    _GlowColor.rgb *
                    pulse *
                    _GlowStrength;

                // Emission
                half3 emission =
                    tex.rgb *
                    _EmissionColor.rgb *
                    _EmissionStrength;

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