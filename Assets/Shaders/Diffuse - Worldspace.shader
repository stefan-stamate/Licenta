Shader "Custom/Diffuse - Worldspace" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Scale ("Texture Scale",Float) = 1.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows

		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		float _Scale;
		half _Glossiness;
		half _Metallic;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float2 UV;
			fixed4 c;

			if (abs(IN.worldNormal.x)>0.5)
			{
				UV = IN.worldPos.yz;
				c = tex2D(_MainTex, UV * _Scale);
			}
			else if (abs(IN.worldNormal.z)>0.5)
			{
				UV = IN.worldPos.xy;
				c = tex2D(_MainTex, UV * _Scale);
			}
			else
			{
				UV = IN.worldPos.xz;
				c = tex2D(_MainTex, UV * _Scale);
			}

			o.Albedo = c.rgb * _Color;

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Diffuse"
}