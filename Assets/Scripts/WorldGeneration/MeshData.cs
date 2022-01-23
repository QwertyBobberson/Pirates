using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Struct representing a mesh to allow for multithreaded world generation
/// </summary>
public struct MeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    public MeshData(Vector3[] _vertices, int[] _triangles)
    {
        vertices = _vertices;
        triangles = _triangles;
    }

    /// <summary>
    /// Generate a square chunk
    /// </summary>
    /// <param name="size">Width and length of a square chunk</param>
    /// <param name="offsetX">Offset on the X Axis</param>
    /// <param name="offsetZ">Offset on the Z Axis</param>
    /// <param name="octaves">Layers of detail to add</param>
    /// <param name="scale">Level of zoom of the chunk</param>
    /// <param name="lacunarity">Amount to decrease scale for each layer</param>
    /// <param name="persistance">Amount to decrease height for each layer</param>
    /// <param name="height">Amount to multiply height map by</param>
    /// <param name="heightPower">Power to raise height map to</param>
    /// <returns>A randomly generated MeshData representing a square chunk</returns>
    public static MeshData GenerateChunk(int size, float offsetX, float offsetZ, int octaves, float scale, float lacunarity, float persistance, float height, float heightPower)
    {
        Vector3[] vertices = GenerateVertices(size, offsetX, offsetZ, octaves, scale, lacunarity, persistance, height, heightPower);
        int[] triangles = GenerateTriangles(size);

        return new MeshData(vertices, triangles);
    }

    /// <summary>
    /// Generate the vertices of a square chunk
    /// </summary>
    /// <param name="size">Width and length of a square chunk</param>
    /// <param name="offsetX">Offset on the X Axis</param>
    /// <param name="offsetZ">Offset on the Z Axis</param>
    /// <param name="octaves">Layers of detail to add</param>
    /// <param name="scale">Level of zoom of the chunk</param>
    /// <param name="lacunarity">Amount to decrease scale for each layer</param>
    /// <param name="persistance">Amount to decrease height for each layer</param>
    /// <param name="height">Amount to multiply height map by</param>
    /// <param name="heightPower">Power to raise height map to</param>
    /// <returns>Array of vertices representing the height map of a square chunk</returns>
    public static Vector3[] GenerateVertices(int size, float offsetX, float offsetZ, int octaves, float scale, float lacunarity, float persistance, float height, float heightPower)
    {
        //Temporary storage for vertice locations
        Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];

        //Max and min heights are needed for normalization later on
        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        //Generate a square of vertices
        //TODO: Replace i with a formula
        for(int i = 0, z = 0; z <= size; z++)
        {
            for(int x = 0; x <= size; x++)
            {
                //Height of the vertice
                float y = 0;

                //Used to decrease the affect of each layer of noise
                float amplitude = 1;
                float frequency = 1;

                //Add layers of noise with decreasing amplitudes and increasing frequencies for extra detail
                for(int j = 0; j < octaves; j++)
                {
                    y += Mathf.PerlinNoise((float)(x + offsetX) / scale * frequency, (float)(z + offsetZ) / scale * frequency) * amplitude * height;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                //Raise y to a power for flatter valleys and steeper hills
                y = Mathf.Pow(y, heightPower);

                //Calculate max and min height of vertices for normalization
                if(y > maxHeight)
                {
                    maxHeight = y;
                }
                if(y < minHeight)
                {
                    minHeight = y;
                }

                //Store the height, x, and z of the new vertice
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        //Normalization: Just lowers map to y = 0
        //TODO: Map y from 0-1 and apply curve
        for(int i = 0, z = 0; z <= size; z++)
        {
            for(int x = 0; x <= size; x++)
            {
                vertices[i].y -= minHeight;
            }
        }

        vertices[0] = new Vector3(0, 0, 0);

        return vertices;
    }

    /// <summary>
    /// Generate an array to tell the computer how to render the mesh
    /// </summary>
    /// <param name="size">Width and Length of a square chunk</param>
    /// <returns>Array of integers representing the triangles of the chunk</returns>
    public static int[] GenerateTriangles(int size)
    {
        int[] triangles = new int[size * size * 6];

        int vert = 0;
        int triNum = 0;
        for(int z = 0; z < size; z++)
        {
            for(int x = 0; x < size; x++)
            {
                triangles[triNum] = vert + 0;
                triangles[triNum + 1] = vert + size + 1;
                triangles[triNum + 2] = vert + 1;
                triangles[triNum + 3] = vert + 1;
                triangles[triNum + 4] = vert + size + 1;
                triangles[triNum + 5] = vert + size + 2;
                
                vert++;
                triNum += 6;
            }
            vert++;
        }
        triangles[0] = 1;
        return triangles;
    }
}