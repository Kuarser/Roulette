Shader "UI/Unlit/ImageShiny"
{
    // 修改Unity官方内置shader, UI-Default
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} // 主纹理
        _Color ("Tint", Color) = (1,1,1,1) // 颜色

        _StencilComp ("Stencil Comparison", Float) = 8 // 模板比较函数
        _Stencil ("Stencil ID", Float) = 0 // 模板ID
        _StencilOp ("Stencil Operation", Float) = 0 // 模板操作
        _StencilWriteMask ("Stencil Write Mask", Float) = 255 // 模板写掩码
        _StencilReadMask ("Stencil Read Mask", Float) = 255 // 模板读掩码

        _ColorMask ("Color Mask", Float) = 15 // 颜色掩码

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0 // 是否使用Alpha剪裁

        _Rotation ("Shine Rotation", Range(0, 360)) = 0 // 闪光旋转角度
        _Factor ("Shine Factor", Range(0, 1)) = 0.5 // 闪光因子
        _Width ("Shine Width", Range(0, 1)) = 0.1 // 闪光宽度
        _Softness ("Shine Softness", Range(0, 1)) = 0.5 // 闪光柔和度
        _Brightness ("Shine Brightness", Range(0, 2)) = 1.0 // 闪光亮度
        _Gloss ("Shine Gloss", Range(0, 1)) = 0.5 // 闪光光泽度
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" // 渲染队列
            "IgnoreProjector"="True" // 忽略投影
            "RenderType"="Transparent" // 渲染类型
            "PreviewType"="Plane" // 预览类型
            "CanUseSpriteAtlas"="True" // 是否可以使用精灵图集
        }

        Stencil
        {
            Ref [_Stencil] // 模板参考值
            Comp [_StencilComp] // 模板比较函数
            Pass [_StencilOp] // 模板通过操作
            ReadMask [_StencilReadMask] // 模板读掩码
            WriteMask [_StencilWriteMask] // 模板写掩码
        }

        Cull Off // 关闭背面剔除
        Lighting Off // 关闭光照计算
        ZWrite Off // 关闭深度写入
        ZTest [unity_GUIZTestMode] // 使用Unity的GUI深度测试模式
        Blend SrcAlpha OneMinusSrcAlpha // 使用Alpha混合
        ColorMask [_ColorMask] // 设置颜色掩码

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert // 顶点着色器入口
            #pragma fragment frag // 片段着色器入口
            #pragma target 2.0 // 着色器目标版本

            #include "UnityCG.cginc" // 包含Unity的CG库
            #include "UnityUI.cginc" // 包含Unity的UI库

            #pragma multi_compile __ UNITY_UI_CLIP_RECT // 多编译选项：裁剪矩形
            #pragma multi_compile __ UNITY_UI_ALPHACLIP // 多编译选项：Alpha剪裁

            // 顶点着色器输入结构体
            struct appdata_t
            {
                float4 vertex   : POSITION; // 顶点位置
                float4 color    : COLOR; // 顶点颜色
                float2 texcoord : TEXCOORD0; // 纹理坐标
                UNITY_VERTEX_INPUT_INSTANCE_ID // 实例ID
            };

            // 顶点着色器输出结构体
            struct v2f
            {
                float4 vertex   : SV_POSITION; // 裁剪空间顶点位置
                fixed4 color    : COLOR; // 颜色
                float2 texcoord  : TEXCOORD0; // 纹理坐标
                float4 worldPosition : TEXCOORD1; // 世界位置
                UNITY_VERTEX_OUTPUT_STEREO // 立体输出
            };

            sampler2D _MainTex; // 主纹理采样器
            fixed4 _Color; // 颜色
            fixed4 _TextureSampleAdd; // 纹理采样加值
            float4 _ClipRect; // 裁剪矩形
            float4 _MainTex_ST; // 主纹理变换
            float _Rotation; // 闪光旋转角度
            float _Factor; // 闪光因子
            float _Width; // 闪光宽度
            float _Softness; // 闪光柔和度
            float _Brightness; // 闪光亮度
            float _Gloss; // 闪光光泽度

            // 旋转矩阵
            float2x2 Rotation(float angle)
            {
                float rad = radians(angle);
                float cosA = cos(rad);
                float sinA = sin(rad);
                return float2x2(cosA, -sinA, sinA, cosA);
            }

            // 顶点着色器
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v); // 设置实例ID
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); // 初始化立体输出
                OUT.worldPosition = v.vertex; // 设置世界位置
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition); // 转换为裁剪空间位置

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex); // 转换纹理坐标

                OUT.color = v.color * _Color; // 计算颜色
                return OUT;
            }

            // 片段着色器
            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color; // 采样纹理并计算颜色
                float2 shinyUv = mul(Rotation(_Rotation), IN.texcoord - 0.5) + 0.5; // 计算闪光UV坐标

                fixed normalizedPos = shinyUv.x; // 归一化位置
                half location = _Factor * 2 - 0.5; // 闪光位置
                half normalized = 1 - saturate(abs((normalizedPos - location) / _Width)); // 归一化闪光强度
                half shinePower = smoothstep(0, _Softness, normalized); // 计算闪光强度
                half3 reflectColor = lerp(fixed3(1,1,1), color.rgb * 7, _Gloss); // 计算反射颜色
                color.rgb += color.a * (shinePower / 2) * _Brightness * reflectColor * _Color.rgb; // 应用闪光效果

                return color;
            }
        ENDCG
        }
    }
}