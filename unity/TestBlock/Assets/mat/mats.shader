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
			float width =1024;
			half2 suv=IN.uv_MainTex;//source UV

			//qu int
		    half2 uv = half2((int)(suv.x*width),(int)(suv.y*width));
			fixed ia = tex2D (_MainTex, uv/width).a;

			fixed sy =  ((int)(ia*16/4))*0.25;
			fixed sx =  fmod((ia * 16),4)*0.25;
			float ux = (frac(suv.x*width)) *0.25 ;
			float uy = frac(suv.y*width)*0.25+sy;


			half4 c = tex2D (_BlockTex,half3(ux,uy,0.9));
			o.Albedo = c.rgb;

			o.Alpha = 1;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
