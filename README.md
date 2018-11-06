# Unity-TerrainPainter

splatmap generated when terrrain heightmap edited. procedural splatmap generation executed on GPU.

support tiled terrain splating. but there is problem on edge of terrain. there may be are visible seam.

![Screenshot](https://github.com/drParadox312/Unity-TerrainPainter/blob/master/Terrain%20Painter/Screenshots/Screenshot%201.JPG)

![Screenshot](https://github.com/drParadox312/Unity-TerrainPainter/blob/master/Terrain%20Painter/Screenshots/Screenshot%202.JPG)


using asset :
1- create TerrainPainter_Splat object in the asset folder with right click menu.
2- assign TerrianLayer to TerrainPainter_Splat object. and set parameters for painting rules. height, slope etc.
3- create a gameobject. add TerrainPainter_Manager compomenent. 
4- add TerrainPainter_Splat objcets to TerrainPainter_Manager. 
5- clikc on the image you want to edit in the list at inspector.
6- after that tweak parameters.

![Screenshot](https://github.com/drParadox312/Unity-TerrainPainter/blob/master/Terrain%20Painter/Screenshots/Screenshot%205.JPG)

![Screenshot](https://github.com/drParadox312/Unity-TerrainPainter/blob/master/Terrain%20Painter/Screenshots/Screenshot%203.JPG)

![Screenshot](https://github.com/drParadox312/Unity-TerrainPainter/blob/master/Terrain%20Painter/Screenshots/Screenshot%204.JPG)


at terrain objcet's inspector panel you can see generated maps : height, slope, snowweight, convexity, concavity, flow.


![Screenshot](https://github.com/drParadox312/Unity-TerrainPainter/blob/master/Terrain%20Painter/Screenshots/Screenshot%206.JPG)

