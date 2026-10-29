Shader "Custom/TextEffect"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        [HDR]_ColorTop ("Gradient Top Color", Color) = (1,1,1,1)
        [HDR]_ColorBottom ("Gradient Bottom Color", Color) = (1,1,1,1)
        
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.5)) = 0
        
        [Header(Shadow)]
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
        _ShadowOffset ("Shadow Offset", Vector) = (0.005, -0.005, 0, 0)
        
        [Header(Underline)]
        _UnderlineColor ("Underline Color", Color) = (1,1,1,1)
        _UnderlinePos ("Underline Position", Range(-0.5, 0.5)) = -0.1
        _UnderlineSize ("Underline Size", Range(0, 0.2)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColorTop, _ColorBottom, _OutlineColor, _ShadowColor,_UnderlineColor;
            float _OutlineWidth, _UnderlinePos, _UnderlineSize;
            float4 _ShadowOffset;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 采样文字 Alpha (SDF 或普通 Font Texture)
                float alpha = tex2D(_MainTex, i.texcoord).a;
                
                // 2. 渐变颜色
                fixed4 gradientCol = lerp(_ColorBottom, _ColorTop, i.texcoord.y);
                
                // 3. 描边逻辑
                float outlineAlpha = tex2D(_MainTex, i.texcoord + _OutlineWidth).a;
                fixed4 finalCol = lerp(fixed4(0,0,0,0), _OutlineColor, smoothstep(0.5, 0.5-_OutlineWidth, outlineAlpha));
                
                // 4. 文字本身
                finalCol = lerp(finalCol, gradientCol * i.color, smoothstep(0.4, 0.6, alpha));

                // 5. 下划线
                float underline = step(0.5 - _UnderlineSize, i.texcoord.y + _UnderlinePos) * step(i.texcoord.y + _UnderlinePos, 0.5 + _UnderlineSize);
                finalCol = lerp(finalCol, _UnderlineColor, underline * step(0.1, i.texcoord.y + _UnderlinePos));

                // 6. 阴影 (简单偏移采样)
                float shadowAlpha = tex2D(_MainTex, i.texcoord - _ShadowOffset.xy).a;
                finalCol = lerp(finalCol, _ShadowColor, shadowAlpha * (1.0 - alpha));

                return finalCol;
            }
            ENDCG
            }
    }
}
