// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// 此Shader用于特效亮度叠加贴图
// 参数：1张无透明通道的贴图，渲染方式为颜色叠加，贴图黑色为无光白色为白光叠加

Shader "E3DEffect/C1/Add" {
Properties {
	_Alpha ("Alpha", Range(0,1.0)) = 1.0
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,1.0)
	_MainTex ("Base Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			float _Alpha;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			
			fixed4 frag (v2f i) : COLOR
			{
				return 2.0f * i.color * _TintColor * _Alpha * tex2D(_MainTex, i.texcoord);
			}
			ENDCG 
		}
	}	
}
}