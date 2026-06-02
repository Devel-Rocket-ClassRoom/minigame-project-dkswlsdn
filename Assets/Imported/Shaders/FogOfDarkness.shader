Shader "Custom/FogOfDarkness"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DarknessFactor ("Darkness Factor", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float _DarknessFactor;

            float4 _SightPositions[8];  // xyz = 위치, w = 반지름
            int _SightCount;

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = FragBlit(input, sampler_LinearClamp);

                float2 uv = input.texcoord;
                float3 worldPos = ComputeWorldSpacePosition(uv, SampleSceneDepth(uv), UNITY_MATRIX_I_VP);

                for (int i = 0; i < _SightCount; i++)
                {
                    float3 sightPos = _SightPositions[i].xyz;
                    float radius    = _SightPositions[i].w;

                    float dist = distance(float2(worldPos.x, worldPos.z),
                                          float2(sightPos.x, sightPos.z));
                    float fade = 1.0 - smoothstep(radius * 0.9, radius, dist);
                    if (fade > 0)
                        return lerp(color * _DarknessFactor, color, fade);
                }

                return color * _DarknessFactor;
            }
            ENDHLSL
        }
    }
}