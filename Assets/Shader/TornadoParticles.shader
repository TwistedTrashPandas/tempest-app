Shader "Custom/TornadoParticles" {
	Properties{
		g_NoiseTex("g_NoiseTex", 2D) = ""  {}
		g_fBillboardSize("g_BillboardSize", Range(10.0,100000.0)) = 1.0
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
			float4 vertex : POSITION;
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
			o.color = fixed4(0.8f, 0.8f,0.8f,1.0f) / 4.0f;
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

				[maxvertexcount(6)]
				void geom(point v2g p[1], inout TriangleStream<g2f> tristream)
				{
					g2f o;
					g2f o1;
					g2f o2;
					g2f o3;
					g2f o4;
					g2f o5;

					float size_x = g_fBillboardSize / _ScreenParams.x;
					float size_y = g_fBillboardSize / _ScreenParams.y;
					float coord_offset = 1.0f;
					float x = 0.0f;// p[0].uv.x;
					float y = 0.0f;// p[0].uv.y;

					o.vertex = UnityObjectToClipPos(p[0].vertex);
					o.texcoord = float2(x, y);
					o1.vertex = UnityObjectToClipPos(p[0].vertex) + float4(size_x, 0, 0, 1);
					o1.texcoord = float2(x + coord_offset, y);
					o2.vertex = UnityObjectToClipPos(p[0].vertex) + float4(0, size_y, 0, 1);
					o2.texcoord = float2(x, y + coord_offset);

					o3.vertex = UnityObjectToClipPos(p[0].vertex) + float4(size_x, 0, 0, 1);
					o3.texcoord = float2(x + coord_offset, y);
					o4.vertex = UnityObjectToClipPos(p[0].vertex) + float4(size_x, size_y, 0, 1);
					o4.texcoord = float2(x + coord_offset, y + coord_offset);
					o5.vertex = UnityObjectToClipPos(p[0].vertex) + float4(0, size_y, 0, 1);
					o5.texcoord = float2(x, y + coord_offset);

					o.projPos = ComputeScreenPos(o.vertex);
					//COMPUTE_EYEDEPTH(o.projPos.z);
					o1.projPos = ComputeScreenPos(o1.vertex);
					//COMPUTE_EYEDEPTH(o1.projPos.z);
					o2.projPos = ComputeScreenPos(o2.vertex);
					//COMPUTE_EYEDEPTH(o2.projPos.z);
					o3.projPos = ComputeScreenPos(o3.vertex);
					//COMPUTE_EYEDEPTH(o3.projPos.z);
					o4.projPos = ComputeScreenPos(o4.vertex);
					//COMPUTE_EYEDEPTH(o4.projPos.z);
					o5.projPos = ComputeScreenPos(o5.vertex);
					//COMPUTE_EYEDEPTH(o5.projPos.z);
					o.color = p[0].color;
					o1.color = p[0].color;
					o2.color = p[0].color;
					o3.color = p[0].color;
					o4.color = p[0].color;
					o5.color = p[0].color;
					UNITY_TRANSFER_FOG(o, o.vertex);
					UNITY_TRANSFER_FOG(o1, o1.vertex);
					UNITY_TRANSFER_FOG(o2, o2.vertex);
					UNITY_TRANSFER_FOG(o3, o3.vertex);
					UNITY_TRANSFER_FOG(o4, o4.vertex);
					UNITY_TRANSFER_FOG(o5, o5.vertex);

					tristream.Append(o);
					tristream.Append(o1);
					tristream.Append(o2);
					tristream.RestartStrip();

					tristream.Append(o3);
					tristream.Append(o4);
					tristream.Append(o5);
					tristream.RestartStrip();
				}

			ENDCG
			}
	}
		Fallback "VertexLit"
}
