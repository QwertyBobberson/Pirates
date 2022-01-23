using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldGeneration : MonoBehaviour
{
    //Mesh mesh;

    public GameObject chunkObj;

    Vector3[] vertices;
    int[] triangles;

    public int chunkSize;
    public int worldSize;

    public int octaves;
    public float scale;
    public float lacunarity;
    public float persistance;

    public float height;
    public float heightPower;

    public int seed;

    public Vector2 offset;



    public void Start()
    {
        for(int x = -worldSize/2; x < worldSize/2; x++)
        {
            for(int z = -worldSize/2; z < worldSize/2; z++)
            {
                GameObject chunk = GameObject.Instantiate(chunkObj, new Vector3(x * chunkSize, 0, z * chunkSize), Quaternion.identity);
                chunk.GetComponent<Chunk>().meshData = MeshData.GenerateChunk(chunkSize, 
                                                                        x * chunkSize, 
                                                                        z * chunkSize, 
                                                                        octaves, 
                                                                        scale, 
                                                                        lacunarity, 
                                                                        persistance, 
                                                                        height, 
                                                                        heightPower);
            }
        }
    }


    // void Start()
    // {
    //     mesh = new Mesh();
    //     UpdateMesh(MeshData.GenerateChunk(size, offset.x + transform.position.x, offset.y + transform.position.z, octaves, scale, lacunarity, persistance, height, heightPower));
    //     GetComponent<MeshFilter>().sharedMesh = mesh;
    //     GetComponent<Chunk>().meshData = new MeshData(mesh.vertices, mesh.triangles);
    //     GetComponent<MeshCollider>().sharedMesh = mesh;
    //     GetComponent<Rigidbody>().isKinematic = false;
    //     GetComponent<MeshCollider>().convex = true;
    //     GetComponent<MeshCollider>().convex = false;
    //     GetComponent<Rigidbody>().isKinematic = true;
    // }

    // int i = 0;

    // void Update()
    // {
    //     if(i >= 600)
    //     {
    //         UpdateMesh(MeshData.GenerateChunk(size, offset.x + transform.position.x, offset.y + transform.position.z, octaves, scale, lacunarity, persistance, height, heightPower));
    //         i = 0;
    //     }
    //     i++;
    // }

    // void UpdateMesh(MeshData meshData)
    // {
    //     mesh.Clear();

    //     mesh.vertices = meshData.vertices;
    //     mesh.triangles = meshData.triangles;
    //     mesh.RecalculateNormals();
    // }
}
