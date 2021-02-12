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
    public bool autoUpdate;
    [Header("Refrences")]
    public Material meshMat;

    [HideInInspector]
    public bool log = false;

    public bool initialized = false;

    public Chunk[,] chunkMap;

    public float[,,] pointMap;

    private void Start()
    {
        chunkMap = new Chunk[mapSize, mapSize];
        pointMap = new float[GameData.chunkWidth + 1, GameData.chunkHeight, GameData.chunkWidth + 1];

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
    
    public Chunk ChunkFromCoord(Vector2Int currentCoord, Vector2Int coord)
    {
        if (coord.x == mapSize && coord.y < mapSize)
        {
            // Debug.Log("X edge case: " + coord);
            return chunkMap[currentCoord.x, coord.y];
        }
            
        else if(coord.y == mapSize && coord.x < mapSize)
        {
            // Debug.Log("Y edge case: " + coord);
            return chunkMap[coord.x, currentCoord.y];
        }
            
        else if (coord.x == mapSize && coord.y == mapSize)
        {
            // Debug.Log("Corner case: " + coord);
            return chunkMap[currentCoord.x, currentCoord.y];
        }
        // if x or z is smaller than 0 default to current coord
        else if(coord.x < 0 && coord.y >= 0)
        {
            return chunkMap[currentCoord.x, coord.y];
        }
        else if(coord.y < 0 && coord.x >= 0)
        {
            return chunkMap[coord.x, currentCoord.y];
        }
        else if(coord.x < 0 && coord.y < 0)
        {
            return chunkMap[currentCoord.x, currentCoord.y];
        }


        return chunkMap[coord.x, coord.y];
    }

    public void ModifyChunkAtPoint(Vector3 point, int _radius, float _quantity)
    {
        int localX = Mathf.RoundToInt(point.x % GameData.chunkWidth);
        int localY = Mathf.RoundToInt(point.y);
        int localZ = Mathf.RoundToInt(point.z % GameData.chunkWidth);

        Chunk chunk = ChunkFromGlobalPoint(new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z)));    

        Vector3Int localHit = new Vector3Int(localX, localY, localZ);

        chunk.ModifyChunk(localHit, _radius, _quantity, point);
    }

    public Chunk ChunkFromGlobalPoint(Vector2Int point)
    {
        int x = Mathf.FloorToInt(point.x / GameData.chunkWidth);
        int z = Mathf.FloorToInt(point.y / GameData.chunkWidth);

        return chunkMap[x, z];
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            log = !log;
        }
    }
}
