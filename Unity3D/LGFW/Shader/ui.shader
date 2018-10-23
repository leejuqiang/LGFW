Shader "LGFW/ui" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		
		pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting Off
			Zwrite Off
			Fog {Mode Off}
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			
			struct appdata
			{
				float4 pos: POSITION;
				float2 uv: TEXCOORD0;
				float4 col: COLOR;
			};
	
			struct v2f {
				float4 pos: SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 col: COLOR;
			};
	
			v2f vert (appdata input)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv = input.uv;
				o.col = input.col;
				return o;
			}
			
			float4 frag(v2f o): COLOR
			{
				return tex2D(_MainTex, o.uv) * o.col;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
