using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TerrainPainter;

[CustomEditor(typeof(TerrainPainter_Manager))]
public class TerrainPainter_ManagerInspector : Editor
{

    private TerrainPainter_Manager managerScript;

    private TerrainPainter_Splat newAddedSplat;
    private int selected_splat_index = -1;

    void OnEnable()
    {
        managerScript = (TerrainPainter_Manager)target;

        managerScript.Initialize();
    }


    public override void OnInspectorGUI()
    {
        //    base.OnInspectorGUI();

        if (managerScript.computeShader && managerScript.isUpdating == false)
        {
            if (GUILayout.Button("Find Terrains"))
            {
                managerScript.FindTerrains();
            }

            managerScript.checkForNewTerrainsOnEnable = EditorGUILayout.Toggle("Check For New Terrains OnEnable", managerScript.checkForNewTerrainsOnEnable);
            managerScript.computeShader = (ComputeShader)EditorGUILayout.ObjectField("ComputeShader", managerScript.computeShader, typeof(ComputeShader));
            EditorGUILayout.Toggle("Initialized", managerScript.isInitialized);
            EditorGUILayout.Toggle("Is updating", managerScript.isUpdating);
            EditorGUILayout.Toggle("Has Terrain Heigtmap Changed", managerScript.hasTerrainHeigtmapChanged);


            if (managerScript.isInitialized)
            {

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Separator();
                managerScript.maxTerrainHeight = Mathf.Max(1f , EditorGUILayout.DelayedFloatField("Max Terrain Height", managerScript.maxTerrainHeight));
                if (EditorGUI.EndChangeCheck())
                {
                    managerScript.UpdateTerrainMaxHeight();
                }


                EditorGUI.BeginChangeCheck();
                managerScript.snowAmount = EditorGUILayout.Slider("Snow Amount", managerScript.snowAmount, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    managerScript.UpdateSnowAmount();
                }
                 EditorGUILayout.Separator();



                EditorGUI.BeginChangeCheck();
                managerScript.flowMapIteration = Mathf.Clamp(EditorGUILayout.DelayedIntField("Flow Map Iteration Count", managerScript.flowMapIteration), 0, 20);
                if (EditorGUI.EndChangeCheck())
                {
                    managerScript.UpdateFlowMap();
                }
                EditorGUILayout.Separator();


                EditorGUI.BeginChangeCheck();
                managerScript.convexityScale = EditorGUILayout.Slider("Convexity Scale", managerScript.convexityScale, 1f, 30f);
                if (EditorGUI.EndChangeCheck())
                {
                    managerScript.UpdateCurvatureMap();
                }


                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                managerScript.autoUpdate = EditorGUILayout.Toggle("Auto Update", managerScript.autoUpdate);
                EditorGUILayout.Separator();
                if (GUILayout.Button("Update Terrains"))
                {
                    managerScript.UpdateTerrainsAtInitialize();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();



                EditorGUILayout.BeginHorizontal();
                managerScript.autoUpdateFlowMap = EditorGUILayout.Toggle("Auto Update Flowmap", managerScript.autoUpdateFlowMap);
                EditorGUILayout.Separator();
                if (managerScript.autoUpdateFlowMap)
                {
                    EditorGUILayout.LabelField("Continuous update may cause dropping performance!");
                }
                else
                {
                    if (GUILayout.Button("Update Flowmap"))
                    {
                        managerScript.UpdateFlowMap();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();



                EditorGUILayout.BeginHorizontal();
                managerScript.autoUpdateCurvatureMap = EditorGUILayout.Toggle("Auto Update Curvature Map", managerScript.autoUpdateCurvatureMap);
                EditorGUILayout.Separator();
                if (managerScript.autoUpdateCurvatureMap == false)
                {
                    if (GUILayout.Button("Update Curvature Map"))
                    {
                        managerScript.UpdateCurvatureMap();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();


                if (GUILayout.Button("Update Splatmap"))
                {
                    managerScript.UpdateSplatmapMap();
                }



                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Terrain count : " + managerScript.terrains.Length);
                EditorGUILayout.LabelField("Splat count   : " + managerScript.splats.Length + "    --   Click on image to change parameters.");
                EditorGUILayout.Separator();

                GUIStyle _title_guiStyle = new GUIStyle();
                _title_guiStyle.fontStyle = FontStyle.Bold;

                EditorGUILayout.LabelField("Splats", _title_guiStyle);


                GUIStyle _label_guiStyle = new GUIStyle();
                _label_guiStyle.fixedWidth = 100f;
                //    _label_guiStyle.fontStyle = FontStyle.Bold;

                GUIStyle _button_guiStyle = new GUIStyle();
                _button_guiStyle.fixedWidth = 50f;
                _button_guiStyle.fixedHeight = 50f;


                if (managerScript.isUpdating == false)
                {
                    for (int i = 0; i < managerScript.splats.Length; i++)
                    {

                        EditorGUILayout.BeginHorizontal();

                        _button_guiStyle.normal.background = managerScript.splats[i].terrainLayer.diffuseTexture;

                        if (GUILayout.Button("", _button_guiStyle))
                        {
                            if (selected_splat_index == i)
                            {
                                selected_splat_index = -1;
                            }
                            else
                            {
                                selected_splat_index = i;
                            }

                        }

                        EditorGUILayout.Separator();

                        if (i == selected_splat_index)
                            _label_guiStyle.normal.textColor = Color.red;
                        else
                            _label_guiStyle.normal.textColor = Color.black;


                        GUILayout.Label(managerScript.splats[i].name, _label_guiStyle);

                        if (GUILayout.Button(" Up "))
                        {
                            managerScript.MoveUpSplat(i);
                            if(selected_splat_index > -1)
                            {
                                selected_splat_index = Mathf.Clamp(selected_splat_index - 1, 0, managerScript.splats.Length-1);
                            }
                        }

                        EditorGUILayout.Separator();

                        if (GUILayout.Button("Down"))
                        {
                            managerScript.MoveDownSplat(i);
                            if(selected_splat_index > -1)
                            {
                                selected_splat_index = Mathf.Clamp(selected_splat_index + 1, 0, managerScript.splats.Length-1);
                            }
                        }

                        EditorGUILayout.Separator();




                        if (GUILayout.Button("-"))
                        {
                            managerScript.RemoveSplat(i);
                            selected_splat_index = -1 ;
                        }
                        else
                        {
                            TerrainPainter_Splat _assignedSplat = (TerrainPainter_Splat)EditorGUILayout.ObjectField(managerScript.splats[i], typeof(TerrainPainter_Splat));
                            if (_assignedSplat != managerScript.splats[i])
                            {
                                managerScript.ChangeSplat(i, _assignedSplat);
                            }
                        }

                        EditorGUILayout.Separator();

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                    }

                    EditorGUILayout.LabelField("To add new splat drop here");
                    newAddedSplat = (TerrainPainter_Splat)EditorGUILayout.ObjectField(newAddedSplat, typeof(TerrainPainter_Splat));

                    if (newAddedSplat != null)
                    {
                        managerScript.AddNewSplat(newAddedSplat);
                        newAddedSplat = null;
                    }

                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();



                    if (selected_splat_index > -1 && selected_splat_index < managerScript.splats.Length)
                    {
                        EditorGUI.BeginChangeCheck();



                        EditorGUILayout.LabelField(managerScript.splats[selected_splat_index].name, _title_guiStyle);
                        EditorGUILayout.Separator();
                        managerScript.splats[selected_splat_index].terrainLayer = (TerrainLayer)EditorGUILayout.ObjectField("Terrain Layer", managerScript.splats[selected_splat_index].terrainLayer, typeof(TerrainLayer));
                        EditorGUILayout.Separator();


                        managerScript.splats[selected_splat_index].splatType = (SplatType)EditorGUILayout.EnumPopup("Splat Type", managerScript.splats[selected_splat_index].splatType);
                        managerScript.splats[selected_splat_index].paintMethod = (PaintMethod)EditorGUILayout.EnumPopup("Paint Method", managerScript.splats[selected_splat_index].paintMethod);
                        EditorGUILayout.Separator();





                        EditorGUILayout.Separator();





                        EditorGUILayout.BeginHorizontal();

                        if (managerScript.splats[selected_splat_index].useFlowMapMask)
                        {
                            managerScript.splats[selected_splat_index].useFlowMapMask = EditorGUILayout.Toggle("Flow Map Weight", managerScript.splats[selected_splat_index].useFlowMapMask);
                            managerScript.splats[selected_splat_index].paintRules.flowMapWeight = EditorGUILayout.Slider(managerScript.splats[selected_splat_index].paintRules.flowMapWeight, 0f, 1f);
                        }
                        else
                        {
                            managerScript.splats[selected_splat_index].useFlowMapMask = EditorGUILayout.Toggle("Use FlowMap Mask", managerScript.splats[selected_splat_index].useFlowMapMask);
                        }


                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();





                        EditorGUILayout.BeginHorizontal();

                        if (managerScript.splats[selected_splat_index].useConvexityMapMask)
                        {
                            managerScript.splats[selected_splat_index].useConvexityMapMask = EditorGUILayout.Toggle("Convexity Map Weight", managerScript.splats[selected_splat_index].useConvexityMapMask);
                            managerScript.splats[selected_splat_index].paintRules.convexityMapWeight = EditorGUILayout.Slider(managerScript.splats[selected_splat_index].paintRules.convexityMapWeight, 0f, 1f);
                        }
                        else
                        {
                            managerScript.splats[selected_splat_index].useConvexityMapMask = EditorGUILayout.Toggle("Use Convexity Mask", managerScript.splats[selected_splat_index].useConvexityMapMask);
                        }

                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();




                        EditorGUILayout.BeginHorizontal();

                        if (managerScript.splats[selected_splat_index].useConcavitiyMapMask)
                        {
                            managerScript.splats[selected_splat_index].useConcavitiyMapMask = EditorGUILayout.Toggle("Concavity Map Weight", managerScript.splats[selected_splat_index].useConcavitiyMapMask);
                            managerScript.splats[selected_splat_index].paintRules.concavityMapWeight = EditorGUILayout.Slider(managerScript.splats[selected_splat_index].paintRules.concavityMapWeight, 0f, 1f);
                        }
                        else
                        {
                            managerScript.splats[selected_splat_index].useConcavitiyMapMask = EditorGUILayout.Toggle("Use Concavity Mask", managerScript.splats[selected_splat_index].useConcavitiyMapMask);
                        }

                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();








                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();



                        EditorGUILayout.LabelField("Height  :  Min  --  Max  --  Min Height Bias");
                        EditorGUILayout.BeginHorizontal();
                        managerScript.splats[selected_splat_index].paintRules.minHeight = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.minHeight), 0f, managerScript.maxTerrainHeight);
                        managerScript.splats[selected_splat_index].paintRules.maxHeight = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.maxHeight), 0f, managerScript.maxTerrainHeight);
                        managerScript.splats[selected_splat_index].paintRules.minHeightBias = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.minHeightBias), 0f, managerScript.maxTerrainHeight);
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.MinMaxSlider(ref managerScript.splats[selected_splat_index].paintRules.minHeight, ref managerScript.splats[selected_splat_index].paintRules.maxHeight, 0f, managerScript.maxTerrainHeight);
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();



                        EditorGUILayout.LabelField("Slope  :  Min  --  Max  --  Min Slope Bias");
                        EditorGUILayout.BeginHorizontal();
                        managerScript.splats[selected_splat_index].paintRules.minSlope = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.minSlope), 0f, 90f);
                        managerScript.splats[selected_splat_index].paintRules.maxSlope = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.maxSlope), 0f, 90f);
                        managerScript.splats[selected_splat_index].paintRules.minSlopeBias = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.minSlopeBias), 0f, 90f);
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.MinMaxSlider(ref managerScript.splats[selected_splat_index].paintRules.minSlope, ref managerScript.splats[selected_splat_index].paintRules.maxSlope, 0f, 90f);
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginHorizontal();
                        managerScript.splats[selected_splat_index].paintRules.minHeightBias = EditorGUILayout.Slider("Min Height Bias", managerScript.splats[selected_splat_index].paintRules.minHeightBias, 0f, managerScript.maxTerrainHeight);
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        managerScript.splats[selected_splat_index].paintRules.minSlopeBias = EditorGUILayout.Slider("Min Slope Bias", managerScript.splats[selected_splat_index].paintRules.minSlopeBias, 0f, 90f);
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Separator();
                        EditorGUILayout.BeginHorizontal();
                        managerScript.splats[selected_splat_index].paintRules.biasFrequency = EditorGUILayout.Slider("Bias Ferquency", managerScript.splats[selected_splat_index].paintRules.biasFrequency, 0f, 20f);
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();
                        EditorGUILayout.Separator();



                        if (EditorGUI.EndChangeCheck())
                        {
                            managerScript.SaveSplatParameterModifications(selected_splat_index);
                            managerScript.UpdateSplats();
                        }
                    }

                }
            }




        }

        else if (managerScript.isUpdating == false)
        {
            managerScript.computeShader = (ComputeShader)EditorGUILayout.ObjectField("ComputeShader", managerScript.computeShader, typeof(ComputeShader));
        }
        else
        {
            EditorGUILayout.LabelField("Updating terrains...");
        }
    }



    void OnSceneGUI()
    {
        if (managerScript.isInitialized)
        {
            if (managerScript.hasTerrainHeigtmapChanged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    managerScript.UpdateTerrains();
                }
            }
        }
    }




}
