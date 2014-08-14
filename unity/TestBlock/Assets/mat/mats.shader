Shader "Custom/mats" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlockTex ("Block (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _BlockTex;
		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed ia = tex2D (_MainTex, IN.uv_MainTex).a;
			int sy =  (ia * 255) /4;
			int sx =  fmod((ia * 255),4);
			float ux = frac(IN.uv_MainTex.x*512) /4;
			float uy = frac(IN.uv_MainTex.y*512) /4+sy;

			half4 c = tex2D (_BlockTex,float2(ux,uy));
			o.Albedo = c.rgb;
			o.Alpha = 1;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
