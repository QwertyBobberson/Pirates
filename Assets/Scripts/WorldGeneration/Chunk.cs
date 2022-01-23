using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshData meshData;

    Mesh mesh;

    public void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    /// <summary>
    /// Resync the in engine mesh to the meshdata
    /// </summary>
    public void UpdateMesh()
    {   
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// INCOMPLETE
    /// Given x and y coords, return the vertice
    /// </summary>
    /// <param name="x">The x coordinate of the vertice</param>
    /// <param name="z">The z coordinate of the vertice</param>
    /// <returns>The relative coordinates of the vertice, or null if it is not in this chun</returns>
    public Vector2 GetVerticeByCoords(int x, int z)
    {
        Vector2 position = new Vector2(x - transform.position.x, z - transform.position.z);

        int chunkSize = (int)Mathf.Pow(meshData.vertices.Length - 1, 0.5f);
        Debug.Log(meshData.vertices.Length);

        return meshData.vertices[(int)(position.x * chunkSize + position.y)];
    }
}
