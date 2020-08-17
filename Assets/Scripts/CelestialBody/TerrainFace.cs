using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Mathd_Lib;

public class TerrainFace
{
    public volatile Mesh mesh;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;
    CelestialBodySettings celestialBodySettingsScript;
    CelestialBody celestialBody;
    public Chunk parentChunk;
    public List<Chunk> visibleChildren = new List<Chunk>();
    public List<float> visibleChildrenDist = new List<float>();
    int chunkIDXMinDistance=-1;
    
    // These will be filled with the generated data
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> borderVertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<int> borderTriangles = new List<int>();
    public Dictionary<int, bool> edgefanIndex = new Dictionary<int, bool>();

    Texture2D bumpMap;
    bool isDefaultHeightMap;
    Transform activeShip;
    Transform activeCamera;

    // Constructor
    public TerrainFace(Mesh mesh, Vector3 localUp, CelestialBody celestialBody, Texture2D bumpMapTexture, bool isDefaultHeightMapBool, Transform playerActiveShip, Transform activeCamera)
    {
        this.mesh = mesh;
        this.localUp = localUp;
        this.celestialBody = celestialBody;
        this.celestialBodySettingsScript = celestialBody.settings;
        this.radius = (float)celestialBody.settings.radiusU;
        this.bumpMap = bumpMapTexture;
        this.isDefaultHeightMap = isDefaultHeightMapBool;
        this.activeShip = playerActiveShip;
        this.activeCamera = activeCamera;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    // Construct a quadtree of chunks (even though the chunks end up 3D, they start out 2D in the quadtree and are later projected onto a sphere)
    public void ConstructTree()
    {
        // Resets the mesh
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        borderVertices.Clear();
        borderTriangles.Clear();
        visibleChildren.Clear();
        visibleChildrenDist.Clear();
        chunkIDXMinDistance=-1; // -1 as initialization value, meaning that the visibleChildrenDist List is null/empty

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Extend the resolution capabilities of the mesh
        
        // Generate chunks
        parentChunk = new Chunk(1, celestialBodySettingsScript, celestialBody, this, null, localUp.normalized * radius, radius, 0, localUp, axisA, axisB, new byte[4], 0, activeShip, activeCamera, bumpMap, isDefaultHeightMap);
        parentChunk.GenerateChildren();

        // Get chunk mesh data
        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();
        if(visibleChildrenDist.Count > 0)
        {
            chunkIDXMinDistance = UsefulFunctions.ListFloatArgMin(visibleChildrenDist);
        }
        
        foreach (Chunk child in visibleChildren)
        {
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);

            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            borderTriangles.AddRange(verticesAndTriangles.Item3);
            borderVertices.AddRange(verticesAndTriangles.Item4);
            normals.AddRange(verticesAndTriangles.Item5);
            triangleOffset += verticesAndTriangles.Item1.Length;
            borderTriangleOffset += verticesAndTriangles.Item4.Length;
        }

        // Reset mesh and apply new data
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
    }

