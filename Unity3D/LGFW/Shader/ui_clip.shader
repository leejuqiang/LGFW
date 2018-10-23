Shader "LGFW/ui_clip" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Clip("clip rect", Vector) = (0, 0, 1, 1)
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
			float4 _Clip;
			uniform float4x4 _Mat;
			
			struct appdata
			{
				float4 pos:POSITION;
				float2 uv: TEXCOORD0;
				float4 col: COLOR;
			};
	
			struct v2f {
				float4 pos: SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 col: COLOR;
				float4 p: TEXCOORD1;
			};
	
			v2f vert (appdata input)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv = input.uv;
				o.p = mul(unity_ObjectToWorld, input.pos);
				o.p = mul(_Mat, o.p);
				o.col = input.col;
				return o;
			}
			
			float4 frag(v2f o): COLOR
			{
				fixed temp = 1 - step(0, (o.p.x - _Clip.x) * (o.p.x - _Clip.z));
				temp *= 1 - step(0, (o.p.y - _Clip.y) * (o.p.y - _Clip.w));
				o.col.a *= temp;
				return tex2D(_MainTex, o.uv) * o.col;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
