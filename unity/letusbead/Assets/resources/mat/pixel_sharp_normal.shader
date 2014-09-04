Shader "Custom/pixel_sharp_normal" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ColorTex ("ColorTex (RGB)", 2D) = "white" {}
		_SharpTex ("SharpTex (RGB)", 2D) = "white" {}
		_NormalTex ("NormalTex (RGB)", 2D) = "white" {}
		_DrawSize ("DrawSize" , Vector)=(32,32,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 200
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _ColorTex;
		sampler2D _SharpTex;
		sampler2D _NormalTex;
		float2 _DrawSize;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			
			float4 c = tex2D (_MainTex, IN.uv_MainTex);
			float uy =  ((int)(c.a * 255/16))/16.0;

			float ux =  fmod(c.a*255,16) /16;

			float4 cc = tex2D (_ColorTex, float2(ux,uy));

			float sux = frac(IN.uv_MainTex.x*_DrawSize.x) ;
			float suy = frac(IN.uv_MainTex.y*_DrawSize.y);
			float4 sc = tex2D (_SharpTex, float2(sux,suy));

			o.Albedo = cc.rgb*sc.rgb;
			o.Normal = tex2D(_NormalTex,IN.uv_MainTex).xyz*2 -1;
			o.Alpha = cc.a*sc.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
