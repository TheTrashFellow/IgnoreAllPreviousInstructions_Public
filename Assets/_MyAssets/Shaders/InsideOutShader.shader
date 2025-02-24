Shader "Custom/InsideOutShader"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        // Render the inside of the object
        Cull Front
        Pass
        {
            // Basic color
            Color(0, 0, 0, 1)
        }
    }
}
