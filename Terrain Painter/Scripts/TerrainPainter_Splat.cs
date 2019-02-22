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
    public TerrainPainter.MapMaskEffect aspectMapEffect;
    public TerrainPainter.MapMaskEffect textureMapEffect;
    public bool useFlowMapMask;
    public bool useConvexityMapMask;
    public bool useConcavityMapMask;
    public bool useAspectMap;
    public bool useTextureMap;
    public bool isInverseFlowMapHeightWeight;
    public bool isInverseFlowMapSlopeWeight;
    public Texture textureMap ;
    public TerrainPainter.TextureMapChannel textureMapChannel ;
}
