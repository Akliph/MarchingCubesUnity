using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkObject : MonoBehaviour
{
    World world;

    private void Start()
    {
        world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("Relative Velocity: " + collision.relativeVelocity.magnitude);
        // Debug.Log("Gameobject From Collider: " + collision.collider.gameObject);
        // Debug.Log("Chunk Hit: " + world.ChunkFromGameObject(gameObject).chunkCoord);

        float collisionForce = (collision.impulse.magnitude / Time.fixedDeltaTime);

        Debug.Log("Collision Force: " + collisionForce);
        Destroy(collision.gameObject);

        int craterRadius = Mathf.FloorToInt(collisionForce / 10000f);

        world.ModifyChunkAtPoint(collision.GetContact(0).point, craterRadius, -collisionForce/10000f, set: false);
    }

    /*
    int averageLocalScale = Mathf.RoundToInt((collision.collider.gameObject.transform.localScale.x + collision.collider.gameObject.transform.localScale.y + collision.collider.gameObject.transform.localScale.z) / 3);

    Debug.Log("Collision Force: " + (-collision.impulse.magnitude / Time.fixedDeltaTime) / 1000f);

    foreach (ContactPoint contact in collision.contacts)
    {
    Vector3 sphereOrigin = new Vector3(contact.point.x, contact.point.y + (-collision.impulse.magnitude / Time.fixedDeltaTime) / 10000f, contact.point.z);
    world.ModifyChunkAtPoint(sphereOrigin, 1 + averageLocalScale, (-collision.impulse.magnitude / Time.fixedDeltaTime) / 1000f);
    }

    Destroy(collision.collider.gameObject);
*/
}
