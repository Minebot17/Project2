Shader "Custom/NormalTest" {
	Properties {
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "white" {}
		_NormalPower("Normal Power", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert
        sampler2D _MainTex;
        sampler2D _NormalMap;
        float _NormalPower;
        
        struct Input {
            float2 uv_MainTex;
            float2 uv_NormalMap;
        };
        
        void surf(Input IN, inout SurfaceOutput o){
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex);
            o.Normal = lerp(half3(0.0, 0.0, 1.0), UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)), _NormalPower);
        }
        
        ENDCG
	}
	FallBack "Diffuse"
}
