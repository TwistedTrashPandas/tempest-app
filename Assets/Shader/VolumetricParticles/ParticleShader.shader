Shader "Custom/CloudPart" {
	Properties{
		g_tex3DNoise("_NoiseTex", 3D) = ""  {}
		g_tex3DParticleDensityLUT("_ParticleDensity", 3D) = ""  {}
		g_tex3DSingleScatteringInParticleLUT("_SingleScatter", 3D) = ""  {}
		g_tex3DMultipleScatteringInParticleLUT("_MultipleScatter", 3D) = ""  {}
		g_Color("Color", Color) = (1,1,1,1)
		g_fSize("Size", Range(0.1,1000.0)) = 1.0
		g_fSizeTop("g_fSizeTop", Range(1.0,100.0)) = 1.0
		g_fDensity("Density", Range(0.01,1000.0)) = 1
		g_fLightAttenuation("Attenuation", Range(0.01,1000.0)) = 1
		g_fMinTransparency("MinTransparency", Range(0.001,1.0)) = 0.25
		g_fMaxTransparency("MaxTransparency", Range(0.001,1.0)) = 1
		g_fMaxTransparencyDist("MaxTransparencyDist", Range(0.001,10000.0)) = 100.0
		g_bSize("FlexibleSize", Range(0.0,1.0)) = 0.0
		g_fTopHeight("TopHeight", Range(0.0,10000.0)) = 0.0
	}
		SubShader{

		Tags {	"Queue" = "Transparent"	"IgnoreProjector" = "True"	"RenderType" = "Transparent"
		}
		CGINCLUDE

		#pragma multi_compile_fwdbase 

		#include "structs.cginc"
		#include "common.cginc"
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Lighting.cginc"

		float4 g_vCenter;
		float g_fHeightInterp;
		float g_fSizeTop;
		float g_fMaxHeight;
		float g_fBillboardSize;
		float g_fTimeDiff;
		float g_fTimeStepTex;
		float g_fTopHeight;
		float g_fMinTransparency;
		float g_fMaxTransparency;
		float g_fMaxTransparencyDist;

		static const float g_fCloudExtinctionCoeff = 100;

		// Minimal cloud transparancy not flushed to zero
		static const float g_fTransparencyThreshold = 0.01;

		// Fraction of the particle cut off distance which serves as
		// a transition region from particles to flat clouds
		static const float g_fParticleToFlatMorphRatio = 0.2;

		static const float g_fTimeScale = 1.f;
		static const float2 g_f2CloudDensitySamplingScale = float2(1.f / 200000.f, 1.f / 20000.f);

		#define FLT_MAX 3.402823466e+38f


		struct appdata {
			uint id : SV_VertexID;
		};

		struct v2g
		{
			float4 vertex : SV_POSITION;
			uint id : VertexID;
		};

		struct PS_Input
		{
			float4 f4Pos : SV_Position;
			nointerpolation uint uiParticleID : PARTICLE_ID;
		};

		struct g2f
		{
			float4 pos : SV_POSITION;
			uint id : VertexID;
			float4 projPos: TEXCOORD1;
			float4 uv: TEXCOORD0;
			//LIGHTING_COORDS(2, 3)
				//UNITY_FOG_COORDS(4)
		};


		Texture3D<float2>			g_tex3DParticleDensityLUT    : register(t0);
		Texture3D<float>			g_tex3DSingleScatteringInParticleLUT   : register(t1);
		Texture3D<float>			g_tex3DMultipleScatteringInParticleLUT : register(t2);
		Texture2D<float3>			g_tex2DAmbientSkylight               : register(t4);
		Texture3D<float>			g_tex3DNoise                 : register(t6);
		StructuredBuffer<float3>	g_vVertices : register(t3);
		StructuredBuffer<float>		g_vRndAzimuth : register(t7);
		StructuredBuffer<int>		g_iIndices : register(t5);
		StructuredBuffer<int>		particleVisibilityRW : register(t8);


		sampler2D _CameraDepthTexture;

		float g_fSize;
		float g_bSize;
		float g_fDensity;
		float g_fLightAttenuation;
		float4 g_Color;
		float4 g_f4ShipPosition;

		// ---------------------------------------------------------------------------------------------------------------------------------

		float3 GetParticleScales(in float fSize, in float fNumActiveLayers)
		{
			float3 f3Scales = fSize;
			//if( fNumActiveLayers > 1 )
			//    f3Scales.y = max(f3Scales.y, g_GlobalCloudAttribs.fCloudThickness/fNumActiveLayers);
			//f3Scales.y = min(f3Scales.y, fCloudThickness / 2.f);
			return f3Scales;
		}

		void IntersectRayWithParticle(const in SParticleAttribs ParticleAttrs,
			const in float3 f3CameraPos,
			const in float3 f3ViewRay,
			const in int id,
			out float2 f2RayIsecs,
			out float3 f3EntryPointUSSpace, // Entry point in Unit Sphere (US) space
			out float3 f3ViewRayUSSpace,    // View ray direction in Unit Sphere (US) space
			out float3 f3LightDirUSSpace,   // Light direction in Unit Sphere (US) space
			out float fDistanceToEntryPoint,
			out float fDistanceToExitPoint)
		{
			/*
			float3 f3Forward = normalize(ParticleAttrs.f3Pos - _WorldSpaceCameraPos).xyz;
			float cosAng = dot(float3(0, 0, 1), f3Forward);
			float3 f3RotAxis = -normalize(cross(float3(0, 0, 1), f3Forward));
			if (cosAng < -1) {
				cosAng = 1.0f - cosAng;
				f3RotAxis = -f3RotAxis;
			}
			float sinAng = sin(acos(cosAng));
			float minCos = 1.0f - cosAng;
			float uxy = minCos * f3RotAxis.x * f3RotAxis.y;
			float uxz = minCos * f3RotAxis.x * f3RotAxis.z;
			float uyz = minCos * f3RotAxis.z * f3RotAxis.y;
			float xSin = sinAng * f3RotAxis.x;
			float ySin = sinAng * f3RotAxis.y;
			float zSin = sinAng * f3RotAxis.z;

			matrix mRotCamDirMatrix = matrix(
				cosAng + f3RotAxis.x*f3RotAxis.x*minCos, uxy - zSin, uxz + ySin, 0,
				uxy + zSin, cosAng + f3RotAxis.y*f3RotAxis.y*minCos, uyz - xSin, 0,
				uxz - ySin, uyz + xSin, cosAng + f3RotAxis.z*f3RotAxis.z*minCos, 0,
				0, 0, 0, 1
				);
			*/
			matrix f3x3WorldToObjSpaceRotation = (unity_WorldToObject);// mRotCamDirMatrix;//  float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);//; float3x3(1, 0, 0, 0, 1, 0,0, 0, 1); //
			// Compute camera location and view direction in particle's object space:
			float3 f3CamPosObjSpace = f3CameraPos - ParticleAttrs.f3Pos;
			f3CamPosObjSpace = mul(f3x3WorldToObjSpaceRotation, float4(f3CamPosObjSpace, 1.0f));
			float3 f3ViewRayObjSpace = mul(f3x3WorldToObjSpaceRotation, f3ViewRay);
			float3 f3LightDirObjSpce = mul(f3x3WorldToObjSpaceRotation, -_WorldSpaceLightPos0.xyz);

			// Compute scales to transform ellipsoid into the unit sphere:
			float3 f3Scale; // 1.f / GetParticleScales(ParticleAttrs.fSize, CellAttrs.uiNumActiveLayers);

			if (g_bSize < 0.5f)
				f3Scale = 1.0f / g_fSize;
			else {
				float dist = distance(ParticleAttrs.f3Pos, g_f4ShipPosition.xyz) / 312.0f;
				f3Scale = 1.0f / (g_fSize * max(min(1.0f, dist), 0.15f));
			}
			if (ParticleAttrs.f3Pos.y > g_fTopHeight)
				f3Scale /= g_fSizeTop;

			float3 f3ScaledCamPosObjSpace;
			f3ScaledCamPosObjSpace = f3CamPosObjSpace.xyz * f3Scale;
			f3ViewRayUSSpace = normalize(f3ViewRayObjSpace.xyz*f3Scale);
			f3LightDirUSSpace = normalize(f3LightDirObjSpce.xyz*f3Scale);
			// Scale camera pos and view dir in obj space and compute intersection with the unit sphere:
			GetRaySphereIntersection(f3ScaledCamPosObjSpace, f3ViewRayUSSpace, 0, 1.0f, f2RayIsecs);

			f3EntryPointUSSpace = f3ScaledCamPosObjSpace + f3ViewRayUSSpace * f2RayIsecs.x;

			fDistanceToEntryPoint = length(f3ViewRayUSSpace / f3Scale) * f2RayIsecs.x;
			fDistanceToExitPoint = length(f3ViewRayUSSpace / f3Scale) * f2RayIsecs.y;
		}


		void ComputeParticleRenderAttribs(const in SParticleAttribs ParticleAttrs,
			in float fTime,
			in float3 f3CameraPos,
			in float3 f3ViewRay,
			in float3 f3EntryPointUSSpace, // Ray entry point in unit sphere (US) space
			in float3 f3ViewRayUSSpace,    // View direction in unit sphere (US) space
			in float  fIsecLenUSSpace,     // Length of the intersection of the view ray with the unit sphere
			in float3 f3LightDirUSSpace,   // Light direction in unit sphere (US) space
			in float fDistanceToExitPoint,
			in float fDistanceToEntryPoint,
			out float fCloudMass,
			out float fTransparency,
			uniform in bool bAutoLOD,
			uniform in bool lightPass,
			uniform in bool shadowCollector,
			in SCloudParticleLighting ParticleLighting,
			out float4 f4Color)
		{
			float3 f3EntryPointWS = f3CameraPos + fDistanceToEntryPoint * f3ViewRay;
			float3 f3ExitPointWS = f3CameraPos + fDistanceToExitPoint * f3ViewRay;

			float4 f4LUTCoords;
			WorldParamsToOpticalDepthLUTCoords(f3EntryPointUSSpace, f3ViewRayUSSpace, f4LUTCoords);
			f4LUTCoords.y += ParticleAttrs.fRndAzimuthBias;

			float2 f2NormalizedDensityAndDist;
			SAMPLE_4D_LUT_FLT2(g_tex3DParticleDensityLUT, OPTICAL_DEPTH_LUT_DIM, f4LUTCoords, 0, f2NormalizedDensityAndDist);

			float3 f3FirstMatterPointWS = f3CameraPos + (fDistanceToEntryPoint + (fDistanceToExitPoint - fDistanceToEntryPoint) * f2NormalizedDensityAndDist.y) * f3ViewRay;
			float3 f3FirstMatterPointUSSpace = f3EntryPointUSSpace + (fIsecLenUSSpace * f2NormalizedDensityAndDist.y) * f3ViewRayUSSpace;

			float3 f3NoiseSamplingPos = f3FirstMatterPointWS;
			float fNoisePeriod = 3412;
			float fNoise = (g_tex3DNoise.Sample(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod)) * 2 + g_tex3DNoise.Sample(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod / 3))) / 3;
			//	(g_tex3DNoise.SampleLevel(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod), 0) * 2 + g_tex3DNoise.SampleLevel(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod / 3), 0)) / 3;

			float fNoNoiseY = -0.7;
			float fTransition = min(1 - fNoNoiseY, 0.5);
			fNoise = lerp(0.5, fNoise, max(saturate((f3EntryPointUSSpace.y - fNoNoiseY) / fTransition), 0.2));

			fCloudMass = f2NormalizedDensityAndDist.x * (fDistanceToExitPoint - fDistanceToEntryPoint);
			fCloudMass *= ParticleAttrs.fDensity *lerp(fNoise, 1, 0.5);

			fTransparency = 1.0f - exp(-fCloudMass * 0.07f); // range 0.01 - 0.1 m^-1
			if (!lightPass && !shadowCollector) {
				float fMultipleScatteringDensityScale = ParticleAttrs.fDensity;
				float fSingleScatteringDensityScale = fMultipleScatteringDensityScale * (f2NormalizedDensityAndDist.x);

				float4 f4SingleScatteringLUTCoords = WorldParamsToParticleScatteringLUT(f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace, true, fSingleScatteringDensityScale);
				float4 f4MultipleScatteringLUTCoords = WorldParamsToParticleScatteringLUT(f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace, true, fMultipleScatteringDensityScale);
				float fSingleScattering = 0, fMultipleScattering = 0;
				SAMPLE_4D_LUT_FLT(g_tex3DSingleScatteringInParticleLUT, SRF_SCATTERING_IN_PARTICLE_LUT_DIM, f4SingleScatteringLUTCoords, 0, fSingleScattering);
				SAMPLE_4D_LUT_FLT(g_tex3DMultipleScatteringInParticleLUT, SRF_SCATTERING_IN_PARTICLE_LUT_DIM, f4MultipleScatteringLUTCoords, 0, fMultipleScattering);
				float fCosTheta = dot(-f3ViewRayUSSpace, f3LightDirUSSpace);
				float PhaseFunc = HGPhaseFunc(fCosTheta);

				fSingleScattering *= PhaseFunc;
				float fSSSStartRadius = 3.f / 5.f;
				float fMultipleScatteringSSSStrength = 0.8f;
				float fFirstMatterPtRadius = length(f3FirstMatterPointUSSpace);
				float fSubSrfScattering = saturate((1 - fFirstMatterPtRadius) / (1 - fSSSStartRadius));
				fMultipleScattering *= (1 + fMultipleScatteringSSSStrength * fSubSrfScattering);
				float3 f3Ambient = ParticleLighting.f4AmbientLight.rgb;

				fSingleScattering *= ParticleLighting.f4LightAttenuation.x;
				fMultipleScattering *= ParticleLighting.f4LightAttenuation.y * (0.5 + 1.0*fNoise);

				float fAmbientSSSStrength = (1 - fNoise)*0.5;//0.3;
				f3Ambient.rgb *= lerp(1, fSubSrfScattering, fAmbientSSSStrength);
				f4Color.rgb = (f3Ambient.rgb + (fSingleScattering + fMultipleScattering) * ParticleLighting.f4SunLight.rgb) * PI;
				if (ParticleAttrs.f3Pos.y <= g_fTopHeight)
					f4Color.rgb *= g_Color.rgb;
			}
			else
				f4Color.rgb = 1.0f;

			if (!shadowCollector) {
				f4Color.a = fTransparency;
			}
			else {
				f4Color.rgb = 0.0f;
				f4Color.a = (1 - ParticleLighting.f4LightAttenuation.x) * 1.0f;
			}
			// max((f2NormalizedDensityAndDist.x), 0); //
			//f4Color.b = 0;// f4LUTCoords.w;// max((f2NormalizedDensityAndDist.y), 0);// (f3Ambient.rgb + (fSingleScattering + fMultipleScattering) * ParticleLighting.f4SunLight.rgb) * PI;
			//f4Color.g = 0;// fTransparency;// (f3Ambient.rgb + (fSingleScattering + fMultipleScattering) * ParticleLighting.f4SunLight.rgb) * PI;
			//f4Color.a = 1.0f;// fTransparency;//  f4Color.g; // 
			//f4Color.r = f2NormalizedDensityAndDist.x; // (fDistanceToExitPoint - fDistanceToEntryPoint); // 
			//f4Color.gb = 0;
			//if (f4LUTCoords.y < 1.0-sqrt(0.5))
			//	f4Color.a = 0.0f;
			//f4Color.b = f4LUTCoords.w;
		}
		
		[maxvertexcount(10 + 4 + 4)]
		void geom(point v2g p[1], inout TriangleStream<g2f> Out)
		{
			uint uiParticleId = p[0].id;
			if (particleVisibilityRW[g_iIndices[uiParticleId]] == 0)
				return;
			// Only visible particles are sent for rendering, so there is no need to
			// test visibility here
			//bool bIsValid = g_VisibleParticleFlags[uiParticleId/32] & (1 << (uiParticleId&31));
			//if( !bIsValid )
			//    return;

			g2f Outs[8];
			// matrix mViewProj = ; // g_CameraAttribs.WorldViewProj;
			// Multiply with camera view-proj matrix
			matrix g_CameraRotation;
			g_CameraRotation = matrix(1, 0, 0, 0,
				0, 1, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1
				);/*
				float3 forward = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				float3 right = cross(up, forward);*/
			/*float3 f3Forward = normalize(p[0].vertex - _WorldSpaceCameraPos).xyz;
			float cosAng = dot(float3(0, 0, 1), f3Forward);
			float3 f3RotAxis = normalize(cross(float3(0, 0, 1), f3Forward));
			if (cosAng < -1) {
				cosAng = 1.0f - cosAng;
				f3RotAxis = -f3RotAxis;
			}
			float sinAng = sin(acos(cosAng));
			float minCos = 1.0f - cosAng;
			float uxy = minCos * f3RotAxis.x * f3RotAxis.y;
			float uxz = minCos * f3RotAxis.x * f3RotAxis.z;
			float uyz = minCos * f3RotAxis.z * f3RotAxis.y;
			float xSin = sinAng * f3RotAxis.x;
			float ySin = sinAng * f3RotAxis.y;
			float zSin = sinAng * f3RotAxis.z;

			matrix mRotCamDirMatrix = matrix(
				cosAng + f3RotAxis.x*f3RotAxis.x*minCos, uxy - zSin, uxz + ySin, 0,
				uxy + zSin, cosAng + f3RotAxis.y*f3RotAxis.y*minCos, uyz - xSin, 0,
				uxz - ySin, uyz + xSin, cosAng + f3RotAxis.z*f3RotAxis.z*minCos, 0,
				0, 0, 0, 1
				);*/

			//matrix ParticleObjToProjSpaceMatr = (UNITY_MATRIX_MVP); // mul(UNITY_MATRIX_VP, mRotCamDirMatrix); // mul(ParticleObjToWorldSpaceMatr, );
			/*matrix ParticleObjToWorldSpaceMatr;
			ParticleObjToWorldSpaceMatr[0].xyzw = float4(1, 0, 0, 0);
			ParticleObjToWorldSpaceMatr[1].xyzw = float4(0, 1, 0, 0);// f3Normal.xyz;
			ParticleObjToWorldSpaceMatr[2].xyzw = float4(0, 0, 1, 0);//f3Bitangent.xyz;
			// Add translation to particle world position
			ParticleObjToWorldSpaceMatr[3].xyzw = p[0].vertex;
			ParticleObjToWorldSpaceMatr = mul(UNITY_MATRIX_VP, transpose(ParticleObjToWorldSpaceMatr));*/

			float3 f3Size;
			if (g_bSize < 0.5f)
				f3Size = g_fSize;
			else {
				float dist = distance(p[0].vertex.xyz, g_f4ShipPosition.xyz) / 312.0f;
				f3Size = g_fSize * max(min(1.0f, dist), 0.15f);
			}
			if (p[0].vertex.y >= g_fTopHeight)
				f3Size *= g_fSizeTop;

			for (int iCorner = 0; iCorner < 8; ++iCorner)
			{
				float4 f4CurrCornerWorldPos;
				f4CurrCornerWorldPos.xyz = p[0].vertex + f3Size * float3((iCorner & 0x01) ? +1 : -1, (iCorner & 0x04) ? +1 : -1, (iCorner & 0x02) ? +1 : -1);
				//f4CurrCornerWorldPos.xyz = p[0].vertex + f3Size *((iCorner & 0x01) ? +right : -right) + ((iCorner & 0x04) ? +up : -up) + ((iCorner & 0x02) ? +forward : -forward);
				f4CurrCornerWorldPos.w = 1.0f;

				float4 f4CurrCornerPosPS = UnityObjectToClipPos(f4CurrCornerWorldPos); //UnityObjectToClipPos(f4CurrCornerWorldPos);//

				Outs[iCorner].id = uiParticleId;
				Outs[iCorner].pos = f4CurrCornerPosPS;
				Outs[iCorner].projPos.xyz = UnityObjectToViewPos(f4CurrCornerWorldPos);
				Outs[iCorner].projPos.w = 1.0f;
				Outs[iCorner].uv = ComputeScreenPos(Outs[iCorner].pos);
				//UNITY_TRANSFER_FOG(Outs[iCorner], Outs[iCorner].pos);
			}
			// Generate bounding box faces
			{
				uint Side[10] = { 0,4,1,5,3,7,2,6,0,4 };
				for (int i = 0; i < 10; ++i)
					Out.Append(Outs[Side[i]]);
			}

			{
				Out.RestartStrip();
				uint uiBottomCap[4] = { 2,0,3,1 };
				for (int i = 0; i < 4; ++i)
					Out.Append(Outs[uiBottomCap[i]]);
			}

			{
				Out.RestartStrip();
				uint uiTopCap[4] = { 4,6,5,7 };
				for (int i = 0; i < 4; ++i)
					Out.Append(Outs[uiTopCap[i]]);
			}
		}
		ENDCG

		//	pass for directional lights
			Pass{ Name "Forward Base"
					Lighting On ZWrite Off Cull Front
					Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcAlpha

			//Blend One SrcColor
			// BlendOp Max
			Tags{"LightMode" = "ForwardBase" "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
			CGPROGRAM
			#pragma require geometry
			#pragma vertex vert
			#pragma multi_compile_fwdbase 
			#pragma geometry geom
			#pragma fragment frag
			//#pragma multi_compile_fog
			#pragma target 4.0

		v2g vert(appdata v)
		{
			v2g o;
			o.vertex = float4(g_vVertices[g_iIndices[v.id]], 1.0f);
			o.id = v.id;
			return o;
		}

		float4 frag(g2f In) : SV_Target
		{
			float fTransparency;
			float4 f4Color;
			SParticleAttribs ParticleAttrs;
			ParticleAttrs.fDensity = g_fDensity;
			ParticleAttrs.f3Pos = g_vVertices[g_iIndices[In.id]];
			ParticleAttrs.fRndAzimuthBias = g_vRndAzimuth[g_iIndices[In.id]];

			SCloudParticleLighting ParticleLighting;

			//float atten = 1.0f;// LIGHT_ATTENUATION(In);
			ParticleLighting.f4SunLight = _LightColor0;
			ParticleLighting.f4LightAttenuation = float4(g_fLightAttenuation, g_fLightAttenuation,1.0f,1.0f); // .x == single scattering; .y == multiple scattering
			ParticleLighting.f4AmbientLight = unity_AmbientSky;// float4(0.15, 0.15, 0.15, 1.0);
			float fTime = 1.0f; // g_fTimeScale * g_GlobalCloudAttribs.fTime;

			float3 f3CameraPos, f3ViewRay;
			/*// For directional light source, we should use position on the near clip plane instead of
			// camera location as a ray start point
			float2 f2PosPS = UVToProj((In.f4Pos.xy / g_GlobalCloudAttribs.f2LiSpCloudDensityDim.xy));
			float4 f4PosOnNearClipPlaneWS = mul(mul(float4(f2PosPS.xy, 1, 1), unity_CameraInvProjection), UNITY_MATRIX_IT_MV);
			f3CameraPos = f4PosOnNearClipPlaneWS.xyz / f4PosOnNearClipPlaneWS.w;

			//f4PosOnNearClipPlaneWS = mul( float4(f2PosPS.xy,1e-4,1), g_CameraAttribs.mViewProjInv );
			//f3CameraPos = f4PosOnNearClipPlaneWS.xyz/f4PosOnNearClipPlaneWS.w;
			float4 f4PosOnFarClipPlaneWS = mul(mul(float4(f2PosPS.xy, 0, 1), unity_CameraInvProjection), UNITY_MATRIX_IT_MV);
			f4PosOnFarClipPlaneWS.xyz = f4PosOnFarClipPlaneWS.xyz / f4PosOnFarClipPlaneWS.w;
			f3ViewRay = normalize(f4PosOnFarClipPlaneWS.xyz - f4PosOnNearClipPlaneWS.xyz);
			*/
			/*
			i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
			float2 uv = i.uv.xy / i.uv.w;

			float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, uv));
			depth = Linear01Depth(depth);
			float4 vpos = float4(i.ray * depth, 1);
			float3 wpos = mul(_CameraToWorld, vpos).xyz;
			*/
			f3CameraPos = _WorldSpaceCameraPos.xyz; // float3(0, 2, 0);//
			//f3CameraPos.y = 0.0f;
			float2 uv = In.uv.xy / In.uv.w;
			//float fDepth = 1.0f - (In.projPos.z);// 1.0f - Linear01Depth(tex2D(_CameraDepthTexture, uv));
			float4 f4ReconstructedPosWS = mul(UNITY_MATRIX_I_V, float4(In.projPos.xyz, 1.0));
			float3 f3WorldPos = f4ReconstructedPosWS.xyz / f4ReconstructedPosWS.w;
			//return float4(fDepth, fDepth, fDepth, 1.0f);
			/*
			float2 f2PosPS = UVToProj(In.vertex.xy / f2ScreenDim);

			float2 uv = i.projPos.xy / i.projPos.w;
			float4 SamplePositionMap(float2 uvCoord) {
				// H is the viewport position at this pixel in the range -1 to 1.
				float4 H = float4((uvCoord.x) * 2 - 1, (uvCoord.y) * 2 - 1, depth, 1.0);
				float4 D = mul(_ViewProjectInverse, H);
				return D / D.w;
			}
			float fDepth = In.projPos.z;
			float4 f4ReconstructedPosWS = mul(UNITY_MATRIX_I_V, float4(In.projPos.xy, fDepth, 1.0f)); // mul(mul(, unity_CameraInvProjection), UNITY_MATRIX_IT_MV); // mul(float4(f2PosPS.xy, fDepth, 1.0), g_CameraAttribs.mViewProjInv);
			float3 f3WorldPos = f4ReconstructedPosWS.xyz / f4ReconstructedPosWS.w;*/
			// Compute view ray
			//f3WorldPos.y = f3CameraPos.y;
			f3ViewRay = f3WorldPos - f3CameraPos;
			float fRayLength = length(f3ViewRay);
			f3ViewRay /= fRayLength;

			float2 f2RayIsecs;
			float fDistanceToEntryPoint, fDistanceToExitPoint;
			float3 f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace;
			IntersectRayWithParticle(ParticleAttrs,  f3CameraPos, f3ViewRay, In.id,
									f2RayIsecs, f3EntryPointUSSpace, f3ViewRayUSSpace,f3LightDirUSSpace,fDistanceToEntryPoint, fDistanceToExitPoint);

			if (f2RayIsecs.y < 0)
				discard;
			if (fRayLength < fDistanceToEntryPoint)
				discard; // return float4(1.0f, 1.0f, 1.0f, 1.0f);
			//return float4(f3ViewRayUSSpace, 1.0f);
			//return float4(normalize(f3ViewRay * 0.5f + 0.5f), 1.0f);
			fDistanceToExitPoint = min(fDistanceToExitPoint, fRayLength);
			//return float4(normalize(f3ViewRay), 1.0f);
			float fCloudMass;
			float fIsecLenUSSpace = f2RayIsecs.y - f2RayIsecs.x;

			// Compute particle rendering attributes
			ComputeParticleRenderAttribs(ParticleAttrs,
				fTime,
				f3CameraPos,
				f3ViewRay,
				f3EntryPointUSSpace,
				f3ViewRayUSSpace,
				fIsecLenUSSpace,
				f3LightDirUSSpace,
				fDistanceToExitPoint,
				fDistanceToEntryPoint,
				fCloudMass,
				fTransparency,
				true,
				false,
				false,
				ParticleLighting,
				f4Color
			);
			float fTransparencyOnDistance = fRayLength / g_fMaxTransparencyDist;
			fTransparencyOnDistance = clamp(fTransparencyOnDistance, g_fMinTransparency, g_fMaxTransparency);
			f4Color.a *= fTransparencyOnDistance;
			//UNITY_APPLY_FOG(In.fogCoord, f4Color);
			return f4Color;
		}
		ENDCG
		}

		/*
		Pass{ Name "Shadow Collector"
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			//Blend One OneMinusSrcAlpha

			//Blend One SrcColor
			// BlendOp Max
			Tags{"LightMode" = "ForwardBase"  "Queue" = "AlphaTest" "RenderType" = "Opaque"}
			CGPROGRAM
			#pragma require geometry
			#pragma multi_compile_fwdbase 
			#pragma vertex vert
			#pragma geometry geom1
			#pragma fragmentoption ARB_fog_exp2
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma fragment frag
			#pragma target 2.0

		v2g vert(appdata v)
		{
			v2g o;
			v.id = g_iIndices[v.id];
			o.vertex = float4(g_vVertices[v.id], 1.0f);
			o.id = v.id;
			return o;
		}
		[maxvertexcount(10 + 4 + 4)]
		void geom1(point v2g p[1], inout TriangleStream<g2f> Out)
		{
			uint uiParticleId = p[0].id;

			// Only visible particles are sent for rendering, so there is no need to
			// test visibility here
			//bool bIsValid = g_VisibleParticleFlags[uiParticleId/32] & (1 << (uiParticleId&31));
			//if( !bIsValid )
			//    return;

			g2f Outs[8];
			// matrix mViewProj = ; // g_CameraAttribs.WorldViewProj;
			// Multiply with camera view-proj matrix
			matrix g_CameraRotation;
			g_CameraRotation = matrix(1, 0, 0, 0,
				0, 1, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1
				);
				float3 forward = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				float3 right = cross(up, forward);
				float3 f3Forward = normalize(p[0].vertex - _WorldSpaceCameraPos).xyz;
				float cosAng = dot(float3(0, 0, 1), f3Forward);
				float3 f3RotAxis = normalize(cross(float3(0, 0, 1), f3Forward));
				if (cosAng < -1) {
					cosAng = 1.0f - cosAng;
					f3RotAxis = -f3RotAxis;
				}
				float sinAng = sin(acos(cosAng));
				float minCos = 1.0f - cosAng;
				float uxy = minCos * f3RotAxis.x * f3RotAxis.y;
				float uxz = minCos * f3RotAxis.x * f3RotAxis.z;
				float uyz = minCos * f3RotAxis.z * f3RotAxis.y;
				float xSin = sinAng * f3RotAxis.x;
				float ySin = sinAng * f3RotAxis.y;
				float zSin = sinAng * f3RotAxis.z;

				matrix mRotCamDirMatrix = matrix(
					cosAng + f3RotAxis.x*f3RotAxis.x*minCos, uxy - zSin, uxz + ySin, 0,
					uxy + zSin, cosAng + f3RotAxis.y*f3RotAxis.y*minCos, uyz - xSin, 0,
					uxz - ySin, uyz + xSin, cosAng + f3RotAxis.z*f3RotAxis.z*minCos, 0,
					0, 0, 0, 1
					);

					//matrix ParticleObjToProjSpaceMatr = (UNITY_MATRIX_MVP); // mul(UNITY_MATRIX_VP, mRotCamDirMatrix); // mul(ParticleObjToWorldSpaceMatr, );
					matrix ParticleObjToWorldSpaceMatr;
					ParticleObjToWorldSpaceMatr[0].xyzw = float4(1, 0, 0, 0);
					ParticleObjToWorldSpaceMatr[1].xyzw = float4(0, 1, 0, 0);// f3Normal.xyz;
					ParticleObjToWorldSpaceMatr[2].xyzw = float4(0, 0, 1, 0);//f3Bitangent.xyz;
					// Add translation to particle world position
					ParticleObjToWorldSpaceMatr[3].xyzw = p[0].vertex;
					ParticleObjToWorldSpaceMatr = mul(UNITY_MATRIX_VP, transpose(ParticleObjToWorldSpaceMatr));

					float3 f3Size;
					if (g_bSize < 0.5f)
						f3Size = g_fSize;
					else {
						float dist = distance(p[0].vertex.xyz, g_f4ShipPosition.xyz) / 312.0f;
						f3Size = g_fSize * max(min(1.0f, dist), 0.15f);
					}
					
					for (int iCorner = 0; iCorner < 8; ++iCorner)
					{
						float4 f4CurrCornerWorldPos;
						f4CurrCornerWorldPos.xyz = p[0].vertex + f3Size * float3((iCorner & 0x01) ? +1 : -1, (iCorner & 0x04) ? +1 : -1, (iCorner & 0x02) ? +1 : -1);
						//f4CurrCornerWorldPos.xyz = p[0].vertex + f3Size *((iCorner & 0x01) ? +right : -right) + ((iCorner & 0x04) ? +up : -up) + ((iCorner & 0x02) ? +forward : -forward);
						f4CurrCornerWorldPos.w = 1.0f;

						float4 f4CurrCornerPosPS = UnityObjectToClipPos(f4CurrCornerWorldPos); //UnityObjectToClipPos(f4CurrCornerWorldPos);//

						Outs[iCorner].id = uiParticleId;
						Outs[iCorner].pos = f4CurrCornerPosPS;
						Outs[iCorner].projPos.xyz = UnityObjectToViewPos(f4CurrCornerWorldPos);
						Outs[iCorner].projPos.w = 1.0f;
						Outs[iCorner].uv = ComputeScreenPos(Outs[iCorner].pos);
						TRANSFER_VERTEX_TO_FRAGMENT(Outs[iCorner]);
					}
					// Generate bounding box faces
					{
						uint Side[10] = { 0,4,1,5,3,7,2,6,0,4 };
						for (int i = 0; i < 10; ++i)
							Out.Append(Outs[Side[i]]);
					}

					{
						Out.RestartStrip();
						uint uiBottomCap[4] = { 2,0,3,1 };
						for (int i = 0; i < 4; ++i)
							Out.Append(Outs[uiBottomCap[i]]);
					}

					{
						Out.RestartStrip();
						uint uiTopCap[4] = { 4,6,5,7 };
						for (int i = 0; i < 4; ++i)
							Out.Append(Outs[uiTopCap[i]]);
					}
				}

		float4 frag(g2f In) : SV_Target
		{
			float fTransparency;
			float4 f4Color;
			SParticleAttribs ParticleAttrs;
			ParticleAttrs.fDensity = g_fDensity;
			ParticleAttrs.f3Pos = g_vVertices[g_iIndices[In.id]];
			ParticleAttrs.fRndAzimuthBias = g_vRndAzimuth[g_iIndices[In.id]];

			SCloudParticleLighting ParticleLighting;

			float atten = LIGHT_ATTENUATION(In);

			ParticleLighting.f4SunLight = _LightColor0;
			ParticleLighting.f4LightAttenuation = float4(atten,atten,1.0f,1.0f); // .x == single scattering; .y == multiple scattering
			ParticleLighting.f4AmbientLight = unity_AmbientSky;// float4(0.15, 0.15, 0.15, 1.0);
			float fTime = 1.0f; // g_fTimeScale * g_GlobalCloudAttribs.fTime;

			float3 f3CameraPos, f3ViewRay;
			f3CameraPos = _WorldSpaceCameraPos.xyz; // float3(0, 2, 0);//
			//f3CameraPos.y = 0.0f;
			float2 uv = In.uv.xy / In.uv.w;
			//float fDepth = 1.0f - (In.projPos.z);// 1.0f - Linear01Depth(tex2D(_CameraDepthTexture, uv));
			float4 f4ReconstructedPosWS = mul(UNITY_MATRIX_I_V, float4(In.projPos.xyz, 1.0));
			float3 f3WorldPos = f4ReconstructedPosWS.xyz / f4ReconstructedPosWS.w;

			f3ViewRay = f3WorldPos - f3CameraPos;
			float fRayLength = length(f3ViewRay);
			f3ViewRay /= fRayLength;

			float2 f2RayIsecs;
			float fDistanceToEntryPoint, fDistanceToExitPoint;
			float3 f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace;
			IntersectRayWithParticle(ParticleAttrs,  f3CameraPos, f3ViewRay, In.id,
									f2RayIsecs, f3EntryPointUSSpace, f3ViewRayUSSpace,f3LightDirUSSpace,fDistanceToEntryPoint, fDistanceToExitPoint);

			if (f2RayIsecs.y < 0)
				discard;
			if (fRayLength < fDistanceToEntryPoint)
				discard; // return float4(1.0f, 1.0f, 1.0f, 1.0f);
			//return float4(f3ViewRayUSSpace, 1.0f);
			//return float4(normalize(f3ViewRay * 0.5f + 0.5f), 1.0f);
			fDistanceToExitPoint = min(fDistanceToExitPoint, fRayLength);
			//return float4(normalize(f3ViewRay), 1.0f);
			float fCloudMass;
			float fIsecLenUSSpace = f2RayIsecs.y - f2RayIsecs.x;

			// Compute particle rendering attributes
			ComputeParticleRenderAttribs(ParticleAttrs,
				fTime,
				f3CameraPos,
				f3ViewRay,
				f3EntryPointUSSpace,
				f3ViewRayUSSpace,
				fIsecLenUSSpace,
				f3LightDirUSSpace,
				fDistanceToExitPoint,
				fDistanceToEntryPoint,
				fCloudMass,
				fTransparency,
				true,
				false,
				true,
				ParticleLighting,
				f4Color
			);
			f4Color.w = atten*0.1f;
			f4Color.rgb = 0.0f;
			return f4Color;
		}
		ENDCG
		}*/
		
	Pass{
		ZWrite On Cull Front
		//Blend SrcAlpha OneMinusSrcAlpha
		Name "ShadowPass"
		Tags {"LightMode" = "ShadowCaster" "Queue" = "AlphaTest"}
		Fog{ Mode Off }

		CGPROGRAM
			#pragma require geometry
			#pragma multi_compile_fwdbase 
			//#pragma multi_compile_fog
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma target 4.0

		v2g vert(appdata v)
		{
			v2g o;
			o.vertex = float4(g_vVertices[g_iIndices[v.id]],1.0f);// -1.42f * g_fSize * _WorldSpaceLightPos0.xyz, 1.0f);
			o.id = v.id;
			return o;
		}

		float4 frag(g2f In) : SV_Target
		{
			float fTransparency;
			float4 f4Color;
			SParticleAttribs ParticleAttrs;
			ParticleAttrs.fDensity = g_fDensity;
			ParticleAttrs.f3Pos = g_vVertices[g_iIndices[In.id]];
			ParticleAttrs.fRndAzimuthBias = g_vRndAzimuth[g_iIndices[In.id]];

			SCloudParticleLighting ParticleLighting;
			
			ParticleLighting.f4SunLight = _LightColor0;
			ParticleLighting.f4LightAttenuation = float4(1.0f,1.0f,1.0f,1.0f); // .x == single scattering; .y == multiple scattering
			ParticleLighting.f4AmbientLight = unity_AmbientSky;// float4(0.15, 0.15, 0.15, 1.0);
			float fTime = 1.0f; // g_fTimeScale * g_GlobalCloudAttribs.fTime;

			float3 f3CameraPos, f3ViewRay;

			float2 f2PosPS = In.projPos.xy;// / float2(1024, 1024);
			float4 f4PosOnNearClipPlaneWS = mul(UNITY_MATRIX_I_V, float4(f2PosPS.xy, 1, 1));
			f3CameraPos = f4PosOnNearClipPlaneWS.xyz / f4PosOnNearClipPlaneWS.w;

			//f4PosOnNearClipPlaneWS = mul( float4(f2PosPS.xy,1e-4,1), g_CameraAttribs.mViewProjInv );
			//f3CameraPos = f4PosOnNearClipPlaneWS.xyz/f4PosOnNearClipPlaneWS.w;
			float4 f4PosOnFarClipPlaneWS = mul(UNITY_MATRIX_I_V, float4(f2PosPS.xy, 0, 1));
			f4PosOnFarClipPlaneWS.xyz = f4PosOnFarClipPlaneWS.xyz / f4PosOnFarClipPlaneWS.w;
			f3ViewRay = normalize(f4PosOnFarClipPlaneWS.xyz - f4PosOnNearClipPlaneWS.xyz);
			
			float2 f2RayIsecs;
			float fDistanceToEntryPoint, fDistanceToExitPoint;
			float3 f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace;
			IntersectRayWithParticle(ParticleAttrs,  f3CameraPos, f3ViewRay, In.id,
									f2RayIsecs, f3EntryPointUSSpace, f3ViewRayUSSpace,f3LightDirUSSpace,fDistanceToEntryPoint, fDistanceToExitPoint);

			if (f2RayIsecs.y == -2 && f2RayIsecs.x == -1)
				discard;
			//return float4(f3ViewRayUSSpace, 1.0f);
			//return float4(normalize(f3ViewRay * 0.5f + 0.5f), 1.0f);
			//return float4(normalize(f3ViewRay), 1.0f);
			float fCloudMass;
			float fIsecLenUSSpace = f2RayIsecs.y - f2RayIsecs.x;

			// Compute particle rendering attributes
			ComputeParticleRenderAttribs(ParticleAttrs,
				fTime,
				f3CameraPos,
				f3ViewRay,
				f3EntryPointUSSpace,
				f3ViewRayUSSpace,
				fIsecLenUSSpace,
				f3LightDirUSSpace,
				fDistanceToExitPoint,
				fDistanceToEntryPoint,
				fCloudMass,
				fTransparency,
				true,
				true,
				false,
				ParticleLighting,
				f4Color
			);
			if (f4Color.w < 0.02)
				discard;
			return f4Color;
		}
			ENDCG
		}
		}
		Fallback "Transparent/Cutout/VertexLit"
}
