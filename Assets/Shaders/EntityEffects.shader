Shader "Unlit/EntityEffects" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _RedTintIntensity ("Red Tint Intensity", Range(0, 1)) = 0.4
    }

    SubShader {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="true"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        ZWrite Off
        Cull Off
        Blend One OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Static shader variants
            #pragma multi_compile __ _OUTLINE_ON
            #pragma multi_compile __ _REDTINT_ON

            #include "UnityCG.cginc"

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float4 _OutlineColor;
            float _RedTintIntensity;

            v2f vert (appdata_base v) {
                v2f o;
            
            // Adds variation / hover effect to bees, but also introduces weird graphical issues
                // float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                // worldPos.y += sin(_Time * 80) * 0.075;
                // o.vertex = mul(UNITY_MATRIX_VP, worldPos);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float4 textureColor = tex2D(_MainTex, i.uv);

                // Ignore parts of the texture that don't have an alpha value
                textureColor.rgb *= textureColor.a;

                // Final colour to be output
                // This may be further modified, depending on shader variants
                float4 finalColor = textureColor;

            // Red Tint variant
            #ifdef _REDTINT_ON
                finalColor.r += _RedTintIntensity * textureColor.a;
            #endif

            // Outline variant
            #if _OUTLINE_ON
                // Is there a neighbouring pixel with non-zero alpha?
                float upAlpha = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).a;
                float downAlpha = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y)).a;
                float rightAlpha = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0)).a;
                float leftAlpha = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0)).a;

                float nonEmptyNeighbour = ceil(clamp(upAlpha + downAlpha + rightAlpha + leftAlpha, 0, 1));

                // Is this pixel transparent? If it's not transparent, we don't want to draw the outline
                float transparent = 1 - ceil(textureColor.a);

                // nonEmptyNeighbour * transparent
                //      -> if no non-empty neighbour,               -> 0
                //      -> if not transparent,                      -> 0
                //      -> if non-empty neighbour and transparent,  -> 1
                finalColor = lerp(finalColor, _OutlineColor, nonEmptyNeighbour * transparent);
            #endif

                return finalColor;
            }

            ENDCG
        }
    }
}
