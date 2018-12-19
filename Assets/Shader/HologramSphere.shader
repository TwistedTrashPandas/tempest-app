Shader "Unlit/Hologram Sphere"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OutlineIntensity ("Outline Intensity", Float) = 1
		_OutlineExponent ("Outline Exponent", Float) = 1
		_Hue ("Hue", Range(0, 1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "ColorConversion.hlsl"

			struct appdata
			{
				float3 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float distanceToTop : DISTANCE;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _OutlineIntensity;
			float _OutlineExponent;
			float _Hue;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.distanceToTop = length(float3(0, 1, 0) - v.position);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv + _Time.y);

				float2 uvOutline = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 colorOutline = tex2D(_MainTex, uvOutline + float2(_Time.y, 0));
				col += colorOutline * _OutlineIntensity * pow(i.distanceToTop, _OutlineExponent);

				float3 hsv = RGBtoHSV(col.rgb);
				hsv.x = _Hue;
				col.rgb = HSVtoRGB(hsv);

				return col;
			}
			ENDCG
		}
	}
}
