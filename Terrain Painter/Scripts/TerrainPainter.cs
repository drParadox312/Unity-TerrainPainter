using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TerrainPainter 
{
    public enum SplatType
    {
        Default = 0,
        Base = 1,
        Snow = 2
    };

    public enum PaintMethod
    {
        PaintOnUnpaintedAreaAndNotBlend = 0,
        PaintAndBlend = 1
    };

    public enum MapMaskEffect
    {
        Include = 0,
        Exclude = 1,
        Additive = 2
    };

    public enum TextureMapChannel
    {
        R = 0,
        G = 1,
        B = 2,
        A = 3
    };

    [System.Serializable]
    public struct SplatPaintRules
    {
        public float splatType;
	    public float paintMethod;

        public float useFlowMap;
	    public float useConvexityMap;
	    public float useConcavityMap;
        public float useAspectMap;
        public float useTextureMap;

        public float flowMapWeight;
        public float flowMapTransition;
        public float flowMapScale;
        public float flowMapHeightWeight;
        public float isInverseFlowMapHeightWeight;
	    public float flowMapSlopeWeight;
	    public float isInverseFlowMapSlopeWeight;
        public float flowMapEffect;

        public float convexityMapWeight;
        public float convexityMapTransition;
        public float convexityMapScale;
        public float convexityMapEffect;

        public float concavityMapWeight;
        public float concavityMapTransition;
        public float concavityMapScale;
        public float concavityMapEffect;

        public float aspectMapWeight;
        public float aspectMapPower;
	    public float aspectMapDirection;
        public float aspectMapEffect;

        public float textureMapWeight;
        public float textureMapChannel;
        public float textureMapResolution;
        public float textureMapEffect;

        public float heightMinStart;
        public float heightMinEnd;
        public float heightMaxStart;
        public float heightMaxEnd;
        public float heightTransitionFrequency;
        public float heightTransitionCutoff;

        public float slopeMinStart;
        public float slopeMinEnd;
        public float slopeMaxStart;
        public float slopeMaxEnd;
        public float slopeTransitionFrequency;
        public float slopeTransitionCutoff;

        public float snowAmount;
        public float snowTransitionSize;
        public float snowTransitionFrequency;
        public float snowTransitionCutoff;
    };

    [System.Serializable]
    public static class NameIDs
    {
        // kernels
        public static int Generate_Height_Map;
        public static int Generate_NeighborTerrain_Height_Map;
        public static int Generate_Slope_Map;
        public static int Generate_NeighborTerrain_Slope_Map;
        public static int Generate_SnowWeight_Map;
        public static int FlowMap_AddWater;
        public static int FlowMap_GenerateNeighborTerrainWaterMaps;
        public static int FlowMap_CalculateWaterOut;
        public static int FlowMap_MoveWater;
        public static int FlowMap_Generate;

        public static int CurvatureMap_FirstPass;
        public static int CurvatureMap_SecondPass;
        public static int CurvatureMap_Generate;

        public static int Generate_SplatMap;
        public static int Normalize_SplatMap;


        // genereted maps
        public static int unity_heightMap;
        public static int unity_normalMap;
        public static int height_slope_snowWeight_water_Maps_left;
        public static int height_slope_snowWeight_water_Maps_right;
        public static int height_slope_snowWeight_water_Maps_down;
        public static int height_slope_snowWeight_water_Maps_up;
        public static int neighbor_terrain_heightMaps;
        public static int neighbor_terrain_slopeMaps;
        public static int waterMap_left;
        public static int waterMap_up;
        public static int waterMap_right;
        public static int waterMap_down;
        public static int neighbor_terrain_waterMaps;
        public static int waterOutMap_this;
        public static int waterOutMap_left;
        public static int waterOutMap_up;
        public static int waterOutMap_right;
        public static int waterOutMap_down;
        public static int height_slope_snowWeight_water_Maps;
        public static int normal_Map;
        public static int convexity_concavitiy_flow_Maps;
        public static int convexity_concavitiy_flow_Maps_left;
        public static int convexity_concavitiy_flow_Maps_up;
        public static int convexity_concavitiy_flow_Maps_right;
        public static int convexity_concavitiy_flow_Maps_down;
        public static int convexity_concavitiy_flow_Maps_newCurvature; 
        public static int splatMapTotalWeight_Maps;
        public static int splatMapsArray;
        public static int textureMap;



        // buffers
        public static int splatPaintRulesBuffer;
        public static int splat_Map_Total_Weight_Buffer;



        // other parameters
        public static int useDrawInstanced;
        public static int terrainSize;
        public static int terrainPosition;
        public static int heightmapResolution;
        public static int splatRuleBufferIndex;
        public static int alphaMapResolution;
        public static int flowMapIteration;
        public static int hasNeighborTerrains;
        public static int cornerNeighborTerrainsHeights;
        public static int cornerNeighborTerrainsSlopes;


        public static void SetUpNameIDS(ComputeShader computeShader)
        {

            // kernels
            Generate_Height_Map = computeShader.FindKernel("Generate_Height_Map");
            Generate_NeighborTerrain_Height_Map = computeShader.FindKernel("Generate_NeighborTerrain_Height_Map");
            Generate_Slope_Map = computeShader.FindKernel("Generate_Slope_Map");
            Generate_NeighborTerrain_Slope_Map = computeShader.FindKernel("Generate_NeighborTerrain_Slope_Map");
            Generate_SnowWeight_Map = computeShader.FindKernel("Generate_SnowWeight_Map");
            FlowMap_AddWater = computeShader.FindKernel("FlowMap_AddWater");
            FlowMap_GenerateNeighborTerrainWaterMaps = computeShader.FindKernel("FlowMap_GenerateNeighborTerrainWaterMaps");
            FlowMap_CalculateWaterOut = computeShader.FindKernel("FlowMap_CalculateWaterOut");
            FlowMap_MoveWater = computeShader.FindKernel("FlowMap_MoveWater");
            FlowMap_Generate = computeShader.FindKernel("FlowMap_Generate");
            CurvatureMap_FirstPass = computeShader.FindKernel("CurvatureMap_FirstPass");
            CurvatureMap_SecondPass = computeShader.FindKernel("CurvatureMap_SecondPass");
            CurvatureMap_Generate = computeShader.FindKernel("CurvatureMap_Generate");
            Generate_SplatMap = computeShader.FindKernel("Generate_SplatMap");
            Normalize_SplatMap = computeShader.FindKernel("Normalize_SplatMap");



            // render textures
            unity_heightMap = Shader.PropertyToID("unity_heightMap");
            unity_normalMap = Shader.PropertyToID("unity_normalMap");
            height_slope_snowWeight_water_Maps_left = Shader.PropertyToID("height_slope_snowWeight_water_Maps_left");
            height_slope_snowWeight_water_Maps_right = Shader.PropertyToID("height_slope_snowWeight_water_Maps_right");
            height_slope_snowWeight_water_Maps_down = Shader.PropertyToID("height_slope_snowWeight_water_Maps_down");
            height_slope_snowWeight_water_Maps_up = Shader.PropertyToID("height_slope_snowWeight_water_Maps_up");
            neighbor_terrain_heightMaps = Shader.PropertyToID("neighbor_terrain_heightMaps");
            neighbor_terrain_slopeMaps = Shader.PropertyToID("neighbor_terrain_slopeMaps");
            waterMap_left = Shader.PropertyToID("waterMap_left");
            waterMap_up = Shader.PropertyToID("waterMap_up");
            waterMap_right = Shader.PropertyToID("waterMap_right");
            waterMap_down = Shader.PropertyToID("waterMap_down");
            neighbor_terrain_waterMaps = Shader.PropertyToID("neighbor_terrain_waterMaps");
            waterOutMap_this = Shader.PropertyToID("waterOutMap_this");
            waterOutMap_left = Shader.PropertyToID("waterOutMap_left");
            waterOutMap_up = Shader.PropertyToID("waterOutMap_up");
            waterOutMap_right = Shader.PropertyToID("waterOutMap_right");
            waterOutMap_down = Shader.PropertyToID("waterOutMap_down");
            height_slope_snowWeight_water_Maps = Shader.PropertyToID("height_slope_snowWeight_water_Maps");
            normal_Map = Shader.PropertyToID("normal_Map");
            convexity_concavitiy_flow_Maps = Shader.PropertyToID("convexity_concavitiy_flow_Maps");
            convexity_concavitiy_flow_Maps_left = Shader.PropertyToID("convexity_concavitiy_flow_Maps_left");
            convexity_concavitiy_flow_Maps_up = Shader.PropertyToID("convexity_concavitiy_flow_Maps_up");
            convexity_concavitiy_flow_Maps_right = Shader.PropertyToID("convexity_concavitiy_flow_Maps_right");
            convexity_concavitiy_flow_Maps_down = Shader.PropertyToID("convexity_concavitiy_flow_Maps_down");
            convexity_concavitiy_flow_Maps_newCurvature = Shader.PropertyToID("convexity_concavitiy_flow_Maps_newCurvature"); 
            splatMapTotalWeight_Maps = Shader.PropertyToID("splatMapTotalWeight_Maps");
            splatMapsArray = Shader.PropertyToID("splatMapsArray");
            textureMap = Shader.PropertyToID("textureMap");



            // buffers
            splatPaintRulesBuffer = Shader.PropertyToID("splatPaintRulesBuffer");
            splat_Map_Total_Weight_Buffer = Shader.PropertyToID("splat_Map_Total_Weight_Buffer");




            // other paramaters
            useDrawInstanced = Shader.PropertyToID("useDrawInstanced");
            terrainSize = Shader.PropertyToID("terrainSize");
            terrainPosition = Shader.PropertyToID("terrainPosition");
            heightmapResolution = Shader.PropertyToID("heightmapResolution");
            splatRuleBufferIndex = Shader.PropertyToID("splatRuleBufferIndex");
            alphaMapResolution = Shader.PropertyToID("alphaMapResolution");
            flowMapIteration = Shader.PropertyToID("flowMapIteration");
            hasNeighborTerrains = Shader.PropertyToID("hasNeighborTerrains");
            cornerNeighborTerrainsHeights = Shader.PropertyToID("cornerNeighborTerrainsHeights");
            cornerNeighborTerrainsSlopes = Shader.PropertyToID("cornerNeighborTerrainsSlopes");
        }
    }
}