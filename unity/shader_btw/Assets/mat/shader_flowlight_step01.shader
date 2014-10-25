Shader "Custom/shader_flowlight_step01" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FlowTex ("Light Texture(A)", 2D) = "black" {} //流光贴图
		_uvadd   ("",range(0,1)) = 0//流光uv改变量

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _FlowTex;//属性
		float _uvadd;//属性

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			
			float2 uv =IN.uv_MainTex;//计算流光uv
			uv.x/=2;//取一半
			uv.x+=_uvadd;//横向加上
			

			float flow = tex2D (_FlowTex, uv).a;//取流光亮度
			
			o.Albedo = c.rgb +  float3(flow,flow,flow);//加上流光亮度颜色
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
