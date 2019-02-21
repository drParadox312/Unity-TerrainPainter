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

    public bool hasTerrainHeigtmapChanged = false;
    public bool[] heigtmapChangedTerrains ;

    public TerrainPainter_Splat[] splats;
    public TerrainLayer[] terrainLayers;

    public SplatPaintRules[] splatPaintRulesArray;

    public bool blurSplatmap = true ;

    public float maxTerrainHeight = 3000f ;

    public int flowMapIteration = 10;


    public bool useLerping = true;
    public float lerpingDistance = 1500f;
    public bool useHeightBlending = true;
    public bool useTransition = true;
    public float transitionAmount = 0.3f;
    public bool useLerpedTransitionAmount = true;
    public bool useOcclusion = true;
    public bool useTriplanar = true;
    public float triplanarCutoffBias = 0.5f;


    public Material customTerrainMaterial ;
    public Texture2DArray texture2DArray_diffuse ;
    public Texture2DArray texture2DArray_normal ;
    public Texture2DArray texture2DArray_height;
    public Texture2DArray texture2DArray_occlusion;


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

            if(_foundTerrains != null)
            {
                if(_foundTerrains.Length > 0)
                {
                    if(terrains != null)
                    {
                        for (int i = 0; i < _foundTerrains.Length; i++)
                        {
                            if (_foundTerrains[i] != terrains[i])
                            {
                                _hasArrayChanged = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        _hasArrayChanged = true;
                    }
                }
            }

            if (_hasArrayChanged)
            {
                isInitialized = false;
                FindTerrains();
            }
        }
        else if(isInitialized == false)
        {
            FindTerrains();
        }
    }


    public void FindTerrains()
    {
        if (computeShader == null)
            return;


        customTerrainMaterial.SetFloat(NameIDs._LerpingDistance, lerpingDistance);
        customTerrainMaterial.SetFloat(NameIDs._HeightBlendingTransition, transitionAmount);
        customTerrainMaterial.SetFloat(NameIDs._TriplanarCutoffBias, triplanarCutoffBias);

        UpdateTerrainMaterialParameters(true);


        terrains = (Terrain[])FindObjectsOfType(typeof(Terrain));

        int _hmR = terrains[0].terrainData.heightmapResolution;
        int _amR = terrains[0].terrainData.alphamapResolution;
        Vector3 _size = terrains[0].terrainData.size ;

        for(int i=0 ; i< terrains.Length; i++)
        {
            if(i > 0)
            {
                if(_hmR != terrains[i].terrainData.heightmapResolution  ||  _amR !=  terrains[i].terrainData.alphamapResolution ||  _size != terrains[i].terrainData.size)
                {
                    Debug.LogError("All terrains must be have same sized heightmap resoluiton and same sized alphamap resolution.") ;
                    return ;
                }
            }
        }


        if(splats == null)
            splats = new TerrainPainter_Splat[0] ;
        else
            RemoveNullElementFromSplatArray();



        NameIDs.SetUpNameIDS(computeShader);



        if (terrains.Length == 0)
            return;



        if(terrains[0].drawInstanced)
            computeShader.SetFloat(NameIDs.useDrawInstanced, 1f);
        else
            computeShader.SetFloat(NameIDs.useDrawInstanced, -1f);



        maxTerrainHeight = terrains[0].terrainData.size.y;


        terrainScripts = new TerrainPainter_Terrain[terrains.Length];




        SetUpTexture2DArrays();


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

        isInitialized = true;
        hasTerrainHeigtmapChanged = false;
        heigtmapChangedTerrains = new bool[terrains.Length];
        isUpdating = false;
    }




    public void SetUpCustomTerrainMaterail()
    {
        if(splats == null || (splats != null && splats.Length == 0))
        {
            Debug.LogError("Assign any splat.") ;
            customTerrainMaterial = null;
            return ;
        }


        if(customTerrainMaterial != null)
        {
            SetUpTexture2DArrays();

            for (int i = 0; i < terrainScripts.Length; i++)
            {
                    terrainScripts[i].SetUpTerrainMaterial();
            }

            for (int i = 0; i < terrainScripts.Length; i++)
            {
                    terrainScripts[i].UpdateTerrainMaterialProperty();
            }

        }
        else
        {
            for (int i = 0; i < terrainScripts.Length; i++)
            {
                    terrainScripts[i].SetUpTerrainMaterial();
            }
        }
    }



    public void SetUpTexture2DArrays()
    {

        if(customTerrainMaterial)
        {
            int _resolution = splats[0].terrainLayer.diffuseTexture.width ;
            int _layerCount = splats.Length ;

            texture2DArray_diffuse = new Texture2DArray(_resolution, _resolution, _layerCount, TextureFormat.RGBA32, true);
            texture2DArray_normal = new Texture2DArray(_resolution, _resolution, _layerCount, TextureFormat.RGBA32, true);
            texture2DArray_height = new Texture2DArray(_resolution, _resolution, _layerCount, TextureFormat.RGBA32, true);
            texture2DArray_occlusion = new Texture2DArray(_resolution, _resolution, _layerCount, TextureFormat.RGBA32, true);

            for (int i=0; i<_layerCount; i++)
            {
                texture2DArray_diffuse.SetPixels32(splats[i].terrainLayer.diffuseTexture.GetPixels32(), i , 0) ;
                texture2DArray_normal.SetPixels32(splats[i].terrainLayer.normalMapTexture.GetPixels32(), i , 0) ;
                texture2DArray_height.SetPixels32(splats[i].terrainLayer.maskMapTexture.GetPixels32(), i, 0);
                texture2DArray_occlusion.SetPixels32(splats[i].occlusionMap.GetPixels32(), i, 0);
            }

            texture2DArray_diffuse.Apply();
            texture2DArray_normal.Apply();
            texture2DArray_height.Apply();
            texture2DArray_occlusion.Apply();


            customTerrainMaterial.SetTexture(NameIDs._TextureArrayDiffuse,texture2DArray_diffuse);
            customTerrainMaterial.SetTexture(NameIDs._TextureArrayNormal,texture2DArray_normal);
            customTerrainMaterial.SetTexture(NameIDs._TextureArrayHeightmap, texture2DArray_height);
            customTerrainMaterial.SetTexture(NameIDs._TextureArrayOcclusion, texture2DArray_occlusion); 
        }
    }








    public void TerrainHeightmapChanged(int p_terrainIndex)
    {
        hasTerrainHeigtmapChanged = true;
        heigtmapChangedTerrains[p_terrainIndex] = true ;
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
            heigtmapChangedTerrains = new bool[terrains.Length];
        }

        isUpdating = false;
    }
    


    public void UpdateTerrains()
    {
        GenerateHeightSlopeSnowWeightsMaps(true);


        if(autoUpdateFlowMap)
            GenerateFlowMaps(true);

        GenerateCurvatureMaps(true);


        GenerateSplatMaps(true);

        isUpdating = false;
    }




    public void UpdateTerrainsAtInitialize()
    {
        GenerateHeightSlopeSnowWeightsMaps(true);
        GenerateFlowMaps(true);
        GenerateCurvatureMaps(true);
        GenerateSplatMaps(true);
        GenerateColorMap(true);

        isUpdating = false;
    }






    public void UpdateFlowMap(bool p_atInitialize = false)
    {
        if (isUpdating == true)
            return;

        isUpdating = true;

        GenerateFlowMaps(true);
        GenerateCurvatureMaps(p_atInitialize);
        GenerateSplatMaps(p_atInitialize);
        GenerateColorMap(p_atInitialize);

        isUpdating = false;
    }

    public void UpdateCurvatureMap(bool p_atInitialize = false)
    {
        if (isUpdating == true)
            return;

        isUpdating = true;

        

        GenerateCurvatureMaps(p_atInitialize);
        GenerateSplatMaps(p_atInitialize);
        GenerateColorMap(p_atInitialize);

        isUpdating = false;
    }

    public void UpdateSplatmapMap(bool p_atInitialize = false)
    {
        if (isUpdating == true)
            return;

        isUpdating = true;

        UpdateSplatPaintRulesArray();
        GenerateSplatMaps(p_atInitialize);
        GenerateColorMap(p_atInitialize);

        isUpdating = false;
    }


    public void UpdateColorMap(bool p_atInitialize = false)
    {
        if (isUpdating == true)
            return;

        isUpdating = true;

        GenerateColorMap(p_atInitialize);

        isUpdating = false;
    }






    public void UpdateSplats(bool p_atInitialize = false)
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

                for (int i = 0; i< terrains.Length; i++)
        {
            terrainScripts[i].SetUpSplatMapArray();
        }

        UpdateSplatPaintRulesArray();
        UpdateSplatmapMap(p_atInitialize);
    }






    public void UpdateSplatmapBlur()
    {
        for (int i = 0; i< terrainScripts.Length; i++)
        {
            terrainScripts[i].BlurSplatmap();
        }
    }






    void UpdateSplatPaintRulesArray()
    {
        splatPaintRulesArray = new SplatPaintRules[splats.Length];

        for (int i = 0; i <  splats.Length; i++)
        {
            splatPaintRulesArray[i] = splats[i].paintRules;
            splatPaintRulesArray[i].splatType =  (float)splats[i].splatType ;
            splatPaintRulesArray[i].paintMethod =  (float)splats[i].paintMethod ;
            splatPaintRulesArray[i].useFlowMap =  splats[i].useFlowMapMask == true ?  1f : -1f;
            splatPaintRulesArray[i].useConvexityMap =  splats[i].useConvexityMapMask == true ?  1f : -1f;
            splatPaintRulesArray[i].useConcavityMap =  splats[i].useConcavityMapMask == true ?  1f : -1f;
            splatPaintRulesArray[i].useAspectMap =  splats[i].useAspectMap == true ?  1f : -1f;
            splatPaintRulesArray[i].useTextureMap =  splats[i].useTextureMap == true ?  1f : -1f;
            splatPaintRulesArray[i].isInverseFlowMapHeightWeight =  splats[i].isInverseFlowMapHeightWeight == true ?  1f : -1f;
            splatPaintRulesArray[i].isInverseFlowMapSlopeWeight =  splats[i].isInverseFlowMapSlopeWeight == true ?  1f : -1f;
            splatPaintRulesArray[i].flowMapEffect =  (float)splats[i].flowMapEffect ;
            splatPaintRulesArray[i].convexityMapEffect =  (float)splats[i].convexityMapEffect ;
            splatPaintRulesArray[i].concavityMapEffect =  (float)splats[i].concavityMapEffect ;
            splatPaintRulesArray[i].aspectMapEffect =  (float)splats[i].aspectMapEffect ;
            splatPaintRulesArray[i].textureMapEffect =  (float)splats[i].textureMapEffect ;
            splatPaintRulesArray[i].textureMapChannel =  (float)splats[i].textureMapChannel ;

            if(splats[i].textureMap)
            {
                if(splats[i].textureMap.isReadable == false)
                    Debug.LogError("Texture Mask must be imported as isReadable. Texture : " + splats[i].textureMap);
                else
                    splatPaintRulesArray[i].textureMapResolution =  splats[i].textureMap.width ;
            }

                
        }
    }




















    public void GenerateHeightSlopeSnowWeightsMaps(bool p_atInitialize = false)
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Generate_Height_Map();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Generate_NeighborTerrain_Height_Map();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Generate_Slope_Map();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Generate_NeighborTerrain_Slope_Map();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Generate_SnowWeight_Maps();
        }
    }


    public void GenerateFlowMaps(bool p_atInitialize = false)
    {
        if (flowMapIteration > 0)
        {
            for (int i = 0; i < terrainScripts.Length; i++)
            {
                if(p_atInitialize || autoUpdateFlowMap )
                    terrainScripts[i].FlowMap_AddWater();
            }

            for (int j = 0; j < flowMapIteration; j++)
            {
                for (int i = 0; i < terrainScripts.Length; i++)
                {
                    if(p_atInitialize || autoUpdateFlowMap )
                        terrainScripts[i].FlowMap_GenerateNeighborTerrainWaterMaps();
                }

                for (int i = 0; i < terrainScripts.Length; i++)
                {
                    if(p_atInitialize || autoUpdateFlowMap )
                        terrainScripts[i].FlowMap_CalculateWaterOut();
                }

                for (int i = 0; i < terrainScripts.Length; i++)
                {
                    if(p_atInitialize || autoUpdateFlowMap )
                        terrainScripts[i].FlowMap_MoveWater();
                }
            }

            for (int i = 0; i < terrainScripts.Length; i++)
            {
                if(p_atInitialize || autoUpdateFlowMap )
                    terrainScripts[i].FlowMap_Generate();
            }

            for (int i = 0; i < terrainScripts.Length; i++)
            {
                if(p_atInitialize || autoUpdateFlowMap )
                    terrainScripts[i].ReleaseRenderTexturesForWaterMap();
            }
        }
    }


    public void GenerateCurvatureMaps(bool p_atInitialize = false)
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].CurvatureMap_FirstPass();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].CurvatureMap_SecondPass();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].CurvatureMap_Generate();
        }
    }


    public void GenerateSplatMaps(bool p_atInitialize = false)
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Generate_SplatMap();
        }

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Normalize_SplatMap();
        }

        /*
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].WriteToTerrainAlphamap();
        }
        */

        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].UpdateTerrainMaterialProperty();
        }

        SaveManagerParameterModifications();
    }



    public void GenerateColorMap(bool p_atInitialize = false)
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].Generate_ColorMap();
        }
    }



    public void UpdateTerrainMaterialManualPaintedArea(bool p_atInitialize = false)
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].UpdateTerrainMaterialManualPaintedArea();
        }
    }


    public void ClearTerrainMaterialManualPaintedArea(bool p_atInitialize = false)
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if(p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].ClearAlphaMap();
        }
    }





    public void UpdateTerrainMaterialParameters(bool p_atInitialize = false)
    {
        for (int i = 0; i < terrainScripts.Length; i++)
        {
            if (p_atInitialize || (autoUpdateFlowMap || (!autoUpdateFlowMap && heigtmapChangedTerrains[i])))
                terrainScripts[i].UpdateTerrainMaterialParameters();
        }
    }







    public void AddNewSplat(TerrainPainter_Splat p_newSplat)
    {
        if(p_newSplat.terrainLayer == null)
            return ;
            
        if(p_newSplat.terrainLayer.diffuseTexture == null)
            return ;

        for (int i = 0; i < splats.Length; i++)
        {
            if (splats[i] == p_newSplat)
                return;
        }

        if(p_newSplat.useTextureMap)
            if(p_newSplat.textureMap == null || (p_newSplat.textureMap && p_newSplat.textureMap.isReadable == false))
            {
                Debug.LogError("Texture Mask must be imported as isReadable. Texture : " + p_newSplat.textureMap);
                return ;
            }
                

        
        TerrainPainter_Splat[] _newSplats = new TerrainPainter_Splat[splats.Length + 1];

        System.Array.Copy(splats, 0, _newSplats, 0, splats.Length);
        _newSplats[_newSplats.Length - 1] = p_newSplat;

        splats = _newSplats;
   

        UpdateSplats(true);
    }

    public void RemoveSplat(int p_index)
    {
        TerrainPainter_Splat[] _newSplats = new TerrainPainter_Splat[splats.Length - 1];
        if(_newSplats.Length > 0)
        {
            for (int i = 0; i < splats.Length; i++)
            {
                if (i < p_index)
                    _newSplats[i] = splats[i];
                else if (i > p_index)
                    _newSplats[i -1] = splats[i];
            }
        }
        splats = _newSplats;

        UpdateSplats(true);
    }

    public void ChangeSplat(int p_index, TerrainPainter_Splat p_newSplat)
    {
        for (int i = 0; i < splats.Length; i++)
        {
            if (splats[i] == p_newSplat)
                return;
        }

        splats[p_index] = p_newSplat;

        UpdateSplats(true);
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

        UpdateSplats(true);
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

        UpdateSplats(true);
    }






    void RemoveNullElementFromSplatArray()
    {
        int _nullCount = 0;

        if(splats != null)
        {
            for (int i = 0; i < splats.Length; i++)
            {
                if (splats[i] == null)
                {
                    _nullCount++;
                }
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
