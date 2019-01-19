
#define PI 3.1415928f

#define BOOL bool // Do not use bool, because sizeof(bool)==1 !

struct SLightAttribs
{
	float4 f4DirOnLight;
	float4 f4AmbientLight;
	float4 f4LightScreenPos;
	float4 f4ExtraterrestrialSunColor;

	BOOL bIsLightOnScreen;
	float fMaxShadowCamSpaceZ;
	float2 f2Dummy;
};

struct SCameraAttribs
{
	float4 f4CameraPos;            ///< Camera world position
	float fNearPlaneZ;
	float fFarPlaneZ; // fNearPlaneZ < fFarPlaneZ
	float2 f2Dummy;

	float3 f3ViewDir;
	float fDummy;

	float4 f4ViewFrustumPlanes[6];

	matrix WorldViewProj;
	matrix mView;
	matrix mProj;
	matrix mViewProjInv;
};

struct SParticleAttribs
{
	float3 f3Pos;
	float fSize;
	float fRndAzimuthBias;
	float fDensity;
};

struct SCloudParticleLighting
{
	float4 f4SunLight;
	float4 f4LightAttenuation; // .x == single scattering; .y == multiple scattering
	float4 f4AmbientLight;
};