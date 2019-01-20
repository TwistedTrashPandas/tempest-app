SamplerState My_Linear_ClampU_RepeatV_RepeatW_Sampler : register(s0);
SamplerState MyLinearRepeatSampler : register(s1);
SamplerState MyLinearClampSampler : register(s2);

#   define OPTICAL_DEPTH_LUT_DIM float4(32,64,32,64) // test: float4(64,32,64,32) //
#   define VOL_SCATTERING_IN_PARTICLE_LUT_DIM float4(32,64,32,8)
#   define SRF_SCATTERING_IN_PARTICLE_LUT_DIM float4(32,64,16,8)

void SAMPLE_4D_LUT_FLT2(Texture3D<float2> tex3DLUT, float4 LUT_DIM, float4 f4LUTCoords, int fLOD, out float2 Result)  \
{                                                               
float3 f3UVW;                                               
f3UVW.xy = f4LUTCoords.xy;                                  
float fQSlice = f4LUTCoords.w *  float(LUT_DIM.w) - 0.5f;
float fQ0Slice = floor(fQSlice);                            
float fQWeight = fQSlice - fQ0Slice;                        

f3UVW.z = (fQ0Slice + f4LUTCoords.z) / float(LUT_DIM.w);           

Result = lerp(
	tex3DLUT.SampleLevel(MyLinearRepeatSampler, f3UVW, fLOD), 
	/* frac() assures wraparound filtering of w coordinate*/                            
	tex3DLUT.SampleLevel(MyLinearRepeatSampler, frac(f3UVW + float3(0.0f, 0.0f, 1.0f / LUT_DIM.w)), fLOD),
	fQWeight);                                                                          
}

void SAMPLE_4D_LUT_FLT(Texture3D<float> tex3DLUT, float4 LUT_DIM, float4 f4LUTCoords, int fLOD, out float Result)  \
{                                                               
float3 f3UVW;                                               
f3UVW.xy = f4LUTCoords.xy;                                  
float fQSlice = f4LUTCoords.w *  (LUT_DIM.w) - 0.5f;
float fQ0Slice = floor(fQSlice);                           
float fQWeight = fQSlice - fQ0Slice;                        

f3UVW.z = (fQ0Slice + f4LUTCoords.z) / (LUT_DIM.w);          

Result = lerp(tex3DLUT.Sample(MyLinearRepeatSampler, f3UVW), 
	/* frac() assures wraparound filtering of w coordinate*/                            
	tex3DLUT.Sample(MyLinearRepeatSampler, frac(f3UVW + float3(0.0f, 0.0f, 1.0f / LUT_DIM.w))), 
	fQWeight);                                                                          
}

