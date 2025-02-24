Shader "Hidden/Nature/Tree Creator Leaves Optimized"
{
Properties
{
    _Color ("Main Color", Color) = (1,1,1,1)
    _TranslucencyColor ("Translucency Color", Color) = (0.73,0.85,0.41,1) // (187,219,106,255)
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
    _TranslucencyViewDependency ("View dependency", Range(0,1)) = 0.7
    _ShadowStrength("Shadow Strength", Range(0,1)) = 0.8
    _ShadowOffsetScale ("Shadow Offset Scale", Float) = 1

    _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    _ShadowTex ("Shadow (RGB)", 2D) = "white" {}
    _BumpSpecMap ("Normalmap (GA) Spec (R) Shadow Offset (B)", 2D) = "bump" {}
    _TranslucencyMap ("Trans (B) Gloss(A)", 2D) = "white" {}

    // These are here only to provide default values
    [HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
    [HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
    [HideInInspector] _SquashAmount ("Squash", Float) = 1
}

SubShader
{
    Tags {
        "IgnoreProjector"="True"
        "RenderType"="TreeLeaf"
    }
    LOD 200
    Cull Off

    Pass
    {
        Tags { "LightMode"="UniversalForward" }
        HLSLPROGRAM
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        TEXTURE2D(_BumpSpecMap);
        TEXTURE2D(_TranslucencyMap);
        SamplerState sampler_MainTex;

        struct Attributes
        {
            float3 position : POSITION;
            float3 normal : NORMAL;
            float2 uv_MainTex : TEXCOORD0;
            // You may need additional data depending on your vertex structure
        };

        struct Varyings
        {
            float2 uv_MainTex : TEXCOORD0;
            float4 color : COLOR;
            half3 viewDir : TEXCOORD1; // Added to pass view direction
        };

        Varyings TreeVertLeaf(Attributes v)
        {
            Varyings o;
            o.uv_MainTex = v.uv_MainTex;
            o.color = float4(1.0, 1.0, 1.0, 1.0); // Assuming no instance color is set
            o.viewDir = normalize(_WorldSpaceCameraPos - v.position);
            return o;
        }

        half4 _Color;
        half4 _TranslucencyColor;
        float _Cutoff;

        void surf(Varyings IN, inout SurfaceOutput o)
        {
            half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv_MainTex);
            half4 trngls = SAMPLE_TEXTURE2D(_TranslucencyMap, sampler_MainTex, IN.uv_MainTex);

            half3 vd = pow(normalize(IN.viewDir), 12);
            half vdlerp = lerp(1, 0, vd.b);

            o.Albedo = c.rgb * IN.color.rgb * IN.color.a;
            o.Alpha = saturate(c.a * vdlerp);

            half4 norspc = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(norspc);
            o.Emission = _TranslucencyColor * 0.02;

            clip(o.Alpha - _Cutoff);
        }
        ENDHLSL
    }

    // Pass to render object as a shadow caster
    Pass
    {
        Name "ShadowCaster"
        Tags { "LightMode" = "ShadowCaster" }

        HLSLPROGRAM
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_ShadowTex);
        SamplerState sampler_ShadowTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        struct v2f_surf
        {
            V2F_SHADOW_CASTER;
            float2 hip_pack0 : TEXCOORD1;
        };

        v2f_surf vert_surf(appdata_full v)
        {
            v2f_surf o;
            TreeVertLeaf(v);
            o.hip_pack0.xy = TRANSFORM_TEX(v.texcoord, _ShadowTex);
            TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
            return o;
        }

        fixed _Cutoff;

        float4 frag_surf(v2f_surf IN) : SV_Target
        {
            half alpha = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, IN.hip_pack0.xy).r;
            clip(alpha - _Cutoff);
            SHADOW_CASTER_FRAGMENT(IN);
        }
        ENDHLSL
    }
}

Fallback "Hidden/Nature/Tree Creator Leaves Rendertex"
}
