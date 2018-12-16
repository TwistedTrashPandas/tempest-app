Shader "Unlit/Hologram"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorTex("Color Tex", 2D) = "white" {}
		_DepthTex("Depth Tecture", 2D) = "white" {}
		_TesselationFactor ("TesselationFactor", Range(1, 64)) = 4
		_DiscardAbove ("Discard Above", Range(0, 1)) = 1
		_WaveSpeed ("Wave Speed", Range(0, 10)) = 0.5
		_WaveFrequency ("Wave Frequency", Range(0, 10)) = 8
		_WaveAmplitude ("Wave Amplitude", Range(0, 1)) = 0.02
		_RotationSpeed ("Rotation Speed", Range(0, 100)) = 1
		_ColorShiftSpeed ("Color Shift Speed", Range(0, 2)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex VS
			#pragma hull HS
			#pragma domain DS
			#pragma fragment FS
			
			#include "UnityCG.cginc"
			#include "ColorConversion.hlsl"

			struct appdata
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2t
			{
				float4 position : INTERNALTESSPOS;
				float2 uv : TEXCOORD0;
			};

			struct t2f
			{
				float4 position : SV_POSITION;
				float2 uvColor : TEXCOORD2;
				float2 uvDepth : TEXCOORD1;
				float2 uv : TEXCOORD0;
			};

			struct TesselationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _ColorTex;
			float4 _ColorTex_ST;
			sampler2D _DepthTex;
			float4 _DepthTex_ST;
			float _TesselationFactor;
			float _DiscardAbove;
			float _WaveSpeed;
			float _WaveFrequency;
			float _WaveAmplitude;
			float _RotationSpeed;
			float _ColorShiftSpeed;
			
			// Vertex shader before tesselation
			v2t VS (appdata v)
			{
				v2t o;
				o.position = v.position;
				o.uv = v.uv;
				return o;
			}

			// Vertex shader after tesselation
			t2f TVS(v2t v)
			{
				t2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.uvColor = TRANSFORM_TEX(v.uv, _ColorTex);
				o.uvDepth = TRANSFORM_TEX(v.uv, _DepthTex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			// Pass vertex data to the tesselation stage
			[UNITY_domain("tri")]
			[UNITY_outputcontrolpoints(3)]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_partitioning("integer")]
			[UNITY_patchconstantfunc("PatchConstantFunction")]
			v2t HS(InputPatch<v2t, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			// Return the tesselation factors for an input patch
			TesselationFactors PatchConstantFunction(InputPatch<v2t, 3> patch)
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
			t2f DS(TesselationFactors factors, OutputPatch<v2t, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
				v2t o;
				o.position = patch[0].position * barycentricCoordinates.x +
							 patch[1].position * barycentricCoordinates.y +
							 patch[2].position * barycentricCoordinates.z;

				o.uv = patch[0].uv * barycentricCoordinates.x +
					   patch[1].uv * barycentricCoordinates.y +
					   patch[2].uv * barycentricCoordinates.z;

				// Run different vertex shader after tesselation
				return TVS(o);
			}
			
			float4 FS (t2f i) : SV_Target
			{
				float4 colorOutput = float4(0, 0, 0, 0);

				// Rotate the magic ring
				float alpha = _RotationSpeed * _Time.y;
				float s = sin(alpha);
				float c = cos(alpha);
				float2x2 rotationMatrix = float2x2(c, -s, s, c);

				// Move to origin, rotate and move back again
				float2 uvRotated = i.uv;
				uvRotated -= float2(0.5f, 0.5f);
				uvRotated = mul(uvRotated, rotationMatrix);
				uvRotated += float2(0.5f, 0.5f);

				float4 colorMainTex = tex2D(_MainTex, uvRotated);
				colorOutput += 0.5f * sin(2 * pow(uvRotated.x, 2)) * colorMainTex;
				colorOutput += 0.5f * abs(sin(_Time.y)) * colorMainTex;

				// Shift colors with hsv representation
				float3 hsv = RGBtoHSV(colorOutput.rgb);
				hsv.x = abs(sin(hsv.x + uvRotated.x + uvRotated.y + _ColorShiftSpeed * _Time.y));
				colorOutput.rgb = HSVtoRGB(hsv);

				// Wave for more magic appeal
				float2 uv = i.uvColor;
				uv.x += _WaveAmplitude * sin(_WaveFrequency * (i.uvColor.y - _WaveSpeed * _Time.y));
				uv.y += _WaveAmplitude * sin(_WaveFrequency * (i.uvColor.x - _WaveSpeed * _Time.y));

				float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_DepthTex, uv)));

				// Check depth and only add color if valid
				if (depth < _DiscardAbove)
				{
					colorOutput += tex2D(_ColorTex, uv);
					//colorOutput.r += 1.0f / depth;
				}

				return colorOutput;
			}

			ENDCG
		}
	}
}
