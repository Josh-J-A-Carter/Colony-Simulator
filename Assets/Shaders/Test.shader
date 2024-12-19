Shader "Unlit/Test" {
    Properties {
        _Width ("Width", Int) = 2
        _Height ("Height", Int) = 2

        _StartX ("Start X-ordinate", Int) = 0
        _StartY ("Start Y-ordinate", Int) = 0

        _OnColor ("On Color", Color) = (1, 1, 1, 1)
        _OffColor ("Off Color", Color) = (0, 0, 0, 1)
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _OnColor;
            float4 _OffColor;

            struct vertexData {
                float4 vertex : POSITION;
                // float2 uv : TEXCOORD0;
            };

            struct fragmentData {
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD0;
                // float2 uv : TEXCOORD0;
            };

            StructuredBuffer<bool> _TileDiscovered;

            int _Width;
            int _Height;

            int _StartX;
            int _StartY;

            // vertex from local space -> world space -> is it discovered?
            bool is_discovered (float4 vertex) {
                float4 p = mul (unity_ObjectToWorld, vertex);
                int3 gridPos = int3 (floor(p.x), floor(p.y), floor(p.z));

                int index = (gridPos.y - _StartY) * _Width + (gridPos.x - _StartX);

                return _TileDiscovered[index];
            }

            fragmentData vert (vertexData input) {
                fragmentData output;
                output.vertex = input.vertex;
                output.color = _OffColor; //is_discovered(input.vertex) ? _OnColor : _OffColor;
                // output.color = float4 (0, 0, 0, 1);
                return output;
            }

            float4 frag (fragmentData input) : SV_Target {                
                return float4 (1, 1, 1, 1);
            }
            ENDCG
        }
    }
}
