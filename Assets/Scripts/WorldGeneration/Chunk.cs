using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Chunk : MonoBehaviour
{
    public MeshData meshData;

    /// <summary>
    /// Resync the in engine mesh to the meshdata
    /// </summary>
    public void UpdateMesh()
    {   
        Mesh mesh = new Mesh();
        mesh.vertices = float3sToVectors(meshData.vertices);
        mesh.triangles = meshData.triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        gameObject.name += $" : {meshData.x}, {meshData.z}";
    }

    /// <summary>
    /// Converts an array of float3s to vectors
    /// </summary>
    /// <param name="float3s">The array of float3s to be converted</param>
    /// <returns>An array of vector3s</returns>
    public static Vector3[] float3sToVectors(float3[] float3s)
    {
        Vector3[] vectors = new Vector3[float3s.Length];
        for(int i = 0; i < float3s.Length; i++)
        {
            vectors[i] = new Vector3(float3s[i].x, float3s[i].y, float3s[i].z);
        }

        return vectors;
    }

    /// <summary>
    /// INCOMPLETE
    /// Given x and y coords, return the vertice
    /// </summary>
    /// <param name="x">The x coordinate of the vertice</param>
    /// <param name="z">The z coordinate of the vertice</param>
    /// <returns>The relative coordinates of the vertice, or null if it is not in this chun</returns>
    public Vector3 GetVerticeByCoords(int x, int z)
    {
        Vector2 position = new Vector2(x - transform.position.x, z - transform.position.z);

        int chunkSize = (int)Mathf.Pow(meshData.vertices.Length - 1, 0.5f);

        return meshData.vertices[(int)(position.x * chunkSize + position.y)];
    }
}
