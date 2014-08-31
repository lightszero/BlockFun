Shader "Custom/pixel" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ColorTex ("Base (RGB)", 2D) = "white" {}
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

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed uy =  ((int)(c.a * 255/16))/16.0;

			fixed ux =  fmod(c.a*255,16) /16;

			half4 cc = tex2D (_ColorTex, half2(ux,uy));
			o.Emission = cc.rgb;
			o.Alpha = cc.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
