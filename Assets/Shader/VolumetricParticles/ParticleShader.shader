Shader "Custom/CloudPart" {
	Properties{
		g_tex3DNoise("_NoiseTex", 3D) = ""  {}
		g_tex3DParticleDensityLUT("_ParticleDensity", 3D) = ""  {}
		g_tex3DSingleScatteringInParticleLUT("_SingleScatter", 3D) = ""  {}
		g_tex3DMultipleScatteringInParticleLUT("_MultipleScatter", 3D) = ""  {}
		g_Color("g_Color", Color) = (1,1,1,1)
		g_SpecColor("g_SpecColor", Color) = (1,1,1,1)
		g_fBillboardSize("g_BillboardSize", Range(0.01,10.0)) = 1.0
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		g_fTimeDiff("TimeDiff", Range(0.0, 1.0)) = 0.01
		g_fTimeStepTex("TimeStepTex", Range(0.05, 1.0)) = 1.0
	}
		SubShader{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}

		CGINCLUDE

		#include "structs.cginc"
		#include "common.cginc"
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

		static const float g_fCloudExtinctionCoeff = 100;

		// Minimal cloud transparancy not flushed to zero
		static const float g_fTransparencyThreshold = 0.01;

		// Fraction of the particle cut off distance which serves as
		// a transition region from particles to flat clouds
		static const float g_fParticleToFlatMorphRatio = 0.2;

		static const float g_fTimeScale = 1.f;
		static const float2 g_f2CloudDensitySamplingScale = float2(1.f / 200000.f, 1.f / 20000.f);

		#define FLT_MAX 3.402823466e+38f
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

	struct PS_Input
	{
		float4 f4Pos : SV_Position;
		nointerpolation uint uiParticleID : PARTICLE_ID;
	};

	struct g2f
	{
		float4 vertex : SV_POSITION;
		float3 normal : NORMAL;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
		float2 texcoord_2 : TEXCOORD1;
		uint id : VertexID;
		UNITY_FOG_COORDS(3)
		float4 projPos : TEXCOORD4;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	Texture2DArray<float>  g_tex2DLightSpaceDepthMap_t0 : register(t0);
	Texture2DArray<float>  g_tex2DLiSpCloudTransparency : register(t0);
	Texture2DArray<float2> g_tex2DLiSpCloudMinMaxDepth  : register(t1);
	Texture2D<float>       g_tex2DCloudDensity          : register(t1);
	Texture2D<float3>      g_tex2DWhiteNoise            : register(t3);
	Texture3D<float>       g_tex3DNoise                 : register(t4);
	Texture2D<float>       g_tex2MaxDensityMip          : register(t3);
	StructuredBuffer<uint> g_PackedCellLocations        : register(t0);
	StructuredBuffer<SCloudCellAttribs> g_CloudCells    : register(t2);
	StructuredBuffer<SParticleAttribs>  g_Particles     : register(t3);
	Texture2D<float3>       g_tex2DAmbientSkylight               : register(t7);
	StructuredBuffer<uint>              g_VisibleParticleFlags   : register(t6);
	StructuredBuffer<SCloudParticleLighting> g_ParticlesLighting : register(t7);
	Texture2DArray<float>   g_tex2DLightSpCloudTransparency      : register(t6);
	Texture2DArray<float2>  g_tex2DLightSpCloudMinMaxDepth       : register(t7);
	Texture2D<int>          g_tex2DFirstKnot                     : register(t8);
	StructuredBuffer<SParticleListKnot> g_TiledParticlesList     : register(t9);
	StructuredBuffer<uint>  g_ParticleOrder                      : register(t11);
	Buffer<uint>            g_ValidCellsCounter                  : register(t0);
	StructuredBuffer<uint>  g_ValidCellsUnorderedList            : register(t1);
	StructuredBuffer<uint>  g_ValidParticlesUnorderedList        : register(t1);
	StructuredBuffer<uint>  g_SortedParticlesOrder               : register(t0);
	Texture3D<float2>       g_tex3DParticleDensityLUT    : register(t10);
	Texture3D<float>        g_tex3DSingleScatteringInParticleLUT   : register(t11);
	Texture3D<float>        g_tex3DMultipleScatteringInParticleLUT : register(t12);

	SamplerState MyLinearClampSampler : register(s0);
	SamplerState MyLinearRepeatSampler : register(s1);

	static SGlobalCloudAttribs g_GlobalCloudAttribs;


		uint uiInnerRingDim;
		uint uiRingExtension;
		uint uiRingDimension;
		uint uiNumRings;

		uint uiMaxLayers; // 
		uint uiNumCells;
		uint uiMaxParticles;
		uint uiDownscaleFactor;

		float fCloudDensityThreshold;
		float fCloudThickness;
		float fCloudAltitude;
		float fParticleCutOffDist; //

		float fTime; //
		float fCloudVolumeDensity;
		float2 f2LiSpCloudDensityDim;

		uint uiBackBufferWidth;
		uint uiBackBufferHeight;
		uint uiDownscaledBackBufferWidth;
		uint uiDownscaledBackBufferHeight;

		float fBackBufferWidth;
		float fBackBufferHeight;
		float fDownscaledBackBufferWidth;
		float fDownscaledBackBufferHeight;

		float fTileTexWidth;
		float fTileTexHeight;
		uint uiLiSpFirstListIndTexDim; 
		uint uiNumCascades;

		float4 f4Parameter;

		float fScatteringCoeff;
		float fAttenuationCoeff; // 
		uint uiNumParticleLayers;
		uint uiDensityGenerationMethod;

		bool bVolumetricBlending;
		float3 f3Dummy;

		float4 f4TilingFrustumPlanes[6];
		matrix mParticleTiling; 

		// ---------------------------------------------------------------------------------------------------------------------------------

		float3 GetParticleScales(in float fSize, in float fNumActiveLayers)
		{
			float3 f3Scales = fSize;
			//if( fNumActiveLayers > 1 )
			//    f3Scales.y = max(f3Scales.y, g_GlobalCloudAttribs.fCloudThickness/fNumActiveLayers);
			f3Scales.y = min(f3Scales.y, fCloudThickness / 2.f);
			return f3Scales;
		}

		void IntersectRayWithParticle(const in SParticleAttribs ParticleAttrs,
			const in SCloudCellAttribs CellAttrs,
			const in float3 f3CameraPos,
			const in float3 f3ViewRay,
			out float2 f2RayIsecs,
			out float3 f3EntryPointUSSpace, // Entry point in Unit Sphere (US) space
			out float3 f3ViewRayUSSpace,    // View ray direction in Unit Sphere (US) space
			out float3 f3LightDirUSSpace,   // Light direction in Unit Sphere (US) space
			out float fDistanceToEntryPoint,
			out float fDistanceToExitPoint)
		{
			// Construct local frame matrix
			float3 f3Normal = CellAttrs.f3Normal.xyz;
			float3 f3Tangent = CellAttrs.f3Tangent.xyz;
			float3 f3Bitangent = CellAttrs.f3Bitangent.xyz;
			float3x3 f3x3ObjToWorldSpaceRotation = float3x3(f3Tangent, f3Normal, f3Bitangent);
			// World to obj space is inverse of the obj to world space matrix, which is simply transpose
			// for orthogonal matrix:
			// float3x3 f3x3WorldToObjSpaceRotation = transpose(f3x3ObjToWorldSpaceRotation);
			
			float4x4 f3x3WorldToObjSpaceRotation = unity_WorldToObject;
			// Compute camera location and view direction in particle's object space:
			float4 f4CamPosObjSpace = float4(f3CameraPos - ParticleAttrs.f3Pos,0.0f);
			f4CamPosObjSpace = mul(f4CamPosObjSpace, f3x3WorldToObjSpaceRotation);
			float4 f3ViewRayObjSpace = mul(float4(f3ViewRay,0.0f), f3x3WorldToObjSpaceRotation);
			float4 f3LightDirObjSpce = mul(float4(-_WorldSpaceLightPos0.xyz,0.0f), f3x3WorldToObjSpaceRotation);

			// Compute scales to transform ellipsoid into the unit sphere:
			float3 f3Scale = 1.0f / ParticleAttrs.fSize; // 1.f / GetParticleScales(ParticleAttrs.fSize, CellAttrs.uiNumActiveLayers);

			float3 f3ScaledCamPosObjSpace;
			f3ScaledCamPosObjSpace = f4CamPosObjSpace.xyz * f3Scale;
			f3ViewRayUSSpace = normalize(f3ViewRayObjSpace.xyz*f3Scale);
			f3LightDirUSSpace = normalize(f3LightDirObjSpce.xyz*f3Scale);
			// Scale camera pos and view dir in obj space and compute intersection with the unit sphere:
			GetRaySphereIntersection(f3ScaledCamPosObjSpace, f3ViewRayUSSpace, 0, 1.f, f2RayIsecs);

			f3EntryPointUSSpace = f3ScaledCamPosObjSpace + f3ViewRayUSSpace * f2RayIsecs.x;

			fDistanceToEntryPoint = length(f3ViewRayUSSpace / f3Scale) * f2RayIsecs.x;
			fDistanceToExitPoint = length(f3ViewRayUSSpace / f3Scale) * f2RayIsecs.y;
		}
		/*
		float2 ComputeLiSpTransparency(const in SParticleAttribs ParticleAttrs)
		{
			float4 f4ParticleCenterPosPS = mul(float4(ParticleAttrs.f3Pos, 1), g_GlobalCloudAttribs.mParticleTiling);
			float2 f2ParticleProjSpaceXY = f4ParticleCenterPosPS.xy / f4ParticleCenterPosPS.w;
			if (any(abs(f2ParticleProjSpaceXY) > 1))
				return 1;

			float2 f2ParticleUV = saturate(ProjToUV(f2ParticleProjSpaceXY));

			float3 f3RayStart = ParticleAttrs.f3Pos.xyz;
			float3 f3RayDir = g_LightAttribs.f4DirOnLight.xyz;

			uint2 uiTileXY = floor(f2ParticleUV * g_GlobalCloudAttribs.uiLiSpFirstListIndTexDim);
			int iCurrKnotInd = g_tex2DFirstKnot.Load(uint3(uiTileXY, 0));
			float2 f2Transparency = 1;

			[loop]
			while (iCurrKnotInd >= 0)
			{
				SParticleListKnot CurrKnot = g_TiledParticlesList[iCurrKnotInd];
				SParticleAttribs CurrParticleAttrs = g_Particles[CurrKnot.uiParticleID];
				SCloudCellAttribs CurrCellAttrs = g_CloudCells[CurrKnot.uiParticleID / g_GlobalCloudAttribs.uiMaxLayers];

				float2 f2RayIsecs;
				float fDistanceToEntryPoint, fDistanceToExitPoint;
				float3 f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace;
				IntersectRayWithParticle(CurrParticleAttrs, CurrCellAttrs, f3RayStart, f3RayDir,
					f2RayIsecs, f3EntryPointUSSpace, f3ViewRayUSSpace,
					f3LightDirUSSpace,
					fDistanceToEntryPoint, fDistanceToExitPoint);

				float2 f2CurrTransparency = 1.f;
				if ( //CurrKnot.uiParticleID != uiParticleOrder && 
					fDistanceToExitPoint > 0
					/*f2RayIsecs.y > f2RayIsecs.x && *//*)
				{
					//float fDensityScale = 1;
					//float fDensity = ParticleAttrs.fDensity * fDensityScale;
					fDistanceToEntryPoint = max(fDistanceToEntryPoint, CurrParticleAttrs.fSize);
					float fCloudMass = max(fDistanceToExitPoint - fDistanceToEntryPoint, 0);//g_GlobalCloudAttribs.fCloudVolumeDensity * fDensity;
					fCloudMass *= CurrCellAttrs.fMorphFadeout * ParticleAttrs.fDensity;
					f2CurrTransparency = exp(-fCloudMass * g_GlobalCloudAttribs.fAttenuationCoeff * float2(0.05, 0.025));
				}

				f2Transparency *= f2CurrTransparency;
				if (all(f2Transparency < g_fTransparencyThreshold))
					break;

				iCurrKnotInd = CurrKnot.iNextKnotInd;
			}

			//fTransparency = saturate((fTransparency - g_fTransparencyThreshold) / (1-g_fTransparencyThreshold));

			return f2Transparency;
		}*/

		void DirectionToZenithAzimuthAngleXZY(in float3 f3Direction, out float fZenithAngle, out float fAzimuthAngle)
		{
			float fZenithCos = f3Direction.y;
			fZenithAngle = acos(fZenithCos);
			//float fZenithSin = sqrt( max(1 - fZenithCos*fZenithCos, 1e-10) );
			float fAzimuthCos = f3Direction.x;// / fZenithSin;
			float fAzimuthSin = f3Direction.z;// / fZenithSin;
			fAzimuthAngle = atan2(fAzimuthSin, fAzimuthCos);
		}

		void WorldParamsToOpticalDepthLUTCoords(in float3 f3NormalizedStartPos, in float3 f3RayDir, out float4 f4LUTCoords)
		{
			DirectionToZenithAzimuthAngleXZY(f3NormalizedStartPos, f4LUTCoords.x, f4LUTCoords.y);

			float3 f3LocalX, f3LocalY, f3LocalZ;
			// Construct local tangent frame for the start point on the sphere (z up)
			// For convinience make the Z axis look into the sphere
			ConstructLocalFrameXYZ(-f3NormalizedStartPos, float3(0, 1, 0), f3LocalX, f3LocalY, f3LocalZ);

			// z coordinate is the angle between the ray direction and the local frame zenith direction
			// Note that since we are interested in rays going inside the sphere only, the allowable
			// range is [0, PI/2]

			float fRayDirLocalZenith, fRayDirLocalAzimuth;
			ComputeLocalFrameAnglesXYZ(f3LocalX, f3LocalY, f3LocalZ, f3RayDir, fRayDirLocalZenith, fRayDirLocalAzimuth);
			f4LUTCoords.z = fRayDirLocalZenith;
			f4LUTCoords.w = fRayDirLocalAzimuth;

			f4LUTCoords.xyzw = f4LUTCoords.xyzw / float4(PI, 2 * PI, PI / 2, 2 * PI) + float4(0.0, 0.5, 0, 0.5);

			// Clamp only zenith (yz) coordinate as azimuth is filtered with wraparound mode
			f4LUTCoords.xz = clamp(f4LUTCoords, 0.5 / OPTICAL_DEPTH_LUT_DIM, 1.0 - 0.5 / OPTICAL_DEPTH_LUT_DIM).xz;
		}

		float GetConservativeScreenDepth(in float2 f2UV)
		{
			return g_tex2DDepthBuffer.SampleLevel(MyLinearClampSampler, f2UV, 0);
		}

		void ComputeParticleRenderAttribs(const in SParticleAttribs ParticleAttrs,
			const in SCloudCellAttribs CellAttrs,
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
			in SCloudParticleLighting ParticleLighting,
			out float4 f4Color)
		{
			float3 f3EntryPointWS = f3CameraPos + fDistanceToEntryPoint * f3ViewRay;
			float3 f3ExitPointWS = f3CameraPos + fDistanceToExitPoint * f3ViewRay;

			float4 f4LUTCoords;
			WorldParamsToOpticalDepthLUTCoords(f3EntryPointUSSpace, f3ViewRayUSSpace, f4LUTCoords);
			// Randomly rotate the sphere
			f4LUTCoords.y += ParticleAttrs.fRndAzimuthBias;
			//float4 f4ddxLUTCoords = ddx(f4LUTCoords);
			//float4 f4ddyLUTCoords = ddy(f4LUTCoords);
			//float4 f4PixelDiff = sqrt(f4ddxLUTCoords*f4ddxLUTCoords + f4ddyLUTCoords*f4ddyLUTCoords) * OPTICAL_DEPTH_LUT_DIM.xyzw;
			//float fMaxDiff = max(f4PixelDiff.x, max(f4PixelDiff.y, max(f4PixelDiff.z,f4PixelDiff.w)));
			float fLOD = 0;//log2(max(fMaxDiff,1));

			float2 f2NormalizedDensityAndDist;
			SAMPLE_4D_LUT(g_tex3DParticleDensityLUT, OPTICAL_DEPTH_LUT_DIM, f4LUTCoords, fLOD, f2NormalizedDensityAndDist);

			float3 f3FirstMatterPointWS = f3CameraPos + (fDistanceToEntryPoint + (fDistanceToExitPoint - fDistanceToEntryPoint) * f2NormalizedDensityAndDist.y) * f3ViewRay;
			float3 f3FirstMatterPointUSSpace = f3EntryPointUSSpace + (fIsecLenUSSpace * f2NormalizedDensityAndDist.y) * f3ViewRayUSSpace;

			float3 f3NoiseSamplingPos = f3FirstMatterPointWS;
			//float3 f3Weights = abs(f3FirstMatterPointUSSpace);
			//f3Weights /= dot(f3Weights, 1);
			float fNoisePeriod = 3412;
			//float fNoise = g_tex2DCloudDensity.Sample(samLinearWrap, (f3NoiseSamplingPos.xz)/fNoisePeriod) * f3Weights.y + 
			//               g_tex2DCloudDensity.Sample(samLinearWrap, (f3NoiseSamplingPos.yz)/fNoisePeriod) * f3Weights.x + 
			//               g_tex2DCloudDensity.Sample(samLinearWrap, (f3NoiseSamplingPos.xy)/fNoisePeriod) * f3Weights.z;
			float fNoise = bAutoLOD ?
				(g_tex3DNoise.Sample(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod)) * 2 + g_tex3DNoise.Sample(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod / 3))) / 3 :
				(g_tex3DNoise.SampleLevel(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod), 0) * 2 + g_tex3DNoise.SampleLevel(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod / 3), 0)) / 3;

			float fNoNoiseY = -0.7;
			float fTransition = min(1 - fNoNoiseY, 0.5);
			fNoise = lerp(0.5, fNoise, max(saturate((f3EntryPointUSSpace.y - fNoNoiseY) / fTransition), 0.2));

			//f2NormalizedDensityAndDist.x *= fNoise;

			fCloudMass = f2NormalizedDensityAndDist.x * (fDistanceToExitPoint - fDistanceToEntryPoint);
			float fFadeOutDistance = g_GlobalCloudAttribs.fParticleCutOffDist * g_fParticleToFlatMorphRatio;
			float fFadeOutFactor = saturate((g_GlobalCloudAttribs.fParticleCutOffDist - fDistanceToEntryPoint) / max(fFadeOutDistance, 1));
			fCloudMass *= fFadeOutFactor * 1.0f;//CellAttrs.fMorphFadeout;
			fCloudMass *= ParticleAttrs.fDensity; // * lerp(fNoise, 1, 0.5);

			//fDistanceToEntryPoint = fDistanceToEntryPoint + (fDistanceToExitPoint - fDistanceToEntryPoint) * f2NormalizedDensityAndDist.y;

			fTransparency = 1.0f; //exp(-fCloudMass * g_GlobalCloudAttribs.fAttenuationCoeff);

			float fMultipleScatteringDensityScale = fFadeOutFactor * ParticleAttrs.fDensity;
			float fSingleScatteringDensityScale = fMultipleScatteringDensityScale * f2NormalizedDensityAndDist.x;

			float4 f4SingleScatteringLUTCoords = WorldParamsToParticleScatteringLUT(f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace, true, fSingleScatteringDensityScale);
			float4 f4MultipleScatteringLUTCoords = WorldParamsToParticleScatteringLUT(f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace, true, fMultipleScatteringDensityScale);
			float fSingleScattering = 0, fMultipleScattering = 0;
			SAMPLE_4D_LUT(g_tex3DSingleScatteringInParticleLUT, SRF_SCATTERING_IN_PARTICLE_LUT_DIM, f4SingleScatteringLUTCoords, 0, fSingleScattering);
			SAMPLE_4D_LUT(g_tex3DMultipleScatteringInParticleLUT, SRF_SCATTERING_IN_PARTICLE_LUT_DIM, f4MultipleScatteringLUTCoords, 0, fMultipleScattering);
			//float2 f2PrecomputedScattering = g_PrecomputedScatteringInParticle.SampleLevel(samLinearWrap, f3ParticleScatteringLUTCoords, 0);

			//float3 _f3EntryPointUSSpace, _f3ViewRayUSSpace, _f3LightDirUSSpace;
			//ParticleScatteringLUTToWorldParams(f3ParticleScatteringLUTCoords, _f3EntryPointUSSpace, _f3ViewRayUSSpace, _f3LightDirUSSpace);
			//float3 _f3ParticleScatteringLUTCoords = WorldParamsToParticleScatteringLUT(_f3EntryPointUSSpace, _f3ViewRayUSSpace, _f3LightDirUSSpace);
			//f4Color.rgb = abs(_f3ParticleScatteringLUTCoords - f3ParticleScatteringLUTCoords)*1e+4;
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
			f4Color.a = fTransparency;
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

		float4 frag(g2f In) : SV_Target
		{
			if (In.id > 0)
				discard;
			float fTransparency;
			float4 f4Color;
			SParticleAttribs ParticleAttrs = g_Particles[In.id];
			ParticleAttrs.fDensity = 1.0f;
			ParticleAttrs.f3Pos = float3(0,0,0);
			ParticleAttrs.fSize = 100.0f;
			ParticleAttrs.fRndAzimuthBias = 0.0f;

			SCloudCellAttribs CellAttribs = g_CloudCells[In.id / max(g_GlobalCloudAttribs.uiMaxLayers, 1)];

			SCloudParticleLighting ParticleLighting = g_ParticlesLighting[In.id];

			ParticleLighting.f4SunLight = (1.0f,1.0f,1.0f,1.0f);
			ParticleLighting.f4LightAttenuation = float4(1.0f,1.0f,1.0f,1.0f); // .x == single scattering; .y == multiple scattering
			ParticleLighting.f4AmbientLight = float4(0.1f,0.1f,0.1f,1.0f);
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

			f3CameraPos = _WorldSpaceCameraPos.xyz;
			//f3ViewRay = normalize(In.f3ViewRay);
			/*float2 f2ScreenDim = float2(g_GlobalCloudAttribs.fDownscaledBackBufferWidth, g_GlobalCloudAttribs.fDownscaledBackBufferHeight);
			float2 f2PosPS = UVToProj(In.vertex.xy / f2ScreenDim);
			float fDepth = GetConservativeScreenDepth(ProjToUV(f2PosPS.xy));
			float4 f4ReconstructedPosWS = mul(mul(float4(f2PosPS.xy, fDepth, 1.0), unity_CameraInvProjection), UNITY_MATRIX_IT_MV); // mul(float4(f2PosPS.xy, fDepth, 1.0), g_CameraAttribs.mViewProjInv);
			float3 f3WorldPos = f4ReconstructedPosWS.xyz / f4ReconstructedPosWS.w;

			// Compute view ray
			f3ViewRay = f3WorldPos - f3CameraPos;
			float fRayLength = length(f3ViewRay);
			f3ViewRay /= fRayLength;
			*/

			f3ViewRay = (WorldSpaceViewDir(In.vertex));
			float fRayLength = length(f3ViewRay);
			f3ViewRay /= fRayLength;

			// Intersect view ray with the particle
			float2 f2RayIsecs;
			float fDistanceToEntryPoint, fDistanceToExitPoint;
			float3 f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace;
			IntersectRayWithParticle(ParticleAttrs, CellAttribs, f3CameraPos, f3ViewRay,
									f2RayIsecs, f3EntryPointUSSpace, f3ViewRayUSSpace,f3LightDirUSSpace,fDistanceToEntryPoint, fDistanceToExitPoint);

	if (f2RayIsecs.y < 0 || fRayLength < fDistanceToEntryPoint)
		discard;

			fDistanceToExitPoint = min(fDistanceToExitPoint, fRayLength);

			float fCloudMass;
			float fIsecLenUSSpace = f2RayIsecs.y - f2RayIsecs.x;
			// Compute particle rendering attributes
			ComputeParticleRenderAttribs(ParticleAttrs, CellAttribs,
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
				ParticleLighting,
				f4Color
			);
			f4Color.a = fTransparency;

			return f4Color;
			// fDistToCloud = fTransparency < 0.9 ? fDistanceToEntryPoint : +FLT_MAX;
		}


				float2 rotate2DVector(float2 v, float2 m, float angle) {
					float sin_val = sin(angle);
					float cos_val = cos(angle);
					// multiply inv transform with rot with transform matrix
					float3x3 mat =
						float3x3(cos_val, -sin_val, 0,
							sin_val, cos_val, 0,
							0, 0, 1);
					return mul(mat, float3(v - m, 1.0f)).xy + m;
				}


				float4 g_i3Dimensions;

				// ------------ GEOMETRY SHADER ---------------
				[maxvertexcount(4)]
				void geom(point v2g p[1], inout TriangleStream<g2f> tristream)
				{
					// TODO: test against view frustum
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
					o.id = p[0].id;

					o.texcoord = float2(min(max((v[0].x + startPos.x)*mul_val_x,0), 1.0f), max(min((v[0].y + startPos.y)*mul_val_y + 0.1f * g_i3Dimensions.y, 1.0), 0.0f));
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
