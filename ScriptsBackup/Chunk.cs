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
    Vector2Int chunkCoord;
    Vector3Int chunkPosInWorldSpace;

    public GameObject chunkObject;
    public Transform player;
    World world;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

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
        chunkObject.transform.position = chunkPosInWorldSpace;
        chunkObject.name = $"Chunk: {chunkCoord.x}, {chunkCoord.y}";
        chunkObject.transform.parent = world.transform;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        PopulateNoiseMap();
        UpdateMesh();
    }

    public void PopulateNoiseMap()
    {
        noiseMap = new float[chunkWidth + 1, chunkHeight + 1, chunkWidth + 1];
        for (int x = 0; x < chunkWidth + 1; x++)
            for (int y = 0; y < chunkHeight + 1; y++)
                for (int z = 0; z < chunkWidth + 1; z++)
                {
                    float xSample = chunkPosInWorldSpace.x + x;
                    float zSample = chunkPosInWorldSpace.z + z;

                    float noise = GameData.GenerateNoise(xSample, zSample, world.octaves, world.scale, world.persistance, world.lacunarity) - (float)y;
                    noiseMap[x, y, z] = noise;

                    world.pointMap[(chunkWidth * chunkWidth * chunkHeight)] = noise;
                    // Debug.Log(perlinValue);
                }
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

            if (!world.initialized)
                cube[i] = SampleTerrrain(vert);
            else
            {
                if (vert.x > chunkWidth && vert.z <= chunkWidth)
                {
                    // Debug.Log($"{vert}: X edge case");
                    cube[i] = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x + 1, chunkCoord.y)).SampleTerrrain(new Vector3Int(1, vert.y, vert.z));
                    continue;
                }
                else if (vert.z > chunkWidth && vert.x <= chunkWidth)
                {
                    // Debug.Log($"{vert}: Z edge case");
                    cube[i] = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x, chunkCoord.y + 1)).SampleTerrrain(new Vector3Int(vert.x, vert.y, 1));
                    continue;
                }
                else if (vert.x > chunkWidth && vert.z > chunkWidth)
                {
                    // Debug.Log($"{vert}: Corner case");
                    cube[i] = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x + 1, chunkCoord.y + 1)).SampleTerrrain(new Vector3Int(1, vert.y, 1));
                    continue;
                }

                cube[i] = SampleTerrrain(vert);
            }
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

            verts.Add(averageVert + pos);
            tris.Add(verts.Count - 1);

        }
    }

    // GetCube(), GetConfig() ---> MarchCube()
    void CreateMeshData()
    {
        for (int x = 0; x < chunkWidth + GameData.IntFromBool(world.initialized); x++)
            for (int y = 0; y < chunkHeight; y++)
                for (int z = 0; z < chunkWidth + GameData.IntFromBool(world.initialized); z++)
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

    public void ModifyChunk(Vector3Int pos, int _radius, float quantity, Vector3 globalHit)
    {
        /*
        Vector3Int[] modPoints = GameData.GetPointsInRadius(pos, _radius);

        Vector3 dirToPlayer = (player.position - globalHit);
        int xDir = (int)Mathf.Sign(dirToPlayer.x);
        int yDir = (int)Mathf.Sign(dirToPlayer.y);
        int zDir = (int)Mathf.Sign(dirToPlayer.z);
        
        for (int i = 0; i < modPoints.Length; i++)
        {
            Vector3Int vert = modPoints[i];
            // Debug.Log(vert);

            // Negative X edge
            if(vert.x <= 0 && vert.z > 0)
            {            
                Chunk chunk = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x - 1, chunkCoord.y));
                chunk.noiseMap[vert.x + chunkWidth + 1 + xDir, vert.y + yDir, vert.z + zDir] += 0.1f;
                if(world.log) Debug.Log(vert + ": X edge ::" + new Vector3(vert.x + chunkWidth, vert.y, vert.z));
                chunk.UpdateMesh();
            }
            // Negative Z edge
            else if(vert.z <= 0 && vert.x >= 0)
            {
                Chunk chunk = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x, chunkCoord.y - 1));
                chunk.noiseMap[vert.x + xDir, vert.y + yDir, vert.z + chunkWidth + 1 + zDir] += 0.1f;
                if (world.log) Debug.Log(vert + ": Z edge ::" + new Vector3(vert.x, vert.y, vert.z + chunkWidth));
                chunk.UpdateMesh();
            }
            // Negative X, Z corner
            else if (vert.z <= 0 && vert.x <= 0)
            {
                Chunk chunk = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x - 1, chunkCoord.y - 1));
                chunk.noiseMap[vert.x + chunkWidth + xDir, vert.y + yDir, vert.z + chunkWidth + 1 + zDir] += 0.1f;
                if (world.log) Debug.Log(vert + ": X Z corner ::" + new Vector3(vert.x + chunkWidth, vert.y, vert.z + chunkWidth));
                chunk.UpdateMesh();
            }
            // Positive X edge
            else if (vert.x > chunkWidth + 1 && vert.z <= chunkWidth + 1)
            {
                Chunk chunk = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x + 1, chunkCoord.y));
                chunk.noiseMap[vert.x - chunkWidth, vert.y, vert.z] += 0.1f;
                chunk.UpdateMesh();
            }
            // Positive Z edge
            else if (vert.z > chunkWidth + 1 && vert.x <= chunkWidth + 1)
            {
                Chunk chunk = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x, chunkCoord.y + 1));
                chunk.noiseMap[vert.x, vert.y, vert.z - chunkWidth] += 0.1f;
                chunk.UpdateMesh();
            }
            // Positve X, Z corner
            else if (vert.z > chunkWidth + 1 && vert.x > chunkWidth + 1)
            {
                Chunk chunk = world.ChunkFromCoord(chunkCoord, new Vector2Int(chunkCoord.x + 1, chunkCoord.y + 1));
                chunk.noiseMap[vert.x - chunkWidth, vert.y, vert.z - chunkWidth] += 0.1f;
                chunk.UpdateMesh();
            }
            else
            {
                noiseMap[vert.x, vert.y, vert.z] += 0.1f;
            }

            noiseMap[vert.x, vert.y, vert.z] += quantity;
        }
        */

        Debug.Log($"{noiseMap[pos.x, pos.y, pos.z]}, {world.pointMap[(chunkCoord.x + 1) * pos.x, pos.y, (chunkCoord.y + 1) * pos.z]}");
        UpdateMesh();
    }
}

