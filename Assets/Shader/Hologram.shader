Shader "Unlit/Hologram"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DepthScale ("Depth Scale", Range(-10, 0)) = -1
		_DiscardAbove ("Discard Above", Range(0, 1)) = 1
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

			float GetDepthAtUV(sampler2D depthTexture, float2 uv)
			{
				return Linear01Depth(UNITY_SAMPLE_DEPTH(tex2Dlod(depthTexture, float4(uv, 0, 0))));
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _DepthScale;
			float _DiscardAbove;
			
			v2g VS (appdata v)
			{
				v2g o;
				o.position = v.position;
				o.uv = v.uv;
				return o;
			}

			[maxvertexcount(12)]
			void GS(triangle v2g input[3], inout TriangleStream<g2f> stream)
			{
				// Split every triangle up at the center
				float4 positionCenter = (input[0].position + input[1].position + input[2].position) / 3.0f;
				float2 uvCenter = (input[0].uv + input[1].uv + input[2].uv) / 3.0f;

				// Center vertex to append to every new triangle
				g2f center;
				center.depth = GetDepthAtUV(_MainTex, uvCenter);
				center.position = UnityObjectToClipPos(positionCenter + float4(0, 0, _DepthScale * center.depth, 0));
				center.uv = uvCenter;

				g2f o;

				for (int i = 0; i < 3; i++)
				{
					// Current
					o.depth = GetDepthAtUV(_MainTex, input[i].uv);
					o.position = UnityObjectToClipPos(input[i].position + float4(0, 0, _DepthScale * o.depth, 0));
					o.uv = input[i].uv;
					stream.Append(o);

					// Center
					stream.Append(center);

					// Next
					uint next = (i + 2) % 3;
					o.depth = GetDepthAtUV(_MainTex, input[next].uv);
					o.position = UnityObjectToClipPos(input[next].position + float4(0, 0, _DepthScale * o.depth, 0));
					o.uv = input[next].uv;
					stream.Append(o);
					stream.RestartStrip();
				}
			}
			
			float4 FS (g2f i) : SV_Target
			{
				if (i.depth > _DiscardAbove)
				{
					discard;
				}

				return float4(i.depth, i.depth, i.depth, 1);
			}
			ENDCG
		}
	}
}
