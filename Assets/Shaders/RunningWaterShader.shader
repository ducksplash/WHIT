Shader "Custom/RunningWaterShader"
{
    Properties
    {
        _BaseMap    ("Base Map", 2D)      = "white" {}
        _NormalMap  ("Normal Map", 2D)    = "bump" {}

        _MainColor  ("Main Color", Color) = (1,1,1,1)
        _TopColor   ("Top Color", Color) = (1,1,1,1)

        _Smoothness ("Smoothness", Range(0,1)) = 0.1
        _FresnelPower ("Fresnel Power", Float) = 0.48

        _FlowDirection ("Flow Direction", Vector) = (0,1,0,0)
        _FlowSpeed     ("Flow Speed", Float)      = 1
        _Tiling        ("Tiling", Vector)         = (1,1,0,0)

        _EmissionMap       ("Emission Map", 2D) = "black" {}
        _EmissionColor     ("Emission Color", Color) = (0,0,0,1)
        _EmissionIntensity ("Emission Intensity", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "HDRenderPipeline"
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
        }

        // ============================================================
        // DEPTH ONLY
        // Used by HDRP to put this object into the camera depth buffer.
        // This is important for correct volumetric fog interaction.
        // ============================================================

        Pass
        {
            Name "DepthOnly"

            Tags
            {
                "LightMode" = "DepthOnly"
            }

            Cull Off
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            };

            v2f DepthVertex(appdata v)
            {
                v2f o;

                o.positionCS = UnityObjectToClipPos(v.vertex);

                return o;
            }

            half4 DepthFragment(v2f i) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }

        // ============================================================
        // DEPTH FORWARD ONLY
        // Some HDRP forward/depth paths use this pass name.
        // ============================================================

        Pass
        {
            Name "DepthForwardOnly"

            Tags
            {
                "LightMode" = "DepthForwardOnly"
            }

            Cull Off
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            };

            v2f DepthVertex(appdata v)
            {
                v2f o;

                o.positionCS = UnityObjectToClipPos(v.vertex);

                return o;
            }

            half4 DepthFragment(v2f i) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }

        // ============================================================
        // FORWARD COLOUR
        // ============================================================

        Pass
        {
            Name "ForwardOnly"

            Tags
            {
                "LightMode" = "ForwardOnly"
            }

            Cull Off
            ZWrite On
            ZTest LEqual
            Blend Off
            AlphaToMask Off

            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex  : POSITION;
                float3 normal  : NORMAL;
                float4 tangent : TANGENT;
                float2 uv      : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS   : SV_POSITION;
                float2 uv            : TEXCOORD0;
                float3 worldPos      : TEXCOORD1;
                float3 worldNormal   : TEXCOORD2;
                float4 worldTangent  : TEXCOORD3;
            };

            sampler2D _BaseMap;
            sampler2D _NormalMap;
            sampler2D _EmissionMap;

            float4 _MainColor;
            float4 _TopColor;

            float _Smoothness;
            float _FresnelPower;

            float4 _FlowDirection;
            float _FlowSpeed;
            float4 _Tiling;

            float4 _EmissionColor;
            float _EmissionIntensity;

            v2f vert(appdata v)
            {
                v2f o;

                o.positionCS = UnityObjectToClipPos(v.vertex);

                o.worldPos =
                    mul(unity_ObjectToWorld, v.vertex).xyz;

                o.worldNormal =
                    UnityObjectToWorldNormal(v.normal);

                float3 worldTangent =
                    UnityObjectToWorldDir(v.tangent.xyz);

                o.worldTangent =
                    float4(worldTangent, v.tangent.w);

                o.uv = v.uv;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // ----------------------------------------------------
                // Animated UVs
                // ----------------------------------------------------

                float2 flowUV =
                    i.uv * _Tiling.xy +
                    _FlowDirection.xy *
                    _Time.y *
                    _FlowSpeed;

                // ----------------------------------------------------
                // Normal map
                // ----------------------------------------------------

                half3 normalTS =
                    UnpackNormal(
                        tex2D(_NormalMap, flowUV)
                    );

                // ----------------------------------------------------
                // Tangent -> World
                // ----------------------------------------------------

                float3 worldBitangent =
                    cross(
                        i.worldNormal,
                        i.worldTangent.xyz
                    ) * i.worldTangent.w;

                float3x3 tangentToWorld =
                    float3x3(
                        i.worldTangent.xyz,
                        worldBitangent,
                        i.worldNormal
                    );

                half3 worldNormal =
                    normalize(
                        mul(
                            normalTS,
                            tangentToWorld
                        )
                    );

                // ----------------------------------------------------
                // View direction
                // ----------------------------------------------------

                float3 viewDir =
                    normalize(
                        _WorldSpaceCameraPos -
                        i.worldPos
                    );

                // ----------------------------------------------------
                // Fresnel
                // ----------------------------------------------------

                half fresnel =
                    pow(
                        1.0 -
                        saturate(
                            dot(
                                worldNormal,
                                viewDir
                            )
                        ),
                        _FresnelPower
                    );

                // ----------------------------------------------------
                // Base texture
                // ----------------------------------------------------

                half4 baseTex =
                    tex2D(
                        _BaseMap,
                        flowUV
                    );

                // ----------------------------------------------------
                // Water colour
                // ----------------------------------------------------

                half3 albedo =
                    lerp(
                        _MainColor.rgb,
                        _TopColor.rgb,
                        fresnel
                    ) *
                    baseTex.rgb;

                // ----------------------------------------------------
                // Emission
                // ----------------------------------------------------

                half3 emissionTex =
                    tex2D(
                        _EmissionMap,
                        flowUV
                    ).rgb;

                half3 emission =
                    emissionTex *
                    _EmissionColor.rgb *
                    _EmissionIntensity;

                // ----------------------------------------------------
                // Final colour
                // ----------------------------------------------------

                half3 finalColor =
                    albedo +
                    emission;

                // Absolutely opaque
                return half4(
                    finalColor,
                    1.0
                );
            }

            ENDHLSL
        }
    }
}