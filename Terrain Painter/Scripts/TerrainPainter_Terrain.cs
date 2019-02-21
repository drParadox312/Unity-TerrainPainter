using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainPainter;




[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class TerrainPainter_Terrain : MonoBehaviour
{
    [HideInInspector]
    public bool hasTerrainHeigtmapChanged = false;


    public Terrain terrain;
    public ComputeShader computeShader;
    public TerrainPainter_Manager manager;
    public int terrainIndex = -1;


    public TerrainPainter_Terrain[] neighbor_terrains_scripts ;



    // genereted maps
    public RenderTexture renderTexture_unity_heightMap;
    public RenderTexture renderTexture_unity_normalMap;
    public RenderTexture renderTexture_neighbor_terrain_heightMaps;
    public RenderTexture renderTexture_neighbor_terrain_slopeMaps;
    public RenderTexture renderTexture_waterMap_left;
    public RenderTexture renderTexture_waterMap_up;
    public RenderTexture renderTexture_waterMap_right;
    public RenderTexture renderTexture_waterMap_down;
    public RenderTexture renderTexture_neighbor_terrain_waterMaps;
    public RenderTexture renderTexture_waterOutMap_this;
    public RenderTexture renderTexture_waterOutMap_left;
    public RenderTexture renderTexture_waterOutMap_up;
    public RenderTexture renderTexture_waterOutMap_right;
    public RenderTexture renderTexture_waterOutMap_down;
    public RenderTexture renderTexture_height_slope_snowWeight_water_Maps;
    public RenderTexture renderTexture_convexity_concavitiy_flow_Maps;
    public RenderTexture renderTexture_convexity_concavitiy_flow_Maps_newCurvature;
    public RenderTexture renderTexture_triplanarWeightMap;





    // other paaramters
    public int extraThread = 8;
    public int hm_x;  // heightMap_Width
    public int hm_y;  // heightMap_Height
    public Vector4 terrainSize;
    public Vector4 terrainPosition;
    public int heightmapResolution;
    public int alphamapResolution;
    public int am_x;            //  alphaMap_Width ;
    public int am_y;            //  alphaMap_Height

    public Vector4 hasNeighborTerrains;
    public Vector4 cornerNeighborTerrainsHeights;
    public Vector4 cornerNeighborTerrainsSlopes;



    // paint rules buffer
    public ComputeBuffer splatPaintRulesBuffer;



    // splatMap output
    private float[] splat_Map_Total_Weight;
    public ComputeBuffer splat_Map_Total_Weight_Buffer;
    public RenderTexture[] splatMapsArray;

    public int flowMapIteration = 10;



    public Material customTerrainMaterial ;
 //   public Texture2DArray texture2DArray_manualPainted ;
    public Texture2DArray texture2DArray_splat ;
    public RenderTexture colorMapDiffuse ;
    public RenderTexture colorMapNormal ;




    //  FUNCTIONS


    [ExecuteInEditMode]
    void OnTerrainChanged(TerrainChangedFlags flag)
    {
        if (flag == TerrainChangedFlags.DelayedHeightmapUpdate)
        {
            hasTerrainHeigtmapChanged = true;
            manager.TerrainHeightmapChanged(terrainIndex) ;
        }
    }




    public void SetUpProperties()
    {
        terrain = this.GetComponent<Terrain>();
        computeShader = manager.computeShader;

        SetUpTerrainMaterial();

        neighbor_terrains_scripts = new TerrainPainter_Terrain[4] ;

        if(terrain.leftNeighbor)
            neighbor_terrains_scripts[0] = terrain.leftNeighbor.GetComponent<TerrainPainter_Terrain>();
        if(terrain.topNeighbor)
            neighbor_terrains_scripts[1] = terrain.topNeighbor.GetComponent<TerrainPainter_Terrain>();
        if(terrain.rightNeighbor)
            neighbor_terrains_scripts[2] = terrain.rightNeighbor.GetComponent<TerrainPainter_Terrain>();
        if(terrain.bottomNeighbor)
            neighbor_terrains_scripts[3] = terrain.bottomNeighbor.GetComponent<TerrainPainter_Terrain>();

        SetUpTerrainParameters();
        SetUpTerrainLayers();
        SetUpSplatMapArray();
        SetUpTextures();
    }



    public void SetUpTerrainMaterial()
    {
        if(manager.customTerrainMaterial)
        {
            terrain.materialType = Terrain.MaterialType.Custom ;
            customTerrainMaterial = (Material)Instantiate(manager.customTerrainMaterial);
        //    customTerrainMaterial = manager.customTerrainMaterial;
            terrain.materialTemplate = customTerrainMaterial ;
        }
        else
        {
            terrain.materialType = Terrain.MaterialType.BuiltInStandard ;
        }
    }


    void SetUpTerrainParameters()
    {
        hm_x = terrain.terrainData.heightmapWidth ;
        hm_y = terrain.terrainData.heightmapHeight ;

        am_x = terrain.terrainData.alphamapWidth ;
        am_y = terrain.terrainData.alphamapHeight ;

        terrainSize = (Vector4)(terrain.terrainData.size);
        terrainPosition = (Vector4)(this.transform.position);
        heightmapResolution = terrain.terrainData.heightmapResolution ;
        alphamapResolution = terrain.terrainData.alphamapResolution;

        hasNeighborTerrains = new Vector4
            (
            (terrain.leftNeighbor != null ? 1f : -1f),
            (terrain.topNeighbor != null ? 1f : -1f),
            (terrain.rightNeighbor != null ? 1f : -1f),
            (terrain.bottomNeighbor != null ? 1f : -1f)
            );
    }




    public void SetUpTerrainLayers()
    {
        terrain.terrainData.terrainLayers = manager.terrainLayers;

        Vector4[] _UvScaleArray = new Vector4[16] ;
        for(int i=0; i < terrain.terrainData.terrainLayers.Length; i++)
        {
            _UvScaleArray[i] = new Vector4(terrain.terrainData.terrainLayers[i].tileSize.x, terrain.terrainData.terrainLayers[i].tileSize.y, terrain.terrainData.terrainLayers[i].tileOffset.x, terrain.terrainData.terrainLayers[i].tileOffset.y) ;
        }


        customTerrainMaterial.SetInt(NameIDs._SplatCount, terrain.terrainData.terrainLayers.Length);
        customTerrainMaterial.SetVectorArray(NameIDs._UvScaleArray, _UvScaleArray);

    }



    public void SetUpSplatMapArray()
    {
        splatMapsArray = new RenderTexture[Mathf.CeilToInt(((float)terrain.terrainData.terrainLayers.Length) / 4f)];
        for (int i = 0; i < splatMapsArray.Length; i++)
        {
            splatMapsArray[i] = new RenderTexture(am_x, am_y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            splatMapsArray[i].enableRandomWrite = true;
            splatMapsArray[i].Create();
        }
    }





    void SetUpSplatPaintRulesBuffer()
    {
        if (splatPaintRulesBuffer != null)
            splatPaintRulesBuffer.Release();


        splatPaintRulesBuffer = new ComputeBuffer(manager.splatPaintRulesArray.Length, 47 * sizeof(float));
    }


    void SetUpSplatmapTotalWeightBuffer()
    {
          splat_Map_Total_Weight = new float[am_x * am_y];

        if (splat_Map_Total_Weight_Buffer != null)
            splat_Map_Total_Weight_Buffer.Release();

        splat_Map_Total_Weight_Buffer = new ComputeBuffer(splat_Map_Total_Weight.Length, sizeof(float));
    }


    void SetUpTextures()
    {
        renderTexture_unity_heightMap = CreateRenderTexture(heightmapResolution , terrain.terrainData.heightmapTexture.format);
        renderTexture_height_slope_snowWeight_water_Maps = CreateRenderTexture(heightmapResolution);
        renderTexture_convexity_concavitiy_flow_Maps = CreateRenderTexture(heightmapResolution);
        renderTexture_convexity_concavitiy_flow_Maps_newCurvature = CreateRenderTexture(heightmapResolution);
        renderTexture_neighbor_terrain_heightMaps = CreateRenderTexture(heightmapResolution);
        renderTexture_neighbor_terrain_slopeMaps = CreateRenderTexture(heightmapResolution);
        renderTexture_triplanarWeightMap = CreateRenderTexture(heightmapResolution);

        if (terrain.drawInstanced)
            renderTexture_unity_normalMap = CopyRenderTexture(terrain.normalmapTexture, terrain.normalmapTexture.format) ;
        else
            renderTexture_unity_normalMap = CreateRenderTexture(heightmapResolution) ;


        colorMapDiffuse = CreateRenderTexture(alphamapResolution);
        colorMapNormal = CreateRenderTexture(alphamapResolution);
    }


















    RenderTexture CreateRenderTexture(int p_size)
    {
        RenderTexture _rT = new RenderTexture(terrain.terrainData.heightmapTexture.descriptor);
        _rT.width = p_size;
        _rT.height = p_size;
        _rT.format = RenderTextureFormat.ARGB64;
        _rT.enableRandomWrite = true;
        _rT.Create();
        return _rT;
    }

    RenderTexture CreateRenderTexture(int p_size, RenderTextureFormat p_format)
    {
        RenderTexture _rT = new RenderTexture(terrain.terrainData.heightmapTexture.descriptor);
        _rT.width = p_size;
        _rT.height = p_size;
        _rT.format = p_format;
        _rT.enableRandomWrite = true;
        _rT.Create();
        return _rT;
    }



    RenderTexture CopyHeightmapRenderTexture(RenderTexture p_renderTexture)
    {
        if (p_renderTexture)
        {
            RenderTexture _rT = new RenderTexture(p_renderTexture.descriptor);
            _rT.width = p_renderTexture.width ;
            _rT.height = p_renderTexture.height ;
            _rT.enableRandomWrite = true;
            _rT.Create();
            Graphics.Blit(p_renderTexture, _rT);
            return _rT;
        }
        else
        {
            RenderTexture _rT = new RenderTexture(terrain.terrainData.heightmapTexture.descriptor);
            _rT.enableRandomWrite = true;
            _rT.Create();
            return _rT;
        }
    }


    
    RenderTexture CopyRenderTexture(RenderTexture p_renderTexture, RenderTextureFormat p_format)
    {
        RenderTexture _rT = new RenderTexture(p_renderTexture.descriptor);
        _rT.format = p_format ;
        _rT.width = p_renderTexture.width ;
        _rT.height = p_renderTexture.height ;
        _rT.enableRandomWrite = true;
        _rT.Create();
        Graphics.Blit(p_renderTexture, _rT);
        return _rT;
    }


    


















    void AssignBuffersAndParametersFor_Generate_Height_Map_Kernel()
    {
        Graphics.Blit(terrain.terrainData.heightmapTexture, renderTexture_unity_heightMap);

        computeShader.SetTexture(NameIDs.Generate_Height_Map, NameIDs.unity_heightMap, renderTexture_unity_heightMap);
        computeShader.SetTexture(NameIDs.Generate_Height_Map, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);




        float _corner_height_x, _corner_height_y, _corner_height_z, _corner_height_w = 0f ;
        float _corner_slope_x, _corner_slope_y, _corner_slope_z, _corner_slope_w = 0f ;

        _corner_height_x = terrain.terrainData.GetHeight(1, hm_y -1);
        _corner_height_y = terrain.terrainData.GetHeight(hm_x -1, hm_y -1);
        _corner_height_z = terrain.terrainData.GetHeight(hm_x -1, 1);
        _corner_height_w = terrain.terrainData.GetHeight(1, 1);


        float _index00 = 1f / (float)hm_x ;
        float _index11 = (float)(hm_x -1) / (float)hm_x ;

        _corner_slope_x = terrain.terrainData.GetSteepness(_index00, _index11);
        _corner_slope_y = terrain.terrainData.GetSteepness(_index11, _index11);
        _corner_slope_z = terrain.terrainData.GetSteepness(_index11, _index00);
        _corner_slope_w = terrain.terrainData.GetSteepness(_index00, _index00);



        if (terrain.leftNeighbor)
        {
            if (terrain.leftNeighbor.topNeighbor)
            {
                _corner_height_x = terrain.leftNeighbor.topNeighbor.terrainData.GetHeight(terrain.leftNeighbor.topNeighbor.terrainData.heightmapWidth - 1, 0);
                _corner_slope_x = terrain.leftNeighbor.topNeighbor.terrainData.GetSteepness((float)(terrain.leftNeighbor.topNeighbor.terrainData.heightmapWidth - 1) / (float)terrain.leftNeighbor.topNeighbor.terrainData.heightmapResolution, 0f);
            }

            if (terrain.leftNeighbor.bottomNeighbor)
            {
                _corner_height_w = terrain.leftNeighbor.bottomNeighbor.terrainData.GetHeight(terrain.leftNeighbor.bottomNeighbor.terrainData.heightmapWidth - 1, terrain.leftNeighbor.bottomNeighbor.terrainData.heightmapHeight - 1);
                _corner_slope_w = terrain.leftNeighbor.bottomNeighbor.terrainData.GetSteepness((float)(terrain.leftNeighbor.bottomNeighbor.terrainData.heightmapWidth - 1) / (float)terrain.leftNeighbor.bottomNeighbor.terrainData.heightmapResolution, (float)(terrain.leftNeighbor.bottomNeighbor.terrainData.heightmapHeight - 1)/ (float)terrain.leftNeighbor.bottomNeighbor.terrainData.heightmapResolution );
            }
        }
        if (terrain.topNeighbor)
        {
            if (terrain.topNeighbor.leftNeighbor)
            {
                _corner_height_x = terrain.topNeighbor.leftNeighbor.terrainData.GetHeight(terrain.topNeighbor.leftNeighbor.terrainData.heightmapWidth - 1, 0);
                _corner_slope_x = terrain.topNeighbor.leftNeighbor.terrainData.GetSteepness((float)(terrain.topNeighbor.leftNeighbor.terrainData.heightmapWidth - 1) / (float)terrain.topNeighbor.leftNeighbor.terrainData.heightmapResolution, 0f);
            }

            if (terrain.topNeighbor.rightNeighbor)
            {
                _corner_height_y = terrain.topNeighbor.rightNeighbor.terrainData.GetHeight(0, 0);
                _corner_slope_y = terrain.topNeighbor.rightNeighbor.terrainData.GetSteepness(0f, 0f);
            }
        }
        if (terrain.rightNeighbor)
        {
            if (terrain.rightNeighbor.topNeighbor)
            {
                _corner_height_y = terrain.rightNeighbor.topNeighbor.terrainData.GetHeight(terrain.rightNeighbor.topNeighbor.terrainData.heightmapWidth - 1, 0);
                _corner_slope_y = terrain.rightNeighbor.topNeighbor.terrainData.GetSteepness((float)(terrain.rightNeighbor.topNeighbor.terrainData.heightmapWidth - 1) / (float)terrain.rightNeighbor.topNeighbor.terrainData.heightmapResolution, 0f);
            }

            if (terrain.rightNeighbor.bottomNeighbor)
            {
                _corner_height_z = terrain.rightNeighbor.bottomNeighbor.terrainData.GetHeight(0, terrain.rightNeighbor.bottomNeighbor.terrainData.heightmapHeight - 1);
                _corner_slope_z = terrain.rightNeighbor.bottomNeighbor.terrainData.GetSteepness(0f, (float)(terrain.rightNeighbor.bottomNeighbor.terrainData.heightmapHeight - 1)/(float)terrain.rightNeighbor.bottomNeighbor.terrainData.heightmapResolution);
            }
        }
        if (terrain.bottomNeighbor)
        {
            if(terrain.bottomNeighbor.rightNeighbor)
            {
                _corner_height_z = terrain.bottomNeighbor.rightNeighbor.terrainData.GetHeight(0, terrain.bottomNeighbor.rightNeighbor.terrainData.heightmapHeight - 1);
                _corner_slope_z = terrain.bottomNeighbor.rightNeighbor.terrainData.GetSteepness(0f, (float)(terrain.bottomNeighbor.rightNeighbor.terrainData.heightmapHeight - 1) / (float)terrain.bottomNeighbor.rightNeighbor.terrainData.heightmapResolution);
            }

            if(terrain.bottomNeighbor.leftNeighbor)
            {
                _corner_height_w = terrain.bottomNeighbor.leftNeighbor.terrainData.GetHeight(terrain.bottomNeighbor.leftNeighbor.terrainData.heightmapWidth - 1, terrain.bottomNeighbor.leftNeighbor.terrainData.heightmapHeight - 1);
                _corner_slope_w = terrain.bottomNeighbor.leftNeighbor.terrainData.GetSteepness((float)(terrain.bottomNeighbor.leftNeighbor.terrainData.heightmapWidth - 1) / (float)terrain.bottomNeighbor.leftNeighbor.terrainData.heightmapResolution, (float)(terrain.bottomNeighbor.leftNeighbor.terrainData.heightmapHeight - 1) / (float)terrain.bottomNeighbor.leftNeighbor.terrainData.heightmapResolution);
            }
        }


        cornerNeighborTerrainsHeights = new Vector4(_corner_height_x, _corner_height_y, _corner_height_z, _corner_height_w) ;
        cornerNeighborTerrainsSlopes = new Vector4(_corner_slope_x, _corner_slope_y, _corner_slope_z, _corner_slope_w) ;
        

        computeShader.SetVector(NameIDs.terrainSize, terrainSize);
        computeShader.SetVector(NameIDs.terrainPosition, terrainPosition);
        computeShader.SetInt(NameIDs.heightmapResolution, heightmapResolution);
        computeShader.SetInt(NameIDs.alphaMapResolution, alphamapResolution);
        computeShader.SetVector(NameIDs.hasNeighborTerrains, hasNeighborTerrains);
        computeShader.SetVector(NameIDs.cornerNeighborTerrainsHeights, cornerNeighborTerrainsHeights);
        computeShader.SetVector(NameIDs.cornerNeighborTerrainsSlopes, cornerNeighborTerrainsSlopes);
    }



    public void Generate_Height_Map()
    {
        AssignBuffersAndParametersFor_Generate_Height_Map_Kernel();

        computeShader.Dispatch(NameIDs.Generate_Height_Map, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }



















    void AssignBuffersAndParametersFor_Generate_NeighborTerrain_Height_Map_Kernel()
    {


        if (terrain.leftNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_left, CopyHeightmapRenderTexture(neighbor_terrains_scripts[0].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_left, CopyHeightmapRenderTexture(null));


        if (terrain.rightNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_right, CopyHeightmapRenderTexture(neighbor_terrains_scripts[2].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_right, CopyHeightmapRenderTexture(null));


        if (terrain.bottomNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_down, CopyHeightmapRenderTexture(neighbor_terrains_scripts[3].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_down, CopyHeightmapRenderTexture(null));


        if (terrain.topNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_up, CopyHeightmapRenderTexture(neighbor_terrains_scripts[1].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.height_slope_snowWeight_water_Maps_up, CopyHeightmapRenderTexture(null));




        computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Height_Map, NameIDs.neighbor_terrain_heightMaps, renderTexture_neighbor_terrain_heightMaps);
    }






    public void Generate_NeighborTerrain_Height_Map()
    {
        AssignBuffersAndParametersFor_Generate_NeighborTerrain_Height_Map_Kernel();

        computeShader.Dispatch(NameIDs.Generate_NeighborTerrain_Height_Map, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }










    void AssignBuffersAndParametersFor_Generate_NeighborTerrain_Slope_Map_Kernel()
    {


        if (terrain.leftNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_left, CopyHeightmapRenderTexture(neighbor_terrains_scripts[0].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_left, CopyHeightmapRenderTexture(null));


        if (terrain.rightNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_right, CopyHeightmapRenderTexture(neighbor_terrains_scripts[2].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_right, CopyHeightmapRenderTexture(null));


        if (terrain.bottomNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_down, CopyHeightmapRenderTexture(neighbor_terrains_scripts[3].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_down, CopyHeightmapRenderTexture(null));


        if (terrain.topNeighbor)
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_up, CopyHeightmapRenderTexture(neighbor_terrains_scripts[1].renderTexture_height_slope_snowWeight_water_Maps));
        else
            computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps_up, CopyHeightmapRenderTexture(null));




        computeShader.SetTexture(NameIDs.Generate_NeighborTerrain_Slope_Map, NameIDs.neighbor_terrain_slopeMaps, renderTexture_neighbor_terrain_slopeMaps);
    }



    public void Generate_NeighborTerrain_Slope_Map()
    {
        AssignBuffersAndParametersFor_Generate_NeighborTerrain_Slope_Map_Kernel();

        computeShader.Dispatch(NameIDs.Generate_NeighborTerrain_Slope_Map, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }



















    void AssignBuffersAndParametersFor_Generate_Slope_Map_Kernel()
    {
        computeShader.SetTexture(NameIDs.Generate_Slope_Map, NameIDs.unity_normalMap, renderTexture_unity_normalMap);
        computeShader.SetTexture(NameIDs.Generate_Slope_Map, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
        computeShader.SetTexture(NameIDs.Generate_Slope_Map, NameIDs.neighbor_terrain_heightMaps, renderTexture_neighbor_terrain_heightMaps);
        computeShader.SetTexture(NameIDs.Generate_Slope_Map, NameIDs.triplanarWeightMap, renderTexture_triplanarWeightMap); 
    }


    public void Generate_Slope_Map()
    {
        AssignBuffersAndParametersFor_Generate_Slope_Map_Kernel();

        computeShader.Dispatch(NameIDs.Generate_Slope_Map, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }












    void AssignBuffersAndParametersFor_Generate_SnowWeight_Map_Kernel()
    {
        computeShader.SetTexture(NameIDs.Generate_SnowWeight_Map, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
    }


    public void Generate_SnowWeight_Maps()
    {
        AssignBuffersAndParametersFor_Generate_SnowWeight_Map_Kernel();

        computeShader.Dispatch(NameIDs.Generate_SnowWeight_Map, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }












    void AssignBuffersAndParametersFor_FlowMap_AddWater_Kernel()
    {
        renderTexture_waterOutMap_this = CreateRenderTexture(heightmapResolution);

        computeShader.SetInt(NameIDs.flowMapIteration, flowMapIteration);

        computeShader.SetTexture(NameIDs.FlowMap_AddWater, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
    }

    public void FlowMap_AddWater()
    {
        flowMapIteration = manager.flowMapIteration;

        AssignBuffersAndParametersFor_FlowMap_AddWater_Kernel();

        computeShader.Dispatch(NameIDs.FlowMap_AddWater, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }











    void AssignBuffersAndParametersFor_FlowMap_GenerateNeighborTerrainWaterMaps()
    {

        renderTexture_neighbor_terrain_waterMaps = CreateRenderTexture(heightmapResolution, RenderTextureFormat.ARGB64);

        renderTexture_waterMap_left = (terrain.leftNeighbor != null ? neighbor_terrains_scripts[0].renderTexture_height_slope_snowWeight_water_Maps : CreateRenderTexture(hm_x));
        renderTexture_waterMap_up = (terrain.topNeighbor != null ? neighbor_terrains_scripts[1].renderTexture_height_slope_snowWeight_water_Maps : CreateRenderTexture(hm_x));
        renderTexture_waterMap_right = (terrain.rightNeighbor != null ? neighbor_terrains_scripts[2].renderTexture_height_slope_snowWeight_water_Maps : CreateRenderTexture(hm_x));
        renderTexture_waterMap_down = (terrain.bottomNeighbor != null ? neighbor_terrains_scripts[3].renderTexture_height_slope_snowWeight_water_Maps : CreateRenderTexture(hm_x));


        computeShader.SetTexture(NameIDs.FlowMap_GenerateNeighborTerrainWaterMaps, NameIDs.waterMap_left, renderTexture_waterMap_left);
        computeShader.SetTexture(NameIDs.FlowMap_GenerateNeighborTerrainWaterMaps, NameIDs.waterMap_up, renderTexture_waterMap_up);
        computeShader.SetTexture(NameIDs.FlowMap_GenerateNeighborTerrainWaterMaps, NameIDs.waterMap_right, renderTexture_waterMap_right);
        computeShader.SetTexture(NameIDs.FlowMap_GenerateNeighborTerrainWaterMaps, NameIDs.waterMap_down, renderTexture_waterMap_down);
        computeShader.SetTexture(NameIDs.FlowMap_GenerateNeighborTerrainWaterMaps, NameIDs.neighbor_terrain_waterMaps, renderTexture_neighbor_terrain_waterMaps);

    }


    public void FlowMap_GenerateNeighborTerrainWaterMaps()
    {
        AssignBuffersAndParametersFor_FlowMap_GenerateNeighborTerrainWaterMaps();

        computeShader.Dispatch(NameIDs.FlowMap_GenerateNeighborTerrainWaterMaps, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }













    void AssignBuffersAndParametersFor_FlowMap_CalculateWaterOut_Kernel()
    {

        renderTexture_waterOutMap_left = (terrain.leftNeighbor != null ? neighbor_terrains_scripts[0].renderTexture_waterOutMap_this : CreateRenderTexture(heightmapResolution));
        renderTexture_waterOutMap_up = (terrain.topNeighbor != null ? neighbor_terrains_scripts[1].renderTexture_waterOutMap_this : CreateRenderTexture(heightmapResolution));
        renderTexture_waterOutMap_right = (terrain.rightNeighbor != null ? neighbor_terrains_scripts[2].renderTexture_waterOutMap_this : CreateRenderTexture(heightmapResolution));
        renderTexture_waterOutMap_down = (terrain.bottomNeighbor != null ? neighbor_terrains_scripts[3].renderTexture_waterOutMap_this : CreateRenderTexture(heightmapResolution));


        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.neighbor_terrain_heightMaps, renderTexture_neighbor_terrain_heightMaps);
        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.neighbor_terrain_waterMaps, renderTexture_neighbor_terrain_waterMaps);
        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.waterOutMap_this, renderTexture_waterOutMap_this);
        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.waterOutMap_left, renderTexture_waterOutMap_left);
        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.waterOutMap_up, renderTexture_waterOutMap_up);
        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.waterOutMap_right, renderTexture_waterOutMap_right);
        computeShader.SetTexture(NameIDs.FlowMap_CalculateWaterOut, NameIDs.waterOutMap_down, renderTexture_waterOutMap_down);

    }


    public void FlowMap_CalculateWaterOut()
    {
        AssignBuffersAndParametersFor_FlowMap_CalculateWaterOut_Kernel();

        computeShader.Dispatch(NameIDs.FlowMap_CalculateWaterOut, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }











    void AssignBuffersAndParametersFor_FlowMap_MoveWater_Kernel()
    {
        computeShader.SetTexture(NameIDs.FlowMap_MoveWater, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
        computeShader.SetTexture(NameIDs.FlowMap_MoveWater, NameIDs.neighbor_terrain_waterMaps, renderTexture_neighbor_terrain_waterMaps);
        computeShader.SetTexture(NameIDs.FlowMap_MoveWater, NameIDs.waterOutMap_this, renderTexture_waterOutMap_this);
        computeShader.SetTexture(NameIDs.FlowMap_MoveWater, NameIDs.waterOutMap_left, renderTexture_waterOutMap_left);
        computeShader.SetTexture(NameIDs.FlowMap_MoveWater, NameIDs.waterOutMap_up, renderTexture_waterOutMap_up);
        computeShader.SetTexture(NameIDs.FlowMap_MoveWater, NameIDs.waterOutMap_right, renderTexture_waterOutMap_right);
        computeShader.SetTexture(NameIDs.FlowMap_MoveWater, NameIDs.waterOutMap_down, renderTexture_waterOutMap_down);
    }

    public void FlowMap_MoveWater()
    {
        AssignBuffersAndParametersFor_FlowMap_MoveWater_Kernel();

        computeShader.Dispatch(NameIDs.FlowMap_MoveWater, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }












    void AssignBuffersAndParametersFor_FlowMap_Generate_Kernel()
    {
        computeShader.SetTexture(NameIDs.FlowMap_Generate, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
        computeShader.SetTexture(NameIDs.FlowMap_Generate, NameIDs.convexity_concavitiy_flow_Maps, renderTexture_convexity_concavitiy_flow_Maps);
        computeShader.SetTexture(NameIDs.FlowMap_Generate, NameIDs.waterOutMap_this, renderTexture_waterOutMap_this);
        computeShader.SetTexture(NameIDs.FlowMap_Generate, NameIDs.waterOutMap_left, renderTexture_waterOutMap_left);
        computeShader.SetTexture(NameIDs.FlowMap_Generate, NameIDs.waterOutMap_up, renderTexture_waterOutMap_up);
        computeShader.SetTexture(NameIDs.FlowMap_Generate, NameIDs.waterOutMap_right, renderTexture_waterOutMap_right);
        computeShader.SetTexture(NameIDs.FlowMap_Generate, NameIDs.waterOutMap_down, renderTexture_waterOutMap_down);
    }

    public void FlowMap_Generate()
    {
        AssignBuffersAndParametersFor_FlowMap_Generate_Kernel();

        computeShader.Dispatch(NameIDs.FlowMap_Generate, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }



    public void ReleaseRenderTexturesForWaterMap()
    {
        renderTexture_neighbor_terrain_waterMaps.Release();

        renderTexture_waterOutMap_this.Release();
        renderTexture_waterOutMap_up.Release();
        renderTexture_waterOutMap_right.Release();
        renderTexture_waterOutMap_down.Release();  
    }













    void AssignBuffersAndParametersFor_CurvatureMap_FirstPass_Kernel()
    {
        computeShader.SetTexture(NameIDs.CurvatureMap_FirstPass, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
        computeShader.SetTexture(NameIDs.CurvatureMap_FirstPass, NameIDs.neighbor_terrain_heightMaps, renderTexture_neighbor_terrain_heightMaps);
        computeShader.SetTexture(NameIDs.CurvatureMap_FirstPass, NameIDs.convexity_concavitiy_flow_Maps, renderTexture_convexity_concavitiy_flow_Maps);
    }


    public void CurvatureMap_FirstPass()
    {
        AssignBuffersAndParametersFor_CurvatureMap_FirstPass_Kernel();

        computeShader.Dispatch(NameIDs.CurvatureMap_FirstPass, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }









    void AssignBuffersAndParametersFor_CurvatureMap_SecondPass_Kernel()
    {
        if (terrain.leftNeighbor)
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_left, neighbor_terrains_scripts[0].renderTexture_convexity_concavitiy_flow_Maps);
        else
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_left, CopyHeightmapRenderTexture(null));


        if (terrain.rightNeighbor)
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_right, neighbor_terrains_scripts[2].renderTexture_convexity_concavitiy_flow_Maps);
        else
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_right, CopyHeightmapRenderTexture(null));


        if (terrain.bottomNeighbor)
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_down, neighbor_terrains_scripts[3].renderTexture_convexity_concavitiy_flow_Maps);
        else
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_down, CopyHeightmapRenderTexture(null));


        if (terrain.topNeighbor)
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_up, neighbor_terrains_scripts[1].renderTexture_convexity_concavitiy_flow_Maps);
        else
            computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_up, CopyHeightmapRenderTexture(null));


        computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps, renderTexture_convexity_concavitiy_flow_Maps);
        computeShader.SetTexture(NameIDs.CurvatureMap_SecondPass, NameIDs.convexity_concavitiy_flow_Maps_newCurvature, renderTexture_convexity_concavitiy_flow_Maps_newCurvature);
    }


    public void CurvatureMap_SecondPass()
    {
        AssignBuffersAndParametersFor_CurvatureMap_SecondPass_Kernel();

        computeShader.Dispatch(NameIDs.CurvatureMap_SecondPass, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }










    void AssignBuffersAndParametersFor_CurvatureMap_Generate_Kernel()
    {
        computeShader.SetTexture(NameIDs.CurvatureMap_Generate, NameIDs.convexity_concavitiy_flow_Maps_newCurvature, renderTexture_convexity_concavitiy_flow_Maps_newCurvature);
        computeShader.SetTexture(NameIDs.CurvatureMap_Generate, NameIDs.convexity_concavitiy_flow_Maps, renderTexture_convexity_concavitiy_flow_Maps);
    }


    public void CurvatureMap_Generate()
    {
        AssignBuffersAndParametersFor_CurvatureMap_Generate_Kernel();

        computeShader.Dispatch(NameIDs.CurvatureMap_Generate, (hm_x + extraThread) / 8, (hm_y + extraThread) / 8, 1);
    }














    void AssignBuffersAndParametersFor_Generate_SplatMap_Kernel()
    {
        computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.height_slope_snowWeight_water_Maps, renderTexture_height_slope_snowWeight_water_Maps);
        computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.neighbor_terrain_heightMaps, renderTexture_neighbor_terrain_heightMaps);
        computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.neighbor_terrain_slopeMaps, renderTexture_neighbor_terrain_slopeMaps);
        computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.convexity_concavitiy_flow_Maps, renderTexture_convexity_concavitiy_flow_Maps);
        computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.unity_normalMap, renderTexture_unity_normalMap);


        splatPaintRulesBuffer.SetData(manager.splatPaintRulesArray);
        computeShader.SetBuffer(NameIDs.Generate_SplatMap, NameIDs.splatPaintRulesBuffer, splatPaintRulesBuffer);


        splat_Map_Total_Weight_Buffer.SetData(splat_Map_Total_Weight);
        computeShader.SetBuffer(NameIDs.Generate_SplatMap, NameIDs.splat_Map_Total_Weight_Buffer, splat_Map_Total_Weight_Buffer);
    }




    public void Generate_SplatMap()
    {
        if( manager.splats.Length > 0)
        {
            SetUpSplatPaintRulesBuffer();
            

            SetUpSplatmapTotalWeightBuffer();
            

            AssignBuffersAndParametersFor_Generate_SplatMap_Kernel();


            for (int i = 0; i <  manager.splats.Length; i++)
            {
                ExecuteComputeShader(i);
            }
        }
    }



    void ExecuteComputeShader(int p_splatMapArrrayIndex)
    {
        if (p_splatMapArrrayIndex >= 0)
        {
            computeShader.SetInt(NameIDs.splatRuleBufferIndex, p_splatMapArrrayIndex);
            computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.splatMapsArray, splatMapsArray[Mathf.FloorToInt(((float)p_splatMapArrrayIndex) / 4)]);

            if(manager.splats[p_splatMapArrrayIndex].textureMap)
                computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.textureMap, manager.splats[p_splatMapArrrayIndex].textureMap);
            else
                computeShader.SetTexture(NameIDs.Generate_SplatMap, NameIDs.textureMap, CreateRenderTexture(hm_x));

            computeShader.Dispatch(NameIDs.Generate_SplatMap, am_x / 8, am_y / 8, 1);

            splat_Map_Total_Weight_Buffer.GetData(splat_Map_Total_Weight);
        }
    }














    void AssignBuffersAndParametersFor_Normalize_SplatMap_Kernel(int p_splatMapArrrayIndex)
    {
        splat_Map_Total_Weight_Buffer.SetData(splat_Map_Total_Weight);
        computeShader.SetBuffer(NameIDs.Normalize_SplatMap, NameIDs.splat_Map_Total_Weight_Buffer, splat_Map_Total_Weight_Buffer);

        computeShader.SetTexture(NameIDs.Normalize_SplatMap, NameIDs.splatMapsArray, splatMapsArray[p_splatMapArrrayIndex]);
    }



    public void Normalize_SplatMap()
    {
        if(splatMapsArray != null)
        {
            for (int i = 0; i < splatMapsArray.Length; i++)
            {
                AssignBuffersAndParametersFor_Normalize_SplatMap_Kernel(i);

                computeShader.Dispatch(NameIDs.Normalize_SplatMap, am_x / 8, am_y / 8, 1);
            }


            if(splat_Map_Total_Weight_Buffer != null)
                splat_Map_Total_Weight_Buffer.Release();

            if(splatPaintRulesBuffer != null)
                splatPaintRulesBuffer.Release();



            if(manager.blurSplatmap)
                BlurSplatmap();
        }
    }





    public void BlurSplatmap()
    {
        if(alphamapResolution > heightmapResolution)
        {
            for (int i = 0; i < splatMapsArray.Length; i++)
            {
                ResizeSplatmap(ref splatMapsArray[i]);
            }
        }
    }






    void AssignBuffersAndParametersFor_Generate_ColorMap_Kernel(int p_splatMapArrrayIndex, int p_splatIndex)
    {
        computeShader.SetTexture(NameIDs.Generate_ColorMap, NameIDs.splatMapsArray, splatMapsArray[p_splatMapArrrayIndex]);
        computeShader.SetTexture(NameIDs.Generate_ColorMap, NameIDs.diffuseTexture, manager.splats[p_splatIndex].terrainLayer.diffuseTexture);
        computeShader.SetTexture(NameIDs.Generate_ColorMap, NameIDs.normalTexture, manager.splats[p_splatIndex].terrainLayer.normalMapTexture);
        computeShader.SetTexture(NameIDs.Generate_ColorMap, NameIDs.colorMapDiffuse, colorMapDiffuse);
        computeShader.SetTexture(NameIDs.Generate_ColorMap, NameIDs.colorMapNormal, colorMapNormal);
        computeShader.SetFloat(NameIDs.uvSize, terrain.terrainData.terrainLayers[p_splatIndex].tileSize.x);
        computeShader.SetInt(NameIDs.splatCount, manager.splatPaintRulesArray.Length);
        computeShader.SetInt(NameIDs.splatRuleBufferIndex, p_splatIndex);
    }



    public void Generate_ColorMap()
    {
        if(splatMapsArray != null)
        {
            for (int i = 0; i < manager.splats.Length; i++)
            {
               
                int _splatMapArrayIndex = Mathf.FloorToInt(((float)i) / 4) ;
                AssignBuffersAndParametersFor_Generate_ColorMap_Kernel(_splatMapArrayIndex, i);

                computeShader.Dispatch(NameIDs.Generate_ColorMap, am_x / 8, am_y / 8, 1);
                
            }
        }
    }









    /*
    public void WriteToTerrainAlphamap()
    {
        for (int i = 0; i < terrain.terrainData.alphamapTextures.Length; i++)
        {
            terrain.terrainData.alphamapTextures[i].SetPixels32(GetPixels32FromRenderTexture(splatMapsArray[i]));
            terrain.terrainData.alphamapTextures[i].Apply();
        }

        terrain.terrainData.SetBaseMapDirty();

    //    UpdateTerrainMaterialProperty();
    }
    */


    public void ClearAlphaMap()
    {
        float[,,] _alphaMap = new float[am_x, am_y, terrain.terrainData.terrainLayers.Length] ;
        terrain.terrainData.SetAlphamaps(0,0,_alphaMap);

        terrain.terrainData.SetBaseMapDirty();

        UpdateTerrainMaterialManualPaintedArea();
    }








    public void UpdateTerrainMaterialProperty()
    {
        if(manager.customTerrainMaterial)
        {
            if(texture2DArray_splat == null)
            {
                texture2DArray_splat = new Texture2DArray(am_x, am_y, terrain.terrainData.alphamapTextures.Length,  TextureFormat.RGBA32, true) ;
                texture2DArray_splat.Apply();
            }

            for(int i=0; i< terrain.terrainData.alphamapTextures.Length; i++)
            {
            //    texture2DArray_splat.SetPixels32(terrain.terrainData.alphamapTextures[i].GetPixels32(), i, 0);
                texture2DArray_splat.SetPixels32(GetPixels32FromRenderTexture(splatMapsArray[i]), i, 0);
            }

            texture2DArray_splat.Apply();


            customTerrainMaterial.SetTexture(NameIDs._TextureArraySplatmap, texture2DArray_splat) ;
        //    customTerrainMaterial.SetTexture(NameIDs._ColorMapDiffuse, colorMapDiffuse) ;
        //    customTerrainMaterial.SetTexture(NameIDs._ColorMapNormal, colorMapNormal) ;

            UpdateTerrainTriplanarWeightMap();
            UpdateTerrainMaterialManualPaintedArea();
        }
    }


    public void UpdateTerrainTriplanarWeightMap()
    {
        if (manager.customTerrainMaterial)
        {
            customTerrainMaterial.SetTexture(NameIDs._TriplanarWeightMap, renderTexture_triplanarWeightMap);
        }
    }


    public void UpdateTerrainMaterialManualPaintedArea()
    {
        if (manager.customTerrainMaterial)
        {
            /*
            if(texture2DArray_manualPainted == null)
            {
                texture2DArray_manualPainted = new Texture2DArray(am_x, am_y, terrain.terrainData.alphamapTextures.Length,  TextureFormat.RGBA32, true) ;
                texture2DArray_manualPainted.Apply();
            }

            for(int i=0; i< terrain.terrainData.alphamapTextures.Length; i++)
            {
                texture2DArray_manualPainted.SetPixels32(terrain.terrainData.alphamapTextures[i].GetPixels32(), i, 0);
            }

            texture2DArray_manualPainted.Apply();


            customTerrainMaterial.SetTexture(NameIDs._TextureArrayManualPainted, texture2DArray_manualPainted) ;
            */


            switch (terrain.terrainData.alphamapTextureCount)
            {
                case 1:
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap0, terrain.terrainData.alphamapTextures[0]);
                    break;

                case 2:
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap0, terrain.terrainData.alphamapTextures[0]);
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap1, terrain.terrainData.alphamapTextures[1]);
                    break;

                case 3:
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap0, terrain.terrainData.alphamapTextures[0]);
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap1, terrain.terrainData.alphamapTextures[1]);
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap2, terrain.terrainData.alphamapTextures[2]);
                    break;

                case 4:
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap0, terrain.terrainData.alphamapTextures[0]);
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap1, terrain.terrainData.alphamapTextures[1]);
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap2, terrain.terrainData.alphamapTextures[2]);
                    customTerrainMaterial.SetTexture(NameIDs._ManualPaintedSplatMap3, terrain.terrainData.alphamapTextures[3]);
                    break;
            }

            
        }
    }


    public void UpdateTerrainMaterialParameters()
    {
        customTerrainMaterial.SetFloat(NameIDs._LerpingDistance, manager.lerpingDistance);
        customTerrainMaterial.SetFloat(NameIDs._HeightBlendingTransition, manager.transitionAmount);
        customTerrainMaterial.SetFloat(NameIDs._TriplanarCutoffBias, manager.triplanarCutoffBias);
    }









    Color32[] GetPixels32FromRenderTexture(RenderTexture p_renderTexture)
    {
        RenderTexture previusRT = RenderTexture.active;

        RenderTexture.active = p_renderTexture;

        Texture2D _newTexture2D = new Texture2D(p_renderTexture.width, p_renderTexture.height, TextureFormat.ARGB32, false);
        _newTexture2D.ReadPixels(new Rect(0, 0, _newTexture2D.width, _newTexture2D.height), 0, 0);

        RenderTexture.active = previusRT;
        return _newTexture2D.GetPixels32(0);
    }



    void ResizeSplatmap(ref RenderTexture p_renderTexture)
    {
        int _tmp_res_x = hm_x -1 ;
        int _tmp_res_y = hm_y -1 ;

        Texture2D _newTexture2D = new Texture2D(p_renderTexture.width, p_renderTexture.height, TextureFormat.ARGB32, false);
        _newTexture2D.filterMode = FilterMode.Bilinear;
        _newTexture2D.SetPixels32(GetPixels32FromRenderTexture(p_renderTexture));
        _newTexture2D.Apply();
        RenderTexture _newRenderTexture = RenderTexture.GetTemporary(_tmp_res_x, _tmp_res_y, 0, RenderTextureFormat.ARGB32);
        _newRenderTexture.enableRandomWrite = true;
        Graphics.Blit(_newTexture2D, _newRenderTexture);

        Texture2D _newTexture2D_second = new Texture2D(_tmp_res_x, _tmp_res_y, TextureFormat.ARGB32, false);
        _newTexture2D_second.filterMode = FilterMode.Bilinear;
        _newTexture2D_second.SetPixels32(GetPixels32FromRenderTexture(_newRenderTexture));
        _newTexture2D_second.Apply();
        p_renderTexture = RenderTexture.GetTemporary(p_renderTexture.width, p_renderTexture.height, 0, RenderTextureFormat.ARGB32);
        p_renderTexture.enableRandomWrite = true ;
        Graphics.Blit(_newTexture2D_second, p_renderTexture);

        RenderTexture.ReleaseTemporary(_newRenderTexture);
    }

    
    Texture2D ConvertRenderTextureToTexture2D(RenderTexture p_renderTexture)
    {
        RenderTexture previusRT = RenderTexture.active;

        RenderTexture.active = p_renderTexture;

        Texture2D _newTexture2D = new Texture2D(p_renderTexture.width, p_renderTexture.height, TextureFormat.ARGB32, false);
        _newTexture2D.ReadPixels(new Rect(0, 0, _newTexture2D.width, _newTexture2D.height), 0, 0);
        _newTexture2D.Apply();

        RenderTexture.active = previusRT;

        return _newTexture2D;
    }
    







}
