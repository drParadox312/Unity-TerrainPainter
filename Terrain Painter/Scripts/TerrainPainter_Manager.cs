using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainPainter;

[ExecuteInEditMode]
public class TerrainPainter_Manager : MonoBehaviour
{
    public bool isUpdating;

    public ComputeShader computeShader;
    public Terrain[] terrains;
    public TerrainPainter_Terrain[] terrainScripts;

    public bool isInitialized = false;

    public bool checkForNewTerrainsOnEnable = true ;
    public bool autoUpdate = true ;
    public bool autoUpdateFlowMap;
    public bool autoUpdateCurvatureMap = true ;

    public bool hasTerrainHeigtmapChanged = false;

    public TerrainPainter_Splat[] splats;
    public TerrainLayer[] terrainLayers;

    public float maxTerrainHeight = 3000f ;


    public float snowAmount = 0.75f;


    public int flowMapIteration = 10;

    void Awake()
    {
        isInitialized = false;
    }

    public void Initialize()
    {

        if (computeShader == null)
            return ;

        
        if (checkForNewTerrainsOnEnable)
        {
            bool _hasArrayChanged = false;
            Terrain[] _foundTerrains = (Terrain[])FindObjectsOfType(typeof(Terrain));

            for (int i = 0; i < _foundTerrains.Length; i++)
            {
                if (_foundTerrains[i] != terrains[i])
                {
                    _hasArrayChanged = true;
                    break;
                }
            }

            if (_hasArrayChanged)
            {
                isInitialized = false;
                FindTerrains();
            }
        }


        if (isInitialized == false)
        {
            FindTerrains();
            isInitialized = true;
        }


        hasTerrainHeigtmapChanged = false;
        isUpdating = false;
    }


    public void FindTerrains()
    {
        terrains = (Terrain[])FindObjectsOfType(typeof(Terrain));

        if (computeShader == null)
            return;

        NameIDs.SetUpNameIDS(computeShader);

        RemoveNullElementFromSplatArray();


        if (terrains.Length == 0 || splats.Length == 0)
            return;


        maxTerrainHeight = terrains[0].terrainData.size.y;


        terrainScripts = new TerrainPainter_Terrain[terrains.Length];


        for (int i = 0; i < terrains.Length; i++)
        {
            if (terrains[i].gameObject.GetComponent<TerrainPainter_Terrain>() == null)
                terrains[i].gameObject.AddComponent<TerrainPainter_Terrain>();

            terrainScripts[i] = terrains[i].gameObject.GetComponent<TerrainPainter_Terrain>();

            terrainScripts[i].manager = this;
            terrainScripts[i].terrainIndex = i;
            terrainScripts[i].SetUpProperties();
        }

        UpdateTerrainsAtInitialize();
    }



    public void TerrainHeightmapChanged(int p_terrainIndex)
    {
        Debug.Log("terrain changed : " + terrains[p_terrainIndex]);
        hasTerrainHeigtmapChanged = true;
    }


    public void TerrainModifyingEnded()
    {
        if (isUpdating == false)
        {
            isUpdating = true;

            if (autoUpdate && hasTerrainHeigtmapChanged)
            {
                UpdateTerrains();
            }

            hasTerrainHeigtmapChanged = false;
        }
    }


    public void UpdateTerrains()
    {
        GenerateHeightSlopeSnowWeightsMaps();


        if(autoUpdateFlowMap)
            GenerateFlowMaps();

        if(autoUpdateCurvatureMap)
            GenerateCurvatureMaps();


        GenerateSplatMaps();

        isUpdating = false;
    }




    public void UpdateTerrainsAtInitialize()
    {
        GenerateHeightSlopeSnowWeightsMaps();
        GenerateFlowMaps();
        GenerateCurvatureMaps();
        GenerateSplatMaps();

        isUpdating = false;
    }






    public void UpdateFlowMap()
    {
        if (isUpdating == true)
            return;

        isUpdating = true;

        GenerateFlowMaps();
        GenerateCurvatureMaps();
        GenerateSplatMaps();

        isUpdating = false;
    }

    public void UpdateCurvatureMap()
    {
        if (isUpdating == true)
            return;

        isUpdating = true;

        GenerateCurvatureMaps();
        GenerateSplatMaps();

        isUpdating = false;
    }

    public void UpdateSplatmapMap()
    {
        if (isUpdating == true)
            return;

        isUpdating = true;

        GenerateSplatMaps();

        isUpdating = false;
    }








    public void UpdateTerrainMaxHeight()
    {
        for (int i = 0; i < terrains.Length; i++)
        {
            terrains[i].terrainData.size = new Vector3(terrains[i].terrainData.size.x, maxTerrainHeight, terrains[i].terrainData.size.z);
        }

        UpdateTerrains();
    }







