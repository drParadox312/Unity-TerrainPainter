Shader "Custom/TerrainPainterCustomTerrainShader"
{
	Properties
	{
		_LerpingDistance("_LerpingDistance", Range(1,5000)) = 1500
		_HeightBlendingTransition("_HeightBlendingTransition", Range(0.1, 0.5)) = 0.35
		_TriplanarCutoffBias("_TriplanarCutoffBias", Range(0,1)) = 0.5
		

		[HideInInspector] _SplatCount("_SplatCount", Int) = 0
		[NoScaleOffset] _ManualPaintedSplatMap0("_ManualPaintedSplatMap0", 2D) = "" {}
		[NoScaleOffset] _ManualPaintedSplatMap1("_ManualPaintedSplatMap1", 2D) = "" {}
		[NoScaleOffset] _ManualPaintedSplatMap2("_ManualPaintedSplatMap2", 2D) = "" {}
		[NoScaleOffset] _ManualPaintedSplatMap3("_ManualPaintedSplatMap3", 2D) = "" {}

		_TextureArraySplatmap("_TextureArraySplatmap", 2DArray) = "" {}
		[NoScaleOffset] _TextureArrayDiffuse("_TextureArrayDiffuse", 2DArray) = "" {}
		[NoScaleOffset] _TextureArrayNormal("_TextureArrayNormal", 2DArray) = "" {}
		[NoScaleOffset] _TextureArrayMOHS("_TextureArrayMOHS", 2DArray) = "" {}
	}

		SubShader
	{
		Tags
		{
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		CGPROGRAM

		#pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
		#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
		#pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
		#pragma target 3.0
		// needs more than 8 texcoords
		#pragma exclude_renderers gles
		#include "UnityPBSLighting.cginc"

		#pragma multi_compile __ _NORMALMAP

		#define TERRAIN_STANDARD_SHADER
		#define TERRAIN_INSTANCED_PERPIXEL_NORMAL
		#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
		#include "TerrainSplatmapCommonCustom.cginc"





		float _LerpingDistance;
		float _HeightBlendingTransition;
		float _TriplanarCutoffBias;
		float _UvScale;
		int _SplatCount;
		sampler2D _ManualPaintedSplatMap0;
		sampler2D _ManualPaintedSplatMap1;
		sampler2D _ManualPaintedSplatMap2;
		sampler2D _ManualPaintedSplatMap3;
		UNITY_DECLARE_TEX2DARRAY(_TextureArraySplatmap);
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayDiffuse);
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayNormal);
		UNITY_DECLARE_TEX2DARRAY(_TextureArrayMOHS);
		float4 _UvScaleArray[16];
		float _HeightArray[16];




		float4 GetManualPaintedSplatMapColor(float2 uv, int splatIndex)
		{
			float4 sampledSplatColor = 0;

			UNITY_BRANCH
			if (splatIndex == 0)
			{
				sampledSplatColor = tex2D(_ManualPaintedSplatMap0, uv);
			}
			else if (splatIndex == 1)
			{
				sampledSplatColor = tex2D(_ManualPaintedSplatMap1, uv);
			}
			else if (splatIndex == 2)
			{
				sampledSplatColor = tex2D(_ManualPaintedSplatMap2, uv);
			}
			else if (splatIndex == 3)
			{
				sampledSplatColor = tex2D(_ManualPaintedSplatMap3, uv);
			}
			
			return sampledSplatColor;
		}



		float CalculateUnpaintedWeight(Input IN)
		{
			float unpaintedWeight = 1;

			for (int i = 0; i < _SplatCount; i++)
			{
				int splatIndex = (int)floor(((float)i) / 4.0);
				float chanelIndex = fmod(((float)i),4);
				float4 sampledSplatColor = GetManualPaintedSplatMapColor(IN.uv_TextureArraySplatmap, splatIndex);

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





		float4 SampleSplatMap(Input IN, int splatIndex, float chanelIndex, float unpaintedWeight)
		{
			float sampledSplat;
			float4 proceduralSplatColor = 0;
			float4 manualSplatColor = 0;

			proceduralSplatColor = unpaintedWeight * UNITY_SAMPLE_TEX2DARRAY(_TextureArraySplatmap, float3(IN.uv_TextureArraySplatmap, splatIndex));
			manualSplatColor = GetManualPaintedSplatMapColor(IN.uv_TextureArraySplatmap, splatIndex);


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


		float4 LerpDiffuse(int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float2 uv)
		{
			float4 d0 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayDiffuse, float3(uv * uvScaleArrayValue.z, splatIndex));
			float4 d1 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayDiffuse, float3(uv, splatIndex));
			return lerp(d0, d1, lerpRatio); // pow(lerpRatio, 0.25 + lerpRatio * 2));
		}


		float3 LerpNormal(int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float2 uv, float normalScale)
		{
			float3 d0 = UnpackNormalWithScale(UNITY_SAMPLE_TEX2DARRAY(_TextureArrayNormal, float3(uv * uvScaleArrayValue.z, splatIndex)), normalScale);
			float3 d1 = UnpackNormalWithScale(UNITY_SAMPLE_TEX2DARRAY(_TextureArrayNormal, float3(uv , splatIndex)), normalScale);
			return lerp(d0, d1, lerpRatio); // pow(lerpRatio, 0.25 + lerpRatio * 2));
		}


		float4 LerpMOHS(int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float2 uv)
		{
			float4 d0 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayMOHS, float3(uv * uvScaleArrayValue.z, splatIndex));
			float4 d1 = UNITY_SAMPLE_TEX2DARRAY(_TextureArrayMOHS, float3(uv, splatIndex));
			return lerp(d0, d1, lerpRatio).z  ;   // pow(lerpRatio, 0.25 + lerpRatio * 2));
		}




		float4 GetBlendedDiffuse(Input IN, int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float4 triplanarWeight)
		{
			float4 sampledDiffuse = 0;
			UNITY_BRANCH
			if (clamp(lerpRatio - _TriplanarCutoffBias, -0.1, 1.0) < triplanarWeight.a)
			{
				float2 xUV = float2(0.25, 0.25) + IN.worldPos.zy / uvScaleArrayValue.x;
				float2 yUV = float2(0.5, 0.5) + IN.worldPos.xz / uvScaleArrayValue.x;
				float2 zUV = float2(0.75, 0.75) + IN.worldPos.xy / uvScaleArrayValue.x;

				float4 sampledDiffuseX = LerpDiffuse(splatIndex, lerpRatio, uvScaleArrayValue, xUV);
				float4 sampledDiffuseY = LerpDiffuse(splatIndex, lerpRatio, uvScaleArrayValue, yUV);
				float4 sampledDiffuseZ = LerpDiffuse(splatIndex, lerpRatio, uvScaleArrayValue, zUV);

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

		float3 GetBlendedNormal(Input IN, int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float3 normal, float normalScale, float4 triplanarWeight)
		{
			float3 sampledNormal = 0;
			UNITY_BRANCH
			if (clamp(lerpRatio - _TriplanarCutoffBias, -0.1, 1.0) < triplanarWeight.a)
			{
				float2 xUV = float2(0.25, 0.25) + IN.worldPos.zy / uvScaleArrayValue.x;
				float2 yUV = float2(0.5, 0.5) + IN.worldPos.xz / uvScaleArrayValue.x;
				float2 zUV = float2(0.75, 0.75) + IN.worldPos.xy / uvScaleArrayValue.x;

				float3 sampledNormalX = LerpNormal(splatIndex, lerpRatio, uvScaleArrayValue, xUV , normalScale);
				float3 sampledNormalY = LerpNormal(splatIndex, lerpRatio, uvScaleArrayValue, yUV , normalScale);
				float3 sampledNormalZ = LerpNormal(splatIndex, lerpRatio, uvScaleArrayValue, zUV , normalScale);

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


		float4 GetBlendedMOHS(Input IN, int splatIndex, float lerpRatio, float4 uvScaleArrayValue, float4 triplanarWeight)
		{
			float sampledMOHS = 0;
			UNITY_BRANCH
			if (clamp(lerpRatio - _TriplanarCutoffBias, -0.1, 1.0) < triplanarWeight.a)
			{
				float2 xUV = float2(0.25, 0.25) + IN.worldPos.zy / uvScaleArrayValue.x;
				float2 yUV = float2(0.5, 0.5) + IN.worldPos.xz / uvScaleArrayValue.x;
				float2 zUV = float2(0.75, 0.75) + IN.worldPos.xy / uvScaleArrayValue.x;

				float4 sampledMOHSX = LerpMOHS(splatIndex, lerpRatio, uvScaleArrayValue, xUV);
				float4 sampledMOHSY = LerpMOHS(splatIndex, lerpRatio, uvScaleArrayValue, yUV);
				float4 sampledMOHSZ = LerpMOHS(splatIndex, lerpRatio, uvScaleArrayValue, zUV);

				sampledMOHS =
					sampledMOHSX * triplanarWeight.x
					+ sampledMOHSY * triplanarWeight.y
					+ sampledMOHSZ * triplanarWeight.z;
			}
			else
			{
				sampledMOHS = LerpMOHS(splatIndex, lerpRatio, uvScaleArrayValue, IN.worldPos.xz / uvScaleArrayValue.x);
			}
			return sampledMOHS;
		}



		void MixSplat(Input IN, float3 terrainNormal, float lerpRatio, inout float4 diffuse, inout float3 normal, inout float metallic, inout float occlusion, inout float smoothness)
		{
			diffuse = 0;
			normal = 0;
			metallic = 0;
			occlusion = 0;
			smoothness = 0;

			float unpaintedWeight = CalculateUnpaintedWeight(IN);


			float3 blendWeights = pow(abs(terrainNormal) , 10.0);
			blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z + 0.001);

			float4 triplanarWeight = float4( blendWeights , saturate( dot( terrainNormal, float3(0,1,0) ) ) );


			float totalHeight = 0;
			for (int i = 0; i < _SplatCount; i++)
			{
				int splatIndex = (int)floor(((float)i) / 4.0);
				float chanelIndex = fmod(((float)i), 4);

				float sampledSplat = SampleSplatMap(IN, splatIndex, chanelIndex, unpaintedWeight);

				float4 mohs = GetBlendedMOHS(IN, i, lerpRatio, _UvScaleArray[i], triplanarWeight);
				_HeightArray[i] = sampledSplat * mohs.z ;
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

				float sampledSplat = _HeightArray[i].r;

				UNITY_BRANCH
				if (sampledSplat > 0)
				{
					diffuse += sampledSplat * GetBlendedDiffuse(IN, i, lerpRatio, _UvScaleArray[i], triplanarWeight);
					normal += sampledSplat * GetBlendedNormal(IN, i, lerpRatio, _UvScaleArray[i], terrainNormal, 1, triplanarWeight);
					float4 mohs = GetBlendedMOHS(IN, i, lerpRatio, _UvScaleArray[i], triplanarWeight);
					metallic += sampledSplat * mohs.x ;
					occlusion += sampledSplat * mohs.y ;
					smoothness += sampledSplat * mohs.w ;
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





		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float3 terrainNormal = 0;

			#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				terrainNormal = normalize(tex2D(_TerrainNormalmapTexture, IN.uv_TextureArraySplatmap).xyz * 2 - 1);
			#else
				terrainNormal = WorldNormalVector(IN, o.Normal);
			#endif


			float4 diffuse = 0;
			float3 normal = 0;
			float metallic = 0;
			float occlusion = 0;
			float smoothness = 0;

			float distanceVertexToCamera = length((float4(IN.worldPos, 0)) - _WorldSpaceCameraPos);
			float lerpRatio = smoothstep(1, _LerpingDistance, distanceVertexToCamera);


			MixSplat(IN, terrainNormal, lerpRatio, diffuse, normal, metallic, occlusion, smoothness);

			o.Albedo = diffuse;
			o.Normal = normal;
		//	o.Metallic = metallic;
			o.Occlusion = occlusion;
		//	o.Smoothness = smoothness;
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
