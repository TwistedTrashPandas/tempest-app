Shader "Unlit/Hologram"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TesselationFactor ("TesselationFactor", Range(1, 64)) = 1
		_DepthScale ("Depth Scale", Range(-10, 10)) = -1
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
			#pragma hull HS
			#pragma domain DS
			#pragma geometry GS
			#pragma fragment FS
			
			#include "UnityCG.cginc"

			struct vertex
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

			struct TesselationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			float GetDepthAtUV(sampler2D depthTexture, float2 uv)
			{
				return Linear01Depth(UNITY_SAMPLE_DEPTH(tex2Dlod(depthTexture, float4(uv, 0, 0))));
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _TesselationFactor;
			float _DepthScale;
			float _DiscardAbove;
			
			v2g VS (vertex v)
			{
				v2g o;
				o.position = v.position;
				o.uv = v.uv;
				return o;
			}

			// Pass vertex data to the tesselation stage
			[UNITY_domain("tri")]
			[UNITY_outputcontrolpoints(3)]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_partitioning("integer")]
			[UNITY_patchconstantfunc("PatchConstantFunction")]
			v2g HS(InputPatch<vertex, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			// Return the tesselation factors for an input patch
			TesselationFactors PatchConstantFunction(InputPatch<vertex, 3> patch)
			{
				TesselationFactors f;
				f.edge[0] = _TesselationFactor;
				f.edge[1] = _TesselationFactor;
				f.edge[2] = _TesselationFactor;
				f.inside = _TesselationFactor;

				return f;
			}

			[UNITY_domain("tri")]
			// Interpolate the values of new vertices by the barycentric coordinates
			v2g DS(TesselationFactors factors, OutputPatch<vertex, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
				v2g o;
				o.position = patch[0].position * barycentricCoordinates.x +
							 patch[1].position * barycentricCoordinates.y +
							 patch[2].position * barycentricCoordinates.z;

				o.uv = patch[0].uv * barycentricCoordinates.x +
					   patch[1].uv * barycentricCoordinates.y +
					   patch[2].uv * barycentricCoordinates.z;

				// Could add a second vertex shader and vertex struct with INTERNALTESSPOS semantic instead of POSITION
				// Then we could have different behaviour for the vertices that were tesselated and maybe no need for a geometry shader
				return VS(o);
			}

			[maxvertexcount(3)]
			void GS(triangle v2g input[3], inout TriangleStream<g2f> stream)
			{
				for (int i = 0; i < 3; i++)
				{
					// Move vertex based on the depth at the texture
					g2f o;
					o.depth = GetDepthAtUV(_MainTex, input[i].uv);
					o.position = UnityObjectToClipPos(input[i].position + float4(0, 0, _DepthScale * o.depth, 0));
					o.uv = input[i].uv;
					stream.Append(o);
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
