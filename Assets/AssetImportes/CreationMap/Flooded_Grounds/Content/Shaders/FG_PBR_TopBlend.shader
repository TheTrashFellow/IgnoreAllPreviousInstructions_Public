Shader "Flooded_Grounds/PBR_TopBlend"
{
    Properties
    {
        _MainTex ("Base Albedo (RGB)", 2D) = "white" {}
        _Spc("Base Metalness (R) Smoothness (A)", 2D) = "black" {}
        _BumpMap ("Base Normal", 2D) = "bump" {} 
        _AO("Base AO", 2D) = "white" {}
        _layer1Tex ("Layer1 Albedo (RGB) Smoothness (A)", 2D) = "white" {}
        _layer1Metal ("Layer1 Metalness", Range(0, 1)) = 0
        _layer1Norm ("Layer 1 Normal", 2D) = "bump" {}
        _layer1Breakup ("Layer1 Breakup (R)", 2D) = "white" {}
        _layer1BreakupAmnt ("Layer1 Breakup Amount", Range(0, 1)) = 0.5
        _layer1Tiling ("Layer1 Tiling", Float) = 10
        _Power ("Layer1 Blend Amount", Float) = 1   
        _Shift ("Layer1 Blend Height", Float) = 1           
        _DetailBump ("Detail Normal", 2D) = "bump" {}  
        _DetailInt ("DetailNormal Intensity", Range(0, 1)) = 0.4
        _DetailTiling ("DetailNormal Tiling", Float) = 2  
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 500

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Spc);
            SAMPLER(sampler_Spc);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_AO);
            SAMPLER(sampler_AO);
            TEXTURE2D(_layer1Tex);
            SAMPLER(sampler_layer1Tex);
            TEXTURE2D(_layer1Norm);
            SAMPLER(sampler_layer1Norm);
            TEXTURE2D(_layer1Breakup);
            SAMPLER(sampler_layer1Breakup);
            TEXTURE2D(_DetailBump);
            SAMPLER(sampler_DetailBump);

            float4 _Color;
            float _layer1Metal;
            float _Power;
            float _DetailInt;
            float _DetailTiling;
            float _layer1Tiling;
            float _layer1BreakupAmnt;
            float _Shift;

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = normalize(mul((float3x3)unity_WorldToObject, IN.positionOS.xyz));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample Textures
                half3 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                half3 norm = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv));
                half4 spec = SAMPLE_TEXTURE2D(_Spc, sampler_Spc, IN.uv);
                half ao = SAMPLE_TEXTURE2D(_AO, sampler_AO, IN.uv).r;
                half4 layer1 = SAMPLE_TEXTURE2D(_layer1Tex, sampler_layer1Tex, IN.uv * _layer1Tiling);
                half3 layer1Norm = UnpackNormal(SAMPLE_TEXTURE2D(_layer1Norm, sampler_layer1Norm, IN.uv * _layer1Tiling));
                half layer1Breakup = SAMPLE_TEXTURE2D(_layer1Breakup, sampler_layer1Breakup, IN.uv * _layer1Tiling).r;
                half3 detailNorm = UnpackNormal(SAMPLE_TEXTURE2D(_DetailBump, sampler_DetailBump, IN.uv * _DetailTiling));

                // Blend Calculations
                half3 blendNormal = norm + half3(layer1Norm.r * 0.6, layer1Norm.g * 0.6, 0);
                half blend = dot(normalize(IN.normalWS), float3(0, 1, 0));
                half blendFactor = saturate(pow((blend * _Power + _Shift) * lerp(1, layer1Breakup, _layer1BreakupAmnt), 3));

                // Combine Layers
                half3 blendedNormal = lerp(norm, layer1Norm, blendFactor) + detailNorm * _DetailInt;
                half3 blendedColor = lerp(mainColor, layer1.rgb, blendFactor);

                // Output
                return half4(blendedColor * ao, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Bumped Specular"
}
