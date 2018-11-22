Shader "Custom/TornadoParticles" {
	Properties{
		g_NoiseTex("g_NoiseTex", 2D) = ""  {}
		g_fBillboardSize("g_BillboardSize", Range(0.1,10.0)) = 1.0
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}
		SubShader{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Lighting.cginc"

		StructuredBuffer<float3> g_vVertices : register(t1);
		float g_fHeightInterp;
		float g_fMaxHeight;
		float g_fBillboardSize;
		struct appdata {
			uint id : SV_VertexID;
		};

		struct v2g
		{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
		};

		struct g2f
		{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_FOG_COORDS(3)
			float4 projPos : TEXCOORD4;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2g vert(appdata v)
		{
			v2g o;
			o.vertex = float4(g_vVertices[v.id], 1);
			o.color = fixed4(1.0f, 1.0f, 1.0f,1.0f) / 4.0f;
			if (o.vertex.y > g_fHeightInterp)
				o.color *= (1.0f - (o.vertex.y - g_fHeightInterp) / (g_fMaxHeight - g_fHeightInterp));
			return o;
		}
		sampler2D g_NoiseTex;
		sampler2D _CameraDepthTexture;
		float _InvFade;

		fixed4 frag(g2f i) : SV_Target
		{
			float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
			float partZ = i.projPos.z;
			float fade = saturate(_InvFade * (sceneZ - partZ));
			i.color.a *= fade;

			fixed4 colA = tex2D(g_NoiseTex, i.texcoord);
			colA.a = colA.r;

			fixed4 col = i.color * colA;
			UNITY_APPLY_FOG(i.fogCoord, col);

			return col;
			//float4 color = tex2D(g_NoiseTex, i.uv) ;
			//return float4(color.x, color.x ,color.x, color.x);
		//return i.color;
	}

		ENDCG

		//	pass for directional lights
	Pass {
			ColorMask RGB
			Cull Off Lighting On ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
				//Blend One OneMinusSrcAlpha

				//Blend One SrcColor
					// BlendOp Max
					Tags{ "LightMode" = "ForwardBase" }
					CGPROGRAM

					#pragma multi_compile_fwdbase 
					#pragma multi_compile_particles
					#pragma multi_compile_fog
					#pragma vertex vert
					#pragma geometry geom
					#pragma fragment frag
					#pragma target 2.0
				// ------------ GEOMETRY SHADER ---------------
					[maxvertexcount(4)]
					void geom(point v2g p[1], inout TriangleStream<g2f> tristream)
					{
						float3 up = float3(0, 1, 0);
						float3 look = _WorldSpaceCameraPos - p[0].vertex;
						look.y = 0;
						look = normalize(look);
						float3 right = cross(up, look);

						float halfS = 0.5f * g_fBillboardSize;

						float4 v[4];
						v[0] = float4(p[0].vertex + halfS * right - halfS * up, 1.0f);
						v[1] = float4(p[0].vertex + halfS * right + halfS * up, 1.0f);
						v[2] = float4(p[0].vertex - halfS * right - halfS * up, 1.0f);
						v[3] = float4(p[0].vertex - halfS * right + halfS * up, 1.0f);

						g2f o;

						float coord_offset = 1.0f;
						float x = 0.0f;// p[0].uv.x;
						float y = 0.0f;// p[0].uv.y;

						o.vertex = UnityObjectToClipPos(v[0]);
						o.texcoord = float2(x + coord_offset, y);
						o.projPos = ComputeScreenPos(v[0]);
						o.color = p[0].color;
						UNITY_TRANSFER_FOG(o, o.vertex);
						//COMPUTE_EYEDEPTH(o.projPos.z);
						tristream.Append(o);

						o.vertex = UnityObjectToClipPos(v[1]);
						o.texcoord = float2(x + coord_offset, y + coord_offset);
						o.projPos = ComputeScreenPos(v[1]);
						o.color = p[0].color;
						UNITY_TRANSFER_FOG(o, o.vertex);
						tristream.Append(o);

						o.vertex = UnityObjectToClipPos(v[2]);
						o.texcoord = float2(x, y);
						o.projPos = ComputeScreenPos(v[2]);
						o.color = p[0].color;
						UNITY_TRANSFER_FOG(o, o.vertex);
						tristream.Append(o);

						o.vertex = UnityObjectToClipPos(v[3]);
						o.texcoord = float2(x, y + coord_offset);
						o.projPos = ComputeScreenPos(v[3]);
						o.color = p[0].color;
						UNITY_TRANSFER_FOG(o, o.vertex);
						tristream.Append(o);
						//tristream.RestartStrip();
					}

				ENDCG
				}
		}
			Fallback "VertexLit"
}
