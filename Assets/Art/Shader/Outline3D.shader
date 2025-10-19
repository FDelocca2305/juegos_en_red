Shader "Custom/URP_InvertedHullOutline"
{
    Properties { 
        _Color ("Color", Color) = (0,1,1,1) 
        _Thickness ("Thickness (World Units)", Float) = 0.02 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            
            Cull Front
            
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;
            float  _Thickness;

            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings   { float4 positionHCS:SV_POSITION; };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 nWS = TransformObjectToWorldDir(IN.normalOS);
                float3 pWS = TransformObjectToWorld(IN.positionOS.xyz);
                pWS += nWS * _Thickness;
                OUT.positionHCS = TransformWorldToHClip(pWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target { return _Color; }
            ENDHLSL
        }
    }
}