    // Update the quadtree
    public void UpdateTree()
    {
        // Resets the mesh
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        borderVertices.Clear();
        borderTriangles.Clear();
        visibleChildren.Clear();
        visibleChildrenDist.Clear();
        edgefanIndex.Clear();

        parentChunk.UpdateChunk();

        // Get chunk mesh data
        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();

        if(visibleChildrenDist.Count > 0)
        {
            chunkIDXMinDistance = UsefulFunctions.ListFloatArgMin(visibleChildrenDist);
        }
        //=====================================
        int idxCounter = 0;
        int idxOfChildToRemove = 0; // index in the visibleChildren list of the child that is replaced by the ground prefab for colliding with the ship
        bool mustRemovethisChild = false;
        bool hasFoundLOD7Child = false;
        //=====================================
        foreach (Chunk child in visibleChildren)
        {
            mustRemovethisChild = false;
            //====================
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) verticesAndTriangles = (new Vector3[0], new int[0], new int[0], new Vector3[0], new Vector3[0]);
            if (child.vertices == null)
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);
            }
            else if (child.vertices.Length == 0 || child.triangles != Presets.quadTemplateTriangles[(child.neighbours[0] | child.neighbours[1] * 2 | child.neighbours[2] * 4 | child.neighbours[3] * 8)])
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);
            }
            else
            {
                verticesAndTriangles = (child.vertices, child.GetTrianglesWithOffset(triangleOffset), child.GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset), child.borderVertices, child.normals);
            }

            if(child.detailLevel == 7 && idxCounter == chunkIDXMinDistance)
            {
                // Removing the current child as it will be replaced with a Ground meshCollider/Box Collider (thus avoiding glitching due to texture superposition)
                idxOfChildToRemove = idxCounter;
                mustRemovethisChild = true;
                hasFoundLOD7Child = true;
                //===================================
                GameObject groundGO;
                MeshCollider meshCollider;
                if(celestialBody.transform.Find("ground"))
                {
                    groundGO = celestialBody.transform.Find("ground").gameObject;
                    meshCollider = groundGO.GetComponent<MeshCollider>();
                }
                else {
                    groundGO = new GameObject("ground", typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer));
                    groundGO.transform.parent = celestialBody.transform;
                    groundGO.transform.localRotation = Quaternion.identity;
                    // NEED TO TAKE INTO ACCOUNT THE FLATENNING OF THE CELESTIAL BODY FOR THE POSITION
                    groundGO.transform.localPosition = Vector3.zero;
                    //================
                    groundGO.layer = 9;
                    MeshRenderer meshRenderer = groundGO.GetComponent<MeshRenderer>();
                    meshRenderer.material = Resources.Load<Material>("Ground");
                    //================
                    PhysicMaterial colliderMaterial = new PhysicMaterial("MeshColliderMaterial");
                    colliderMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
                    colliderMaterial.bounciness = 0.01f;
                    colliderMaterial.dynamicFriction = 1f;
                    colliderMaterial.frictionCombine = PhysicMaterialCombine.Multiply;
                    colliderMaterial.staticFriction = 1f;
                    //======
                    meshCollider = groundGO.GetComponent<MeshCollider>();
                    meshCollider.convex = true;
                    meshCollider.material = colliderMaterial;
                }

                MeshFilter meshFilter = groundGO.GetComponent<MeshFilter>();
                Mesh tmpMesh = new Mesh();
                tmpMesh.name = "closestChunk";
                tmpMesh.vertices = verticesAndTriangles.Item1;
                tmpMesh.triangles = child.triangles;
                tmpMesh.normals = verticesAndTriangles.Item5;
                meshFilter.mesh = tmpMesh;
                meshCollider.sharedMesh = tmpMesh;
                //========================================
                //========================================
                //===============TEST=====================
                /*List<Vector3> verticesAndBorderVerticesList = new List<Vector3>();
                verticesAndBorderVerticesList.AddRange(verticesAndTriangles.Item1); // adding regular vertices
                verticesAndBorderVerticesList.AddRange(verticesAndTriangles.Item4); // adding border vertices
                Vector3[] verticesAndBorderVerticesArr = verticesAndBorderVerticesList.ToArray();
                //===============
                List<int> trianglesAndBorderTrianglesList = new List<int>();
                trianglesAndBorderTrianglesList.AddRange(child.triangles); // adding regular triangles
                int[] tmpBorderTrianglesArr = child.borderTriangles;
                for(int i = 0; i < tmpBorderTrianglesArr.Length; i++)
                {
                    tmpBorderTrianglesArr[i] +=  -child.borderTriangles[0] + child.triangles.Length; // offsetting each triangles index 
                }
                Debug.Log("solo arr: \n" + string.Join("\n", tmpBorderTrianglesArr));
                trianglesAndBorderTrianglesList.AddRange(tmpBorderTrianglesArr); // adding border triangles
                int[] trianglesAndBorderTrianglesArr = trianglesAndBorderTrianglesList.ToArray();
                //===============*/

                //========================================
                //========================================
                if(celestialBody.name == "Earth")
                {
                    Debug.Log("vertices: \n" + string.Join("\n", verticesAndTriangles.Item1));
                    Debug.Log("triangles: \n" + string.Join("\n", child.triangles));
                    Debug.Log("borderVertices: \n" + string.Join("\n", verticesAndTriangles.Item4));
                    Debug.Log("borderTriangles: \n" + string.Join("\n", child.borderTriangles));
                    Debug.Log("========================");
                }
            }

            if(!mustRemovethisChild)
            {
                vertices.AddRange(verticesAndTriangles.Item1);
                triangles.AddRange(verticesAndTriangles.Item2);
                borderTriangles.AddRange(verticesAndTriangles.Item3);
                borderVertices.AddRange(verticesAndTriangles.Item4);
                normals.AddRange(verticesAndTriangles.Item5);

                // Increase offset to accurately point to the next slot in the lists
                triangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
                borderTriangleOffset += verticesAndTriangles.Item4.Length;
            }
            //===============
            idxCounter++;
        }
        // Removing the child that is replaced by the ground collider from the 'visibleChildren' List<Chunk>
        visibleChildren.RemoveAt(idxOfChildToRemove);
        //===============================================
        //===============================================
        Vector2[] uvs = new Vector2[vertices.Count];

        float planetScriptSizeDivide = (1 / (float)celestialBodySettingsScript.radiusU);
        float twoPiDivide = (1 / (2 * Mathf.PI));

        for (int i = 0; i < uvs.Length; i++)
        {
            Vector3 d = vertices[i] * planetScriptSizeDivide;
            float u = 0.5f + Mathf.Atan2(d.z, d.x) * twoPiDivide;
            float v = 0.5f - Mathf.Asin(-d.y) / Mathf.PI; // '-d.y' to have a positive value for the y tilling of the texture

            uvs[i] = new Vector2(u, v);
        }
        //===========
        //===========
        // Reset mesh and apply new data
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs;
    }
}

