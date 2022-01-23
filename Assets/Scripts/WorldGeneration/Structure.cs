using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Chunk chunk = collision.gameObject.GetComponent<Chunk>();

        Debug.Log("Collided");
        if(chunk != null)
        {
            ContactPoint[] collisionVertices = new ContactPoint[10];
            int collisionCount = collision.GetContacts(collisionVertices);
            Debug.Log($"Collided with {collisionCount} vertices in a chunk");

            for(int i = 0; i < collisionVertices.Length; i++)
            {
                Debug.Log(collisionVertices[i].point);
                Debug.Log("On Chunk: " + chunk.GetVerticeByCoords((int)collisionVertices[i].point.x, (int)collisionVertices[i].point.z));
            }

        }
    }
}
