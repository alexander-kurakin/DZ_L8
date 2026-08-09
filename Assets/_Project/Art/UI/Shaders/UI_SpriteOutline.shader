Shader "UI/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 8)) = 1.5
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _AlphaThreshold;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            float SampleAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 texelOffset = _MainTex_TexelSize.xy * _OutlineWidth;

                float centerAlpha = SampleAlpha(uv);
                float outlineAlpha = centerAlpha;
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(-texelOffset.x, 0)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(texelOffset.x, 0)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(0, -texelOffset.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(0, texelOffset.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(-texelOffset.x, -texelOffset.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(-texelOffset.x, texelOffset.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(texelOffset.x, -texelOffset.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(uv + float2(texelOffset.x, texelOffset.y)));

                fixed4 spriteColor = (tex2D(_MainTex, uv) + _TextureSampleAdd) * IN.color;
                float spriteMask = step(_AlphaThreshold, centerAlpha);
                float outlineMask = step(_AlphaThreshold, outlineAlpha) * (1.0 - spriteMask);

                fixed4 color;
                color.rgb = spriteColor.rgb * spriteMask + _OutlineColor.rgb * outlineMask;
                color.a = max(spriteColor.a * spriteMask, _OutlineColor.a * outlineMask);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
