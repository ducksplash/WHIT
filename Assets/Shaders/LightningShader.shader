Shader "Custom/LightningOnly"
{
    Properties
    {
        [Toggle] _LightningEnabled ("Enable Lightning", Float) = 1

        _LightningForeground ("Lightning Color", Color) = (0.7, 0.85, 1, 1)
        _CoreColor           ("Core Color", Color) = (1, 1, 1, 1)

        _LightningMinTime    ("Min Time Between Flashes", Range(0.2, 8)) = 1.5
        _LightningMaxTime    ("Max Time Between Flashes", Range(0.5, 12)) = 4.5
        _LightningMinDuration("Min Flash Duration", Range(0.02, 0.4)) = 0.06
        _LightningMaxDuration("Max Flash Duration", Range(0.05, 0.8)) = 0.18
        _LightningStrength   ("Overall Strength", Range(0.5, 10)) = 3.5

        _BoltThickness       ("Bolt Thickness", Range(0.01, 0.25)) = 0.07
        _BoltJaggedness      ("Jaggedness", Range(0, 1.5)) = 0.55
        _BoltBranches        ("Branch Amount", Range(0, 1)) = 0.4
        _GlowSize            ("Glow Size", Range(0.05, 0.6)) = 0.22
        _EdgeFade            ("Cylinder Edge Fade", Range(0.5, 6)) = 2.8

        // Optional texture detail (your bolt images)
        _LightningTex0 ("Detail Texture 0", 2D) = "black" {}
        _LightningTex1 ("Detail Texture 1", 2D) = "black" {}
        _DetailStrength ("Texture Detail Strength", Range(0, 2)) = 0.7
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos       : SV_POSITION;
                float3 worldPos  : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir   : TEXCOORD2;
                float3 localPos  : TEXCOORD3;   // object space – important for centering
            };

            float4 _LightningForeground;
            float4 _CoreColor;
            float  _LightningEnabled;
            float  _LightningMinTime, _LightningMaxTime;
            float  _LightningMinDuration, _LightningMaxDuration;
            float  _LightningStrength;
            float  _BoltThickness, _BoltJaggedness, _BoltBranches;
            float  _GlowSize, _EdgeFade, _DetailStrength;

            sampler2D _LightningTex0;
            sampler2D _LightningTex1;

            float Hash21(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Cheap 1D value noise
            float Noise(float x) {
                float i = floor(x);
                float f = frac(x);
                float u = f * f * (3.0 - 2.0 * f);
                return lerp(Hash21(float2(i, 0.13)), Hash21(float2(i + 1.0, 0.13)), u);
            }

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = _WorldSpaceCameraPos - o.worldPos;
                o.localPos = v.vertex.xyz;          // keep object-space for centering
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                if (_LightningEnabled < 0.5)
                    return 0;

                // ── Flash timing (same spirit as your old script) ──────
                float t = _Time.y;
                float cycle = floor(t / _LightningMaxTime);
                float seed = Hash21(float2(cycle, 7.1));
                float interval = lerp(_LightningMinTime, _LightningMaxTime, Hash21(float2(cycle, 3.9)));
                float duration = lerp(_LightningMinDuration, _LightningMaxDuration, Hash21(float2(cycle, 11.3)));
                float localT = t - cycle * _LightningMaxTime;
                float flash = step(localT, interval) * step(interval - localT, duration);
                if (flash < 0.001) return 0;

                // ── Center the bolt on the local Y axis ────────────────
                float3 lp = i.localPos;
                float height = lp.y;                     // -0.5 → 0.5 if pivot is center
                float2 radial = lp.xz;                   // distance from central axis
                float distFromAxis = length(radial);

                // ── Procedural jagged path (inspired by your LineRenderer code) ──
                // Main bolt wanders left/right as it goes down
                float jag = (Noise(height * 8.0 + seed * 20.0) - 0.5) * _BoltJaggedness;
                float jag2 = (Noise(height * 14.0 + seed * 37.0) - 0.5) * _BoltJaggedness * 0.6;

                // Distance to the wandering center line
                float2 boltCenter = float2(jag, jag2);
                float distToBolt = length(radial - boltCenter);

                // Main bolt body
                float bolt = smoothstep(_BoltThickness, 0.0, distToBolt);

                // Soft glow around the bolt
                float glow = smoothstep(_GlowSize, 0.0, distToBolt);

                // Simple branches (cheap)
                float branch = 0.0;
                if (_BoltBranches > 0.01) {
                    float b = (Noise(height * 22.0 + seed * 50.0) - 0.5);
                    float2 branchOffset = float2(b, b * 0.7) * 0.15;
                    float distBranch = length(radial - boltCenter - branchOffset);
                    branch = smoothstep(_BoltThickness * 0.7, 0.0, distBranch) * _BoltBranches;
                }

                // ── Optional texture detail (your bolt images) ─────────
                // Map them only near the bolt so they don’t wrap the whole cylinder
                float2 detailUV = float2(distToBolt * 4.0, height * 2.0 + seed);
                float detail = 0.0;
                if (_DetailStrength > 0.01) {
                    float d0 = tex2D(_LightningTex0, detailUV).r;
                    float d1 = tex2D(_LightningTex1, detailUV * 1.3 + 0.17).r;
                    detail = max(d0, d1) * _DetailStrength;
                    detail *= smoothstep(0.15, 0.0, distToBolt); // only near the core
                }

                // ── Cylinder edge fade (so the mesh outline disappears) ─
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(i.viewDir);
                float edge = 1.0 - pow(1.0 - saturate(dot(N, V)), _EdgeFade);

                // ── Compose ────────────────────────────────────────────
                float core = bolt + branch * 0.6 + detail;
                float final = (core * 1.8 + glow * 0.6) * flash * _LightningStrength * edge;

                half3 color = lerp(_LightningForeground.rgb, _CoreColor.rgb, bolt);
                return half4(color * final, final);
            }
            ENDHLSL
        }
    }
    FallBack "Transparent/Diffuse"
}