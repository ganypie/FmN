Shader "Hidden/SSFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (1,1,1,1)
        _FogDensity ("Fog Density", Range(0,1)) = 0.5
        _FogOffset ("Fog Offset", Range(0,100)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGINCLUDE
        #include "UnityCG.cginc"

        struct VertexData
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        sampler2D _CameraDepthTexture;
        float4 _FogColor;
        float _FogDensity;
        float _FogOffset;

        v2f vp(VertexData v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        float4 fp(v2f i) : SV_Target
        {
            float4 col = tex2D(_MainTex, i.uv);
            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
            depth = Linear01Depth(depth); // используем стандартную функцию из UnityCG.cginc

            float viewDistance = depth * _ProjectionParams.z;
            float fogFactor = (_FogDensity / sqrt(log(2))) * max(0.0, viewDistance - _FogOffset);
            fogFactor = exp2(-fogFactor * fogFactor);

            float4 fogOutput = lerp(_FogColor, col, saturate(fogFactor));
            return fogOutput;
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp
            ENDCG
        }
    }
}
