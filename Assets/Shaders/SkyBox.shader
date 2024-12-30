Shader "Unlit/SkyBox" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        
        _B_Sparse_Plains ("Sparse Plains Biome Texture", 2D) = "white" {}
        _B_Plains ("Plains Biome Texture", 2D) = "white" {}
        _B_Meadow ("Flower Meadow Biome Texture", 2D) = "white" {}
        _B_Forest ("Forest Biome Texture", 2D) = "white" {}
        _B_Mountain ("Mountain Biome Texture", 2D) = "white" {}

        _Sky_Day ("Sky Texture (Day)", 2D) = "white" {}
        _Sky_Dusk ("Sky Texture (Dawn/dusk)", 2D) = "white" {}
        _Sky_Night ("Sky Texture (Night)", 2D) = "white" {}

        _VertOffset ("Vertical Offset", Float) = 20
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

            struct interpolator {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _B_Sparse_Plains;
            sampler2D _B_Plains;
            sampler2D _B_Meadow;
            sampler2D _B_Forest;
            sampler2D _B_Mountain;

            sampler2D _Sky_Day;
            sampler2D _Sky_Dusk;
            sampler2D _Sky_Night;

            float _VertOffset;

            interpolator vert (appdata input) {
                interpolator output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.worldPos = mul(unity_ObjectToWorld, input.vertex).xy;
                return output;
            }

            float4 frag (interpolator i) : SV_Target {
                float2 uv = float2 (i.worldPos.x, i.worldPos.y * 2 + _VertOffset) / 32;

                // Foreground colour - depends on biome
                float4 foreground;
                int biome = (((int) i.worldPos.x) / 16) % 5;
                if (biome == 0) foreground = tex2D(_B_Plains, uv);
                else if (biome == 1) foreground = tex2D(_B_Sparse_Plains, uv);
                else if (biome == 2) foreground = tex2D(_B_Meadow, uv);
                else if (biome == 3) foreground = tex2D(_B_Mountain, uv);
                else foreground = tex2D(_B_Forest, uv);

                // Get background colour, depending on time
                float4 background;
                float time = cos(_Time) * 0.5 + 0.5;
                
                float4 day = tex2D(_Sky_Day, uv);
                float4 dusk = tex2D(_Sky_Dusk, uv);
                float4 night = tex2D(_Sky_Night, uv);

                if (time > 0.75) background = day;
                else if (time < 0.25) background = night;
                else if (time < 0.5) background = lerp(night, dusk, (time - 0.25) * 4);
                else background = lerp(dusk, day, (time - 0.5) * 4);

                // If fg is transparent, this is 0 -> show bg. Otherwise, this is 1 -> show fg.
                float show_foreground = clamp(foreground.a, 0, 1);
                float4 final_colour = lerp(background, foreground, show_foreground);

                return final_colour;
            }

            ENDCG
        }
    }
}