public class Chunk
{
    public uint hashvalue; // First bit is not used for anything but preserving zeros in the beginning
    public CelestialBodySettings celestialBodySettingsScript;
    public CelestialBody celestialBody;
    public TerrainFace terrainFace;

    public Chunk[] children;
    public Vector3 position;
    public float radius;
    public int detailLevel;
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;
    public byte corner;

    public Vector3 normalizedPos;

    public Vector3[] vertices;
    public Vector3[] borderVertices;
    public int[] triangles;
    public int[] borderTriangles;
    public Vector3[] normals;
    
    Transform universePlayerCamera;
    Transform universePlayerShip;
    Texture2D initialBumpMap;
    bool hasDefaultHeightmap; // Meaning the heightmap is all black (no elevation), so some calculations will not be done
    float MAX_ALTITUDE;
    float pixelWidthOffset;

    public byte[] neighbours = new byte[4]; //East, west, north, south. True if less detailed (Lower LOD)
    // Constructor
    public Chunk(uint hashvalue, CelestialBodySettings celestialBodySettingsScript, CelestialBody celestialBody, TerrainFace terrainFace, Chunk[] children, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, byte[] neighbours, byte corner, Transform activeShip, Transform activeCamera, Texture2D bumpMapTexture, bool isDefaultHeightMapBool)
    {
        this.hashvalue = hashvalue;
        this.celestialBodySettingsScript = celestialBodySettingsScript;
        this.celestialBody = celestialBody;
        this.terrainFace = terrainFace;
        this.children = children;
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
        this.neighbours = neighbours;
        this.corner = corner;
        this.normalizedPos = position.normalized;

        this.initialBumpMap = bumpMapTexture;
        this.MAX_ALTITUDE = (float)UniCsts.pl2u*(float)celestialBodySettingsScript.planetBaseParamsDict[CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()].value;
        this.pixelWidthOffset = initialBumpMap.width - (0.5f + Mathf.Atan2(1f, 0f) / (2f*Mathf.PI)) * initialBumpMap.width;

        this.universePlayerCamera = activeCamera;
        this.universePlayerShip = activeShip;
        this.hasDefaultHeightmap = isDefaultHeightMapBool;
    }

