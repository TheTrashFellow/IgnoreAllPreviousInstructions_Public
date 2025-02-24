Shader "Flooded_Grounds/PBR_Water_HLSL"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Emis ("Self-Ilumination", Range(0,1)) = 0.1
        _Smth ("Smoothness", Range(0,1)) = 0.9
        _Parallax ("Height", Range (0.005, 0.08)) = 0.02
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpMap2 ("Normalmap2", 2D) = "bump" {}
        _BumpLerp ("Normalmap2 Blend", Range(0,1)) = 0.5
        _ParallaxMap ("Heightmap", 2D) = "black" {}
        _ScrollSpeed ("Scroll Speed", float) = 0.2
        _WaveFreq ("Wave Frequency", float) = 20
        _WaveHeight ("Wave Height", float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_BumpMap2);
            SAMPLER(sampler_BumpMap2);
            TEXTURE2D(_ParallaxMap);
            SAMPLER(sampler_ParallaxMap);

            float4 _Color;
            float _Smth;
            float _Emis;
            float _Parallax;
            float _ScrollSpeed;
            float _WaveFreq;
            float _WaveHeight;
            float _BumpLerp;

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv_MainTex  : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 viewDir     : TEXCOORD2;
                float3 normalWS    : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Vertex position adjustments for wave effect
                float phase = _Time.y * _WaveFreq;
                float offset = (IN.positionOS.x + IN.positionOS.z * 2) * 8;
                IN.positionOS.y += sin(phase + offset) * _WaveHeight;

                // Transform position and UVs
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv_MainTex = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                // Use stereo-aware camera position
                float3 cameraPosWS = GetCameraPositionWS();
                OUT.viewDir = normalize(cameraPosWS - OUT.worldPos);

                OUT.normalWS = normalize(mul((float3x3)unity_ObjectToWorld, IN.positionOS.xyz));

                return OUT;
            }

            float2 CalculateParallaxOffset(float height, float parallaxScale, float3 viewDir)
            {
                float3 viewDirLocal = normalize(viewDir);
                float parallax = (height - 0.5) * parallaxScale;
                return parallax * viewDirLocal.xy;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Scrolling for parallax map
                half scrollX = _ScrollSpeed * _Time.y;
                half scrollY = _ScrollSpeed * 0.5 * _Time.y;
                IN.uv_MainTex += half2(scrollX, scrollY);

                // Parallax mapping with custom offset function
                half height = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, IN.uv_MainTex).r;
                half2 offset = CalculateParallaxOffset(height, _Parallax, IN.viewDir);
                half2 uvOffset = IN.uv_MainTex + offset;

                // Main texture and normal map sampling
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvOffset);
                half3 normalMap1 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvOffset));
                half3 normalMap2 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap2, sampler_BumpMap2, IN.uv_MainTex));

                // Combine normals
                half3 finalNormal = lerp(normalMap1, normalMap2, _BumpLerp);

                // Set output color and properties
                half4 finalColor = mainTexColor * _Color;
                finalColor.rgb *= lerp(1.0, _Emis, _Emis);

                half metallic = 0.0;
                half smoothness = _Smth;

                return half4(finalColor.rgb, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback "Diffuse"
}