    public void UpdateSplats()
    {
        if (isUpdating == true)
            return;

        terrainLayers = new TerrainLayer[splats.Length];

        for (int i = 0; i < splats.Length; i++)
        {
            terrainLayers[i] = splats[i].terrainLayer;
        }


        for (int i = 0; i< terrains.Length; i++)
        {
            terrainScripts[i].SetUpTerrainLayers();
        }

        UpdateSplatmapMap();
    }










    public void UpdateSnowAmount()
    {
        for (int i = 0; i < terrains.Length; i++)
        {
            terrainScripts[i].snowAmount = snowAmount;
        }

        UpdateSplatmapMap();
    }




   











    public void GenerateHeightSlopeSnowWeightsMaps()
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].Generate_Height_Map();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].Generate_NeighborTerrain_Height_Map();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].Generate_Slope_Map();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].Generate_SnowWeight_Maps();
        }
    }


    public void GenerateFlowMaps()
    {
        if (flowMapIteration > 0)
        {
            for (int i = 0; i < terrainScripts.Length; i++)
            {
                terrainScripts[i].FlowMap_AddWater();
            }

            for (int j = 0; j < flowMapIteration; j++)
            {
                for (int i = 0; i < terrainScripts.Length; i++)
                {
                    terrainScripts[i].FlowMap_GenerateNeighborTerrainWaterMaps();
                }

                for (int i = 0; i < terrainScripts.Length; i++)
                {
                    terrainScripts[i].FlowMap_CalculateWaterOut();
                }

                for (int i = 0; i < terrainScripts.Length; i++)
                {
                    terrainScripts[i].FlowMap_MoveWater();
                }
            }

            for (int i = 0; i < terrainScripts.Length; i++)
            {
                terrainScripts[i].FlowMap_Generate();
            }

            for (int i = 0; i < terrainScripts.Length; i++)
            {
                terrainScripts[i].ReleaseRenderTexturesForWaterMap();
            }
        }
    }


    public void GenerateCurvatureMaps()
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].CurvatureMap_Generate();
        }
    }


    public void GenerateSplatMaps()
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].Generate_SplatMap();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].Normalize_SplatMap();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            terrainScripts[i].WriteToTerrainAlphamap();
        }


        SaveManagerParameterModifications();
    }






    public void AddNewSplat(TerrainPainter_Splat p_newSplat)
    {
        for (int i = 0; i < splats.Length; i++)
        {
            if (splats[i] == p_newSplat)
                return;
        }

        
        TerrainPainter_Splat[] _newSplats = new TerrainPainter_Splat[splats.Length + 1];

        System.Array.Copy(splats, 0, _newSplats, 0, splats.Length);
        _newSplats[_newSplats.Length - 1] = p_newSplat;

        splats = _newSplats;
   

        UpdateSplats();
    }

    public void RemoveSplat(int p_index)
    {
        TerrainPainter_Splat[] _newSplats = new TerrainPainter_Splat[splats.Length - 1];
        for (int i = 0; i < splats.Length; i++)
        {
            if (i < p_index)
                _newSplats[i] = splats[i];
            else if (i > p_index)
                _newSplats[i -1] = splats[i];
        }
        splats = _newSplats;

        UpdateSplats();
    }

    public void ChangeSplat(int p_index, TerrainPainter_Splat p_newSplat)
    {
        for (int i = 0; i < splats.Length; i++)
        {
            if (splats[i] == p_newSplat)
                return;
        }

        splats[p_index] = p_newSplat;

        UpdateSplats();
    }

    public void MoveUpSplat(int p_index)
    {
        if (p_index >= 1)
        {
            TerrainPainter_Splat _indexedElement = splats[p_index];
            TerrainPainter_Splat _previusElement = splats[p_index - 1];
            splats[p_index - 1] = _indexedElement;
            splats[p_index] = _previusElement;
        }

        UpdateSplats();
    }

    public void MoveDownSplat(int p_index)
    {
        if (p_index <= splats.Length - 2)
        {
            TerrainPainter_Splat _indexedElement = splats[p_index];
            TerrainPainter_Splat _nextElement = splats[p_index + 1];
            splats[p_index] = _nextElement;
            splats[p_index + 1] = _indexedElement;
        }

        UpdateSplats();
    }






    void RemoveNullElementFromSplatArray()
    {
        int _nullCount = 0;
        for (int i = 0; i < splats.Length; i++)
        {
            if (splats[i] == null)
            {
                _nullCount++;
            }
        }

        if (_nullCount > 0)
        {
            TerrainPainter_Splat[] _newSplatAray = new TerrainPainter_Splat[splats.Length - _nullCount];

            for (int i = 0, indexCounter =0 ; i < splats.Length; i++)
            {
                if (splats[i] != null)
                {
                    _newSplatAray[indexCounter] = splats[i];
                    indexCounter++;
                }
            }


            splats = _newSplatAray;
        }

    }







    void SaveManagerParameterModifications()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
    }



    public void SaveSplatParameterModifications(int p_splatIndex)
    {
        splats[p_splatIndex].SetDirty();
    }







}
