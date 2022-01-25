using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

/// <summary>
/// Struct representing a mesh to allow for multithreaded world generation
/// </summary>
public struct MeshData
{
    public float3[] vertices;
    public int[] triangles;

    public MeshData(float3[] _vertices, int[] _triangles)
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
    public static MeshData GenerateChunk(int size, float offsetX, float offsetZ, int octaves, float scale, float lacunarity, float persistance, float height, float oceanHeight)
    {
        float3[] vertices = GenerateVertices(size, offsetX, offsetZ, octaves, scale, lacunarity, persistance, height);
        int[] triangles = GenerateTriangles(size);
        return TrimWater(triangles, vertices, oceanHeight);
        //return new MeshData(vertices, triangles);
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
    public static float3[] GenerateVertices(int size, float offsetX, float offsetZ, int octaves, float scale, float lacunarity, float persistance, float height)
    {
        //Temporary storage for vertice locations
        float3[] vertices = new float3[(size + 1) * (size + 1)];

        //Generate a square of vertices
        //TODO: Replace i with a formula
        for(int i = 0, z = 0; z <= size; z++)
        {
            for(int x = 0; x <= size; x++)
            {
                //Height of the vertice
                float y = 0;

                //Used to decrease the affect of each layer of noise
                float amplitude = .5f;
                float frequency = 1;

                //Add layers of noise with decreasing amplitudes and increasing frequencies for extra detail
                for(int j = 0; j < octaves; j++)
                {
                    y += Mathf.PerlinNoise((float)(x + offsetX) / scale * frequency, (float)(z + offsetZ) / scale * frequency) * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                //Raise y to a power for flatter valleys and steeper hills
                //y = Mathf.Pow(y, heightPower);

                //Store the height, x, and z of the new vertice
                vertices[i] = new float3(x, y, z);
                i++;
            }
        }

        //Normalization: Just lowers map to y = 0
        //TODO: Map y from 0-1 and apply curve
        for(int i = 0, z = 0; z <= size; z++)
        {
            for(int x = 0; x <= size; x++)
            {
                vertices[i].y *= WorldGeneration.singleton.curve.Evaluate(vertices[i].y) * height;
                i++;
            }
        }

        vertices[0].y = 0;

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

    public static MeshData TrimWater(int[] triangles, float3[] vertices, float oceanLevel)
    {
        int trisToRemove = 0;
        
        for(int i = 0; i < triangles.Length - 3; i+=3)
        {
            bool deleteTriangle = true;
            for(int j = 0; j < 3; j++)
            {
                if(vertices[triangles[i + j]].y > oceanLevel)
                {
                    deleteTriangle = false;
                }
            }
            if(deleteTriangle)
            {
                triangles[i] = triangles[i + 1] = triangles[i + 2] = -1;
                trisToRemove++;
            }
        }

        int[] newTriangles = new int[triangles.Length - trisToRemove * 3];
        for(int i = 0, newTriIndex = 0; i < triangles.Length; i++)
        {
            if(triangles[i] != -1)
            {
                newTriangles[newTriIndex] = triangles[i];
                newTriIndex++;
            }
        }

        return new MeshData(vertices, newTriangles);
    }
}