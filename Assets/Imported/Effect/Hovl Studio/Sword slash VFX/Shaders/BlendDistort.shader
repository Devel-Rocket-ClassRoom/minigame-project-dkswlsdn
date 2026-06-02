Shader "Hovl/Particles/BlendDistort"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _Flow("Flow", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}
        _NormalMap("NormalMap", 2D) = "bump" {}
        _Color("Color", Color) = (0.5,0.5,0.5,1)
        _Distortionpower("Distortion power", Float) = 0
        _SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
        _DistortionSpeedXYPowerZ("Distortion Speed XY Power Z", Vector) = (0,0,0,0)
        _Emission("Emission", Float) = 2
        _Opacity("Opacity", Range(0, 3)) = 1
        [Toggle]_Usedepth("Use depth?", Float) = 1
        [Toggle]_Softedges("Soft edges", Float) = 0
        _Depthpower("Depth power", Float) = 1
        [HideInInspector] _texcoord("", 2D) = "white" {}
        [HideInInspector] _tex4coord("", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+0"
            "IgnoreProjector" = "True"
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Noise_ST;
                float4 _Flow_ST;
                float4 _Mask_ST;
                float4 _NormalMap_ST;
                float4 _Color;
                float4 _SpeedMainTexUVNoiseZW;
                float4 _DistortionSpeedXYPowerZ;
                float  _Distortionpower;
                float  _Emission;
                float  _Opacity;
                float  _Usedepth;
                float  _Softedges;
                float  _Depthpower;
            CBUFFER_END

            TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
            TEXTURE2D(_Noise);     SAMPLER(sampler_Noise);
            TEXTURE2D(_Flow);      SAMPLER(sampler_Flow);
            TEXTURE2D(_Mask);      SAMPLER(sampler_Mask);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            struct appdata
            {
                float4 vertex   : POSITION;
                half4  color    : COLOR;
                float4 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos        : SV_POSITION;
                half4  color      : COLOR;
                float4 texcoord   : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float3 worldNormal: TEXCOORD2;
                float3 viewDir    : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos        = TransformObjectToHClip(v.vertex.xyz);
                o.color      = v.color;
                o.texcoord   = v.texcoord;
                o.screenPos  = ComputeScreenPos(o.pos);

                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.worldNormal   = TransformObjectToWorldNormal(float3(0, 0, 1));
                o.viewDir       = normalize(GetWorldSpaceViewDir(worldPos));

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv0 = i.texcoord.xy;

                // 속도 벡터
                float2 speedMain  = float2(_SpeedMainTexUVNoiseZW.x, _SpeedMainTexUVNoiseZW.y);
                float2 speedNoise = float2(_SpeedMainTexUVNoiseZW.z, _SpeedMainTexUVNoiseZW.w);
                float2 speedFlow  = float2(_DistortionSpeedXYPowerZ.x, _DistortionSpeedXYPowerZ.y);
                float  Flowpower  = _DistortionSpeedXYPowerZ.z;

                // Flow / Mask 왜곡
                float2 uvFlow    = uv0 * _Flow_ST.xy + _Flow_ST.zw;
                float2 panner110 = _Time.y * speedFlow + uvFlow;
                float2 uvMask    = uv0 * _Mask_ST.xy + _Mask_ST.zw;
                float2 flowDistort = (SAMPLE_TEXTURE2D(_Flow, sampler_Flow, panner110)
                                    * SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uvMask)).rg * Flowpower;

                // MainTex
                float2 uvMain    = uv0 * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 panner107 = _Time.y * speedMain + uvMain;
                float4 mainTex   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, panner107 - flowDistort);

                // Noise
                float2 uvNoise      = uv0 * _Noise_ST.xy + _Noise_ST.zw;
                float2 randomOffset = float2(i.texcoord.w, 0.0);
                float2 panner108    = _Time.y * speedNoise + (uvNoise + randomOffset);
                float4 noiseTex     = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, panner108);

                // NormalMap 기반 화면 왜곡
                float2 uvNormal  = uv0 * _NormalMap_ST.xy + _NormalMap_ST.zw;
                float2 panner146 = _Time.y * speedNoise + uvNormal;
                float4 normalSample   = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, panner146);
                float3 unpackedNormal = UnpackNormalScale(normalSample, _Distortionpower);

                float2 screenUV = i.screenPos.xy / (i.screenPos.w + 0.00001);
                float2 grabUV   = screenUV + unpackedNormal.xy;

                // _CameraOpaqueTexture 샘플링 (GrabPass 대체)
                float3 screenColor = SampleSceneColor(grabUV);

                // 이미션 컬러
                float4 vertexColor = i.color;
                float  alphaFactor = mainTex.a * noiseTex.a * _Color.a * vertexColor.a * _Opacity;
                float3 emissive    = (mainTex * noiseTex * _Color * vertexColor).rgb * _Emission * alphaFactor;

                // W 블렌드 (uv.z)
                float  W        = i.texcoord.z;
                float3 finalRGB = lerp(screenColor + emissive, screenColor * emissive, W);

                // 알파
                float alpha = saturate(alphaFactor);

                // Depth Fade
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float fragDepth  = i.screenPos.w;
                float depthFade  = saturate(abs(sceneDepth - fragDepth) / _Depthpower);
                float alphaFinal = lerp(alpha, alpha * depthFade, _Usedepth);

                // Soft Edges
                float3 worldNormal = normalize(i.worldNormal);
                float3 viewDir     = normalize(i.viewDir);
                float  dotNV       = dot(worldNormal, viewDir);
                float  powDot      = pow(abs(dotNV), 3.0) * 5.0;
                float  signDot     = sign(dotNV);
                float  softEdge    = clamp(powDot * signDot, 0.0, 1.0);
                alphaFinal = lerp(alphaFinal, alphaFinal * softEdge, _Softedges);

                half4 col = half4(finalRGB, alphaFinal);
                return col;
            }
            ENDHLSL
        }
    }
}