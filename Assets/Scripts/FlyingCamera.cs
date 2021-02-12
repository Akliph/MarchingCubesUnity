using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlyingCamera : MonoBehaviour
{

    public float mouseSensitivity = 5f;
    public float speed = 10f;
    public World world;
    public GameObject ball;

    public TextMeshProUGUI radiusText;
    public TextMeshProUGUI quantityText;

    List<GameObject> currentPrefabs;
    Camera cam;

    int radius = 1;
    float quantity = 0.5f;
    int yCap = int.MaxValue;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        currentPrefabs = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.Rotate(-Vector3.right * Input.GetAxisRaw("Mouse Y") * mouseSensitivity);
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        transform.position += cam.transform.forward * Input.GetAxisRaw("Vertical") * speed;
        transform.position += transform.right * Input.GetAxisRaw("Horizontal") * speed;

        if (Input.GetMouseButton(0))
            TerrainTool(radius, quantity);
        else if (Input.GetMouseButton(1))
            TerrainTool(radius, -quantity);

        if (Input.GetKeyDown(KeyCode.X))
            radius += 1;
        else if(Input.GetKeyDown(KeyCode.Z))
            radius -= 1;

        if (radius < 1)
            radius = 1;

        radiusText.text = $"Radius: {radius}";

        if(Input.mouseScrollDelta.y != 0)
            quantity += Input.mouseScrollDelta.y/ 10f;
        if (quantity < 0.1f)
            quantity = 0.1f;

        quantityText.text = $"Quantity: {Mathf.RoundToInt(quantity * 10)/10f}";

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPrefabs.Add(GameObject.Instantiate(ball, transform.position, Quaternion.identity));    
        }
        else if(Input.GetKeyDown(KeyCode.B))
        {
            foreach(GameObject prefab in currentPrefabs)
            {
                currentPrefabs.Remove(prefab);
                Destroy(prefab);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, cam.transform.forward, out hit))
            {
                yCap = Mathf.RoundToInt(hit.point.y);
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl)) yCap = int.MaxValue;
    }

    void TerrainTool(int radius, float _quantity)
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, cam.transform.forward, out hit))
        {
            world.ModifyChunkAtPoint(hit.point,radius,_quantity, yCap);
        }
        
    }
}
