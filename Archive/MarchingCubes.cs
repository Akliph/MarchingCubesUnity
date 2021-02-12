using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    int worldWidth = 5;
    int worldHeight = 5;

    // Sphere Variables
    GameObject[,,] spheres;
    float[,,] noiseValues;
    float prevNoiseThreshold;
    float minNoise = float.MaxValue;
    float maxNoise = float.MinValue;

    public float noiseThreshold = 0.2f;

    // Marching Cube Variables
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();

    void Start()
    {
        // Instaintiate Spheres
        spheres = new GameObject[worldWidth + 1, worldHeight + 1, worldWidth + 1];
        noiseValues = new float[worldWidth + 1, worldHeight + 1, worldWidth + 1];

        for (int x = 0; x < worldWidth + 1; x++)
            for(int y = 0; y < worldHeight + 1; y++)
                for(int z = 0; z < worldWidth + 1; z++)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = new Vector3((x - (worldWidth / 2)), (y - (worldHeight / 2)), (z - (worldWidth / 2)));
                    sphere.transform.localScale = Vector3.one * 0.3f;
                    sphere.name = $"Sphere {x},{y},{z}";
                    sphere.transform.parent = gameObject.transform;

                    float noiseValue = Random.Range(0f, 100f);
                    if (noiseValue < minNoise) minNoise = noiseValue;
                    if (noiseValue > maxNoise) maxNoise = noiseValue;

                    noiseValues[x, y, z] = noiseValue;

                    spheres[x, y, z] = sphere;
                }


    }

    private void Update()
    {
        UpdatePoints();


    }

    void UpdatePoints()
    {
        if (noiseThreshold == prevNoiseThreshold) return;

        for (int x = 0; x < worldWidth + 1; x++)
            for (int y = 0; y < worldHeight + 1; y++)
                for (int z = 0; z < worldWidth + 1; z++)
                {
                    float noiseGradient = noiseValues[x, y, z] / (maxNoise - minNoise);
                    if (noiseGradient <= noiseThreshold) { spheres[x, y, z].SetActive(false); continue; }
                    else if (noiseGradient > noiseThreshold) { spheres[x, y, z].SetActive(true); }

                    spheres[x, y, z].GetComponent<Renderer>().material.color = Color.Lerp(Color.black, Color.white, noiseGradient);
                }

        prevNoiseThreshold = noiseThreshold;
    }

    void GetConfiguration(Vector3 point)
    {
        noiseValues
    }

}
