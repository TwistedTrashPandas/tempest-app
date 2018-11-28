Shader "Custom/TornadoParticles" {
	Properties{
		g_NoiseTex("g_NoiseTex", 2D) = ""  {}
		g_Tex1("g_Tex1", 2D) = ""  {}
		g_Tex2("g_Tex2", 2D) = ""  {}
		g_NormalTex1("g_NormalTex1", 2D) = ""  {}
		g_NormalTex2("g_NormalTex2", 2D) = ""  {}
		g_Color("g_Color", Color) = (1,1,1,1)
		g_SpecColor("g_SpecColor", Color) = (1,1,1,1)
		g_fBillboardSize("g_BillboardSize", Range(0.01,10.0)) = 1.0
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		g_fTimeDiff("TimeDiff", Range(0.0, 1.0)) = 0.01
		g_fTimeStepTex("TimeStepTex", Range(0.05, 1.0)) = 1.0
	}
		SubShader{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Lighting.cginc"

		StructuredBuffer<float3> g_vVertices : register(t0);
		StructuredBuffer<float3> g_vInitialWorldPos : register(t1);
		StructuredBuffer<int> g_iIndices : register(t2);

		fixed4 g_Color;
		fixed4 g_SpecColor;
		float4 g_vCenter;
		float g_fHeightInterp;
		float g_fMaxHeight;
		float g_fBillboardSize;
		float g_fTimeDiff;
		float g_fTimeStepTex;
		sampler2D g_TornadoTex;
		sampler2D g_Tex1;
		sampler2D g_Tex2;
		sampler2D g_NormalTex1;
		sampler2D g_NormalTex2;

		struct appdata {
			uint id : SV_VertexID;
		};

		struct v2g
		{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float3 normal : NORMAL;
			uint id : VertexID;
		};

		struct g2f
		{
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float2 texcoord_2 : TEXCOORD1;
			UNITY_FOG_COORDS(3)
			float4 projPos : TEXCOORD4;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		half3 BlinnPhong(half3 lightDir, half3 normal, half3 viewDir) {

			half3 lightOut;
			half distance = length(lightDir);
			lightDir = lightDir / distance;
			distance = distance * distance;

			half NdotL = dot(normal, lightDir);
			half intensity = saturate(NdotL);

			half3 diffuse = intensity * _LightColor0.rgb * g_Color.rgb / distance;

			half3 H = normalize(lightDir + viewDir);

			half NdotH = dot(normal, H);
			intensity = pow(saturate(NdotH), 0.0);

			lightOut = diffuse + intensity * _LightColor0.rgb  * g_SpecColor.rgb / distance;
			return lightOut;
		}

		v2g vert(appdata v)
		{
			v2g o;
			v.id = g_iIndices[v.id];
			o.vertex = float4(g_vVertices[v.id], 1);
			o.color = fixed4(1.0f, 1.0f, 1.0f,1.0f);
			if (o.vertex.y > g_fHeightInterp)
				o.color *= (1.0f - (o.vertex.y - g_fHeightInterp) / (g_fMaxHeight - g_fHeightInterp));
			o.normal = normalize(o.vertex.xyz - float3(g_vCenter.x, o.vertex.y, g_vCenter.z));
			o.id = v.id;
			return o;
		}

		sampler2D g_NoiseTex;
		sampler2D _CameraDepthTexture;
		float _InvFade;

		fixed4 frag(g2f i) : SV_Target
		{
			float3 look = WorldSpaceViewDir(i.vertex);
			look.y = 0;
			look = normalize(look);
			float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
			float partZ = i.projPos.z;
			float fade = saturate(_InvFade * (sceneZ - partZ));
			i.color.a *= fade;

			float alpha = max(min(g_fTimeStepTex / g_fTimeDiff,1),0);
			fixed4 colA = fixed4((tex2D(g_Tex2, i.texcoord) * alpha + tex2D(g_Tex1, i.texcoord) * (1.0 - alpha)).xyz, g_Color.a);
			colA.a += colA.r;
			float3 normal = normalize(tex2D(g_NormalTex2, i.texcoord) * alpha + tex2D(g_NormalTex1, i.texcoord) * (1.0 - alpha));
			fixed4 colL = fixed4(BlinnPhong(_WorldSpaceLightPos0.xyz, i.normal, look), g_Color.a);

			fixed4 col = (colL * colA) * i.color;
			UNITY_APPLY_FOG(i.fogCoord, col);
			col.a *= tex2D(g_NoiseTex, i.texcoord_2).r;
			return col;
	}

		ENDCG

		//	pass for directional lights
	Pass {
			ColorMask RGB
			Cull Back Lighting On ZWrite Off
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

				float2 rotate2DVector(float2 v, float2 m, float angle) {
					float sin_val = sin(angle);
					float cos_val = cos(angle);
					// multiply inv transform with rot with transform matrix
					float3x3 mat =
						float3x3(cos_val, -sin_val, 0,
							sin_val, cos_val, 0,
							0, 0, 1);
					return mul(mat, float3(v-m, 1.0f)).xy+m;
				}


				float4 g_i3Dimensions;

				// ------------ GEOMETRY SHADER ---------------
				[maxvertexcount(4)]
				void geom(point v2g p[1], inout TriangleStream<g2f> tristream)
				{
					float3 up = float3(0, 1, 0);
					float3 look = WorldSpaceViewDir(p[0].vertex);
					look.y = 0;
					look = normalize(look);
					float3 right = cross(up, look);

					float halfS = 0.5f * g_fBillboardSize;

					float4 v[4];
					v[0] = float4(halfS * right - halfS * up, 1.0f);
					v[1] = float4(halfS * right + halfS * up, 1.0f);
					v[2] = float4(-halfS * right - halfS * up, 1.0f);
					v[3] = float4(-halfS * right + halfS * up, 1.0f);

					float mul_val_x = 1.0f / g_i3Dimensions.w;
					float mul_val_y = mul_val_x / g_i3Dimensions.y;
					mul_val_x /= g_i3Dimensions.x;

					float3 startPos = g_vInitialWorldPos[p[0].id];
					look = WorldSpaceViewDir(float4(startPos,1.0));
					look.y = 0;
					look = normalize(look);

					float angle = atan2(look.x, look.z) - atan2(0.0, 1.0);
					// angle = (angle < 0.0) ? 360.0 + angle : angle;
					startPos.xz = rotate2DVector(startPos.xz, g_vCenter.xz, radians(angle));

					g2f o;
					o.normal = p[0].normal;
					o.color = p[0].color;

					o.texcoord = float2(min(max((v[0].x+ startPos.x)*mul_val_x,0), 1.0f), max(min((v[0].y + startPos.y)*mul_val_y + 0.1f * g_i3Dimensions.y, 1.0), 0.0f));
					o.texcoord_2 = float2(1, 0);
					v[0] += p[0].vertex;
					o.vertex = UnityObjectToClipPos(v[0]);
					o.projPos = ComputeScreenPos(v[0]);
					UNITY_TRANSFER_FOG(o, o.vertex);
					//COMPUTE_EYEDEPTH(o.projPos.z);
					tristream.Append(o);

					o.texcoord = float2(min(max((v[1].x + startPos.x)*mul_val_x, 0), 1.0f), max(min((v[1].y + startPos.y)*mul_val_y, 1.0), 0.0f));
					o.texcoord_2 = float2(1, 1);
					v[1] += p[0].vertex;
					o.vertex = UnityObjectToClipPos(v[1]);
					o.projPos = ComputeScreenPos(v[1]);
					UNITY_TRANSFER_FOG(o, o.vertex);
					tristream.Append(o);

					o.texcoord = float2(min(max((v[2].x + startPos.x)*mul_val_x, 0), 1.0f), max(min((v[2].y + startPos.y)*mul_val_y + 0.1f * g_i3Dimensions.y, 1.0), 0.0f));
					o.texcoord_2 = float2(0, 0);
					v[2] += p[0].vertex;
					o.vertex = UnityObjectToClipPos(v[2]);
					o.projPos = ComputeScreenPos(v[2]);
					UNITY_TRANSFER_FOG(o, o.vertex);
					tristream.Append(o);

					o.texcoord = float2(min(max((v[3].x + startPos.x)*mul_val_x, 0), 1.0f), max(min((v[3].y + startPos.y)*mul_val_y, 1.0), 0.0f));
					o.texcoord_2 = float2(0, 1);
					v[3] += p[0].vertex;
					o.vertex = UnityObjectToClipPos(v[3]);
					o.projPos = ComputeScreenPos(v[3]);
					UNITY_TRANSFER_FOG(o, o.vertex);
					tristream.Append(o);
					//tristream.RestartStrip();
				}

			ENDCG
			}
		}
			Fallback "VertexLit"
}
