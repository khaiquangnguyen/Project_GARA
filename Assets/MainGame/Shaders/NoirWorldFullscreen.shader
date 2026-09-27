// Noir World full-screen pass: the Sin City look over the whole camera
// image, driven by globals that NoirWorldScreenEffect sets. A no-op copy
// while _NoirWorldPhase is 0. Skips pixels with stencil bit 128 (characters
// in their own noir look, see SpineSinCity.shader), so they aren't cut twice;
// needs the renderer feature's Bind Depth-Stencil on.
Shader "GARA/Noir World Fullscreen"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "NoirWorld"

            Stencil
            {
                Ref 128
                ReadMask 128
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "SinCityLook.hlsl"

            float _NoirWorldPhase;
            float _NoirWorldShadowThreshold;
            float _NoirWorldLightThreshold;
            float _NoirWorldMidTone;
            float _NoirWorldSoftness;
            float4 _NoirWorldInkColor;
            float4 _NoirWorldPaperColor;
            float _NoirWorldAccentHue;
            float _NoirWorldAccentHueRange;
            float _NoirWorldAccentMinSaturation;
            float _NoirWorldAccentFlatness;
            float _NoirWorldAccentAmount;
            float _NoirWorldInvert;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                if (_NoirWorldPhase <= 0)
                {
                    return color;
                }

                SinCityParams p;
                p.shadowThreshold = _NoirWorldShadowThreshold;
                p.lightThreshold = _NoirWorldLightThreshold;
                p.midTone = _NoirWorldMidTone;
                p.softness = _NoirWorldSoftness;
                p.ink = _NoirWorldInkColor.rgb;
                p.paper = _NoirWorldPaperColor.rgb;
                p.accentHue = _NoirWorldAccentHue;
                p.accentHueRange = _NoirWorldAccentHueRange;
                p.accentMinSaturation = _NoirWorldAccentMinSaturation;
                p.accentFlatness = _NoirWorldAccentFlatness;
                p.accentAmount = _NoirWorldAccentAmount;
                p.invert = _NoirWorldInvert;

            #if defined(UNITY_COLORSPACE_GAMMA)
                float isLinear = 0;
            #else
                float isLinear = 1;
            #endif
                float3 look = SinCityLook(color.rgb, p, isLinear);
                return float4(lerp(color.rgb, look, _NoirWorldPhase), color.a);
            }
            ENDHLSL
        }
    }
}
