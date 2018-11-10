using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TerrainPainter 
{
    public enum SplatType
    {
        Defalut = 0,
        Base = 1,
        Snow = 2
    };

    public enum PaintMethod
    {
        PaintOnUnpaintedAreaAndNotBlend = 0,
        PaintAndBlend = 1
    };

    [System.Serializable]
    public struct SplatPaintRules
    {
        public float flowMapWeight;
        public float convexityMapWeight;
        public float concavityMapWeight;

        public float maxHeight;
        public float minHeight;
        public float minHeightBias;

        public float maxSlope;
        public float minSlope;
        public float minSlopeBias;

        public float biasFrequency;
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

        public static int CurvatureMap_Generate;

        public static int Generate_SplatMap;
        public static int Normalize_SplatMap;


        // genereted maps
        public static int unity_heightMap;
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
        public static int splatMapTotalWeight_Maps;
        public static int splatMapsArray;




        // buffers
        public static int splatPaintRulesBuffer;
        public static int splat_Map_Total_Weight_Buffer;



        // other parameters
        public static int terrainSize;
        public static int terrainPosition;
        public static int terrainHeightMapResolution;
        public static int splatType;
        public static int paintMethod;
        public static int splatRuleBufferIndex;
        public static int alphaMapResolution;
        public static int snowAmount;
        public static int flowMapIteration;
        public static int convexityScale;
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
            CurvatureMap_Generate = computeShader.FindKernel("CurvatureMap_Generate");
            Generate_SplatMap = computeShader.FindKernel("Generate_SplatMap");
            Normalize_SplatMap = computeShader.FindKernel("Normalize_SplatMap");



            // render textures
            unity_heightMap = Shader.PropertyToID("unity_heightMap");
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
            splatMapTotalWeight_Maps = Shader.PropertyToID("splatMapTotalWeight_Maps");
            splatMapsArray = Shader.PropertyToID("splatMapsArray");




            // buffers
            splatPaintRulesBuffer = Shader.PropertyToID("splatPaintRulesBuffer");
            splat_Map_Total_Weight_Buffer = Shader.PropertyToID("splat_Map_Total_Weight_Buffer");




            // other paramaters
            terrainSize = Shader.PropertyToID("terrainSize");
            terrainPosition = Shader.PropertyToID("terrainPosition");
            terrainHeightMapResolution = Shader.PropertyToID("terrainHeightMapResolution");
            splatRuleBufferIndex = Shader.PropertyToID("splatRuleBufferIndex");
            splatType = Shader.PropertyToID("splatType");
            paintMethod = Shader.PropertyToID("paintMethod");
            alphaMapResolution = Shader.PropertyToID("alphaMapResolution");
            snowAmount = Shader.PropertyToID("snowAmount");
            flowMapIteration = Shader.PropertyToID("flowMapIteration");
            convexityScale = Shader.PropertyToID("convexityScale");
            hasNeighborTerrains = Shader.PropertyToID("hasNeighborTerrains");
            cornerNeighborTerrainsHeights = Shader.PropertyToID("cornerNeighborTerrainsHeights");
            cornerNeighborTerrainsSlopes = Shader.PropertyToID("cornerNeighborTerrainsSlopes");
        }
    }
}