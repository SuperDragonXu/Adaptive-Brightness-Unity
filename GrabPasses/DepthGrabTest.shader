Shader "Unlit/DepthGrabTest"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
               #pragma vertex vert
               #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"

            TEXTURE2D(_GrabbedDepthTex);
            SAMPLER(sampler_GrabbedDepthTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                
                return SAMPLE_DEPTH_TEXTURE(_GrabbedDepthTex,sampler_GrabbedDepthTex,i.uv);
            }
                ENDHLSL
        }
    }
}
