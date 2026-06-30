Shader "Spellcore/Character"
{
    Properties
    {
        [MainTexture] _Albedo_Map("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)

        [Normal] _Normal_Map("Normal", 2D) = "bump" {}
        _Normal_Amount("Normal Amount", Range(0, 2)) = 1

        _Emission_Map("Emission", 2D) = "black" {}
        [HDR] _Emission_Color("Emission Color", Color) = (0, 0, 0, 0)
        _Enable_Emission("Enable Emission", Float) = 0

        _Smoothness("Smoothness", Range(0, 1)) = 0.2
        _Metallic("Metallic", Range(0, 1)) = 0
        _Alpha_Clip_Threshold("Alpha Clip", Range(0, 1)) = 0.5

        [HDR] _DamageFlashColor("Damage Flash Color", Color) = (1, 1, 1, 1)
        _DamageFlashAmount("Damage Flash Amount", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "SpellcoreDamageFlash.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Albedo_Map_ST;
                float4 _BaseColor;
                float4 _Emission_Color;
                float4 _DamageFlashColor;
                float _Normal_Amount;
                float _Smoothness;
                float _Metallic;
                float _Alpha_Clip_Threshold;
                float _Enable_Emission;
                float _DamageFlashAmount;
            CBUFFER_END

            TEXTURE2D(_Albedo_Map);
            SAMPLER(sampler_Albedo_Map);
            TEXTURE2D(_Normal_Map);
            SAMPLER(sampler_Normal_Map);
            TEXTURE2D(_Emission_Map);
            SAMPLER(sampler_Emission_Map);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _Albedo_Map);
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = normalInputs.tangentWS;
                output.bitangentWS = normalInputs.bitangentWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_Albedo_Map, sampler_Albedo_Map, input.uv) * _BaseColor;
                clip(albedo.a - _Alpha_Clip_Threshold);

                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_Normal_Map, sampler_Normal_Map, input.uv),
                    _Normal_Amount);
                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 normalWS = normalize(mul(normalTS, tangentToWorld));

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.alpha = albedo.a;
                surfaceData.occlusion = 1;

                if (_Enable_Emission > 0.5)
                {
                    half3 emissionSample = SAMPLE_TEXTURE2D(_Emission_Map, sampler_Emission_Map, input.uv).rgb;
                    surfaceData.emission = emissionSample * _Emission_Color.rgb;
                }

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = ApplySpellcoreDamageFlash(color.rgb, _DamageFlashColor.rgb, _DamageFlashAmount);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Albedo_Map_ST;
                float4 _BaseColor;
                float _Alpha_Clip_Threshold;
            CBUFFER_END

            TEXTURE2D(_Albedo_Map);
            SAMPLER(sampler_Albedo_Map);

            float3 _LightDirection;
            float3 _LightPosition;

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.uv = TRANSFORM_TEX(input.uv, _Albedo_Map);

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_Albedo_Map, sampler_Albedo_Map, input.uv).a * _BaseColor.a;
                clip(alpha - _Alpha_Clip_Threshold);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
