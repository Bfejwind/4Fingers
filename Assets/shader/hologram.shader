Shader "Custom/VertexPointHighlight"
{
    Properties
    {
        _VertexColor("Vertex Color", Color) = (0,1,1,1)
        _VertexSize("Vertex Size", Range(0.001,0.2)) = 0.02
        _Intensity("Intensity", Range(0,5)) = 3
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend One One
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 bary   : TEXCOORD1; // barycentric coords
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            float4 _VertexColor;
            float _VertexSize;
            float _Intensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.bary = v.bary;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // distance to vertex (1 = vertex, 0 = opposite edge)
                float d = 1.0 - max(i.bary.x, max(i.bary.y, i.bary.z));

                // highlight only very close to vertex
                float mask = smoothstep(_VertexSize, 0.0, d);

                float3 color = mask * _VertexColor.rgb * _Intensity;

                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