void ComputeLocalFrameAnglesXYZ(in float3 f3LocalX,
	in float3 f3LocalY,
	in float3 f3LocalZ,
	in float3 f3RayDir,
	out float fLocalZenithAngle,
	out float fLocalAzimuthAngle)
{
	fLocalZenithAngle = acos(saturate(dot(f3LocalZ, f3RayDir)));

	// Compute azimuth angle in the local frame
	float fViewDirLocalAzimuthCos = dot(f3RayDir, f3LocalX);
	float fViewDirLocalAzimuthSin = dot(f3RayDir, f3LocalY);
	fLocalAzimuthAngle = atan2(fViewDirLocalAzimuthSin, fViewDirLocalAzimuthCos);
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

void ConstructLocalFrameXYZ(in float3 f3Up, in float3 f3Inward, out float3 f3X, out float3 f3Y, out float3 f3Z)
{
	//      Z (Up)
	//      |    Y  (Inward)
	//      |   /
	//      |  /
	//      | /  
	//      |/
	//       -----------> X
	//
	f3Z = normalize(f3Up);
	f3X = normalize(cross(f3Inward, f3Z));
	f3Y = normalize(cross(f3Z, f3X));
}
float4 WorldParamsToParticleScatteringLUT(in float3 f3StartPosUSSpace,
	in float3 f3ViewDirInUSSpace,
	in float3 f3LightDirInUSSpace,
	in uniform bool bSurfaceOnly,
	in float fDensityLevel)
{
	float4 f4LUTCoords = 0;

	float fDistFromCenter = 0;
	if (!bSurfaceOnly)
	{
		// Compute distance from center and normalize start position
		fDistFromCenter = length(f3StartPosUSSpace);
		f3StartPosUSSpace /= max(fDistFromCenter, 1e-5);
	}
	float fStartPosZenithCos = dot(f3StartPosUSSpace, f3LightDirInUSSpace);
	f4LUTCoords.x = acos(fStartPosZenithCos);

	float3 f3LocalX, f3LocalY, f3LocalZ;
	ConstructLocalFrameXYZ(-f3StartPosUSSpace, f3LightDirInUSSpace, f3LocalX, f3LocalY, f3LocalZ);

	float fViewDirLocalZenith, fViewDirLocalAzimuth;
	ComputeLocalFrameAnglesXYZ(f3LocalX, f3LocalY, f3LocalZ, f3ViewDirInUSSpace, fViewDirLocalZenith, fViewDirLocalAzimuth);
	f4LUTCoords.y = fViewDirLocalAzimuth;
	f4LUTCoords.z = fViewDirLocalZenith;

	// In case the parameterization is performed for the sphere surface, the allowable range for the 
	// view direction zenith angle is [0, PI/2] since the ray should always be directed into the sphere.
	// Otherwise the range is whole [0, PI]
	f4LUTCoords.xyz = f4LUTCoords.xyz / float3(PI, 2 * PI, bSurfaceOnly ? (PI / 2) : PI) + float3(0, 0.5, 0);
	if (bSurfaceOnly)
		f4LUTCoords.w = log2(fDensityLevel) / SRF_SCATTERING_IN_PARTICLE_LUT_DIM.w + 0.5 + 0.5 / SRF_SCATTERING_IN_PARTICLE_LUT_DIM.w;
	else
		f4LUTCoords.w = fDistFromCenter;
	if (bSurfaceOnly)
		f4LUTCoords.xzw = clamp(f4LUTCoords, 0.5 / SRF_SCATTERING_IN_PARTICLE_LUT_DIM, 1 - 0.5 / SRF_SCATTERING_IN_PARTICLE_LUT_DIM).xzw;
	else
		f4LUTCoords.xzw = clamp(f4LUTCoords, 0.5 / VOL_SCATTERING_IN_PARTICLE_LUT_DIM, 1 - 0.5 / VOL_SCATTERING_IN_PARTICLE_LUT_DIM).xzw;

	return f4LUTCoords;
}
float HGPhaseFunc(float fCosTheta, const float g = 0.9)
{
	return (1 / (4 * PI) * (1 - g * g)) / pow(max((1 + g * g) - (2 * g)*fCosTheta, 0), 3.f / 2.f);
}

float2 UVToProj(in float2 f2UV)
{
	return float2(-1.0, -1.0) + float2(2.0, 2.0) * f2UV;
}

float2 ProjToUV(in float2 f2ProjSpaceXY)
{
	return float2(0.5, 0.5) + float2(0.5, -0.5) * f2ProjSpaceXY;
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

	f4LUTCoords.xyzw = f4LUTCoords.xyzw / float4(PI, 2 * PI, PI / 2.0f, 2 * PI) + float4(0.0f, 0.5, 0, 0.5);
	// Clamp only zenith (yz) coordinate as azimuth is filtered with wraparound mode
	f4LUTCoords.xz = clamp(f4LUTCoords, 0.5 / OPTICAL_DEPTH_LUT_DIM, 1.0 - 0.5 / OPTICAL_DEPTH_LUT_DIM).xz;
}

void GetRaySphereIntersection(in float3 f3RayOrigin,
	in float3 f3RayDirection,
	in float3 f3SphereCenter,
	in float fSphereRadius,
	out float2 f2Intersections)
{
	f3RayOrigin -= f3SphereCenter;
	float A = dot(f3RayDirection, f3RayDirection);
	float B = 2 * dot(f3RayOrigin, f3RayDirection);
	float C = dot(f3RayOrigin, f3RayOrigin) - fSphereRadius * fSphereRadius;
	float D = B * B - 4 * A*C;

	if (D < 0)
	{
		f2Intersections = float2(-1, -2);
	}
	else
	{
		D = sqrt(D);
		f2Intersections = float2(-B - D, -B + D) / (2 * A);
	}
}
