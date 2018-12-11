

Texture2D<float>  g_tex2DDepthBuffer            : register(t0);
Texture2D<float>  g_tex2DCamSpaceZ              : register(t0);
Texture2D<float4> g_tex2DSliceEndPoints         : register(t4);
Texture2D<float2> g_tex2DCoordinates            : register(t1);
Texture2D<float>  g_tex2DEpipolarCamSpaceZ      : register(t2);
Texture2D<uint2>  g_tex2DInterpolationSource    : register(t6);
Texture2DArray<float> g_tex2DLightSpaceDepthMap : register(t3);
Texture2D<float4> g_tex2DSliceUVDirAndOrigin    : register(t6);
Texture2D<float3> g_tex2DInitialInsctrIrradiance: register(t5);
Texture2D<float4> g_tex2DColorBuffer            : register(t1);
Texture2D<float3> g_tex2DScatteredColor         : register(t3);
Texture2D<float2> g_tex2DOccludedNetDensityToAtmTop : register(t5);
Texture2D<float3> g_tex2DEpipolarExtinction     : register(t6);
Texture3D<float3> g_tex3DSingleSctrLUT          : register(t7);
Texture3D<float3> g_tex3DHighOrderSctrLUT       : register(t8);
Texture3D<float3> g_tex3DMultipleSctrLUT        : register(t9);
Texture2D<float3> g_tex2DSphereRandomSampling   : register(t1);
Texture3D<float3> g_tex3DPreviousSctrOrder      : register(t0);
Texture3D<float3> g_tex3DPointwiseSctrRadiance  : register(t0);
Texture2D<float>  g_tex2DAverageLuminance       : register(t10);
Texture2D<float>  g_tex2DLowResLuminance        : register(t0);
Texture2D<float>  g_tex2DScrSpaceCloudTransparency : register(t11);
Texture2D<float2> g_tex2DScrSpaceCloudMinMaxDist   : register(t12);
Texture2D<float4> g_tex2DScrSpaceCloudColor        : register(t13);
Texture2DArray<float> g_tex2DLiSpaceCloudTransparency : register(t14);
Texture2D<float> g_tex2DLiSpCldDensityEpipolarScan  : register(t15);
Texture2D<float> g_tex2DEpipolarCloudTransparency : register(t16);

static SAirScatteringAttribs g_MediaParams;
static SLightAttribs g_LightAttribs;

#define SAMPLE_4D_LUT(tex3DLUT, LUT_DIM, f4LUTCoords, fLOD, Result)  \
{                                                               \
    float3 f3UVW;                                               \
    f3UVW.xy = f4LUTCoords.xy;                                  \
    float fQSlice = f4LUTCoords.w * LUT_DIM.w - 0.5;            \
    float fQ0Slice = floor(fQSlice);                            \
    float fQWeight = fQSlice - fQ0Slice;                        \
                                                                \
    f3UVW.z = (fQ0Slice + f4LUTCoords.z) / LUT_DIM.w;           \
                                                                \
    Result = lerp(                                              \
        tex3DLUT.SampleLevel(MyLinearRepeatSampler, f3UVW, fLOD),       \
        /* frac() assures wraparound filtering of w coordinate*/                            \
        tex3DLUT.SampleLevel(MyLinearRepeatSampler, frac(f3UVW + float3(0,0,1/LUT_DIM.w)), fLOD),   \
        fQWeight);                                                                          \
}

float HGPhaseFunc(float fCosTheta, const float g = 0.9)
{
	return (1 / (4 * PI) * (1 - g * g)) / pow(max((1 + g * g) - (2 * g)*fCosTheta, 0), 3.f / 2.f);
}

float2 UVToProj(in float2 f2UV)
{
	return float2(-1.0, 1.0) + float2(2.0, -2.0) * f2UV;
}

float2 ProjToUV(in float2 f2ProjSpaceXY)
{
	return float2(0.5, 0.5) + float2(0.5, -0.5) * f2ProjSpaceXY;
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
