using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Painter Create New Splat")]
[System.Serializable]
public class TerrainPainter_Splat : ScriptableObject
{
    public string name;
    public TerrainLayer terrainLayer;
    public TerrainPainter.SplatType splatType;
    public TerrainPainter.PaintMethod paintMethod;
    public TerrainPainter.SplatPaintRules paintRules;
    public bool useFlowMapMask;
    public bool useConvexityMapMask;
    public bool useConcavitiyMapMask;
}
