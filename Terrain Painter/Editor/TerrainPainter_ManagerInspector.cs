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

        if (managerScript.computeShader)
        {
            if(managerScript.isUpdating == false)
            {
                if(managerScript.isInitialized)
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
                        managerScript.flowMapIteration = Mathf.Clamp(EditorGUILayout.DelayedIntField("Flow Map Iteration Count", managerScript.flowMapIteration), 0, 30);
                        if (EditorGUI.EndChangeCheck())
                        {
                            managerScript.UpdateFlowMap(true);
                        }

                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginHorizontal();
                        managerScript.blurSplatmap = EditorGUILayout.Toggle("Blur Splatmap", managerScript.blurSplatmap);
                        EditorGUILayout.Separator();
                        EditorGUILayout.EndHorizontal();


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
                                managerScript.UpdateFlowMap(true);
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Separator();



                        if (GUILayout.Button("Update Splatmap"))
                        {
                            managerScript.UpdateSplatmapMap(true);
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
                                EditorGUILayout.Separator();
                                EditorGUILayout.Separator();
                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


                                if(managerScript.splats[selected_splat_index].splatType != SplatType.Base)
                                {

                                    if (managerScript.splats[selected_splat_index].useFlowMapMask)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        managerScript.splats[selected_splat_index].useFlowMapMask = EditorGUILayout.Toggle("Use FlowMap Mask", managerScript.splats[selected_splat_index].useFlowMapMask);
                                        managerScript.splats[selected_splat_index].flowMapEffect = (MapMaskEffect)EditorGUILayout.EnumPopup("FlowMap Effect", managerScript.splats[selected_splat_index].flowMapEffect);
                                        EditorGUILayout.EndHorizontal();
                                                                            
                                        managerScript.splats[selected_splat_index].paintRules.flowMapWeight = EditorGUILayout.Slider("Flow Map Weight", managerScript.splats[selected_splat_index].paintRules.flowMapWeight, 0f, 1f);
                                        managerScript.splats[selected_splat_index].paintRules.flowMapTransition = EditorGUILayout.Slider("Flow Map Transition", managerScript.splats[selected_splat_index].paintRules.flowMapTransition, 0f, 1f);
                                        managerScript.splats[selected_splat_index].paintRules.flowMapScale = EditorGUILayout.Slider("Flow Map Scale", managerScript.splats[selected_splat_index].paintRules.flowMapScale, 1f, 1000f);
                                        EditorGUILayout.Separator();
                                        managerScript.splats[selected_splat_index].paintRules.flowMapHeightWeight = EditorGUILayout.Slider("Flow Map Height Weight", managerScript.splats[selected_splat_index].paintRules.flowMapHeightWeight, 0f, 1f);
                                        managerScript.splats[selected_splat_index].isInverseFlowMapHeightWeight = EditorGUILayout.Toggle("Use Inverse Flow Map Height Weight", managerScript.splats[selected_splat_index].isInverseFlowMapHeightWeight);
                                        EditorGUILayout.Separator();
                                        managerScript.splats[selected_splat_index].paintRules.flowMapSlopeWeight = EditorGUILayout.Slider("Flow Map Slope Weight", managerScript.splats[selected_splat_index].paintRules.flowMapSlopeWeight, 0f, 1f);
                                        managerScript.splats[selected_splat_index].isInverseFlowMapSlopeWeight = EditorGUILayout.Toggle("Use Inverse Flow Map Slope Weight", managerScript.splats[selected_splat_index].isInverseFlowMapSlopeWeight);
                                    }
                                    else
                                    {
                                        managerScript.splats[selected_splat_index].useFlowMapMask = EditorGUILayout.Toggle("Use FlowMap Mask", managerScript.splats[selected_splat_index].useFlowMapMask);
                                    }

                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);




                                    if (managerScript.splats[selected_splat_index].useConvexityMapMask)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        managerScript.splats[selected_splat_index].useConvexityMapMask = EditorGUILayout.Toggle("Use Convexity Mask", managerScript.splats[selected_splat_index].useConvexityMapMask);
                                        managerScript.splats[selected_splat_index].convexityMapEffect = (MapMaskEffect)EditorGUILayout.EnumPopup("Convexity Map Effect", managerScript.splats[selected_splat_index].convexityMapEffect);
                                        EditorGUILayout.EndHorizontal();

                                        managerScript.splats[selected_splat_index].paintRules.convexityMapWeight = EditorGUILayout.Slider("Convexity Map Weight",managerScript.splats[selected_splat_index].paintRules.convexityMapWeight, 0.01f, 1f);
                                        managerScript.splats[selected_splat_index].paintRules.convexityMapTransition = EditorGUILayout.Slider("Convexity Map Weight Transition",managerScript.splats[selected_splat_index].paintRules.convexityMapTransition, 0.001f, 1f);
                                        managerScript.splats[selected_splat_index].paintRules.convexityMapScale = EditorGUILayout.Slider("Convexity Map Scale",managerScript.splats[selected_splat_index].paintRules.convexityMapScale, 1f, 1000f);                               
                                    }
                                    else
                                    {
                                        managerScript.splats[selected_splat_index].useConvexityMapMask = EditorGUILayout.Toggle("Use Convexity Mask", managerScript.splats[selected_splat_index].useConvexityMapMask);
                                    }

                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                                    if (managerScript.splats[selected_splat_index].useConcavityMapMask)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        managerScript.splats[selected_splat_index].useConcavityMapMask = EditorGUILayout.Toggle("Use Concavity Mask", managerScript.splats[selected_splat_index].useConcavityMapMask);
                                        managerScript.splats[selected_splat_index].concavityMapEffect = (MapMaskEffect)EditorGUILayout.EnumPopup("Concavity Map Effect", managerScript.splats[selected_splat_index].concavityMapEffect);
                                        EditorGUILayout.EndHorizontal();
                                
                                        managerScript.splats[selected_splat_index].paintRules.concavityMapWeight = EditorGUILayout.Slider("Concavity Map Weight", managerScript.splats[selected_splat_index].paintRules.concavityMapWeight, 0f, 1f);
                                        managerScript.splats[selected_splat_index].paintRules.concavityMapTransition = EditorGUILayout.Slider("Concavity Map Weight Transition", managerScript.splats[selected_splat_index].paintRules.concavityMapTransition, 0.001f, 1f);
                                        managerScript.splats[selected_splat_index].paintRules.concavityMapScale = EditorGUILayout.Slider("Concavity Map Scale",managerScript.splats[selected_splat_index].paintRules.concavityMapScale, 1f, 1000f); 
                                    }
                                    else
                                    {
                                        managerScript.splats[selected_splat_index].useConcavityMapMask = EditorGUILayout.Toggle("Use Concavity Mask", managerScript.splats[selected_splat_index].useConcavityMapMask);
                                    }
                                    



                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


                                    if (managerScript.splats[selected_splat_index].useAspectMap)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        managerScript.splats[selected_splat_index].useAspectMap = EditorGUILayout.Toggle("Use Aspect Mask", managerScript.splats[selected_splat_index].useAspectMap);
                                        managerScript.splats[selected_splat_index].aspectMapEffect = (MapMaskEffect)EditorGUILayout.EnumPopup("Aspect Map Effect", managerScript.splats[selected_splat_index].aspectMapEffect);
                                        EditorGUILayout.EndHorizontal();
                                
                                        managerScript.splats[selected_splat_index].paintRules.aspectMapWeight = EditorGUILayout.Slider("Aspect Map Weight", managerScript.splats[selected_splat_index].paintRules.aspectMapWeight, 0f, 1f);
                                        managerScript.splats[selected_splat_index].paintRules.aspectMapPower = EditorGUILayout.Slider("Aspect Map Power", managerScript.splats[selected_splat_index].paintRules.aspectMapPower, 1f, 10f);
                                        managerScript.splats[selected_splat_index].paintRules.aspectMapDirection = EditorGUILayout.Slider("Aspect Map Direction", managerScript.splats[selected_splat_index].paintRules.aspectMapDirection, 0f, 360f);
                                    }
                                    else
                                    {
                                        managerScript.splats[selected_splat_index].useAspectMap = EditorGUILayout.Toggle("Use Aspect Mask", managerScript.splats[selected_splat_index].useAspectMap);
                                    }




                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                                    if (managerScript.splats[selected_splat_index].useTextureMap)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        managerScript.splats[selected_splat_index].useTextureMap = EditorGUILayout.Toggle("Use Texture Mask", managerScript.splats[selected_splat_index].useTextureMap);
                                        managerScript.splats[selected_splat_index].textureMapEffect = (MapMaskEffect)EditorGUILayout.EnumPopup("Texture Map Effect", managerScript.splats[selected_splat_index].textureMapEffect);
                                        EditorGUILayout.EndHorizontal();
                                
                                        managerScript.splats[selected_splat_index].paintRules.textureMapWeight = EditorGUILayout.Slider("Texture Map Weight", managerScript.splats[selected_splat_index].paintRules.textureMapWeight, 0f, 1f);
                                        
                                        EditorGUILayout.BeginHorizontal();
                                        Texture _newTextureMap = (Texture)EditorGUILayout.ObjectField("Texture and Channel",managerScript.splats[selected_splat_index].textureMap, typeof(Texture));
                                        if(_newTextureMap)
                                        {
                                            if(_newTextureMap.isReadable == false)
                                            {
                                                Debug.LogError("Texture Mask must be imported as isReadable. Texture : " + managerScript.splats[selected_splat_index].textureMap);
                                            }
                                            else
                                            {
                                                managerScript.splats[selected_splat_index].textureMap = _newTextureMap ;
                                            }
                                        }
                                        else
                                        {
                                            managerScript.splats[selected_splat_index].textureMap = null ;
                                        }
                                        managerScript.splats[selected_splat_index].textureMapChannel = (TextureMapChannel)EditorGUILayout.EnumPopup(managerScript.splats[selected_splat_index].textureMapChannel);
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    else
                                    {
                                        managerScript.splats[selected_splat_index].useTextureMap = EditorGUILayout.Toggle("Use Texture Mask", managerScript.splats[selected_splat_index].useTextureMap);
                                    }






                                    EditorGUILayout.Separator();
                                }


                                EditorGUILayout.Separator();


                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                                if(managerScript.splats[selected_splat_index].splatType == SplatType.Snow)
                                {
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();

                                    managerScript.splats[selected_splat_index].paintRules.snowAmount = EditorGUILayout.Slider("Snow Amount", managerScript.splats[selected_splat_index].paintRules.snowAmount, 0f, 1f);
                                    managerScript.splats[selected_splat_index].paintRules.snowTransitionSize = EditorGUILayout.Slider("Snow Transition Size", managerScript.splats[selected_splat_index].paintRules.snowTransitionSize, 0f, 0.1f);
                                    managerScript.splats[selected_splat_index].paintRules.snowTransitionFrequency = EditorGUILayout.Slider("Snow Transition Ferquency", managerScript.splats[selected_splat_index].paintRules.snowTransitionFrequency, 0f, 20f);
                                    managerScript.splats[selected_splat_index].paintRules.snowTransitionCutoff = EditorGUILayout.Slider("Snow Transition Cutoff", managerScript.splats[selected_splat_index].paintRules.snowTransitionCutoff, 0f, 1f);

                                }
                                else if(managerScript.splats[selected_splat_index].splatType == SplatType.Default)
                                {
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();

                                    EditorGUILayout.LabelField("Height  :   Min Start   --   Min End   --   Max Start   --   Max End");
                                    EditorGUILayout.BeginHorizontal();
                                    managerScript.splats[selected_splat_index].paintRules.heightMinStart = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.heightMinStart), 0f,  managerScript.splats[selected_splat_index].paintRules.heightMinEnd);
                                    managerScript.splats[selected_splat_index].paintRules.heightMinEnd = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.heightMinEnd), 0f, managerScript.maxTerrainHeight);
                                    managerScript.splats[selected_splat_index].paintRules.heightMaxStart = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.heightMaxStart), 0f, managerScript.maxTerrainHeight);
                                    managerScript.splats[selected_splat_index].paintRules.heightMaxEnd = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.heightMaxEnd),  managerScript.splats[selected_splat_index].paintRules.heightMaxStart, managerScript.maxTerrainHeight);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.MinMaxSlider(ref managerScript.splats[selected_splat_index].paintRules.heightMinEnd, ref managerScript.splats[selected_splat_index].paintRules.heightMaxStart, 0f, managerScript.maxTerrainHeight);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();
                                    
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.MinMaxSlider(ref managerScript.splats[selected_splat_index].paintRules.heightMinStart, ref managerScript.splats[selected_splat_index].paintRules.heightMaxEnd, 0f, managerScript.maxTerrainHeight);
                                    managerScript.splats[selected_splat_index].paintRules.heightMinStart = Mathf.Clamp(managerScript.splats[selected_splat_index].paintRules.heightMinStart, 0f, managerScript.splats[selected_splat_index].paintRules.heightMinEnd);
                                    managerScript.splats[selected_splat_index].paintRules.heightMaxEnd = Mathf.Clamp(managerScript.splats[selected_splat_index].paintRules.heightMaxEnd, managerScript.splats[selected_splat_index].paintRules.heightMaxStart, managerScript.maxTerrainHeight);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();
                                    
                                    EditorGUILayout.BeginHorizontal();
                                    managerScript.splats[selected_splat_index].paintRules.heightTransitionFrequency = EditorGUILayout.Slider("Height Transition Ferquency", managerScript.splats[selected_splat_index].paintRules.heightTransitionFrequency, 0f, 20f);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    managerScript.splats[selected_splat_index].paintRules.heightTransitionCutoff = EditorGUILayout.Slider("Height Transition Cutoff", managerScript.splats[selected_splat_index].paintRules.heightTransitionCutoff, 0f, 1f);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();


                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();


                                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();


                                    EditorGUILayout.LabelField("Slope  :   Min Start   --   Min End   --   Max Start   --   Max End");
                                    EditorGUILayout.BeginHorizontal();
                                    managerScript.splats[selected_splat_index].paintRules.slopeMinStart = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.slopeMinStart), 0f, managerScript.splats[selected_splat_index].paintRules.slopeMinEnd);
                                    managerScript.splats[selected_splat_index].paintRules.slopeMinEnd = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.slopeMinEnd), 0f, 90f);
                                    managerScript.splats[selected_splat_index].paintRules.slopeMaxStart = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.slopeMaxStart), 0f, 90f);
                                    managerScript.splats[selected_splat_index].paintRules.slopeMaxEnd = Mathf.Clamp(EditorGUILayout.DelayedFloatField(managerScript.splats[selected_splat_index].paintRules.slopeMaxEnd), managerScript.splats[selected_splat_index].paintRules.slopeMaxStart, 90f);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.MinMaxSlider(ref managerScript.splats[selected_splat_index].paintRules.slopeMinEnd, ref managerScript.splats[selected_splat_index].paintRules.slopeMaxStart, 0f, 90f);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.MinMaxSlider(ref managerScript.splats[selected_splat_index].paintRules.slopeMinStart, ref managerScript.splats[selected_splat_index].paintRules.slopeMaxEnd, 0f, 90f);
                                    managerScript.splats[selected_splat_index].paintRules.slopeMinStart = Mathf.Clamp(managerScript.splats[selected_splat_index].paintRules.slopeMinStart, 0f, managerScript.splats[selected_splat_index].paintRules.slopeMinEnd);
                                    managerScript.splats[selected_splat_index].paintRules.slopeMaxEnd = Mathf.Clamp(managerScript.splats[selected_splat_index].paintRules.slopeMaxEnd, managerScript.splats[selected_splat_index].paintRules.slopeMaxStart, 90f);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    managerScript.splats[selected_splat_index].paintRules.slopeTransitionFrequency = EditorGUILayout.Slider("Slope Transition Ferquency", managerScript.splats[selected_splat_index].paintRules.slopeTransitionFrequency, 0f, 20f);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    managerScript.splats[selected_splat_index].paintRules.slopeTransitionCutoff = EditorGUILayout.Slider("Slope Transition Cutoff", managerScript.splats[selected_splat_index].paintRules.slopeTransitionCutoff, 0f, 1f);
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.EndHorizontal();


                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                }
                                else
                                {
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.LabelField("Base splat don't have paint rule parameters.");
                                }

                                

                                EditorGUILayout.Separator();
                                EditorGUILayout.Separator();
                                EditorGUILayout.Separator();
                                EditorGUILayout.Separator();
                                EditorGUILayout.Separator();



                                if (EditorGUI.EndChangeCheck())
                                {
                                    managerScript.SaveSplatParameterModifications(selected_splat_index);
                                    managerScript.UpdateSplatmapMap(true);
                                }
                            }

                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Not initialized.");
                    if(GUILayout.Button("FindTerrains"))
                    {
                        managerScript.FindTerrains();
                    } 
                }
            }
            else
            {
                EditorGUILayout.LabelField("Updating terrains...");
                if(GUILayout.Button("FindTerrains"))
                {
                    managerScript.FindTerrains();
                }
            }
        }
        else
        {
            if (managerScript.isUpdating == false)
            {
                managerScript.computeShader = (ComputeShader)EditorGUILayout.ObjectField("ComputeShader", managerScript.computeShader, typeof(ComputeShader));
            }
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
