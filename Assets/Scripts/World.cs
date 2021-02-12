using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class World : MonoBehaviour
{
    [Header("Map Parameters")]
    public int mapSize;
    [Header("Noise Parameters")]
    public float scale;
    public float persistance;
    public float lacunarity;
    public int octaves;
    public float surface;
    [Header("Render Options")]
    public bool meshSmoothing = false;
    public bool flatShaded = true;
    // public bool normalizeNoiseValues = false;
    public bool autoUpdate;
    [Header("Refrences")]
    public Material meshMat;

    [HideInInspector]
    public bool log = false;

    public bool initialized = false;

    public Chunk[,] chunkMap;

    private void Start()
    {
        chunkMap = new Chunk[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
            for (int z = 0; z < mapSize; z++)
            {
                Chunk chunk = new Chunk(this, new Vector2Int(x, z));
                chunkMap[x, z] = chunk;
            }

        initialized = true;
    }

    public void UpdateMap()
    {
        for(int x = 0; x < mapSize; x++)
            for(int z = 0; z < mapSize; z++)
            {
                chunkMap[x, z].PopulateNoiseMap();
                chunkMap[x,z].UpdateMesh();
            }
    }
    
    public Chunk ChunkFromCoord(Vector2Int coord)
    {
        return chunkMap[coord.x, coord.y];
    }

    public void ModifyChunkAtPoint(Vector3 point, int _radius, float _quantity, int yCap = int.MaxValue, bool set = false)
    {
        // Debug.Log("Initial Vector3 Point: " + point);

        int localX = Mathf.RoundToInt(point.x % GameData.chunkWidth);
        int localY = Mathf.RoundToInt(point.y);
        int localZ = Mathf.RoundToInt(point.z % GameData.chunkWidth);

        Chunk chunk = ChunkFromGlobalPoint(new Vector2(point.x, point.z));    

        Vector3Int localHit = new Vector3Int(localX, localY, localZ);

        chunk.ModifyChunk(localHit, _radius, _quantity, point, yCap, set);
    }

    public Chunk ChunkFromGlobalPoint(Vector2 point)
    {
        int x = Mathf.FloorToInt(point.x / GameData.chunkWidth);
        int z = Mathf.FloorToInt(point.y / GameData.chunkWidth);

        return chunkMap[x, z];
    }

    public Chunk ChunkFromGameObject(GameObject chunkObject)
    {
        for(int x = 0; x < mapSize; x++)
            for(int z = 0; z < mapSize; z++)
            {
                if (chunkMap[x, z].chunkObject == chunkObject)
                    return chunkMap[x, z];
            }

        return null;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            log = !log;
        }
    }
}
