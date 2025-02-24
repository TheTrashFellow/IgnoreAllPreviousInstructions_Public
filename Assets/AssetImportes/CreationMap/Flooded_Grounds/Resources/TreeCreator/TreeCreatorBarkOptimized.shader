Shader "Hidden/Nature/Tree Creator Bark Optimized"
{
Properties
{
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    _BumpSpecMap ("Normalmap (GA) Spec (R)", 2D) = "bump" {}
    _TranslucencyMap ("Trans (RGB) Gloss(A)", 2D) = "white" {}
    
    // These are here only to provide default values
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    [HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
    [HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
    [HideInInspector] _SquashAmount ("Squash", Float) = 1
}

SubShader
{
    Tags { "RenderType"="TreeBark" }
    LOD 200

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
        };

        struct Varyings
        {
            float2 uv_MainTex : TEXCOORD0;
            float4 color : COLOR;
            float3 position : TEXCOORD1;
        };

        Varyings TreeVertBark(Attributes v)
        {
            Varyings o;
            o.uv_MainTex = v.uv_MainTex;
            o.color = float4(1.0, 1.0, 1.0, 1.0);  // Assuming no instance color is set
            o.position = v.position;
            return o;
        }

        half4 _Color;
        
        void surf(Varyings IN, inout SurfaceOutput o)
        {
            half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * IN.color.rgb * IN.color.a;
            o.Alpha = c.a;

            half4 norspc = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(norspc);
            //o.Smoothness = 0;
            //o.Metallic = 0;
        }

        ENDHLSL
    }
}

Fallback "Hidden/Nature/Tree Creator Bark Rendertex"
}
