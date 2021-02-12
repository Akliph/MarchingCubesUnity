using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour
{

    public float mouseSensitivity = 5f;
    public float speed = 10f;
    public World world;
    Camera cam;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        transform.position = world.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.Rotate(-Vector3.right * Input.GetAxisRaw("Mouse Y") * mouseSensitivity);
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        transform.position += cam.transform.forward * Input.GetAxisRaw("Vertical") * speed;
        transform.position += transform.right * Input.GetAxisRaw("Horizontal") * speed;

        if (Input.GetMouseButton(0))
            TerrainTool(2, 0.3f);
        else if (Input.GetMouseButton(1))
            TerrainTool(2, -0.3f);
    }

    void TerrainTool(int radius, float quantity)
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, cam.transform.forward, out hit))
        {
            world.ModifyChunkAtPoint(hit.point,radius,quantity);
        }
        
    }
}
