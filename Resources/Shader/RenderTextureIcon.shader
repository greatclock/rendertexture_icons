Shader "Hidden/GreatClock/RenderTextureAtlas/IconRender" {
	Properties {
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
	}
	SubShader {
		ZTest Always
		Cull Off
		ZWrite Off
		
		Pass  {
			Blend Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _TargetRect;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_t v) {
				v2f o;
                float4 vertex = UnityObjectToClipPos(v.vertex);
                float4 rect = _TargetRect;
                #if UNITY_UV_STARTS_AT_TOP
                rect.y = 1 - rect.y - rect.w;
                #endif
                float2 xy = rect.xy + rect.zw * (vertex.xy / vertex.w + 1) * 0.5;
                o.vertex = float4((xy * 2 - 1) * vertex.w, vertex.z, vertex.w);
				o.uv = v.uv;
				return o;
			}

			half4 frag(v2f i) : SV_Target {
				return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}
	} 
	FallBack off
}