    public void GenerateChildren()
    {
        // If the detail level is under max level and above 0. Max level depends on how many detail levels are defined in planets and needs to be changed manually.
        if (detailLevel <= celestialBodySettingsScript.detailLevelDistances.Length - 1 && detailLevel >= 0)
        {
            /*Debug.Log("normalizedPos = " + normalizedPos + " ; normalizedPos * (float)celestialBodySettingsScript.radiusU = " + (normalizedPos * (float)celestialBodySettingsScript.radiusU));
            Debug.Log("worldPos vertex = " + (celestialBody.transform.TransformDirection(normalizedPos * (float)celestialBodySettingsScript.radiusU) + celestialBody.transform.position));
            Debug.LogFormat("detailLevel = {0}", detailLevel);
            Debug.Log("===============");*/
            if (Vector3.Distance(celestialBody.transform.TransformDirection(normalizedPos * (float)celestialBodySettingsScript.radiusU) + celestialBody.transform.position, universePlayerShip.position) <= celestialBodySettingsScript.detailLevelDistances[detailLevel])
            {
                // Assign the children of the quad (grandchildren not included). 
                // Position is calculated on a cube and based on the fact that each child has 1/2 the radius of its parent
                // Detail level is increased by 1. This doesn't change anything itself, but rather symbolizes that something HAS been changed (the detail).
                children = new Chunk[4];
                children[0] = new Chunk(hashvalue * 4, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position + axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 0, universePlayerShip, universePlayerCamera, initialBumpMap, hasDefaultHeightmap); // TOP LEFT
                children[1] = new Chunk(hashvalue * 4 + 1, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position + axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 1, universePlayerShip, universePlayerCamera, initialBumpMap, hasDefaultHeightmap); // TOP RIGHT
                children[2] = new Chunk(hashvalue * 4 + 2, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position - axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 2, universePlayerShip, universePlayerCamera, initialBumpMap, hasDefaultHeightmap); // BOTTOM RIGHT
                children[3] = new Chunk(hashvalue * 4 + 3, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position - axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 3, universePlayerShip, universePlayerCamera, initialBumpMap, hasDefaultHeightmap); // BOTTOM LEFT

                // Create grandchildren
                foreach (Chunk child in children)
                {
                    child.GenerateChildren();
                }
            }
        }
    }

    // Update the chunk (and maybe its children too)
    public void UpdateChunk()
    {
        float distanceToPlayer = Vector3.Distance(celestialBody.transform.TransformDirection(normalizedPos * (float)celestialBodySettingsScript.radiusU) + celestialBody.transform.position, universePlayerShip.position);
        /*Debug.Log("normalizedPos = " + normalizedPos + " ; normalizedPos * (float)celestialBodySettingsScript.radiusU = " + (normalizedPos * (float)celestialBodySettingsScript.radiusU));
        Debug.Log("worldPos vertex = " + (celestialBody.transform.TransformDirection(normalizedPos * (float)celestialBodySettingsScript.radiusU) + celestialBody.transform.position));
        Debug.LogFormat("distance = {0} - detailLevel = {1}", distanceToPlayer, detailLevel);
        Debug.Log("===============");*/
        if (detailLevel <= celestialBodySettingsScript.detailLevelDistances.Length - 1)
        {
            if (distanceToPlayer > celestialBodySettingsScript.detailLevelDistances[detailLevel])
            {
                children = new Chunk[0];
            }
            else
            {
                if (children.Length > 0)
                {
                    foreach (Chunk child in children)
                    {
                        child.UpdateChunk();
                    }
                }
                else
                {
                    GenerateChildren();
                }
            }
        }
    }

    // Returns the latest chunk in every branch, aka the ones to be rendered
    public void GetVisibleChildren()
    {
        if (children.Length > 0)
        {
            foreach (Chunk child in children)
            {
                child.GetVisibleChildren();
            }
        }
        else
        {
            float b = Vector3.Distance(celestialBody.transform.TransformDirection(normalizedPos * (float)celestialBodySettingsScript.radiusU) +
                celestialBody.transform.position, universePlayerCamera.position);

            if (Mathf.Acos((((float)(celestialBodySettingsScript.radiusU * celestialBodySettingsScript.radiusU)) + (b * b) -
                celestialBody.distanceToPlayerPow2) / (2 * (float)celestialBodySettingsScript.radiusU * b)) > celestialBodySettingsScript.cullingMinAngle)
            {
                terrainFace.visibleChildren.Add(this);
                terrainFace.visibleChildrenDist.Add(b);
            }
        }
    }

    public void GetNeighbourLOD()
    {
        byte[] newNeighbours = new byte[4];

        if (corner == 0) // Top left
        {
            newNeighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            newNeighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        }
        else if (corner == 1) // Top right
        {
            newNeighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            newNeighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        }
        else if (corner == 2) // Bottom right
        {
            newNeighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            newNeighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        }
        else if (corner == 3) // Bottom left
        {
            newNeighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            newNeighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        }

        neighbours = newNeighbours;
    }

