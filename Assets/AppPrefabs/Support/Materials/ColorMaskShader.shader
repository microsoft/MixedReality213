Shader "HUX/ColorMaskShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_ColorMask("Alpha", 2D) = "white" {}
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		_GlossMapScale("Smoothness", Range(0, 1)) = 0
		_MetallicFactor("Metallic Factor",Range(0, 1)) = 0
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Scale", Float) = 1.0
		_DetailAlbedoMap("Detail Albedo x2", 2D) = "white" {}	}
		SubShader{
			Tags{ "RenderType" = "Opaque" }
			LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows

		sampler2D _MainTex;
		sampler2D _DetailAlbedoMap;
		sampler2D _BumpMap;
		sampler2D _MetallicGlossMap;
		sampler2D _ColorMask;

		float _MetallicFactor;
		float _BumpScale;
		float _GlossMapScale;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_DetailAlbedoMap;
		};

		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 m = tex2D(_ColorMask, IN.uv_MainTex);
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * tex2D(_DetailAlbedoMap, IN.uv2_DetailAlbedoMap);
			c.rgb *= lerp(1, _Color, m.a);
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex)) * _BumpScale;
			fixed4 mc = tex2D(_MetallicGlossMap, IN.uv_MainTex);
			o.Metallic = mc.r * _MetallicFactor;
			o.Smoothness = mc.a * _GlossMapScale;
		}
		ENDCG
	}
	FallBack "Diffuse"
}