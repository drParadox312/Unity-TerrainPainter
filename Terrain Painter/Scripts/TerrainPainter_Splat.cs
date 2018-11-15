using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Painter Create New Splat")]
[System.Serializable]
public class TerrainPainter_Splat : ScriptableObject
{
    public string name;
    public TerrainLayer terrainLayer;
    public TerrainPainter.SplatPaintRules paintRules;
    public TerrainPainter.SplatType splatType;
    public TerrainPainter.PaintMethod paintMethod;
    public TerrainPainter.MapMaskEffect flowMapEffect;
    public TerrainPainter.MapMaskEffect convexityMapEffect;
    public TerrainPainter.MapMaskEffect concavityMapEffect;
    public bool isInverseHeightBias;
    public bool isInverseSlopeBias;
    public bool useFlowMapMask;
    public bool useConvexityMapMask;
    public bool useConcavityMapMask;
    public bool isInverseFlowMap ;
    public bool isInverseConvexityMap ;
    public bool isInverseConcavityMap ;
    public bool isInverseFlowMapHeightWeight;
    public bool isInverseFlowMapSlopeWeight;
}
