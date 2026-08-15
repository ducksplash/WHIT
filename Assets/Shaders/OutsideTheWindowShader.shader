Shader "Custom/OutsideTheWindowShader"
{
    Properties
    {
        _CubeTex          ("Cubemap (6 Faces)", CUBE)          = "" {}
        _TintColor        ("Tint Color (RGBA)", Color)          = (1,1,1,1)

        [Toggle] _OverlayEnabled ("Enable Overlay", Float)      = 1
        _OverlayTex       ("Overlay Texture (RGBA)", 2D)        = "white" {}
        _OverlayTint      ("Overlay Tint", Color)               = (1,1,1,1)
        _OverlayTiling    ("Overlay Tiling", Vector)            = (1,1,0,0)
        _OverlayStrength  ("Overlay Strength", Range(0,1))      = 1

        [Toggle] _RainEnabled ("Enable Rain (Master)", Float)   = 1
        _RainTex          ("Rain Texture (RGBA)", 2D)           = "white" {}
        _RainTint         ("Rain Tint (Sides)", Color)          = (0.8, 0.9, 1.0, 1)
        _RainTiling       ("Rain Tiling (Sides)", Vector)       = (12, 8, 0, 0)
        _RainSpeed        ("Rain Speed", Range(0, 20))          = 8.0
        _RainStrength     ("Rain Strength", Range(0,1))         = 0.75
        [Toggle] _RainFront   ("Enable Rain - Front",  Float)  = 1
        [Toggle] _RainBack    ("Enable Rain - Back",   Float)  = 1
        [Toggle] _RainLeft    ("Enable Rain - Left",   Float)  = 1
        [Toggle] _RainRight   ("Enable Rain - Right",  Float)  = 1
        [Toggle] _RainTop     ("Enable Rain - Top",    Float)  = 1
        [Toggle] _RainBottom  ("Enable Rain - Bottom", Float)  = 0
        _TopRainDensity   ("Top Rain Density",          Range(5, 80))    = 38
        _TopRainSpeed     ("Top Rain Speed",            Range(0.5, 8))   = 3.2
        _TopRainSize      ("Top Rain Size",             Range(0.1, 4.0)) = 1.4
        _TopRainVariation ("Top Rain Size Variation",   Range(0, 1))     = 0.75

        [Toggle] _GlowEnabled ("Enable Pulsing Glow", Float)    = 1
        _GlowColor        ("Glow Color", Color)                 = (1,1,1,1)
        _GlowStrength     ("Glow Strength",   Range(0,10))      = 1
        _GlowSpeed        ("Glow Speed",      Range(0,10))      = 1

        [Toggle] _EmissionEnabled ("Enable Emission", Float)    = 1
        _EmissionColor    ("Emission Color",  Color)            = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10))    = 1

        [Toggle] _FresnelEnabled ("Enable Fresnel Rim", Float)  = 1
        _Smoothness       ("Smoothness", Range(0,1))            = 0.5
        _Metallic         ("Metallic",   Range(0,1))            = 0.0

        _Alpha            ("Alpha",      Range(0,1))            = 1
        _RotationY        ("Horizontal Rotation", Range(0,360)) = 0
        _RotationX        ("Vertical Rotation", Range(-180,180))= 0

        [Toggle] _MovementEnabled  ("Enable Movement", Float)         = 0
        _MovementSpeed             ("Movement Speed", Range(0, 50))   = 5
        _MovementDirection         ("Movement Direction (X=Yaw, Y=Pitch)", Vector) = (1,0,0,0)

        [Toggle] _LightningEnabled    ("Enable Lightning",         Float)  = 1
        _LightningForeground          ("Lightning Foreground Color", Color) = (1,1,1,1)
        _LightningMinTime             ("Min Time Between Flashes", Range(0.1,10))  = 2
        _LightningMaxTime             ("Max Time Between Flashes", Range(0.1,20))  = 6
        _LightningMinDuration         ("Min Flash Duration",       Range(0.01,1))  = 0.05
        _LightningMaxDuration         ("Max Flash Duration",       Range(0.01,2))  = 0.2
        _LightningStrength            ("Lightning Strength",       Range(0,5))     = 1
        _LightningTiling              ("Lightning Tiling", Vector)                 = (1,1,0,0)
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
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 worldDir   : TEXCOORD0;
                float2 uv         : TEXCOORD1;
            };

            samplerCUBE _CubeTex;
            sampler2D   _OverlayTex;
            sampler2D   _RainTex;
            sampler2D   _LightningTex0, _LightningTex1, _LightningTex2, _LightningTex3;

            float4 _TintColor;

            float  _OverlayEnabled;
            float4 _OverlayTint, _OverlayTiling;
            float  _OverlayStrength;

            float  _RainEnabled;
            float4 _RainTint, _RainTiling;
            float  _RainSpeed, _RainStrength;
            float  _RainFront, _RainBack, _RainLeft, _RainRight, _RainTop, _RainBottom;
            float  _TopRainDensity, _TopRainSpeed, _TopRainSize, _TopRainVariation;

            float  _GlowEnabled;
            float4 _GlowColor;
            float  _GlowStrength, _GlowSpeed;

            float  _EmissionEnabled;
            float4 _EmissionColor;
            float  _EmissionStrength;

            float  _FresnelEnabled;
            float  _Smoothness, _Metallic;

            float  _MovementEnabled;
            float  _MovementSpeed;
            float4 _MovementDirection;

            float  _LightningEnabled;
            float4 _LightningForeground;
            float  _LightningMinTime, _LightningMaxTime, _LightningStrength;
            float4 _LightningTiling;

            float  _Alpha, _RotationY, _RotationX;

            // ─── HELPERS ───────────────────────────────────────────────

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 rotUV(float2 p, float a)
            {
                float s, c;
                sincos(a, s, c);
                p -= 0.5;
                p = float2(p.x * c - p.y * s, p.x * s + p.y * c);
                return p + 0.5;
            }

            float3 RotateYaw(float3 d, float angle)
            {
                float s, c;
                sincos(angle, s, c);
                return float3(d.x * c - d.z * s, d.y, d.x * s + d.z * c);
            }

            float3 RotatePitch(float3 d, float angle)
            {
                float s, c;
                sincos(angle, s, c);
                return float3(d.x, d.y * c - d.z * s, d.y * s + d.z * c);
            }

            // Merged: returns face index, face UV, and isTop in one abs(dir) pass
            int GetFaceInfo(float3 dir, out float2 uv, out bool isTop)
            {
                float3 a = abs(dir);
                isTop = (a.y >= a.x && a.y >= a.z && dir.y > 0.0);

                int face;
                if (a.x >= a.y && a.x >= a.z)
                {
                    uv   = float2(dir.z, dir.y) / a.x;
                    face = (dir.x > 0) ? 0 : 1;
                }
                else if (a.z >= a.x && a.z >= a.y)
                {
                    uv   = float2(dir.x, dir.y) / a.z;
                    face = (dir.z > 0) ? 2 : 3;
                }
                else
                {
                    uv   = float2(dir.x, dir.z) / a.y;
                    face = (dir.y > 0) ? 4 : 5;
                }
                uv = uv * 0.5 + 0.5;
                return face;
            }

            // ─── VERTEX ────────────────────────────────────────────────

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.worldDir   = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
                o.uv         = v.uv;
                return o;
            }

            // ─── FRAGMENT ──────────────────────────────────────────────

            half4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldDir);

                if (_MovementEnabled > 0.5)
                {
                    float2 moveDir = length(_MovementDirection.xy) > 0.0001
                        ? normalize(_MovementDirection.xy)
                        : float2(1, 0);

                    float angle = _Time.y * _MovementSpeed * 0.1;

                    dir = RotateYaw(dir, angle * moveDir.x);
                    dir = RotatePitch(dir, angle * moveDir.y * 0.5);
                }

                half4  tex = texCUBE(_CubeTex, dir) * _TintColor;

                // ── Rain ──────────────────────────────────────────────
                if (_RainEnabled > 0.5)
                {
                    bool   isTopFace;
                    float2 rainUV;
                    int    faceIndex = GetFaceInfo(dir, rainUV, isTopFace);

                    float enableFlags[6] = { _RainRight, _RainLeft, _RainFront,
                                             _RainBack,  _RainTop,  _RainBottom };
                    bool  enable = enableFlags[faceIndex] > 0.5;

                    if (enable)
                    {
                        half4 rain = 0;
                        if (isTopFace)
                        {
                            float2 topUV  = rainUV * _TopRainDensity;
                            float2 cell   = floor(topUV);
                            float2 local  = frac(topUV);
                            float  h      = Hash21(cell);
                            float  h2     = Hash21(cell * 1.37);
                            float  fade   = smoothstep(0.2, 0.8,
                                                sin(_Time.y * _TopRainSpeed + h * 6.28));
                            float  size   = _TopRainSize * (1.0 - h2 * _TopRainVariation);
                            float  drop   = smoothstep(size * 0.2, 0.0, length(local - 0.5));
                            float  da     = drop * fade;
                            rain.rgb = _RainTint.rgb * da;
                            rain.a   = da;
                        }
                        else
                        {
                            rainUV  *= _RainTiling.xy;
                            float2 cell = floor(rainUV);
                            float  h    = Hash21(cell);
                            float  speed = lerp(0.6, 1.6, h);
                            rainUV.x += sin(h * 10 + _Time.y);
                            rainUV.y += _Time.y * _RainSpeed * speed;
                            rain = tex2D(_RainTex, rainUV) * _RainTint;
                        }

                        tex.rgb = lerp(tex.rgb, rain.rgb, rain.a * _RainStrength);
                    }
                }

                // ── Lightning ─────────────────────────────────────────
                if (_LightningEnabled > 0.5)
                {
                    float t         = _Time.y;
                    float cycle     = floor(t / _LightningMaxTime);
                    float seed      = Hash21(float2(cycle, 5.3));
                    float interval  = lerp(_LightningMinTime, _LightningMaxTime,
                                          Hash21(float2(cycle, 9.1)));
                    float localTime = t - cycle * _LightningMaxTime;
                    float flash     = step(localTime, interval)
                                    * step(interval - localTime, 0.15);

                    if (flash > 0.0)
                    {
                        tex.rgb += tex.rgb * _GlowColor.rgb * flash;

                        float2 baseUV = float2(
                            atan2(dir.z, dir.x) / 6.2831853 + 0.5,
                            asin(dir.y)          / 3.1415926 + 0.5);

                        float2 tiledUV = baseUV * _LightningTiling.xy;
                        half4 c0 = tex2D(_LightningTex0, rotUV(tiledUV, seed *  6.28));
                        half4 c1 = tex2D(_LightningTex1, rotUV(tiledUV, seed * 12.13));
                        half4 c2 = tex2D(_LightningTex2, rotUV(tiledUV, seed * 21.37));
                        half4 c3 = tex2D(_LightningTex3, rotUV(tiledUV, seed * 33.91));

                        half4 lightningTex = (c0 + c1 + c2 + c3) * 0.25;
                        float alpha = saturate(lightningTex.a);
                        tex.rgb += lightningTex.rgb * _LightningForeground.rgb
                                 * alpha * flash * _LightningStrength;
                    }
                }

                // ── Overlay ───────────────────────────────────────────
                if (_OverlayEnabled > 0.5)
                {
                    float2 overlayUV = i.uv * _OverlayTiling.xy;
                    half4  overlay   = tex2D(_OverlayTex, overlayUV) * _OverlayTint;
                    tex.rgb = lerp(tex.rgb, overlay.rgb, overlay.a * _OverlayStrength);
                }

                // ── Glow (pulsing) ────────────────────────────────────
                if (_GlowEnabled > 0.5)
                {
                    float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                    tex.rgb += tex.rgb * _GlowColor.rgb * pulse * _GlowStrength;
                }

                // ── Emission ───────────────────────────────────────────
                if (_EmissionEnabled > 0.5)
                {
                    tex.rgb += tex.rgb * _EmissionColor.rgb * _EmissionStrength;
                }

                // ── Fresnel ────────────────────────────────────────────
                if (_FresnelEnabled > 0.5)
                {
                    float fresnel   = pow(1.0 - saturate(dot(dir,
                                          normalize(_WorldSpaceCameraPos))), 4.0);
                    float smoothGlow = fresnel * _Smoothness * 2.0;
                    tex.rgb += smoothGlow * lerp(0.04, tex.rgb, _Metallic);
                }

                return half4(tex.rgb, tex.a * _Alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Transparent/Diffuse"
}
