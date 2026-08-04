Shader "LUX/TextMeshPro/Distance Field 2D Lit"
{
    Properties
    {
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0

        _MainTex ("Font Atlas", 2D) = "white" {}
        _GradientScale ("Gradient Scale", Float) = 5
        _TextureWidth ("Texture Width", Float) = 512
        _TextureHeight ("Texture Height", Float) = 512
        _WeightNormal ("Weight Normal", Float) = 0
        _WeightBold ("Weight Bold", Float) = 0.5
        _ScaleRatioA ("Scale Ratio A", Float) = 1
        _Sharpness ("Sharpness", Range(-1,1)) = 0

        _VertexOffsetX ("Vertex Offset X", Float) = 0
        _VertexOffsetY ("Vertex Offset Y", Float) = 0
        _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)
        _MaskSoftnessX ("Mask Softness X", Float) = 0
        _MaskSoftnessY ("Mask Softness Y", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _CullMode ("Cull Mode", Float) = 0
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull [_CullMode]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _FaceColor;
                half4 _OutlineColor;
                float4 _ClipRect;
                float _FaceDilate;
                float _OutlineWidth;
                float _OutlineSoftness;
                float _GradientScale;
                float _WeightNormal;
                float _WeightBold;
                float _ScaleRatioA;
                float _Sharpness;
                float _VertexOffsetX;
                float _VertexOffsetY;
                float _MaskSoftnessX;
                float _MaskSoftnessY;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 atlasUV : TEXCOORD0;
                half4 color : COLOR;
                float2 lightingUV : TEXCOORD1;
                float2 localPosition : TEXCOORD2;
                float bold : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                input.positionOS.xy += float2(_VertexOffsetX, _VertexOffsetY);
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.atlasUV = TRANSFORM_TEX(input.uv0.xy, _MainTex);
                output.color = input.color;
                output.bold = step(input.uv0.w, 0.0);
                output.localPosition = input.positionOS.xy;
                output.lightingUV = ComputeScreenPos(output.positionCS).xy / output.positionCS.w;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half distanceValue = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.atlasUV).a;
                float weight = lerp(_WeightNormal, _WeightBold, input.bold) * 0.25;
                float faceThreshold = 0.5 - (weight + _FaceDilate * 0.5) * _ScaleRatioA;
                float outlineSize = _OutlineWidth * _ScaleRatioA * 0.5;

                // Screen-space derivatives preserve the SDF edge at any world scale.
                float smoothing = max(fwidth(distanceValue), 0.0001);
                smoothing *= max(0.05, (1.0 + _OutlineSoftness * 8.0) / max(0.05, 1.0 + _Sharpness));
                half faceAlpha = smoothstep(faceThreshold - smoothing, faceThreshold + smoothing, distanceValue);
                half outerAlpha = smoothstep(faceThreshold - outlineSize - smoothing,
                                             faceThreshold - outlineSize + smoothing,
                                             distanceValue);
                half outlineMix = saturate(outerAlpha - faceAlpha);

                half4 face = _FaceColor * input.color;
                half4 outline = _OutlineColor;
                outline.a *= input.color.a;
                half alpha = saturate(face.a * faceAlpha + outline.a * outlineMix);
                half3 albedo = lerp(outline.rgb, face.rgb, faceAlpha);

                #if defined(UNITY_UI_CLIP_RECT)
                    float2 inside = step(_ClipRect.xy, input.localPosition) * step(input.localPosition, _ClipRect.zw);
                    alpha *= inside.x * inside.y;
                #endif

                #if defined(UNITY_UI_ALPHACLIP)
                    clip(alpha - 0.001);
                #endif

                SurfaceData2D surfaceData;
                InputData2D inputData;
                InitializeSurfaceData(albedo, alpha, half4(1,1,1,1), half3(0,0,1), surfaceData);
                InitializeInputData(input.atlasUV, input.lightingUV, inputData);
                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }
    }

    Fallback "TextMeshPro/Mobile/Distance Field"
}
