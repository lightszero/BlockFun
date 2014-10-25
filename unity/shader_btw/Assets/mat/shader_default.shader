Shader "Custom/shader_default" //Shader的名字
{
//这一段是出现在Inspector面板中的Shader的属性
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert	//这里指定shader类型
		//surface代表surfaceshader，surf代表surfaceshader的函数名为surf
		//Lambert代表光照模型

		sampler2D _MainTex;//这里要有和Properties里面的同名变量

		struct Input {
			float2 uv_MainTex;//指定surfaceshader输入参数
		};

		void surf (Input IN, inout SurfaceOutput o) {
			//采样贴图
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			
			o.Albedo = c.rgb;//这是输出的漫反射颜色，会受光照影响
			o.Alpha = c.a;//这是输出的alpha，在默认的renderqueue设置下，alpha没有效果
		}
		ENDCG
	} 
	FallBack "Diffuse"//在当前shader不支持的时候使用Diffuse替代
					  //有很多用途，比如我们没有指定计算投影的shader函数
					  //使用Fallback选项，在此材质计算投影时，使用Diffuse替代
					  //否则我们不指定投影shader的情况下，该shader没有投影功能
}	
