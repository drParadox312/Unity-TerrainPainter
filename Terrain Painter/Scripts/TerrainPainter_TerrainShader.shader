Shader "Custom/TerrainPainterCustomTerrainShader"
{
	Properties
	{
		_LerpingDistance("_LerpingDistance", Range(1,5000)) = 1500
		_HeightBlendingTransition("_HeightBlendingTransition", Range(0.1, 0.5)) = 0.35
		_TriplanarCutoffBias("_UseTriplanar", Range(0,1)) = 0.5

		[HideInInspector] _SplatCount("_SplatCount", Int) = 0
		_TriplanarWeightMap("_TriplanarWeightMap", 2D) = "" {}
		_TextureArrayManualPainted("_TextureArrayManualPainted", 2DArray) = "" {}
		_TextureArraySplatmap("_TextureArraySplatmap", 2DArray) = "" {}
		_TextureArrayDiffuse("_TextureArrayDiffuse", 2DArray) = "" {}
		_TextureArrayNormal("_TextureArrayNormal", 2DArray) = "" {}
		_TextureArrayHeightmap("_TextureArrayHeightmap", 2DArray) = "" {}
		_TextureArrayOcclusion("_TextureArrayOcclusion", 2DArray) = "" {}
	//	_ColorMapDiffuse("_ColorMapDiffuse", 2D) = "" {}
	//	_ColorMapNormal("_ColorMapNormal", 2D) = "" {}
	}

		SubShader
	{
		Tags
		{
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		CGPROGRAM

		#pragma surface surf Lambert vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
		#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
		#pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
		#pragma target 3.0
		// needs more than 8 texcoords
		#pragma exclude_renderers gles
		#include "UnityPBSLighting.cginc"

		#pragma multi_compile __ _NORMALMAP

		#define TERRAIN_STANDARD_SHADER
		#define TERRAIN_INSTANCED_PERPIXEL_NORMAL
		#define TERRAIN_SURFACE_OUTPUT SurfaceOutput
		#include "TerrainSplatmapCommonCustom.cginc"





		float _LerpingDistance;
		float _HeightBlendingTransition;
		float _TriplanarCutoffBias;
		float _ColorMapDistance;
		float _UvScale;
		int _SplatCount;
		sampler2D _TriplanarWeightMap;
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayManualPainted);
		UNITY_DECLARE_TEX2DARRAY(_TextureArraySplatmap);
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayDiffuse);
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayNormal);
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayHeightmap);
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayOcclusion);
	//	sampler2D _ColorMapDiffuse;
	//	sampler2D _ColorMapNormal;
		float4 _UvScaleArray[16];
		float _HeightArray[16];







		float CalculateUnpaintedWeight(Input IN)
		{
			float unpaintedWeight = 1;

			for (int i = 0; i < _SplatCount; i++)
			{
				int splatIndex = (int)floor(((float)i) / 4.0);
				float chanelIndex = fmod(((float)i),4);
				float4 sampledSplatColor = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayManualPainted, float3(IN.uv_TextureArraySplatmap, splatIndex));

				UNITY_BRANCH
				if (chanelIndex == 0)
					unpaintedWeight -= sampledSplatColor.r;
				else if (chanelIndex == 1)
					unpaintedWeight -= sampledSplatColor.g;
				else if (chanelIndex == 2)
					unpaintedWeight -= sampledSplatColor.b;
				else if (chanelIndex == 3)
					unpaintedWeight -= sampledSplatColor.a;

			}

			return saturate(unpaintedWeight); ;
		}





		fixed SampleSplatMap(Input IN, int splatIndex, float chanelIndex, float unpaintedWeight)
		{
			fixed sampledSplat;
			fixed4 proceduralSplatColor = 0;
			fixed4 manualSplatColor = 0;

			proceduralSplatColor = unpaintedWeight * UNITY_SAMPLE_TEX2DARRAY(_TextureArraySplatmap, float3(IN.uv_TextureArraySplatmap, splatIndex));
			manualSplatColor = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayManualPainted, float3(IN.uv_TextureArraySplatmap, splatIndex));


			UNITY_BRANCH
			if (chanelIndex == 0)
				sampledSplat = manualSplatColor.r + proceduralSplatColor.r;
			else if (chanelIndex == 1)
				sampledSplat = manualSplatColor.g + proceduralSplatColor.g;
			else if (chanelIndex == 2)
				sampledSplat = manualSplatColor.b + proceduralSplatColor.b;
			else if (chanelIndex == 3)
				sampledSplat = manualSplatColor.a + proceduralSplatColor.a;

			return sampledSplat;
		}


		fixed4 LerpDiffuse(int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float2 uv)
		{
			fixed4 d0 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayDiffuse, float3(uv * uvScaleArrayValue.z, splatIndex));
			fixed4 d1 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayDiffuse, float3(uv, splatIndex));
			return lerp(d0, d1, lerpRatio); // pow(lerpRatio, 0.25 + lerpRatio * 2));
		}


		fixed3 LerpNormal(int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float2 uv, float normalScale)
		{
			fixed3 d0 = UnpackNormalWithScale(UNITY_SAMPLE_TEX2DARRAY(_TextureArrayNormal, float3(uv * uvScaleArrayValue.z, splatIndex)), normalScale);
			fixed3 d1 = UnpackNormalWithScale(UNITY_SAMPLE_TEX2DARRAY(_TextureArrayNormal, float3(uv , splatIndex)), normalScale);
			return lerp(d0, d1, lerpRatio); // pow(lerpRatio, 0.25 + lerpRatio * 2));
		}


		fixed4 LerpHeight(int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float2 uv)
		{
			fixed4 d0 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayHeightmap, float3(uv * uvScaleArrayValue.z, splatIndex));
			fixed4 d1 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayHeightmap, float3(uv, splatIndex));
			return lerp(d0, d1, lerpRatio); // pow(lerpRatio, 0.25 + lerpRatio * 2));
		}


		fixed4 LerpOcclusion(int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float2 uv)
		{
			fixed4 d0 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayOcclusion, float3(uv * uvScaleArrayValue.z, splatIndex));
			fixed4 d1 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayOcclusion, float3(uv, splatIndex));
			return lerp(d0, d1, lerpRatio); // pow(lerpRatio, 0.25 + lerpRatio * 2));
		}



		fixed4 GetBlendedDiffuse(Input IN, int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float4 triplanarWeight)
		{
			fixed4 sampledDiffuse = 0;
			UNITY_BRANCH
			if (clamp(lerpRatio - _TriplanarCutoffBias, -0.1, 1.0) < triplanarWeight.a)
			{
				half2 xUV = IN.worldPos.zy / uvScaleArrayValue.x;
				half2 yUV = IN.worldPos.xz / uvScaleArrayValue.x;
				half2 zUV = IN.worldPos.xy / uvScaleArrayValue.x;

				fixed4 sampledDiffuseX = LerpDiffuse(splatIndex, lerpRatio, uvScaleArrayValue, xUV);
				fixed4 sampledDiffuseY = LerpDiffuse(splatIndex, lerpRatio, uvScaleArrayValue, yUV);
				fixed4 sampledDiffuseZ = LerpDiffuse(splatIndex, lerpRatio, uvScaleArrayValue, zUV);

				sampledDiffuse =
						sampledDiffuseX * triplanarWeight.x
					+ sampledDiffuseY * triplanarWeight.y
					+ sampledDiffuseZ * triplanarWeight.z;
			}
			else
			{
				sampledDiffuse = LerpDiffuse(splatIndex, lerpRatio, uvScaleArrayValue, IN.worldPos.xz / uvScaleArrayValue.x);
			}
			return sampledDiffuse;
		}

		fixed3 GetBlendedNormal(Input IN, int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float3 normal, float normalScale, float4 triplanarWeight)
		{
			fixed3 sampledNormal = 0;
			UNITY_BRANCH
			if (clamp(lerpRatio - _TriplanarCutoffBias, -0.1, 1.0) < triplanarWeight.a)
			{
				half2 xUV = IN.worldPos.zy / uvScaleArrayValue.x;
				half2 yUV = IN.worldPos.xz / uvScaleArrayValue.x;
				half2 zUV = IN.worldPos.xy / uvScaleArrayValue.x;

				fixed3 sampledNormalX = LerpNormal(splatIndex, lerpRatio, uvScaleArrayValue, xUV , normalScale);
				fixed3 sampledNormalY = LerpNormal(splatIndex, lerpRatio, uvScaleArrayValue, yUV , normalScale);
				fixed3 sampledNormalZ = LerpNormal(splatIndex, lerpRatio, uvScaleArrayValue, zUV , normalScale);

				sampledNormal =
					sampledNormalX * triplanarWeight.x
					+ sampledNormalY * triplanarWeight.y
					+ sampledNormalZ * triplanarWeight.z;
			}
			else
			{
				sampledNormal = LerpNormal(splatIndex, lerpRatio, uvScaleArrayValue, IN.worldPos.xz / uvScaleArrayValue.x, normalScale);
			}
			return sampledNormal;
		}

		fixed4 GetBlendedHeight(Input IN, int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float4 triplanarWeight)
		{
			fixed4 sampledHeight = 0;
			UNITY_BRANCH
			if (clamp(lerpRatio - _TriplanarCutoffBias, -0.1, 1.0) < triplanarWeight.a)
			{
				half2 xUV = IN.worldPos.zy / uvScaleArrayValue.x;
				half2 yUV = IN.worldPos.xz / uvScaleArrayValue.x;
				half2 zUV = IN.worldPos.xy / uvScaleArrayValue.x;

				fixed4 sampledHeightX = LerpHeight(splatIndex, lerpRatio, uvScaleArrayValue, xUV);
				fixed4 sampledHeightY = LerpHeight(splatIndex, lerpRatio, uvScaleArrayValue, yUV);
				fixed4 sampledHeightZ = LerpHeight(splatIndex, lerpRatio, uvScaleArrayValue, zUV);

				sampledHeight =
					sampledHeightX * triplanarWeight.x
					+ sampledHeightY * triplanarWeight.y
					+ sampledHeightZ * triplanarWeight.z;
			}
			else
			{
				sampledHeight = LerpHeight(splatIndex, lerpRatio, uvScaleArrayValue, IN.worldPos.xz / uvScaleArrayValue.x);
			}
			return sampledHeight;
		}

		fixed4 GetBlendedOcclusion(Input IN, int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float4 triplanarWeight)
		{
			fixed4 sampledOcclusion = 0;
			UNITY_BRANCH
			if (clamp(lerpRatio - _TriplanarCutoffBias, -0.1, 1.0) < triplanarWeight.a)
			{
				half2 xUV = IN.worldPos.zy / uvScaleArrayValue.x;
				half2 yUV = IN.worldPos.xz / uvScaleArrayValue.x;
				half2 zUV = IN.worldPos.xy / uvScaleArrayValue.x;

				fixed4 sampledOcclusionX = LerpOcclusion(splatIndex, lerpRatio, uvScaleArrayValue, xUV);
				fixed4 sampledOcclusionY = LerpOcclusion(splatIndex, lerpRatio, uvScaleArrayValue, yUV);
				fixed4 sampledOcclusionZ = LerpOcclusion(splatIndex, lerpRatio, uvScaleArrayValue, zUV);

				sampledOcclusion =
					sampledOcclusionX * triplanarWeight.x
					+ sampledOcclusionY * triplanarWeight.y
					+ sampledOcclusionZ * triplanarWeight.z;
			}
			else
			{
				sampledOcclusion = LerpOcclusion(splatIndex, lerpRatio, uvScaleArrayValue, IN.worldPos.xz / uvScaleArrayValue.x);
			}
			return sampledOcclusion;
		}



		void MixSplat(Input IN, float3 terrainNormal, float lerpRatio, inout fixed4 diffuse,  inout fixed3 normal)
		{
			diffuse = 0;
			normal = 0;

			float unpaintedWeight = CalculateUnpaintedWeight(IN);

			float4 triplanarWeight = tex2D(_TriplanarWeightMap, IN.uv_TextureArraySplatmap);

			float totalHeight = 0;
			for (int i = 0; i < _SplatCount; i++)
			{
				int splatIndex = (int)floor(((float)i) / 4.0);
				float chanelIndex = fmod(((float)i), 4);

				fixed sampledSplat = SampleSplatMap(IN, splatIndex, chanelIndex, unpaintedWeight);


				_HeightArray[i] = sampledSplat * GetBlendedHeight(IN, i, lerpRatio, _UvScaleArray[i], triplanarWeight).r;
				totalHeight += _HeightArray[i];
			}


			totalHeight += 0.001;
			float maxHeight = 0;
			for (int i = 0; i < _SplatCount; i++)
			{
				_HeightArray[i] /= totalHeight;
				maxHeight = max(maxHeight, _HeightArray[i]);
			}


			totalHeight = 0;
			maxHeight -= _HeightBlendingTransition;
			maxHeight *= (1 - lerpRatio);

			for (int i = 0; i < _SplatCount; i++)
			{
				float weight = max(0, _HeightArray[i] - maxHeight);
				_HeightArray[i] = weight;
				totalHeight += _HeightArray[i];
			}

			totalHeight += 0.001;
			for (int i = 0; i < _SplatCount; i++)
			{
				_HeightArray[i] /= totalHeight;
			}

			for (int i = 0; i < _SplatCount; i++)
			{
				int splatIndex = (int)floor(((float)i) / 4.0);
				float chanelIndex = fmod(((float)i), 4);

				fixed sampledSplat = _HeightArray[i].r;

				UNITY_BRANCH
				if (sampledSplat > 0)
				{
					float occlusion = GetBlendedOcclusion(IN, i, lerpRatio, _UvScaleArray[i], triplanarWeight);

					diffuse += sampledSplat * occlusion * GetBlendedDiffuse(IN, i, lerpRatio, _UvScaleArray[i], triplanarWeight);
					normal += sampledSplat * occlusion * GetBlendedNormal(IN, i, lerpRatio, _UvScaleArray[i], terrainNormal, 1, triplanarWeight);
				}
			}
		
			




			#if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				normal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
			#endif


			#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				#ifdef _NORMALMAP
					float3 terrainTangent = normalize(cross(terrainNormal, float3(0, 0, 1)));
					float3 terrainBitangent = normalize(cross(terrainTangent, terrainNormal));
					normal = normal.x * terrainTangent
								+ normal.y * terrainBitangent
								+ normal.z * terrainNormal;
				#else
					normal = terrainNormal;
				#endif

				normal = normal.xzy;
			#endif
		}




		void surf(Input IN, inout SurfaceOutput o)
		{
			float3 terrainNormal = 0;

			#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				terrainNormal = normalize(tex2D(_TerrainNormalmapTexture, IN.uv_TextureArraySplatmap).xyz * 2 - 1);
			#else
				terrainNormal = WorldNormalVector(IN, o.Normal);
			#endif


			fixed4 diffuse = 0;
			fixed3 normal = 0;

			float distanceVertexToCamera = length((float4(IN.worldPos, 0)) - _WorldSpaceCameraPos);
			float lerpRatio = smoothstep(1, _LerpingDistance, distanceVertexToCamera);

			MixSplat(IN, terrainNormal, lerpRatio, diffuse, normal);

			o.Albedo = diffuse.rgb;
			o.Normal = normal;
			o.Alpha = 1;
		}

		ENDCG

		UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
		UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
	}


		Dependency "AddPassShader" = "Custom/TerrainPainterCustomTerrainShader"
			Dependency "BaseMapShader" = "Custom/TerrainPainterCustomTerrainShader"
			//    Dependency "BaseMapShader"    = "Hidden/TerrainEngine/Splatmap/Standard-Base"
			//    Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Standard-BaseGen"


			Fallback "Nature/Terrain/Diffuse"
}
