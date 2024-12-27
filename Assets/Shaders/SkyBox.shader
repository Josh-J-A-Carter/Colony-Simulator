Shader "Unlit/SkyBox" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}

        _VertOffset ("Vertical Offset", Float) = 20

        _GroundLevel ("Ground level", Float) = 2
        _SkyLevel ("Sky level", Float) = 40

        _GroundColor ("Ground Default Color", Color) = (0.043, 0.522, 0.239, 1)
        _SkyColor ("Sky Default Color", Color) = (0.75, 0.75, 1, 1)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _GroundLevel;
            float _SkyLevel;

            float4 _GroundColor;
            float4 _SkyColor;

            float _VertOffset;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                if (i.worldPos.y <= _GroundLevel) return _GroundColor;
                if (i.worldPos.y >= _SkyLevel) return _SkyColor;

                fixed4 col = tex2D(_MainTex, float2 (i.worldPos.x, i.worldPos.y * 4 + _VertOffset) / 70);

                return col;
            }

            ENDCG
        }
    }
}
