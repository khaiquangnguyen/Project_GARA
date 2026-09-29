// A 3D die for the 2D renderer: its face texture shaded by a fixed light, so
// the cube reads as solid while it tumbles. Back faces are culled, which
// sorts a lone convex cube correctly without a depth buffer.
Shader "GARA/Die"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Faces", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _LightDir ("Light Direction (view)", Vector) = (-0.4, 0.6, -0.7, 0)
        _Ambient ("Ambient", Range(0, 1)) = 0.55
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque" }
        Cull Back
        ZWrite On
        ZTest LEqual
        Blend Off

        Pass
        {
            Tags { "LightMode"="Universal2D" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _LightDir;
            float _Ambient;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalVS : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normalVS = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // View space looks down -Z, so flip the light's Z to keep
                // the property in the camera-facing (-Z toward viewer) sense.
                float3 light = normalize(float3(_LightDir.x, _LightDir.y, -_LightDir.z));
                float lit = _Ambient + (1 - _Ambient) * saturate(dot(normalize(i.normalVS), light));
                fixed4 color = tex2D(_MainTex, i.uv) * _Color;
                color.rgb *= lit;
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
