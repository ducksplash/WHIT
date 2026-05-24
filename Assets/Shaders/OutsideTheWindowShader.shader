Shader "Custom/OutsideTheWindowShader"
{
    Properties
    {
        _CubeTex ("Cubemap (6 Faces)", CUBE) = "" {}
        _TintColor ("Tint Color (RGBA)", Color) = (1,1,1,1)

        _OverlayTex ("Overlay Texture (RGBA)", 2D) = "white" {}
        _OverlayTint ("Overlay Tint", Color) = (1,1,1,1)
        _OverlayTiling ("Overlay Tiling", Vector) = (1,1,0,0)
        _OverlayRotation ("Overlay Rotation", Range(0,360)) = 0
        _OverlayStrength ("Overlay Strength", Range(0,1)) = 1

        _RainTex ("Rain Texture (RGBA)", 2D) = "white" {}
        _RainTint ("Rain Tint (Sides)", Color) = (0.8, 0.9, 1.0, 1)
        _RainTiling ("Rain Tiling (Sides)", Vector) = (12, 8, 0, 0)
        _RainSpeed ("Rain Speed", Range(0, 20)) = 8.0
        _RainStrength ("Rain Strength", Range(0,1)) = 0.75

        [Toggle] _RainFront  ("Enable Rain - Front", Float) = 1
        [Toggle] _RainBack   ("Enable Rain - Back", Float) = 1
        [Toggle] _RainLeft   ("Enable Rain - Left", Float) = 1
        [Toggle] _RainRight  ("Enable Rain - Right", Float) = 1
        [Toggle] _RainTop    ("Enable Rain - Top", Float) = 1
        [Toggle] _RainBottom ("Enable Rain - Bottom", Float) = 0

        _TopRainDensity ("Top Rain Density", Range(5, 80)) = 38
        _TopRainSpeed ("Top Rain Speed", Range(0.5, 8)) = 3.2
        _TopRainSize ("Top Rain Size", Range(0.1, 4.0)) = 1.4
        _TopRainVariation ("Top Rain Size Variation", Range(0, 1)) = 0.75

        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowStrength ("Glow Strength", Range(0,10)) = 1
        _GlowSpeed ("Glow Speed", Range(0,10)) = 1

        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1

        _Alpha ("Alpha", Range(0,1)) = 1

        _RotationY ("Horizontal Rotation", Range(0,360)) = 0
        _RotationX ("Vertical Rotation", Range(-180,180)) = 0

        [Toggle] _LightningEnabled ("Enable Lightning", Float) = 1
        _LightningForeground ("Lightning Foreground Color", Color) = (1,1,1,1)
        _LightningMinTime ("Min Time Between Flashes", Range(0.1,10)) = 2
        _LightningMaxTime ("Max Time Between Flashes", Range(0.1,20)) = 6
        _LightningMinDuration ("Min Flash Duration", Range(0.01,1)) = 0.05
        _LightningMaxDuration ("Max Flash Duration", Range(0.01,2)) = 0.2
        _LightningStrength ("Lightning Strength", Range(0,5)) = 1
        _LightningTiling ("Lightning Tiling", Vector) = (1,1,0,0)

        _LightningTex0 ("Lightning Texture 0", 2D) = "white" {}
        _LightningTex1 ("Lightning Texture 1", 2D) = "white" {}
        _LightningTex2 ("Lightning Texture 2", 2D) = "white" {}
        _LightningTex3 ("Lightning Texture 3", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 worldDir : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            samplerCUBE _CubeTex;
            sampler2D _OverlayTex;
            sampler2D _RainTex;

            sampler2D _LightningTex0;
            sampler2D _LightningTex1;
            sampler2D _LightningTex2;
            sampler2D _LightningTex3;

            float4 _TintColor;

            float4 _OverlayTint;
            float4 _OverlayTiling;
            float _OverlayStrength;

            float4 _RainTint;
            float4 _RainTiling;
            float _RainSpeed;
            float _RainStrength;

            float _RainFront, _RainBack, _RainLeft, _RainRight, _RainTop, _RainBottom;

            float _TopRainDensity, _TopRainSpeed, _TopRainSize, _TopRainVariation;

            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpeed;

            float4 _EmissionColor;
            float _EmissionStrength;

            float _LightningEnabled;
            float4 _LightningForeground;
            float _LightningMinTime, _LightningMaxTime;
            float _LightningStrength;
            float4 _LightningTiling;

            float _Alpha;

            float _RotationY;
            float _RotationX;

            // ================= HELPERS =================

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 rotUV(float2 p, float a)
            {
                float s = sin(a);
                float c = cos(a);
                p -= 0.5;
                p = float2(p.x * c - p.y * s, p.x * s + p.y * c);
                return p + 0.5;
            }

            float2 GetFaceUVStable(float3 dir, out bool isTop)
            {
                float3 a = abs(dir);
                isTop = (a.y >= a.x && a.y >= a.z && dir.y > 0.0);

                float2 uv;
                if (a.x >= a.y && a.x >= a.z)
                    uv = float2(dir.z, dir.y) / a.x;
                else if (a.z >= a.x && a.z >= a.y)
                    uv = float2(dir.x, dir.y) / a.z;
                else
                    uv = float2(dir.x, dir.z) / a.y;

                return uv * 0.5 + 0.5;
            }

            int GetFaceIndex(float3 dir)
            {
                float3 a = abs(dir);
                if (a.x > a.y && a.x > a.z) return (dir.x > 0) ? 0 : 1;
                if (a.z > a.x && a.z > a.y) return (dir.z > 0) ? 2 : 3;
                return (dir.y > 0) ? 4 : 5;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldDir = worldPos - _WorldSpaceCameraPos;
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldDir);

                half4 tex = texCUBE(_CubeTex, dir) * _TintColor;

                // ================= RAIN =================
                bool isTopFace;
                float2 rainUV = GetFaceUVStable(dir, isTopFace);

                int faceIndex = GetFaceIndex(dir);

                bool enable = true;
                if (faceIndex == 0) enable = _RainRight;
                else if (faceIndex == 1) enable = _RainLeft;
                else if (faceIndex == 2) enable = _RainFront;
                else if (faceIndex == 3) enable = _RainBack;
                else if (faceIndex == 4) enable = _RainTop;
                else if (faceIndex == 5) enable = _RainBottom;

                half4 rain = 0;

                if (enable)
                {
                    if (isTopFace)
                    {
                        float2 topUV = rainUV * _TopRainDensity;
                        float2 cell = floor(topUV);
                        float2 local = frac(topUV);

                        float h = Hash21(cell);
                        float h2 = Hash21(cell * 1.37);

                        float fade = sin(_Time.y * _TopRainSpeed + h * 6.28);
                        fade = smoothstep(0.2, 0.8, fade);

                        float size = _TopRainSize * (1 - h2 * _TopRainVariation);

                        float d = length(local - 0.5);
                        float drop = smoothstep(size * 0.2, 0.0, d);

                        rain.rgb = _RainTint.rgb * drop * fade;
                        rain.a = drop * fade;
                    }
                    else
                    {
                        rainUV *= _RainTiling.xy;

                        float2 cell = floor(rainUV);
                        float h = Hash21(cell);

                        float speed = lerp(0.6, 1.6, h);

                        rainUV.x += sin(h * 10 + _Time.y);
                        rainUV.y += _Time.y * _RainSpeed * speed;

                        rain = tex2D(_RainTex, rainUV) * _RainTint;
                    }
                }

                // ================= LIGHTNING =================
                if (_LightningEnabled > 0.5)
                {
                    float t = _Time.y;

                    float cycle = floor(t / _LightningMaxTime);     
                    
                    float seedBase =floor(t / _LightningMaxTime);
                    float intervalSeed = Hash21(float2(seedBase, 9.1));

                    float interval = lerp(_LightningMinTime, _LightningMaxTime, intervalSeed);

                    // actual start time of this cycle window
                    float cycleStart = seedBase * _LightningMaxTime;

                    // correct local time within window
                    float localTime = t - cycleStart;

                    // ensure strict cutoff (prevents “sticky flash”)
                    float flash = step(localTime, interval) * step(interval - localTime, 0.15);

                    tex.rgb += tex.rgb * _GlowColor.rgb * flash;

                    float3 dirN = normalize(i.worldDir);

                    float2 baseUV = float2(
                        atan2(dirN.z, dirN.x) / 6.2831853 + 0.5,
                        asin(dirN.y) / 3.1415926 + 0.5
                    );

                    float2 uv = baseUV;

                    float mask = flash * _LightningStrength;


                    
                    float seed = Hash21(float2(cycle, 5.3));

                    half4 c0 = tex2D(_LightningTex0, rotUV(uv * _LightningTiling.xy, seed * 6.28));
                    half4 c1 = tex2D(_LightningTex1, rotUV(uv * _LightningTiling.xy, seed * 12.13));
                    half4 c2 = tex2D(_LightningTex2, rotUV(uv * _LightningTiling.xy, seed * 21.37));
                    half4 c3 = tex2D(_LightningTex3, rotUV(uv * _LightningTiling.xy, seed * 33.91));

                    half4 lightningTex = (c0 + c1 + c2 + c3) * 0.25;

                    half3 colored = lightningTex.rgb * _LightningForeground.rgb;

                    float alpha = saturate((c0.a + c1.a + c2.a + c3.a) * 0.25);

                    tex.rgb += colored * alpha * mask;
                }

                // ================= APPLY RAIN AFTER LIGHTNING =================
                tex.rgb = lerp(tex.rgb, rain.rgb, rain.a * _RainStrength);

                // ================= OVERLAY =================
                float2 overlayUV = i.uv * _OverlayTiling.xy;
                half4 overlay = tex2D(_OverlayTex, overlayUV) * _OverlayTint;
                tex.rgb = lerp(tex.rgb, overlay.rgb, overlay.a * _OverlayStrength);

                // ================= FINAL =================
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                tex.rgb += tex.rgb * _GlowColor.rgb * pulse * _GlowStrength;
                tex.rgb += tex.rgb * _EmissionColor.rgb * _EmissionStrength;

                return half4(tex.rgb, tex.a * _Alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Transparent/Diffuse"
}