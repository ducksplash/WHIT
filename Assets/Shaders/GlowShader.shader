Shader "Custom/GlowShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
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
 
            // Input struct for vertex shader
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            // Output struct for vertex shader
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            // Uniform variables
            sampler2D _MainTex;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpeed;
 
            // Vertex shader
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            // Fragment shader
            fixed4 frag (v2f i) : SV_Target {
                // Sample the main texture
                fixed4 col = tex2D(_MainTex, i.uv);
 
                // Calculate the pulsating glow color
                float glow = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                fixed4 glowColor = _GlowColor * glow * (_GlowStrength * 10);
 
                // Add the glow to the main color
                col.rgb += glowColor.rgb;
 
                return col;
            }
            ENDCG
        }
    }
 
    FallBack "Lit"
}
