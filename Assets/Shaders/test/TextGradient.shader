Shader "Custom/UI/TextGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Font Texture", 2D) = "white" {}
        _ColorTop ("Top Color", Color) = (1,1,1,1)
        _ColorBottom ("Bottom Color", Color) = (0,0,0,1)
        _Scale ("Gradient Scale", Float) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }

        Lighting Off 
        Cull Off 
        ZWrite Off 
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            fixed4 _ColorTop;
            fixed4 _ColorBottom;
            float _Scale;
            sampler2D _MainTex;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.worldPos = v.vertex;
                // 将顶点颜色初始为白色，并在片元着色器中应用渐变
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 获取字体纹理alpha
                fixed4 col = tex2D(_MainTex, i.texcoord);
                
                // 计算垂直方向上的渐变 (基于顶点位置)
                fixed4 gradient = lerp(_ColorBottom, _ColorTop, i.worldPos.y * _Scale);
                
                // 应用颜色和Alpha
                col.rgb *= gradient.rgb;
                col.a *= i.color.a * gradient.a;
                
                return col;
            }
            ENDCG
        }
    }
}
