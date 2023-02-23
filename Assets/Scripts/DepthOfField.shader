Shader "Hidden/Turing/DepthOfField" {
    Properties {
        _MainTex("", any) = "" {}
    }

    HLSLINCLUDE
        #pragma target 3.0
    ENDHLSL

    Subshader {	

        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off ZTest Always Blend Off Cull Off

        HLSLINCLUDE
        #pragma target 3.0
        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x
        
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
        ENDHLSL

        Pass {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Merge
            #include "DepthOfField.hlsl"
            ENDHLSL
        }

        Pass {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment PassHorizontal
            #include "DepthOfField.hlsl"
            ENDHLSL
        }

        Pass {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment PassVertical
            #include "DepthOfField.hlsl"
            ENDHLSL
        }

        Pass {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Gauss
            #include "DepthOfField.hlsl"
            ENDHLSL
        }
    }
    FallBack Off
}
