Shader "Custom/VehicleWindowStream"
{
    Properties
    {
        [Toggle] _MovementEnabled ("Enable Movement", Float) = 1
        _MovementSpeed            ("Movement Speed", Range(0, 50)) = 8
        _MovementDirection        ("Movement Direction (XY)", Vector) = (1,0,0,0)

        _BackTex     ("Background Layer", 2D)              = "white" {}
        _BackTint    ("Background Tint", Color)             = (1,1,1,1)
        _BackTiling  ("Background Tiling", Vector)          = (1,1,0,0)
        _BackParallax ("Background Parallax (Speed Mult)", Range(0,3)) = 0.3

        [Toggle] _MidEnabled ("Enable Mid Layer", Float)    = 0
        _MidTex      ("Mid Layer", 2D)                      = "white" {}
        _MidTint     ("Mid Tint", Color)                    = (1,1,1,1)
        _MidTiling   ("Mid Tiling", Vector)                 = (1,1,0,0)
        _MidParallax ("Mid Parallax (Speed Mult)", Range(0,3)) = 1.0

        [Toggle] _NearEnabled ("Enable Near Layer", Float)  = 0
        _NearTex     ("Near Layer", 2D)                     = "white" {}
        _NearTint    ("Near Tint", Color)                   = (1,1,1,1)
        _NearTiling  ("Near Tiling", Vector)                = (1,1,0,0)
        _NearParallax ("Near Parallax (Speed Mult)", Range(0,5)) = 2.0

        _Alpha ("Alpha", Range(0,1)) = 1
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
                float2 uv         : TEXCOORD0;
            };

            sampler2D _BackTex, _MidTex, _NearTex;
            float4 _BackTint, _MidTint, _NearTint;
            float4 _BackTiling, _MidTiling, _NearTiling;
            float  _BackParallax, _MidParallax, _NearParallax;
            float  _MidEnabled, _NearEnabled;

            float  _MovementEnabled;
            float  _MovementSpeed;
            float4 _MovementDirection;

            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 SampleLayer(sampler2D tex, float4 tint, float4 tiling, float parallax, float2 baseUV, float2 scrollDir, float time)
            {
                float2 uv = baseUV * tiling.xy;
                uv += scrollDir * time * parallax;
                return tex2D(tex, uv) * tint;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 scrollDir = length(_MovementDirection.xy) > 0.0001
                    ? normalize(_MovementDirection.xy)
                    : float2(1, 0);

                float time = _MovementEnabled > 0.5 ? _Time.y * _MovementSpeed : 0.0;

                half4 col = SampleLayer(_BackTex, _BackTint, _BackTiling, _BackParallax, i.uv, scrollDir, time);

                if (_MidEnabled > 0.5)
                {
                    half4 mid = SampleLayer(_MidTex, _MidTint, _MidTiling, _MidParallax, i.uv, scrollDir, time);
                    col.rgb = lerp(col.rgb, mid.rgb, mid.a);
                    col.a   = max(col.a, mid.a);
                }

                if (_NearEnabled > 0.5)
                {
                    half4 near = SampleLayer(_NearTex, _NearTint, _NearTiling, _NearParallax, i.uv, scrollDir, time);
                    col.rgb = lerp(col.rgb, near.rgb, near.a);
                    col.a   = max(col.a, near.a);
                }

                return half4(col.rgb, col.a * _Alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Transparent/Diffuse"
}
