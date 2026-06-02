Shader "Hovl/Particles/Distortion"
{
    Properties
    {
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Distortionpower("Distortion power", Float) = 0.05
        _InvFade("Soft Particles Factor", Range(0.01, 3.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _NormalMap_ST;
                float  _Distortionpower;
                float  _InvFade;
            CBUFFER_END

            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            struct appdata_t
            {
                float4 vertex   : POSITION;
                half4  color    : COLOR;
                float4 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex    : SV_POSITION;
                half4  color     : COLOR;
                float4 screenPos : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                float  distScale : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex    = TransformObjectToHClip(v.vertex.xyz);
                o.color     = v.color;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.texcoord2 = TRANSFORM_TEX(v.texcoord.xy, _NormalMap);

                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.distScale = 1.0 / distance(_WorldSpaceCameraPos, worldPos);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 screenUV  = i.screenPos.xy / i.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float fragDepth  = i.screenPos.w;
                float fade       = saturate(_InvFade * (sceneDepth - fragDepth));
                i.color.a       *= fade;

                half3 normalTex  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.texcoord2));
                half clampResult = saturate((abs(normalTex.r) + abs(normalTex.g) * 30.0) - 0.03);

                half2 distortion = normalTex.rg * _Distortionpower * i.color.a * i.distScale;
                float2 grabUV    = screenUV + distortion;

                half4 col = half4(SampleSceneColor(grabUV), 1.0);
                col.a     = saturate(col.a * clampResult);

                return col;
            }
            ENDHLSL
        }
    }
}