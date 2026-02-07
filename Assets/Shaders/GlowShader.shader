Shader "Custom/GlowShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color (Tint)", Color) = (1,1,1,1)
        _GlowStrength ("Glow Strength", Range(0.0, 1.0)) = 0.5
        _GlowSpeed ("Glow Speed", Range(0.0, 10.0)) = 1.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Base texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Pulsing factor 0..1
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;

                // Texture-driven glow (like emission map = MainTex, tinted by GlowColor)
                fixed3 glowTex = col.rgb * _GlowColor.rgb;

                // Apply strength + pulse
                fixed3 glow = glowTex * pulse * (_GlowStrength * 10.0);

                // Add glow on top
                col.rgb += glow;

                return col;
            }
            ENDCG
        }
    }

    FallBack "Lit"
}
