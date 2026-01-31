Shader "Custom/FogofWar"
{
    Properties
    {
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 3
        _Softness ("Softness", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _PlayerPos;
            float _Radius;
            float _Softness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.worldPos.xy, _PlayerPos.xy);
                float alpha = smoothstep(_Radius, _Radius + _Softness, dist);
                return fixed4(0, 0, 0, alpha);
            }
            ENDCG
        }
    }
}