    // Find neighbouring chunks by applying a partial inverse bitmask to the hash
    private byte CheckNeighbourLOD(byte side, uint hash)
    {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < detailLevel * 2) // 0 through 3 can be represented as a two bit number
        {
            count += 2;
            twoLast = (hash & 3); // Get the two last bits of the hash. 0b_10011 --> 0b_11

            bitmask = bitmask * 4; // Add zeroes to the end of the bitmask. 0b_10011 --> 0b_1001100

            // Create mask to get the quad on the opposite side. 2 = 0b_10 and generates the mask 0b_11 which flips it to 1 = 0b_01
            if (side == 2 || side == 3)
            {
                bitmask += 3; // Add 0b_11 to the bitmask
            }
            else
            {
                bitmask += 1; // Add 0b_01 to the bitmask
            }

            // Break if the hash goes in the opposite direction
            if ((side == 0 && (twoLast == 0 || twoLast == 3)) ||
                (side == 1 && (twoLast == 1 || twoLast == 2)) ||
                (side == 2 && (twoLast == 3 || twoLast == 2)) ||
                (side == 3 && (twoLast == 0 || twoLast == 1)))
            {
                break;
            }

            // Remove already processed bits. 0b_1001100 --> 0b_10011
            hash = hash >> 2;
        }

        // Return 1 (true) if the quad in quadstorage is less detailed
        if (terrainFace.parentChunk.GetNeighbourDetailLevel(hashvalue ^ bitmask, detailLevel) < detailLevel)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    // Find the detail level of the neighbouring quad using the querryHash as a map
    public int GetNeighbourDetailLevel(uint querryHash, int dl)
    {
        int dlResult = 0; // dl = detail level

        if (hashvalue == querryHash)
        {
            dlResult = detailLevel;
        }
        else
        {
            if (children.Length > 0)
            {
                dlResult += children[((querryHash >> ((dl - 1) * 2)) & 3)].GetNeighbourDetailLevel(querryHash, dl - 1);
            }
        }

        return dlResult; // Returns 0 if no quad with the given hash is found
    }

    // Return triangles including offset
    public int[] GetTrianglesWithOffset(int triangleOffset)
    {
        int[] newTriangles = new int[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            newTriangles[i] = triangles[i] + triangleOffset;
        }

        return newTriangles;
    }

    // Return triangles without the offset
    public int[] GetTrianglesWithoutOffset(int triangleOffset)
    {
        int[] newTriangles = new int[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            newTriangles[i] = triangles[i] - triangleOffset;
        }

        return newTriangles;
    }

    // Return border triangles including offset
    public int[] GetBorderTrianglesWithOffset(int borderTriangleOffset, int triangleOffset)
    {
        int[] newBorderTriangles = new int[borderTriangles.Length];

        for (int i = 0; i < borderTriangles.Length; i++)
        {
            newBorderTriangles[i] = (borderTriangles[i] < 0) ? borderTriangles[i] - borderTriangleOffset : borderTriangles[i] + triangleOffset;
        }

        return newBorderTriangles;
    }

