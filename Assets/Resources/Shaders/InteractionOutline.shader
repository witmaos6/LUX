Shader "LUX2D/Interaction Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 0.8, 0.15, 1)
        _OutlineWidth ("Outline Width (Pixels)", Range(1, 4)) = 4
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.05
        [HideInInspector] _OutlinePixelStep ("Outline Pixel Step", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float _AlphaThreshold;
                float4 _OutlinePixelStep;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 offset = _OutlinePixelStep.xy * _OutlineWidth;
                half centerAlpha = SampleAlpha(input.uv);

                half neighbourAlpha = 0;
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2( offset.x, 0)));
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2(-offset.x, 0)));
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2(0,  offset.y)));
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2(0, -offset.y)));
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2( offset.x,  offset.y)));
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2(-offset.x,  offset.y)));
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2( offset.x, -offset.y)));
                neighbourAlpha = max(neighbourAlpha, SampleAlpha(input.uv + float2(-offset.x, -offset.y)));

                half outsideSprite = 1.0h - step(_AlphaThreshold, centerAlpha);
                half touchesSprite = step(_AlphaThreshold, neighbourAlpha);
                half outlineAlpha = outsideSprite * touchesSprite * _OutlineColor.a;

                return half4(_OutlineColor.rgb, outlineAlpha);
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
