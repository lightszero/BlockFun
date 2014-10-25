Shader "Custom/shader_flowlight3d" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_LightTex ("_LightTex Base (RGB) Trans (A)", 2D) = "black" {}
		_speed ("LightSpeed", Vector) = (1,1,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _LightTex;//流光图
		float2  _speed;//速度
		
		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;//增加最终的法线参数
		};

		void surf (Input IN, inout SurfaceOutput o) {
		
		 	float2 ruv = IN.worldNormal.xy;//使用normal来做uv，可以取得如环境反射版的效果
	     	ruv = ruv *0.5;//收敛到-0.5到0.5之间的一个球形单位
	     	ruv+=_Time.xx*_speed;//运动他们
	
			half4 c = tex2D (_MainTex, IN.uv_MainTex)+ tex2D(_LightTex, ruv.xy);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
