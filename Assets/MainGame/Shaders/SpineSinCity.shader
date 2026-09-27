// Spine skeleton in the Sin City look (see SinCityLook.hlsl), faded in by
// _GrayPhase, with Spine's outline pass drawn behind as a white rim.
// Swapped in for a character's atlas materials by OnNoirifiedEffect.
// The body marks stencil bit 128 so the Noir World full-screen pass leaves
// the character's own look alone.
Shader "GARA/Spine/Skeleton Sin City"
{
    Properties
    {
        _GrayPhase ("Phase", Range(0, 1)) = 1
        [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
        _Cutoff ("Shadow alpha cutoff", Range(0, 1)) = 0.1
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput ("Straight Alpha Texture", Int) = 0

        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.4
        _LightThreshold ("Light Threshold", Range(0, 1)) = 0.55
        _MidTone ("Mid Tone", Range(0, 1)) = 0.35
        _Softness ("Edge Softness", Range(0, 0.2)) = 0.02
        _InkColor ("Ink", Color) = (0, 0, 0, 1)
        [HDR] _PaperColor ("Paper", Color) = (1, 1, 1, 1)
        _AccentHue ("Accent Hue", Range(0, 1)) = 0
        _AccentHueRange ("Accent Hue Range", Range(0, 0.5)) = 0.04
        _AccentMinSaturation ("Accent Min Saturation", Range(0, 1)) = 0.45
        _AccentFlatness ("Accent Flatness", Range(0, 1)) = 1
        _AccentAmount ("Accent Amount", Range(0, 1)) = 1
        _Invert ("Invert", Range(0, 1)) = 0

        [HideInInspector] _StencilRef ("Stencil Reference", Float) = 1.0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8

        // Read by Spine's OUTLINE pass (the rim).
        _OutlineWidth ("Rim Width", Range(0, 8)) = 3.0
        [HDR] _OutlineColor ("Rim Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _OutlineReferenceTexWidth ("Reference Texture Width", Int) = 1024
        [HideInInspector] _ThresholdEnd ("Outline Threshold", Range(0, 1)) = 0.25
        [HideInInspector] _OutlineSmoothness ("Outline Smoothness", Range(0, 1)) = 1.0
        [HideInInspector] _OutlineOpaqueAlpha ("Opaque Alpha", Range(0, 1)) = 1.0
        [HideInInspector] _OutlineMipLevel ("Outline Mip Level", Range(0, 3)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        Stencil
        {
            Ref[_StencilRef]
            Comp[_StencilComp]
            Pass Keep
        }

        // URP draws only the first untagged pass (the rim here), so the body
        // gets the 2D renderer's own tag, drawn after the untagged one.
        UsePass "Spine/Outline/Skeleton/OUTLINE"

        Pass
        {
            Name "Normal"
            Tags { "LightMode"="Universal2D" }

            // Replaces the subshader's Spine-mask stencil; no character uses
            // masking. Keep in sync with NoirWorldFullscreen.shader.
            Stencil
            {
                Ref 128
                WriteMask 128
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma multi_compile_local _ _STRAIGHT_ALPHA_INPUT
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Packages/com.esotericsoftware.spine.spine-unity/Runtime/spine-unity/Shaders/CGIncludes/Spine-Common.cginc"
            #include "SinCityLook.hlsl"

            sampler2D _MainTex;
            float _GrayPhase;
            float _ShadowThreshold;
            float _LightThreshold;
            float _MidTone;
            float _Softness;
            float4 _InkColor;
            float4 _PaperColor;
            float _AccentHue;
            float _AccentHueRange;
            float _AccentMinSaturation;
            float _AccentFlatness;
            float _AccentAmount;
            float _Invert;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.uv = v.uv;
                o.vertexColor = PMAGammaToTargetSpace(v.vertexColor);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(VertexOutput i) : SV_Target
            {
                float4 rawColor = tex2D(_MainTex, i.uv);
            #if defined(_STRAIGHT_ALPHA_INPUT)
                rawColor.rgb *= rawColor.a;
            #endif
                float finalAlpha = rawColor.a * i.vertexColor.a;
                float3 premultiplied = rawColor.rgb * i.vertexColor.rgb;

                // Additive attachments carry colour with ~0 alpha, so coverage
                // falls back to brightness to keep them visible.
                float coverage = max(finalAlpha, max(premultiplied.r, max(premultiplied.g, premultiplied.b)));
                float3 straight = premultiplied / max(coverage, 1e-4);

                // No stencil mark on (near) empty pixels, so the world pass
                // still reaches the background around the character.
                clip(coverage - 0.02);

                SinCityParams p;
                p.shadowThreshold = _ShadowThreshold;
                p.lightThreshold = _LightThreshold;
                p.midTone = _MidTone;
                p.softness = _Softness;
                p.ink = _InkColor.rgb;
                p.paper = _PaperColor.rgb;
                p.accentHue = _AccentHue;
                p.accentHueRange = _AccentHueRange;
                p.accentMinSaturation = _AccentMinSaturation;
                p.accentFlatness = _AccentFlatness;
                p.accentAmount = _AccentAmount;
                p.invert = _Invert;

            #if defined(UNITY_COLORSPACE_GAMMA)
                float isLinear = 0;
            #else
                float isLinear = 1;
            #endif
                float3 look = SinCityLook(straight, p, isLinear) * coverage;
                return float4(lerp(premultiplied, look, _GrayPhase), finalAlpha);
            }
            ENDCG
        }

        UsePass "Spine/Skeleton/CASTER"
    }
    FallBack "Spine/Special/Skeleton Grayscale"
}
