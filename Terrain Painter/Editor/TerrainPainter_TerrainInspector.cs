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
        //      base.OnInspectorGUI();

        if(terrainScript.manager)
        {
            if (terrainScript.manager.isInitialized)
            {
                EditorGUILayout.HelpBox("Keep opened this inpector for heightmap update.", MessageType.Info);
                EditorGUILayout.ObjectField("", terrainScript.renderTexture_unity_normalMap, typeof(RenderTexture));
                EditorGUILayout.ObjectField("", terrainScript.renderTexture_convexity_concavitiy_flow_Maps, typeof(RenderTexture));
            }
            else
            {
                EditorGUILayout.LabelField("TerrainPainter_Manager not initialized.");
            }
        }
        else
        {
             EditorGUILayout.LabelField("TerrainPainter_Manager not assigned.");
        }
    }



    void OnSceneGUI()
    {
        terrainScript = (TerrainPainter_Terrain)target;

        if (terrainScript.hasTerrainHeigtmapChanged)
        {
            if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0)
                {
                    if (terrainScript.terrain && terrainScript.computeShader)
                    {
                        terrainScript.manager.TerrainModifyingEnded();
                    }
                }
            }
        }
    }



}








