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

    // Constructor
    public TerrainFace(Mesh mesh, Vector3 localUp, CelestialBody celestialBody, Texture2D bumpMapTexture, bool isDefaultHeightMapBool, Transform playerActiveShip)
    {
        this.mesh = mesh;
        this.localUp = localUp;
        this.celestialBody = celestialBody;
        this.celestialBodySettingsScript = celestialBody.settings;
        this.radius = (float)celestialBody.settings.radiusU;
        this.bumpMap = bumpMapTexture;
        this.isDefaultHeightMap = isDefaultHeightMapBool;
        this.activeShip = playerActiveShip;

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
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Extend the resolution capabilities of the mesh
        
        // Generate chunks
        parentChunk = new Chunk(1, celestialBodySettingsScript, celestialBody, this, null, localUp.normalized * radius, radius, 0, localUp, axisA, axisB, new byte[4], 0, activeShip, bumpMap, isDefaultHeightMap);
        parentChunk.GenerateChildren();

        // Get chunk mesh data
        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();
        
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
        edgefanIndex.Clear();

        parentChunk.UpdateChunk();

        // Get chunk mesh data
        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();

        //=====================================
        List<int> idxOfChildrenToRemove = new List<int>(); // index in the visibleChildren list of the child that is replaced by the ground prefab for colliding with the ship
        bool mustRemovethisChild = false;
        List<Vector3> colliderVertices = new List<Vector3>();
        List<Vector3> colliderBorderVertices = new List<Vector3>();
        List<int> colliderTriangles = new List<int>();
        List<int> colliderBorderTriangles = new List<int>();
        List<Vector3> colliderNormals = new List<Vector3>();
        int colliderTriangleOffset= 0;
        int colliderBorderTriangleOffset= 0;
        //========
        int lodToCheck = 6;
        foreach(Chunk child in visibleChildren) {
            if(child.detailLevel == 7) {
                lodToCheck = 7;
                break;
            }
        }
        //================
        int idxCounter = 0;
        foreach(Chunk child in visibleChildren)
        {
            int tmpTriangleOffset, tmpBorderTriangleOffset;
            mustRemovethisChild = false;
            if(child.detailLevel == lodToCheck) {
                idxOfChildrenToRemove.Add(idxCounter);
                mustRemovethisChild = true;
                tmpTriangleOffset = colliderTriangleOffset;
                tmpBorderTriangleOffset = colliderBorderTriangleOffset;
            }
            else {
                tmpTriangleOffset = triangleOffset;
                tmpBorderTriangleOffset = borderTriangleOffset;
            }
            //====================
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) verticesAndTriangles = (new Vector3[0], new int[0], new int[0], new Vector3[0], new Vector3[0]);
            if (child.vertices == null)
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(tmpTriangleOffset, tmpBorderTriangleOffset);
            }
            else if (child.vertices.Length == 0 || child.triangles != Presets.quadTemplateTriangles[(child.neighbours[0] | child.neighbours[1] * 2 | child.neighbours[2] * 4 | child.neighbours[3] * 8)])
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(tmpTriangleOffset, tmpBorderTriangleOffset);
            }
            else
            {
                verticesAndTriangles = (child.vertices, child.GetTrianglesWithOffset(tmpTriangleOffset), child.GetBorderTrianglesWithOffset(tmpBorderTriangleOffset, tmpTriangleOffset), child.borderVertices, child.normals);
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
            else {
                colliderVertices.AddRange(verticesAndTriangles.Item1);
                colliderTriangles.AddRange(verticesAndTriangles.Item2);
                colliderBorderTriangles.AddRange(verticesAndTriangles.Item3);
                colliderBorderVertices.AddRange(verticesAndTriangles.Item4);
                colliderNormals.AddRange(verticesAndTriangles.Item5);

                colliderTriangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
                colliderBorderTriangleOffset += verticesAndTriangles.Item4.Length;
            }
            idxCounter++;
        }

        CreateAssignGroundMeshCollider(colliderVertices.ToArray(), colliderTriangles.ToArray(), colliderNormals.ToArray());
        RemoveGroundChildrenFromPlanet(idxOfChildrenToRemove.ToArray());

        Vector3[] bodyVertices = vertices.ToArray();
        Vector2[] uvs = Compute_UV(bodyVertices);
        
        // Reset mesh and apply new data
        mesh.Clear();
        mesh.vertices = bodyVertices;
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs;
    }

    private Vector2[] Compute_UV(Vector3[] vertices)
    {
        Vector2[] uvs = new Vector2[vertices.Length];

        float planetScriptSizeDivide = (1 / (float)celestialBodySettingsScript.radiusU);
        float twoPiDivide = (1 / (2 * Mathf.PI));

        for (int i = 0; i < uvs.Length; i++)
        {
            Vector3 d = vertices[i] * planetScriptSizeDivide;
            float u = 0.5f + Mathf.Atan2(d.z, d.x) * twoPiDivide;
            float v = 0.5f - Mathf.Asin(-d.y) / Mathf.PI; // '-d.y' to have a positive value for the y tilling of the texture
            uvs[i] = new Vector2(u, v);
        }
        return uvs;
    }

    private void RemoveGroundChildrenFromPlanet(int[] idxOfChildrenToRemove)
    {
        // Removing the children that are replaced by the ground collider from the 'visibleChildren' List<Chunk>
        for(int i=idxOfChildrenToRemove.Length-1; i>=0; i--)
        {
            visibleChildren.RemoveAt(idxOfChildrenToRemove[i]);
        }
    }

    private void CreateAssignGroundMeshCollider(Vector3[] colliderVertices, int[] colliderTriangles, Vector3[] colliderNormals)
    {
        if(colliderVertices.Length <= 0 && colliderTriangles.Length <= 0 && colliderNormals.Length <= 0)
            return;

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
            double geoRad = Spaceship.GetGeocentricRadiusFromShipPos(celestialBody, activeShip.position);
            double equaRad = celestialBodySettingsScript.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
            double ratioRadius = (1d-geoRad/equaRad);
            Vector3d dir = -ratioRadius*(new Vector3d(activeShip.position) - new Vector3d(celestialBody.transform.position));
            groundGO.transform.localPosition = (Vector3)dir;
            //================
            groundGO.layer = 9;
            MeshRenderer meshRenderer = groundGO.GetComponent<MeshRenderer>();
            meshRenderer.material = Resources.Load<Material>("Ground");
            //================
            PhysicMaterial colliderMaterial = new PhysicMaterial("MeshColliderMaterial");
            colliderMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
            colliderMaterial.bounciness = 0f;
            colliderMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
            colliderMaterial.staticFriction = 2_000f;
            colliderMaterial.dynamicFriction = 2_000f;
            //======
            meshCollider = groundGO.GetComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.material = colliderMaterial;
        }
        MeshFilter meshFilter = groundGO.GetComponent<MeshFilter>();
        Mesh tmpMesh = new Mesh();
        tmpMesh.name = "closestChunk";

        Vector2[] uvs = Compute_UV(colliderVertices);

        tmpMesh.vertices = colliderVertices;
        tmpMesh.triangles = colliderTriangles;
        tmpMesh.normals = colliderNormals;
        tmpMesh.uv = uvs;
        meshFilter.mesh = tmpMesh;
        meshCollider.sharedMesh = tmpMesh;
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
    public Chunk(uint hashvalue, CelestialBodySettings celestialBodySettingsScript, CelestialBody celestialBody, TerrainFace terrainFace, Chunk[] children, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, byte[] neighbours, byte corner, Transform activeShip, Texture2D bumpMapTexture, bool isDefaultHeightMapBool)
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
                children[0] = new Chunk(hashvalue * 4, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position + axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 0, universePlayerShip , initialBumpMap, hasDefaultHeightmap); // TOP LEFT
                children[1] = new Chunk(hashvalue * 4 + 1, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position + axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 1, universePlayerShip, initialBumpMap, hasDefaultHeightmap); // TOP RIGHT
                children[2] = new Chunk(hashvalue * 4 + 2, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position - axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 2, universePlayerShip, initialBumpMap, hasDefaultHeightmap); // BOTTOM RIGHT
                children[3] = new Chunk(hashvalue * 4 + 3, celestialBodySettingsScript, celestialBody, terrainFace, new Chunk[0], position - axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 3, universePlayerShip, initialBumpMap, hasDefaultHeightmap); // BOTTOM LEFT

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
                celestialBody.transform.position, universePlayerShip.position);

            if (Mathf.Acos((((float)(celestialBodySettingsScript.radiusU * celestialBodySettingsScript.radiusU)) + (b * b) -
                celestialBody.distanceToPlayerPow2) / (2 * (float)celestialBodySettingsScript.radiusU * b)) > celestialBodySettingsScript.cullingMinAngle)
            {
                terrainFace.visibleChildren.Add(this);
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
        //=====================================================================
        //=====================================================================
        //=====================================================================
        //=====================================================================
        //var timer = System.Diagnostics.Stopwatch.StartNew();
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

                pixel_w = (pixel_w + 3) > initialBumpMap.width ? pixel_w-3 : pixel_w;
                pixel_h = (pixel_h + 3) > initialBumpMap.height ? pixel_h-3 : pixel_h;
                
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
        //Debug.Log("Ellapsed time = " + timer.ElapsedMilliseconds + " ms");
        //=====================================================================
        //=====================================================================
        //=====================================================================
        //=====================================================================
        // Calling the Compute Shader here and running it
        /*ComputeShader shader = celestialBody.elevationComputeShader;
        int kernelIdx = shader.FindKernel("VerticesWithElevationCompute");
        ComputeBuffer buffer = null;
        int maxNbVertices = (Presets.quadRes+1)*(Presets.quadRes+1);
        ComputeShaderHelper.CreateStructuredBuffer<Vector3>(ref buffer, maxNbVertices);

        Vector3[] computeVertices = new Vector3[maxNbVertices];
        for(int i=0; i<computeVertices.Length; i++) {
            computeVertices[i] = Vector3.zero;
        }
        buffer.SetData(computeVertices);
        shader.SetBuffer(kernelIdx, "Vertices", buffer);

        Vector3[] preset = Presets.quadTemplateVertices[quadIndex];
        Vector4[] quadVerticesPreset_v4 = new Vector4[maxNbVertices];
        int count = 0;
        foreach(Vector3 vertexPreset in preset)
            quadVerticesPreset_v4[count++] = new Vector4(vertexPreset.x, vertexPreset.y, vertexPreset.z);
        for(int i=preset.Length; i<quadVerticesPreset_v4.Length; i++)
            quadVerticesPreset_v4[i] = Vector4.zero;
        
        float rad = (float)celestialBodySettingsScript.radiusU;
        ElevationComputeInputData[] inputData = new ElevationComputeInputData[1];
        inputData[0] = new ElevationComputeInputData(vertices.Length, transformMatrix, rad, MAX_ALTITUDE, maxRatio);

        ComputeBuffer inputBuffer = null; 
        ComputeShaderHelper.CreateStructuredBuffer<ElevationComputeInputData>(ref inputBuffer, 1);
        inputBuffer.SetData(inputData);
        shader.SetBuffer(kernelIdx, "Input", inputBuffer);
        shader.SetTexture(kernelIdx, "HeightMap", initialBumpMap);
        shader.SetVectorArray("QuadVerticesPreset", quadVerticesPreset_v4);

        Debug.Log("Before Data: \n" + string.Join("\n", computeVertices));
        var timer = System.Diagnostics.Stopwatch.StartNew();

        ComputeShaderHelper.Run(shader, vertices.Length);
        buffer.GetData(computeVertices);
        buffer.Release();

        Debug.Log("Ellapsed time = " + timer.ElapsedMilliseconds + " ms");
        Debug.Log("After Data: \n" + string.Join("\n", computeVertices));

        for(int i=0; i<vertices.Length; i++)
            vertices[i] = computeVertices[i];*/
        //=====================================================================
        //=====================================================================
        //=====================================================================
        //=====================================================================
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

    struct ElevationComputeInputData {
        // Size of 'QuadVerticesPreset' depends on the parameter 'quadRes' in the Preset.cs file. Defualt to 16
        public int numVertices;
        public Matrix4x4 TransformMat;
        public float RadiusU; 
        public float MaxAltitude;
        public float MaxRatio;

        public ElevationComputeInputData(int _numVertices, Matrix4x4 _transformMat, float _RadiusU, float _maxAltitude, float _maxRatio)
        {
            this.numVertices = _numVertices;
            this.TransformMat = _transformMat;
            this.RadiusU = _RadiusU;
            this.MaxAltitude = _maxAltitude;
            this.MaxRatio = _maxRatio;
        }
    };
}