Shader "Custom/GlowCubemapShaderRotatableTintedDoubleSided"
{
    Properties
    {
        _CubeTex ("Cubemap (6 Faces)", CUBE) = "" {}

        _TintColor ("Tint Color (RGBA)", Color) = (1,1,1,1)

        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowStrength ("Glow Strength", Range(0,10)) = 1
        _GlowSpeed ("Glow Speed", Range(0,10)) = 1

        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1

        _Alpha ("Alpha", Range(0,1)) = 1

        _RotationY ("Horizontal Rotation", Range(0,360)) = 0
        _RotationX ("Vertical Rotation", Range(-180,180)) = 0

        [Toggle] _DoubleSided ("Double Sided", Float) = 0
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
        Cull Off

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

            float4 _TintColor;

            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpeed;

            float4 _EmissionColor;
            float _EmissionStrength;

            float _Alpha;

            float _RotationY;
            float _RotationX;

            float _DoubleSided;

            float3 RotateY(float3 dir, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);

                return float3(
                    dir.x * c - dir.z * s,
                    dir.y,
                    dir.x * s + dir.z * c
                );
            }

            float3 RotateX(float3 dir, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);

                return float3(
                    dir.x,
                    dir.y * c - dir.z * s,
                    dir.y * s + dir.z * c
                );
            }

            v2f vert(appdata v)
            {
                v2f o;

                o.positionCS = UnityObjectToClipPos(v.vertex);

                float3 worldPos =
                    mul(unity_ObjectToWorld, v.vertex).xyz;

                o.worldDir = worldPos - _WorldSpaceCameraPos;

                return o;
            }

            half4 frag(v2f i, bool facing : SV_IsFrontFace) : SV_Target
            {
                if (_DoubleSided < 0.5 && !facing)
                {
                    discard;
                }

                float3 dir = normalize(i.worldDir);

                dir = RotateY(dir, radians(_RotationY));
                dir = RotateX(dir, radians(_RotationX));

                half4 tex = texCUBE(_CubeTex, dir);

                tex *= _TintColor;

                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;

                half3 glow =
                    tex.rgb *
                    _GlowColor.rgb *
                    pulse *
                    _GlowStrength;

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