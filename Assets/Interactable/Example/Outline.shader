Shader "MyCustom/Outline"
{
    Properties
    {
        [Toggle(ENABLE_OUTLINE)] _EnableOutline("Enable Outline", int) = 0
        _OutlineWidth("Outline Width", float) = 0.05
        _MaxOutlineWidth("Max Outline Width", float) = 0.1
        _EdgeColor("Edge Color", Color) = (0,0,0,1)
        [IntRange] _StencilRef("Stencil Reference", Range(0, 255)) = 1

    }
    SubShader
    {
        Tags
        {
            "Queue"="Geometry"
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #pragma multi_compile_local __ ENABLE_OUTLINE
        
        CBUFFER_START(UnityPerMaterial)
        
            float _OutlineWidth;
            float _MaxOutlineWidth;
            float4 _EdgeColor;
            int _StencilRef;
        
        CBUFFER_END
        
        ENDHLSL

        Pass
        {
            Name "Outline Mask"
            Tags
            { 
                "LightMode" = "SRPDefaultUnlit"
            }
            Cull Front
            ColorMask 0
            //ZTest Always
            
            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
                Fail Replace
                ZFail Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            struct OutlineAttributes
            {
                float4 positionOS : POSITION;
            };

            struct OutlineVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            OutlineVaryings vert(OutlineAttributes IN)
            {
                OutlineVaryings OUT;
                OUT.positionCS = mul(UNITY_MATRIX_MVP, IN.positionOS);
                return OUT;
            }

            float4 frag(OutlineVaryings IN): SV_Target
            {
                return float4(0, 0, 0, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "UniversalForward" 
            }
            Cull Front
            ZTest Always 
            
            Stencil
            {
                Ref [_StencilRef]
                Comp NotEqual
                Pass Keep
            }

            HLSLPROGRAM
            #pragma multi_compile_fog

            #pragma vertex vert
            #pragma fragment frag


            struct OutlineAttributes
            {
                float4 positionOS : POSITION;
                float4 smoothNormalOS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct OutlineVaryings
            {
                float4 positionCS : SV_POSITION;
                float fogCoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            OutlineVaryings vert(OutlineAttributes IN)
            {
                OutlineVaryings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 positionVS = mul(UNITY_MATRIX_MV, IN.positionOS);

                #ifdef ENABLE_OUTLINE
                float3 normalVS = mul(UNITY_MATRIX_IT_MV, IN.smoothNormalOS);
                //normalVS.z = -0.1;
                positionVS = positionVS + float4(normalize(normalVS), 0) * min(_OutlineWidth * (-positionVS.z), _MaxOutlineWidth);
                #endif

                OUT.positionCS = mul(UNITY_MATRIX_P, positionVS);
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);

                return OUT;
            }

            float4 frag(OutlineVaryings IN): SV_Target
            {
                #ifdef ENABLE_OUTLINE
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                _EdgeColor.rgb = MixFog(_EdgeColor.rgb, IN.fogCoord);
                return _EdgeColor;
                
                #else
                return float4(0, 0, 0, 0);
                
                #endif
            }
            ENDHLSL
        }
    }
}
