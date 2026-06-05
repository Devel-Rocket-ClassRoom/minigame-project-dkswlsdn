Shader "Custom/VisibilityMesh"
{
    Properties
    {
        _Color     ("Color",     Color)      = (1,1,0.9,1)
        _Intensity ("Intensity", Float)      = 1.0
        _Range     ("Range",     Float)      = 10.0
        _FlatZone  ("Flat Zone", Range(0,1)) = 0.8   // 이 비율까지 균일, 이후 감쇠
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend One One        // 가산: 어두운 맵 위에 빛을 더함
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _Intensity;
                float  _Range;
                float  _FlatZone;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float  distNorm    : TEXCOORD0; // 중심으로부터 정규화 거리
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // 메시 로컬 원점(0,0,0)이 캐릭터 중심 → 정점까지 거리 = 시야 거리
                OUT.distNorm = length(IN.positionOS.xyz) / max(_Range, 0.0001);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float d = IN.distNorm;
                float atten = (d < _FlatZone)
                    ? 1.0
                    : 1.0 - smoothstep(_FlatZone, 1.0, d);

                return half4(_Color.rgb * _Intensity * atten, atten);
            }
            ENDHLSL
        }
    }
}
