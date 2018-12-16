Shader "Custom/CloudPart" {
	Properties{
		g_tex3DNoise("_NoiseTex", 3D) = ""  {}
		g_tex3DParticleDensityLUT("_ParticleDensity", 3D) = ""  {}
		g_tex3DSingleScatteringInParticleLUT("_SingleScatter", 3D) = ""  {}
		g_tex3DMultipleScatteringInParticleLUT("_MultipleScatter", 3D) = ""  {}
		g_Color("g_Color", Color) = (1,1,1,1)
		g_SpecColor("g_SpecColor", Color) = (1,1,1,1)
		g_fSize("BillboardSize", Range(0.1,100.0)) = 1.0
		g_fDensity("Density", Range(0.01,100.0)) = 1
	}
		SubShader{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}

		CGINCLUDE

		#include "structs.cginc"
		#include "common.cginc"
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Lighting.cginc"

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
				Lighting On ZWrite Off Cull Front
				Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcAlpha

			//Blend One SrcColor
				// BlendOp Max
				Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
				CGPROGRAM
				#pragma require geometry
			// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
						//	#pragma exclude_renderers d3d11 gles

							#pragma multi_compile_fwdbase 
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
					float2 texcoord : TEXCOORD0;
					float2 texcoord_2 : TEXCOORD1;
					uint id : VertexID;
					UNITY_FOG_COORDS(3)
					float4 projPos : TEXCOORD4;
					UNITY_VERTEX_OUTPUT_STEREO
				};


				Texture3D<half2>			g_tex3DParticleDensityLUT    : register(t0);
				Texture3D<float>			g_tex3DSingleScatteringInParticleLUT   : register(t1);
				Texture3D<float>			g_tex3DMultipleScatteringInParticleLUT : register(t2);
				Texture2D<float3>			g_tex2DAmbientSkylight               : register(t4);
				Texture3D<float>			g_tex3DNoise                 : register(t6);
				StructuredBuffer<float3>	g_vVertices : register(t3);
				StructuredBuffer<int>		g_iIndices : register(t5);

				SamplerState MyLinearClampSampler : register(s0);
				SamplerState MyLinearRepeatSampler : register(s1);
				
				float g_fSize;
				float g_fDensity;
					
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
						out float2 f2RayIsecs,
						out float3 f3EntryPointUSSpace, // Entry point in Unit Sphere (US) space
						out float3 f3ViewRayUSSpace,    // View ray direction in Unit Sphere (US) space
						out float3 f3LightDirUSSpace,   // Light direction in Unit Sphere (US) space
						out float fDistanceToEntryPoint,
						out float fDistanceToExitPoint)
					{
						float4x4 f3x3WorldToObjSpaceRotation = unity_WorldToObject; // float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);//;
						// Compute camera location and view direction in particle's object space:
						float3 f3CamPosObjSpace = f3CameraPos - ParticleAttrs.f3Pos;
						f3CamPosObjSpace = mul(f3CamPosObjSpace, f3x3WorldToObjSpaceRotation);
						float3 f3ViewRayObjSpace = mul(f3ViewRay, f3x3WorldToObjSpaceRotation);
						float3 f3LightDirObjSpce = mul(_WorldSpaceLightPos0.xyz, f3x3WorldToObjSpaceRotation);

						// Compute scales to transform ellipsoid into the unit sphere:
						float3 f3Scale = 1.0f / g_fSize; // 1.f / GetParticleScales(ParticleAttrs.fSize, CellAttrs.uiNumActiveLayers);

						float3 f3ScaledCamPosObjSpace;
						f3ScaledCamPosObjSpace = f3CamPosObjSpace.xyz * f3Scale;
						f3ViewRayUSSpace = normalize(f3ViewRayObjSpace.xyz*f3Scale);
						f3LightDirUSSpace = normalize(f3LightDirObjSpce.xyz*f3Scale);
						// Scale camera pos and view dir in obj space and compute intersection with the unit sphere:
						GetRaySphereIntersection(f3ScaledCamPosObjSpace, f3ViewRayUSSpace, 0, 1.f, f2RayIsecs);

						f3EntryPointUSSpace = f3ScaledCamPosObjSpace + f3ViewRayUSSpace * f2RayIsecs.x;

						fDistanceToEntryPoint = length(f3ViewRayUSSpace / f3Scale) * f2RayIsecs.x;
						fDistanceToExitPoint = length(f3ViewRayUSSpace / f3Scale) * f2RayIsecs.y;
					}

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

						f4LUTCoords.xyzw = f4LUTCoords.xyzw / float4(PI, 2 * PI, PI / 2, 2 * PI) + float4(0.0, 0.5f, 0, 0.5f);

						// Clamp only zenith (yz) coordinate as azimuth is filtered with wraparound mode
						f4LUTCoords.xz = clamp(f4LUTCoords, 0.5f / OPTICAL_DEPTH_LUT_DIM, 1.0f - 0.5f / OPTICAL_DEPTH_LUT_DIM).xz;
						f4LUTCoords.xyzw = f4LUTCoords.xyzw;
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
						in SCloudParticleLighting ParticleLighting,
						out float4 f4Color)
					{
						float3 f3EntryPointWS = f3CameraPos + fDistanceToEntryPoint * f3ViewRay;
						float3 f3ExitPointWS = f3CameraPos + fDistanceToExitPoint * f3ViewRay;

						float4 f4LUTCoords;
						WorldParamsToOpticalDepthLUTCoords(f3EntryPointUSSpace, f3ViewRayUSSpace, f4LUTCoords);
						// Randomly rotate the sphere
						f4LUTCoords.y += ParticleAttrs.fRndAzimuthBias;
						float fLOD = 0;//log2(max(fMaxDiff,1));

						float2 f2NormalizedDensityAndDist;
						SAMPLE_4D_LUT(g_tex3DParticleDensityLUT, OPTICAL_DEPTH_LUT_DIM, f4LUTCoords, fLOD, f2NormalizedDensityAndDist);
						f2NormalizedDensityAndDist.x += f2NormalizedDensityAndDist.y;
						float3 f3FirstMatterPointWS = f3CameraPos + (fDistanceToEntryPoint + (fDistanceToExitPoint - fDistanceToEntryPoint) * f2NormalizedDensityAndDist.y) * f3ViewRay;
						float3 f3FirstMatterPointUSSpace = f3EntryPointUSSpace + (fIsecLenUSSpace * f2NormalizedDensityAndDist.y) * f3ViewRayUSSpace;

						float3 f3NoiseSamplingPos = f3FirstMatterPointWS;
						float fNoisePeriod = 3412;
						float fNoise = bAutoLOD ?
							(g_tex3DNoise.Sample(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod)) * 2 + g_tex3DNoise.Sample(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod / 3))) / 3 :
							(g_tex3DNoise.SampleLevel(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod), 0) * 2 + g_tex3DNoise.SampleLevel(MyLinearRepeatSampler, f3NoiseSamplingPos / (fNoisePeriod / 3), 0)) / 3;

						float fNoNoiseY = -0.7;
						float fTransition = min(1 - fNoNoiseY, 0.5);
						fNoise = lerp(0.5, fNoise, max(saturate((f3EntryPointUSSpace.y - fNoNoiseY) / fTransition), 0.2));

						fCloudMass = f2NormalizedDensityAndDist.x * (fDistanceToExitPoint - fDistanceToEntryPoint);
						fCloudMass *= ParticleAttrs.fDensity *lerp(fNoise, 1, 0.5);

						fTransparency = 1.0f - exp(-fCloudMass); // range 0.01 - 0.1 m^-1

						float fMultipleScatteringDensityScale = ParticleAttrs.fDensity;
						float fSingleScatteringDensityScale = fMultipleScatteringDensityScale * (f2NormalizedDensityAndDist.x);

						float4 f4SingleScatteringLUTCoords = WorldParamsToParticleScatteringLUT(f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace, true, fSingleScatteringDensityScale);
						float4 f4MultipleScatteringLUTCoords = WorldParamsToParticleScatteringLUT(f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace, true, fMultipleScatteringDensityScale);
						float fSingleScattering = 0, fMultipleScattering = 0;
						SAMPLE_4D_LUT(g_tex3DSingleScatteringInParticleLUT, SRF_SCATTERING_IN_PARTICLE_LUT_DIM, f4SingleScatteringLUTCoords, 0, fSingleScattering);
						SAMPLE_4D_LUT(g_tex3DMultipleScatteringInParticleLUT, SRF_SCATTERING_IN_PARTICLE_LUT_DIM, f4MultipleScatteringLUTCoords, 0, fMultipleScattering);
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
						f4Color.rgb = (f3Ambient.rgb + (fSingleScattering + fMultipleScattering) * ParticleLighting.f4SunLight.rgb) * PI; // max((f2NormalizedDensityAndDist.x), 0); //
						f4Color.b = 0;// f4LUTCoords.w;// max((f2NormalizedDensityAndDist.y), 0);// (f3Ambient.rgb + (fSingleScattering + fMultipleScattering) * ParticleLighting.f4SunLight.rgb) * PI;
						f4Color.g = 0;// fTransparency;// (f3Ambient.rgb + (fSingleScattering + fMultipleScattering) * ParticleLighting.f4SunLight.rgb) * PI;
						f4Color.a = 1.0f;// fTransparency;//  f4Color.g; // 
						f4Color.r = f2NormalizedDensityAndDist.x;
						//f4Color.rb = f2NormalizedDensityAndDist.xy;
						//f4Color.gb = 0;
					}


							v2g vert(appdata v)
							{
								v2g o;
								v.id = g_iIndices[v.id];
								o.vertex = float4(g_vVertices[v.id], 1.0f);
								o.id = v.id;
								return o;
							}
							
							[maxvertexcount(10 + 4 + 4)]
							void geom(point v2g p[1], inout TriangleStream<g2f> Out)
							{
								uint uiParticleId = p[0].id;

								// Only visible particles are sent for rendering, so there is no need to
								// test visibility here
								//bool bIsValid = g_VisibleParticleFlags[uiParticleId/32] & (1 << (uiParticleId&31));
								//if( !bIsValid )
								//    return;

								float3 f3Size = g_fSize;

								g2f Outs[8];
								matrix mViewProj = UNITY_MATRIX_VP; // g_CameraAttribs.WorldViewProj;
								// Multiply with camera view-proj matrix
								matrix ParticleObjToProjSpaceMatr = mViewProj; // mul(ParticleObjToWorldSpaceMatr, );

								for (int iCorner = 0; iCorner < 8; ++iCorner)
								{
									float4 f4CurrCornerWorldPos;
									f4CurrCornerWorldPos.xyz = p[0].vertex + f3Size * float3((iCorner & 0x01) ? +1 : -1, (iCorner & 0x04) ? +1 : -1, (iCorner & 0x02) ? +1 : -1);
									f4CurrCornerWorldPos.w = 1.0f;

									float4 f4CurrCornerPosPS = UnityObjectToClipPos(f4CurrCornerWorldPos);//mul(f4CurrCornerWorldPos, ParticleObjToProjSpaceMatr); //

									Outs[iCorner].id = uiParticleId;
									Outs[iCorner].vertex = f4CurrCornerPosPS;
									Outs[iCorner].id = p[0].id;

									Outs[iCorner].texcoord = float2(0, 1);// float2(min(max((v[0].x + startPos.x)*mul_val_x, 0), 1.0f), max(min((v[0].y + startPos.y)*mul_val_y + 0.1f * g_i3Dimensions.y, 1.0), 0.0f));
									Outs[iCorner].texcoord_2 = float2(1, 0);
									Outs[iCorner].projPos = float4(UnityObjectToViewPos(f4CurrCornerWorldPos),1.0f);
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
								ParticleAttrs.f3Pos = g_vVertices[In.id];
								ParticleAttrs.fSize = g_fSize;
								ParticleAttrs.fRndAzimuthBias = 0.0f;

								SCloudParticleLighting ParticleLighting;

								ParticleLighting.f4SunLight = _LightColor0;
								ParticleLighting.f4LightAttenuation = float4(3.0f,3.0f,1.0f,1.0f); // .x == single scattering; .y == multiple scattering
								ParticleLighting.f4AmbientLight = float4(0.0,0.0,0.0,1.0);
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

								f3CameraPos = float3(0, 0, -2);//_WorldSpaceCameraPos.xyz; // 
								//f3ViewRay = normalize(In.f3ViewRay);
								//(float2 f2PosPS = UVToProj(In.vertex.xy / f2ScreenDim);
								float fDepth = In.projPos.z;
								float4 f4ReconstructedPosWS =mul(UNITY_MATRIX_I_V, float4(In.projPos.xy, fDepth, 1.0f)); // mul(mul(, unity_CameraInvProjection), UNITY_MATRIX_IT_MV); // mul(float4(f2PosPS.xy, fDepth, 1.0), g_CameraAttribs.mViewProjInv);
								float3 f3WorldPos = f4ReconstructedPosWS.xyz / f4ReconstructedPosWS.w;
								// Compute view ray
								f3ViewRay = f3WorldPos - f3CameraPos;
								float fRayLength = length(f3ViewRay);
								f3ViewRay /= fRayLength;
								
								float2 f2RayIsecs;
								float fDistanceToEntryPoint, fDistanceToExitPoint;
								float3 f3EntryPointUSSpace, f3ViewRayUSSpace, f3LightDirUSSpace;
								IntersectRayWithParticle(ParticleAttrs,  f3CameraPos, f3ViewRay,
														f2RayIsecs, f3EntryPointUSSpace, f3ViewRayUSSpace,f3LightDirUSSpace,fDistanceToEntryPoint, fDistanceToExitPoint);

								if (f2RayIsecs.y < 0)
									discard;
								if (fRayLength < fDistanceToEntryPoint)
									discard; // return float4(1.0f, 1.0f, 1.0f, 1.0f);
								fDistanceToExitPoint = min(fDistanceToExitPoint, fRayLength);

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
									ParticleLighting,
									f4Color
								);
								return f4Color;
							}
							ENDCG
							}
		}
			Fallback "VertexLit"
}
