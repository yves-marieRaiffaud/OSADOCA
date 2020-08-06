using UnityEngine;
using UnityEditor;
 
public static class Obj2Terrain
{
	/*private int resolution = 512;
	private Vector3 addTerrain;
	int bottomTopRadioSelected = 0;
	private float shiftHeight = 0f;*/
    static string[] bottomTopRadio = new string[] { "Bottom Up", "Top Down"};
 
	public static void CreateTerrain(Transform parent, MeshCollider collider, int resolution, Vector3 terrainOffset, float shiftHeight, int bottomTopRadioSelected)
    {	
        GameObject terrainObj;
        Terrain terrainObject;
        TerrainCollider terrainCollider;

        Vector3 addTerrain = terrainOffset;
		TerrainData terrain = new TerrainData();
		terrain.heightmapResolution = resolution;
        if(parent.Find("Terrain"))
        {
            terrainObj = parent.Find("Terrain").gameObject;
            terrainObject = terrainObj.GetComponent<Terrain>();
            terrainCollider = terrainObj.GetComponent<TerrainCollider>();
        }
        else {
            terrainObj = new GameObject("Terrain", typeof(Terrain), typeof(TerrainCollider));
            terrainObj.transform.parent = parent;
            terrainObject = terrainObj.GetComponent<Terrain>();
            terrainCollider = terrainObj.GetComponent<TerrainCollider>();
        }
 
		Bounds bounds = collider.bounds;	
		float sizeFactor = collider.bounds.size.y / (collider.bounds.size.y + addTerrain.y);
		terrain.size = collider.bounds.size + addTerrain;
		bounds.size = new Vector3(terrain.size.x, collider.bounds.size.y, terrain.size.z);
 
		// Do raycasting samples over the object to see what terrain heights should be
		float[,] heights = new float[terrain.heightmapResolution, terrain.heightmapResolution];	
		Ray ray = new Ray(new Vector3(bounds.min.x, bounds.max.y + bounds.size.y, bounds.min.z), -Vector3.up);
		RaycastHit hit = new RaycastHit();
		float meshHeightInverse = 1 / bounds.size.y;
		Vector3 rayOrigin = ray.origin;
 
		int maxHeight = heights.GetLength(0);
		int maxLength = heights.GetLength(1);
 
		Vector2 stepXZ = new Vector2(bounds.size.x / maxLength, bounds.size.z / maxHeight);
 
		for(int zCount = 0; zCount < maxHeight; zCount++){
 
			for(int xCount = 0; xCount < maxLength; xCount++){
 
				float height = 0.0f;
 
				if(collider.Raycast(ray, out hit, bounds.size.y * 3)){
 
					height = (hit.point.y - bounds.min.y) * meshHeightInverse;
					height += shiftHeight;
 
					//bottom up
					if(bottomTopRadioSelected == 0){
 
						height *= sizeFactor;
					}
 
					//clamp
					if(height < 0){
 
						height = 0;
					}
				}
 
				heights[zCount, xCount] = height;
           		rayOrigin.x += stepXZ[0];
           		ray.origin = rayOrigin;
			}
 
			rayOrigin.z += stepXZ[1];
      		rayOrigin.x = bounds.min.x;
      		ray.origin = rayOrigin;
		}
 
		terrain.SetHeights(0, 0, heights);
        terrainObject.terrainData = terrainCollider.terrainData = terrain;
	}
}