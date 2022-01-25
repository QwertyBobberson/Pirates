using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public GameObject chunk;
    // Start is called before the first frame update
    void Start()
    {
        for(int x = -50; x <= 50; x++)
        {
            for(int z = -50; z <= 50; z++)
            {
                GameObject.Instantiate(chunk, new Vector3(x * 100, 0, z * 100), Quaternion.identity);
            }
        }   
    }
}