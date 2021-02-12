using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk
{
    List<Vector3> verts;
    List<int> tris;
    public float[,,] noiseMap;
    int chunkWidth;
    int chunkHeight;
    public Vector2Int chunkCoord;
    Vector3Int chunkPosInWorldSpace;

    public GameObject chunkObject;
    public Transform player;
    World world;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    ChunkObject chunkObjectScript;

    // PopulateNoiseMap(), UpdateMesh()
    public Chunk(World _world, Vector2Int _chunkCoord)
    {
        world = _world;
        chunkCoord = _chunkCoord;
        verts = new List<Vector3>();
        tris = new List<int>();
        chunkWidth = GameData.chunkWidth;
        chunkHeight = GameData.chunkHeight;

        chunkPosInWorldSpace = new Vector3Int(chunkCoord.x * chunkWidth, 0, chunkCoord.y * chunkWidth);

        chunkObject = new GameObject();      
        chunkObject.name = $"Chunk: {chunkCoord.x}, {chunkCoord.y}";
        chunkObject.transform.parent = world.transform;

        chunkObject.transform.position = chunkPosInWorldSpace;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        chunkObjectScript = chunkObject.AddComponent<ChunkObject>();

        PopulateNoiseMap();
        CreateMeshData();
        CreateMesh();
    }
 
    Chunk[] GetAdjacentChunks()
    {
        Chunk[] adjacentChunks = new Chunk[6];

        if (chunkCoord.x < world.mapSize - 1)
        {
            // Debug.Log($"{chunkCoord}: 0, Returning Chunk at: {new Vector2Int(chunkCoord.x + 1, chunkCoord.y)}");
            adjacentChunks[0] = world.ChunkFromCoord(new Vector2Int(chunkCoord.x + 1, chunkCoord.y));
        }
        else
        {
            // Debug.Log($"Edge chunk, returning: {this.chunkCoord}");
            adjacentChunks[0] = this;
        }
        if (chunkCoord.y < world.mapSize - 1)
        {
            // Debug.Log($"{chunkCoord}: 1, Returning Chunk at: {new Vector2Int(chunkCoord.x, chunkCoord.y + 1)}");
            adjacentChunks[1] = world.ChunkFromCoord(new Vector2Int(chunkCoord.x, chunkCoord.y + 1));
        }
        else
        {
            // Debug.Log($"Edge chunk, returning: {this.chunkCoord}");
            adjacentChunks[1] = this;
        }

        if (chunkCoord.x < world.mapSize - 1 && chunkCoord.y < world.mapSize - 1)
        {
            // Debug.Log($"{chunkCoord}: 2, Returning Chunk at: {new Vector2Int(chunkCoord.x + 1, chunkCoord.y + 1)}");
            adjacentChunks[2] = world.ChunkFromCoord(new Vector2Int(chunkCoord.x + 1, chunkCoord.y + 1));
        }
        else
        {
            // Debug.Log($"Edge chunk, returning: {this.chunkCoord}");
            adjacentChunks[2] = this;
        }

        if (chunkCoord.x > 0)
        {
            // Debug.Log($"{chunkCoord}: 3, Returning Chunk at: {new Vector2Int(chunkCoord.x - 1, chunkCoord.y)}");
            adjacentChunks[3] = world.ChunkFromCoord(new Vector2Int(chunkCoord.x - 1, chunkCoord.y));
        }
        else
        {
            // Debug.Log($"Edge chunk, returning: {this.chunkCoord}");
            adjacentChunks[3] = this;
        }

        if (chunkCoord.y > 0)
        {
            // Debug.Log($"{chunkCoord}: 4, Returning Chunk at: {new Vector2Int(chunkCoord.x, chunkCoord.y - 1)}");
            adjacentChunks[4] = world.ChunkFromCoord(new Vector2Int(chunkCoord.x, chunkCoord.y - 1));
        }
        else
        {
            // Debug.Log($"Edge chunk, returning: {this.chunkCoord}");
            adjacentChunks[4] = this;
        }

        if (chunkCoord.x > 0 && chunkCoord.y > 0)
        {
            // Debug.Log($"{chunkCoord}: 5, Returning Chunk at: {new Vector2Int(chunkCoord.x - 1, chunkCoord.y - 1)}");
            adjacentChunks[5] = world.ChunkFromCoord(new Vector2Int(chunkCoord.x - 1, chunkCoord.y - 1));
        }
        else
        {
            // Debug.Log($"Edge chunk, returning: {this.chunkCoord}");
            adjacentChunks[5] = this;
        }

        return adjacentChunks;

    }

    void GetAdjacentValues()
    {
        Chunk[] adjacentChunks = GetAdjacentChunks();


        int x, y, z;

        // X edge
        if (adjacentChunks[0] != this)
        {
            x = chunkWidth;
            for (z = 0; z < chunkWidth + 1; z++)
            {
                for (y = 0; y < chunkHeight + 1; y++)
                {
                    noiseMap[x, y, z] = adjacentChunks[0].SampleTerrrain(new Vector3Int(0, y, z));
                }
            }
        }

        if (adjacentChunks[1] != this)
        {
            z = chunkWidth;
            for (x = 0; x < chunkWidth + 1; x++)
            {
                for (y = 0; y < chunkHeight + 1; y++)
                {
                    noiseMap[x, y, z] = adjacentChunks[1].SampleTerrrain(new Vector3Int(x, y, 0));
                }
            }
        }

        if (adjacentChunks[2] != this)
        {
            x = chunkWidth;
            z = chunkWidth;

            for (y = 0; y < chunkHeight + 1; y++)
            {
                noiseMap[x, y, z] = adjacentChunks[2].SampleTerrrain(new Vector3Int(0, y, 0));
            }
        }

        // Debug.Log($"Got adjacent values at: {chunkCoord}");
    }

    Vector3Int[] GetPointsInSphere(Vector3Int center, int radius, int yCap)
    {
        List<Vector3Int> pointsInSphere = new List<Vector3Int>();

        for (int x = center.x - radius; x < center.x + radius; x++)
            for (int y = center.y - radius; y < center.y + radius; y++)
                for (int z = center.z - radius; z < center.z + radius; z++)
                {
                    if (y < 1) y = 1;

                    if (y > yCap) continue;

                    if (Mathf.Pow((x - center.x), 2) + Mathf.Pow((y - center.y), 2) + Mathf.Pow((z - center.z), 2) < Mathf.Pow(radius, 2))
                        pointsInSphere.Add(new Vector3Int(x, y, z));
                }

        return pointsInSphere.ToArray();
    }

    int IndexFromVertex(Vector3 vertex)
    {
        for (int i = 0; i < verts.Count; i++)
        {

            if (verts[i] == vertex)
                return i;
        }

        verts.Add(vertex);
        return verts.Count - 1;
    }

    public void PopulateNoiseMap()
    {
        // float minNoise = float.MaxValue;
        // float maxNoise = float.MinValue;

        noiseMap = new float[chunkWidth + 1, chunkHeight + 1, chunkWidth + 1];
        for (int x = 0; x < chunkWidth + 1; x++)
            for (int y = 0; y < chunkHeight + 1; y++)
                for (int z = 0; z < chunkWidth + 1; z++)
                {

                    float xSample = chunkPosInWorldSpace.x + x;
                    float zSample = chunkPosInWorldSpace.z + z;
                    float ySample = chunkPosInWorldSpace.y + y;

                    noiseMap[x, y, z] = GameData.GenerateNoise(xSample, zSample, world.octaves, world.scale, world.persistance, world.lacunarity) - (float)y;


                    /*
                    if (y < 3)
                        noiseMap[x, y, z] = 1;
                    else if (y < 49)
                        noiseMap[x, y, z] = (float)NoiseS3D.Noise((double)xSample / 20, (double)ySample / 20, (double)zSample / 20);
                    else
                        noiseMap[x, y, z] = GameData.GenerateNoise(xSample, zSample, world.octaves, world.scale, world.persistance, world.lacunarity) - (float)y;
                    */

                    /*
                    if (noise < minNoise)
                        minNoise = noise;
                    if (noise > maxNoise)
                        maxNoise = noise;
                    */


                    // noiseMap[x, y, z] = noise;
                }

        // world.surface = (minNoise + maxNoise) / 2;

        /*
        // Normalize noise values
        for (int x = 0; x < chunkWidth + 1; x++)
            for (int y = 0; y < chunkHeight + 1; y++)
                for (int z = 0; z < chunkWidth + 1; z++)
                {
                    // noise = ((noise - -1) / (1 - -1)) * (1 - -1) + 1;
                    noiseMap[x, y, z] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y, z]);
                }
        */
    }

    float SampleTerrrain(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        return noiseMap[x, y, z];
        
    }

    // SampleTerrain()
    float[] GetCube(Vector3Int pos)
    {
        float[] cube = new float[8];
        for (int i = 0; i < GameData.verts.Length; i++)
        {
            Vector3Int vert = pos + GameData.verts[i];

            cube[i] = SampleTerrrain(vert);
        }

        return cube;
    }

    int GetConfig(float[] cube)
    {
        int config = 0;
        for (int i = 0; i < GameData.verts.Length; i++)
        {
            if (cube[i] >= world.surface)
            {
                config |= 1 << i;
            }
        }

        return config;
    }

    void MarchCube(float[] cube, int config, Vector3 pos)
    {
        if (config == 0 || config == 255) { return; }

        for (int i = 0; i < GameData.triangulationTable.GetLength(1); i++)
        {
            int edgeIndex = GameData.triangulationTable[config, i];

            if (edgeIndex == -1)
                break;

            Vector3 vert1 = GameData.verts[GameData.edge[edgeIndex, 0]];
            Vector3 vert2 = GameData.verts[GameData.edge[edgeIndex, 1]];

            Vector3 averageVert;
            if (!world.meshSmoothing)
                averageVert = (vert2 + vert1) / 2f;
            else
                averageVert = vert1 + (world.surface - cube[GameData.edge[edgeIndex, 0]]) * (vert2 - vert1) / (cube[GameData.edge[edgeIndex, 1]] - cube[GameData.edge[edgeIndex, 0]]);

            // Every face has its own tris, so there are only outward facing normals
            if (world.flatShaded)
            {
                verts.Add(averageVert + pos);
                tris.Add(verts.Count - 1);
            }
            // Tris share verts, so that normals share faces
            else
            {
                tris.Add(IndexFromVertex(averageVert + pos));
            }
        }
    }

    // GetCube(), GetConfig() ---> MarchCube()
    void CreateMeshData()
    {
        for (int x = 0; x < chunkWidth; x++)
            for (int y = 0; y < chunkHeight; y++)
                for (int z = 0; z < chunkWidth; z++)
                {
                    float[] cube = GetCube(new Vector3Int(x, y, z));
                    int config = GetConfig(cube);

                    MarchCube(cube, config, new Vector3Int(x, y, z));
                }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshRenderer.material = world.meshMat;
        meshCollider.sharedMesh = mesh;


    }

    // CreateMeshData(), CreateMesh()
    public void UpdateMesh()
    {
        verts.Clear();
        tris.Clear();

        CreateMeshData();      
        CreateMesh();
    }

    public void ModifyChunk(Vector3Int pos, int radius, float quantity, Vector3 globalHit, int yCap, bool set)
    {
        // chunkObject.transform.position = new Vector3(chunkObject.transform.position.x, chunkObject.transform.position.y + 30, chunkObject.transform.position.z);

        List<Chunk> chunksModified = new List<Chunk>();

        Vector3Int[] pointsInRadius = GetPointsInSphere(pos, radius, yCap);

        for (int i = 0; i < pointsInRadius.Length; i++)
        {
            // Debug.Log($"{x},{y},{z}");

            Vector2Int adjacentChunk = chunkCoord;

            int x = pointsInRadius[i].x;
            int y = pointsInRadius[i].y;
            int z = pointsInRadius[i].z;

            if(pointsInRadius[i].z < 0)
            {
                // Debug.Log("Z is negative: " + z);
                z = chunkWidth + pointsInRadius[i].z;
                adjacentChunk.y -= 1;
            }
            else if(pointsInRadius[i].z >= chunkWidth)
            {
                z = pointsInRadius[i].z - chunkWidth;
                adjacentChunk.y += 1;
            }


            if (pointsInRadius[i].x < 0)
            {
                // Debug.Log("X is negative: " + x);
                x = chunkWidth + pointsInRadius[i].x;
                adjacentChunk.x -= 1;
            }
            else if (pointsInRadius[i].x >= chunkWidth)
            {
                x = pointsInRadius[i].x - chunkWidth;
                adjacentChunk.x += 1;
            }

            if(adjacentChunk == chunkCoord)
            {
                // Debug.Log($"Within Chunk({adjacentChunk}), Edited at: {chunk_x}, {chunk_y}, {chunk_z}");
                if (!set)
                    noiseMap[x, y, z] += quantity;
                else
                    noiseMap[x, y, z] = quantity;
            }
            else
            {
                // Debug.Log($"Outside Chunk({adjacentChunk}), Edited at: {chunk_x}, {chunk_y}, {chunk_z}");
                Chunk chunk = world.ChunkFromCoord(adjacentChunk);

                if (!set)
                    chunk.noiseMap[x, y, z] += quantity;
                else
                    chunk.noiseMap[x, y, z] = quantity;

                if (!chunksModified.Contains(chunk))
                    chunksModified.Add(chunk);
            }
        }

        Chunk[] negativeChunks = GetAdjacentChunks();

        GetAdjacentValues();
        for (int i = 0; i < 6; i++)
            negativeChunks[i].GetAdjacentValues();

        foreach (Chunk chunk in chunksModified)
            chunk.GetAdjacentValues();

        UpdateMesh();
        for (int i = 0; i < 6; i++)
            negativeChunks[i].UpdateMesh();

        foreach (Chunk chunk in chunksModified)
            chunk.UpdateMesh();

        // Debug.Log(noiseMap[pos.x, pos.y, pos.z]);

    }
}

/*
//xCorner
    x = width;
    for (z = 0; z<width; z++)
    {
        for (y = minHeight; y<height + 1; y++)
        {
            SetTerrain(x, y, z, chunkCorner[0].block[0, y, z]);
        }
    }
//zCorner
    z = width;
    for (x = 0; x<width; x++)
    {
        for (y = minHeight; y<height + 1; y++)
        {
            SetTerrain(x, y, z, chunkCorner[1].block[x, y, 0]);
        }
    }
//xzCorner
    z = width;
    x = width;
    for (y = minHeight; y<height + 1; y++)
    {
        SetTerrain(x, y, z, chunkCorner[2].block[0, y, 0]);
    }
*/

