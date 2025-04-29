Shader "Hidden/GreatClock/RenderTextureAtlas/DrawSlicedRect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv_border : TEXCOORD0;
                float4 border_lbrt : TEXCOORD1;
                float2 uv_mask : TEXCOORD2;
            };

            float4 _TargetRect;
            float4 _BorderRect;
            float4 _MaskRect;

            v2f vert (appdata v)
            {
                v2f o;
                float4 vertex = UnityObjectToClipPos(v.vertex);
                float4 rect = _TargetRect;
#if UNITY_UV_STARTS_AT_TOP
                rect.y = 1 - rect.y - rect.w;
#endif
                float2 xy = rect.xy + rect.zw * (vertex.xy / vertex.w + 1) * 0.5;
                o.vertex = float4((xy * 2 - 1) * vertex.w, vertex.z, vertex.w);
                o.uv_border.xy = v.uv;
                o.uv_border.zw = (v.uv - _BorderRect.xy) / max(_BorderRect.zw, 0.0000001);
                o.border_lbrt.xy = v.uv / max(_BorderRect.xy, 0.0000001);
                o.border_lbrt.zw = (v.uv - _BorderRect.xy - _BorderRect.zw) / max(1 - _BorderRect.xy - _BorderRect.zw, 0.0000001);
                o.uv_mask = _MaskRect.xy + _MaskRect.zw * v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _MaskChannelType;
            float4 _UV_x;
            float4 _UV_y;
            float4x4 _ColorMatrix;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 rtlb = float4(step(i.uv_border.z, 1), step(i.uv_border.w, 1), step(0, i.uv_border.z), step(0, i.uv_border.w));

                float2 uv_lb = float2(lerp(_UV_x.x, _UV_x.y, i.border_lbrt.x), lerp(_UV_y.x, _UV_y.y, i.border_lbrt.y));
                float2 uv_mid = float2(lerp(_UV_x.y, _UV_x.z, i.uv_border.z), lerp(_UV_y.y, _UV_y.z, i.uv_border.w));
                float2 uv_rt = float2(lerp(_UV_x.z, _UV_x.w, i.border_lbrt.z), lerp(_UV_y.z, _UV_y.w, i.border_lbrt.w));

                float2 uv = uv_lb * (1 - rtlb.zw) + uv_mid * rtlb.zw * rtlb.xy + uv_rt * (1 - rtlb.xy);
                float4 mask = tex2D(_MaskTex, i.uv_mask);
                float4 col = tex2D(_MainTex, uv) * lerp(mask, mask.a, _MaskChannelType);

                col.rgb = mul(_ColorMatrix, float4(col.rgb, 1)).rgb;
                col.a *= _ColorMatrix[3][3];

                return col;
            }
            ENDCG
        }
    }
}
