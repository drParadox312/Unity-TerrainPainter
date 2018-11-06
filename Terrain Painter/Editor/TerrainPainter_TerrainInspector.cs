using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TerrainPainter;

[CustomEditor(typeof(TerrainPainter_Terrain))]
public class TerrainPainter_TerrainInspector : Editor
{

    private TerrainPainter_Terrain terrainScript;

    void OnEnable()
    {
        terrainScript = (TerrainPainter_Terrain)target;
    }

    public override void OnInspectorGUI()
    {
        //       base.OnInspectorGUI();

        if (terrainScript.manager.isInitialized)
        {
            EditorGUILayout.ObjectField("Unity heightmap", terrainScript.renderTexture_unity_heightMap, typeof(RenderTexture));
            EditorGUILayout.ObjectField("Height, Slope, SnowWeight maps", terrainScript.renderTexture_height_slope_snowWeight_Maps, typeof(RenderTexture));
            EditorGUILayout.ObjectField("Neighbor terrrains heightmaps", terrainScript.renderTexture_neighbor_terrain_heightMaps, typeof(RenderTexture));
            EditorGUILayout.ObjectField("Convexity, Concavitiy, Flow maps", terrainScript.renderTexture_convexity_concavitiy_flow_Maps, typeof(RenderTexture));
        }
        else
        {
            EditorGUILayout.LabelField("TerrainPainter Manager not initialized");
        }
    }



    void OnSceneGUI()
    {
        terrainScript = (TerrainPainter_Terrain)target;

        if (terrainScript.hasTerrainHeigtmapChanged)
        {
            if (Event.current.type == EventType.MouseUp)
            {
                if (terrainScript.terrain && terrainScript.computeShader)
                {
                    terrainScript.manager.TerrainModifyingEnded();
                }
            }
        }
    }



}