    public (Vector3[], int[], int[], Vector3[], Vector3[]) CalculateVerticesAndTriangles(int triangleOffset, int borderTriangleOffset)
    {
        Matrix4x4 transformMatrix;
        Vector3 rotationMatrixAttrib = new Vector3(0, 0, 0);
        Vector3 scaleMatrixAttrib = new Vector3(radius, radius, 1);

        // Adjust rotation according to the side of the planet
        if (terrainFace.localUp == Vector3.forward)
        {
            rotationMatrixAttrib = new Vector3(0, 0, 180);
        }
        else if (terrainFace.localUp == Vector3.back)
        {
            rotationMatrixAttrib = new Vector3(0, 180, 0);
        }
        else if (terrainFace.localUp == Vector3.right)
        {
            rotationMatrixAttrib = new Vector3(0, 90, 270);
        }
        else if (terrainFace.localUp == Vector3.left)
        {
            rotationMatrixAttrib = new Vector3(0, 270, 270);
        }
        else if (terrainFace.localUp == Vector3.up)
        {
            rotationMatrixAttrib = new Vector3(270, 0, 90);
        }
        else if (terrainFace.localUp == Vector3.down)
        {
            rotationMatrixAttrib = new Vector3(90, 0, 270);
        }

        // Create transform matrix
        transformMatrix = Matrix4x4.TRS(position, Quaternion.Euler(rotationMatrixAttrib), scaleMatrixAttrib);
  
        // Index of quad template
        int quadIndex = (neighbours[0] | neighbours[1] * 2 | neighbours[2] * 4 | neighbours[3] * 8);

        // Choose a quad from the templates, then move it using the transform matrix, normalize its vertices, scale it and store it
        vertices = new Vector3[(Presets.quadRes + 1) * (Presets.quadRes + 1)];

        if(hasDefaultHeightmap)
        {
            float pixelWidthOffset = initialBumpMap.width - (0.5f + Mathf.Atan2(1f, 0f) / (2f*Mathf.PI)) * initialBumpMap.width;
        }

        float pixel_w, pixel_h, elevation, pi_x_2;
        pi_x_2 = 2f*Mathf.PI;

        float maxRatio = MAX_ALTITUDE / (float)(celestialBodySettingsScript.radiusU);
        for (int i = 0; i < vertices.Length; i++)
        {
            // pointOnCube goes along the x values, then the y values (with a localUp normal along +z axis)
            Vector3 pointOnCube = transformMatrix.MultiplyPoint(Presets.quadTemplateVertices[quadIndex][i]);
            Vector3 pointOnUnitSphere = pointOnCube.normalized;
            
            if(!hasDefaultHeightmap)
            {
                pixel_w = (0.5f + Mathf.Atan2(pointOnUnitSphere.x, pointOnUnitSphere.z) / pi_x_2) * initialBumpMap.width;
                pixel_h = (0.5f - Mathf.Asin(pointOnUnitSphere.y) / Mathf.PI) * initialBumpMap.height;
                pixel_w = (pixel_w-pixelWidthOffset) < 0f ? initialBumpMap.width + (pixel_w-pixelWidthOffset) : pixel_w-pixelWidthOffset;
                pixel_w = pixel_w > initialBumpMap.width ? (pixel_w-initialBumpMap.width) : pixel_w;
                
                Color[] surroundingPixels = initialBumpMap.GetPixels((int)Mathf.Floor(pixel_w), (int)Mathf.Floor(pixel_h), 3, 3);
                float p1 = surroundingPixels[0].grayscale; // bottom left
                float p2 = surroundingPixels[2].grayscale; // bottom right
                float p3 = surroundingPixels[6].grayscale; // top left
                float p4 = surroundingPixels[8].grayscale; // top right
                float medianGrayVal = (p1+p2+p3+p4)/4;

                // First value is minimum grayscale value: 0 == black
                // Second value is the coorepsonding elevation for minimum grayscale value: 0 == ocean floor
                // third value is maximum grayscale value: 255 == white
                // Fourth value is the corresponding elevation for maximum grayscale value: MAX_ALTITUDE in km
                elevation = UsefulFunctions.linearInterpolation(0, 0, 1, MAX_ALTITUDE, medianGrayVal) / (float)(celestialBodySettingsScript.radiusU);
                elevation = 1f + Mathf.Clamp(elevation, 0f, maxRatio); // Adding the altitude elevation to the base surface elevation
            }
            else
            {
                elevation = 1f;
            }
            vertices[i] = pointOnUnitSphere * elevation * (float)celestialBodySettingsScript.radiusU;
        }

        // Do the same for the border vertices
        borderVertices = new Vector3[Presets.quadTemplateBorderVertices[quadIndex].Length];

        for (int i = 0; i < borderVertices.Length; i++)
        {
            Vector3 pointOnCube = transformMatrix.MultiplyPoint(Presets.quadTemplateBorderVertices[quadIndex][i]);
            Vector3 pointOnUnitSphere = pointOnCube.normalized;

            if(!hasDefaultHeightmap)
            {
                pixel_w = (0.5f + Mathf.Atan2(pointOnUnitSphere.x, pointOnUnitSphere.z) / pi_x_2) * initialBumpMap.width;
                pixel_h = (0.5f - Mathf.Asin(pointOnUnitSphere.y) / Mathf.PI) * initialBumpMap.height;
                pixel_w = (pixel_w-pixelWidthOffset) < 0f ? initialBumpMap.width + (pixel_w-pixelWidthOffset) : pixel_w-pixelWidthOffset;
                pixel_w = pixel_w > initialBumpMap.width ? (pixel_w-initialBumpMap.width) : pixel_w;
                
                float medianGrayVal;
                if(pixel_w + 3 > initialBumpMap.width || pixel_h + 3 > initialBumpMap.height)
                {
                    medianGrayVal = initialBumpMap.GetPixel((int)pixel_w, (int)pixel_h).grayscale;
                }
                else {
                    Color[] surroundingPixels = initialBumpMap.GetPixels((int)Mathf.Floor(pixel_w), (int)Mathf.Floor(pixel_h), 3, 3);
                    float p1 = surroundingPixels[0].grayscale; // bottom left
                    float p2 = surroundingPixels[2].grayscale; // bottom right
                    float p3 = surroundingPixels[6].grayscale; // top left
                    float p4 = surroundingPixels[8].grayscale; // top right
                    medianGrayVal = (p1+p2+p3+p4)/4;
                }

                // First value is minimum grayscale value: 0 == black
                // Second value is the coorepsonding elevation for minimum grayscale value: 0 == ocean floor
                // third value is maximum grayscale value: 255 == white
                // Fourth value is the corresponding elevation for maximum grayscale value: MAX_ALTITUDE in km
                elevation = UsefulFunctions.linearInterpolation(0, 0, 1, MAX_ALTITUDE, medianGrayVal) / (float)(celestialBodySettingsScript.radiusU);
                elevation = 1f + Mathf.Clamp(elevation, 0f, maxRatio); // Adding the altitude elevation to the base surface elevation 
            }
            else
            {
                elevation = 1f;
            }

            borderVertices[i] = pointOnUnitSphere * elevation * (float)celestialBodySettingsScript.radiusU;
        }

        // Store the triangles
        triangles = Presets.quadTemplateTriangles[quadIndex];
        borderTriangles = Presets.quadTemplateBorderTriangles[quadIndex];

        // Calculate the normals
        normals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;

        int vertexIndexA;
        int vertexIndexB;
        int vertexIndexC;

        Vector3 triangleNormal;

        int[] edgefansIndices = Presets.quadTemplateEdgeIndices[quadIndex];

        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            vertexIndexA = triangles[normalTriangleIndex];
            vertexIndexB = triangles[normalTriangleIndex + 1];
            vertexIndexC = triangles[normalTriangleIndex + 2];

            triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            // Don't calculate the normals on the edge edgefans here. They are only calculated using the border vertices.
            if (edgefansIndices[vertexIndexA] == 0)
            {
                normals[vertexIndexA] += triangleNormal;
            }
            if (edgefansIndices[vertexIndexB] == 0)
            {
                normals[vertexIndexB] += triangleNormal;
            }
            if (edgefansIndices[vertexIndexC] == 0)
            {
                normals[vertexIndexC] += triangleNormal;
            }
        }

        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            vertexIndexA = borderTriangles[normalTriangleIndex];
            vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            // Apply the normal if the vertex is on the visible edge of the quad
            if (vertexIndexA >= 0 && (vertexIndexA % (Presets.quadRes + 1) == 0 ||
                vertexIndexA % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexA >= 0 && vertexIndexA <= Presets.quadRes) ||
                (vertexIndexA >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexA < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0 && (vertexIndexB % (Presets.quadRes + 1) == 0 ||
                vertexIndexB % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexB >= 0 && vertexIndexB <= Presets.quadRes) ||
                (vertexIndexB >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexB < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0 && (vertexIndexC % (Presets.quadRes + 1) == 0 ||
                vertexIndexC % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexC >= 0 && vertexIndexC <= Presets.quadRes) ||
                (vertexIndexC >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexC < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexC] += triangleNormal;
            }
        }

        // Normalize the result to combine the aproximations into one
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        return (vertices, GetTrianglesWithOffset(triangleOffset), GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset), borderVertices, normals);
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        // Get an aproximation of the vertex normal using two other vertices that share the same triangle
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}