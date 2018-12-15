Shader "Unlit/Hologram"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DepthScale ("Depth Scale", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex VS
			#pragma geometry GS
			#pragma fragment FS
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2g
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct g2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float depth : DEPTH;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _DepthScale;
			
			v2g VS (appdata v)
			{
				v2g o;
				o.position = v.position;
				o.uv = 1.0f - v.uv;
				return o;
			}

			[maxvertexcount(12)]
			void GS(triangle v2g input[3], inout TriangleStream<g2f> stream)
			{
				float4 positionCenter = (input[0].position + input[1].position + input[2].position) / 3.0f;
				float uvCenter = (input[0].uv + input[1].uv + input[2].uv) / 3.0f;

				g2f o;

				for (int i = 0; i < 3; i++)
				{
					float depth = UNITY_SAMPLE_DEPTH(tex2Dlod(_MainTex, float4(input[i].uv, 0, 0)));

					o.position = UnityObjectToClipPos(input[i].position + float4(0, _DepthScale * depth, 0, 0));
					o.uv = input[i].uv;
					o.depth = depth;
					stream.Append(o);
				}
			}
			
			float4 FS (g2f i) : SV_Target
			{
				return Linear01Depth(i.depth);
			}
			ENDCG
		}
	}
}
