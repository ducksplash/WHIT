Shader "Custom/WaterShader"
{
    Properties
    {
        _BaseMap    ("Base Map", 2D)      = "white" {}
        _NormalMap  ("Normal Map", 2D)    = "bump" {}

        _MainColor  ("Main Color", Color) = (1,1,1,1)
        _TopColor   ("Top Color", Color)  = (1,1,1,1)

        _Smoothness ("Smoothness", Range(0,1)) = 0.1
        _PanSpeed   ("Pan Speed", Float)       = 0
        _FresnelPower ("Fresnel Power", Float) = 0.48

        _Tiling1 ("Normal Tiling 1", Vector) = (90,90,0,0)
        _PanDir1 ("Pan Direction 1", Vector) = (0,1,0,0)
        _Tiling2 ("Normal Tiling 2", Vector) = (99,99,0,0)
        _PanDir2 ("Pan Direction 2", Vector) = (0,-1,0,0)

        _Alpha ("Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Transparent" }
        Cull Off
        ZWrite On

        Pass
        {
            HLSLPROGRAM
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
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 worldPos   : TEXCOORD1;
                float3 worldNormal   : TEXCOORD2;
                float4 worldTangent  : TEXCOORD3;
            };

            sampler2D _BaseMap;
            sampler2D _NormalMap;

            float4 _MainColor, _TopColor;
            float  _Smoothness;
            float  _PanSpeed;
            float  _FresnelPower;
            float4 _Tiling1, _PanDir1;
            float4 _Tiling2, _PanDir2;
            float  _Alpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldTangent = float4(worldTangent, v.tangent.w);
                o.uv = v.uv;
                return o;
            }

            half3 SampleAndBlendNormals(float2 baseUV, float time)
            {
                float2 uv1 = baseUV * _Tiling1.xy + _PanDir1.xy * time;
                float2 uv2 = baseUV * _Tiling2.xy + _PanDir2.xy * time;

                half3 n1 = UnpackNormal(tex2D(_NormalMap, uv1));
                half3 n2 = UnpackNormal(tex2D(_NormalMap, uv2));

                return normalize(half3(n1.xy + n2.xy, n1.z * n2.z));
            }

            half4 frag(v2f i) : SV_Target
            {
                float time = _Time.y * _PanSpeed;

                half3 normalTS = SampleAndBlendNormals(i.uv, time);

                float3 worldBitangent = cross(i.worldNormal, i.worldTangent.xyz) * i.worldTangent.w;
                float3x3 tangentToWorld = float3x3(i.worldTangent.xyz, worldBitangent, i.worldNormal);
                half3 worldNormal = normalize(mul(normalTS, tangentToWorld));

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half fresnel = pow(1.0 - saturate(dot(worldNormal, viewDir)), _FresnelPower);

                half4 baseTex = tex2D(_BaseMap, i.uv);
                half3 albedo = lerp(_MainColor.rgb, _TopColor.rgb, fresnel) * baseTex.rgb;

                return half4(albedo, _Alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}