using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System;

/// <summary>
/// Struct representing a mesh to allow for multithreaded world generation
/// </summary>
public struct MeshData
{
    public float3[] vertices;
    public int[] triangles;

    public NativeArray<float> heights;

    public MeshData(float3[] _vertices, int[] _triangles)
    {
        vertices = _vertices;
        triangles = _triangles;

        heights = new NativeArray<float>();
    }
    /// <summary>
    /// Generate a square chunk
    /// </summary>
    /// <param name="WorldGeneration.singleton.chunkSize">Width and length of a square chunk</param>
    /// <param name="offsetX">Offset on the X Axis</param>
    /// <param name="offsetZ">Offset on the Z Axis</param>
    /// <param name="octaves">Layers of detail to add</param>
    /// <param name="scale">Level of zoom of the chunk</param>
    /// <param name="lacunarity">Amount to decrease scale for each layer</param>
    /// <param name="persistance">Amount to decrease height for each layer</param>
    /// <param name="height">Amount to multiply height map by</param>
    /// <param name="heightPower">Power to raise height map to</param>
    /// <returns>A randomly generated MeshData representing a square chunk</returns>
    public void GenerateChunk(float offsetX, float offsetZ, float chunkHeight, int resolution)
    {
        GenerateVertices(offsetX, offsetZ, chunkHeight, resolution);
        GenerateTriangles(resolution);
        TrimWater();
    }

    /// <summary>
    /// Generate the vertices of a square chunk
    /// </summary>
    /// <param name="WorldGeneration.singleton.chunkSize">Width and length of a square chunk</param>
    /// <param name="offsetX">Offset on the X Axis</param>
    /// <param name="offsetZ">Offset on the Z Axis</param>
    /// <param name="octaves">Layers of detail to add</param>
    /// <param name="scale">Level of zoom of the chunk</param>
    /// <param name="lacunarity">Amount to decrease scale for each layer</param>
    /// <param name="persistance">Amount to decrease height for each layer</param>
    /// <param name="height">Amount to multiply height map by</param>
    /// <param name="heightPower">Power to raise height map to</param>
    /// <returns>Array of vertices representing the height map of a square chunk</returns>
    public void GenerateVertices(float xOffset, float zOffset, float chunkHeight, int resolution)
    {
        //Temporary storage for vertice locations
        WorldGeneration worldGen = WorldGeneration.singleton;
        int chunkSize = (int)(worldGen.chunkSize/resolution);
        float3[] vertices = new float3[(chunkSize + 1) * (chunkSize + 1)];

        //Generate a square of vertices
        //TODO: Replace i with a formula
        for(int i = 0, z = 0; z <= worldGen.chunkSize; z += resolution)
        {
            for(int x = 0; x <= worldGen.chunkSize; x += resolution)
            {
                //Height of the vertice
                float y = 0;

                //Used to decrease the affect of each layer of noise
                float amplitude = .5f;
                float frequency = 2;

                //Add layers of noise with decreasing amplitudes and increasing frequencies for extra detail
                for(int j = 0; j < worldGen.octaves; j++)
                {
                    y += Mathf.PerlinNoise((float)(x + xOffset) / worldGen.scale * frequency, (float)(z + zOffset) / worldGen.scale * frequency) * amplitude;
                    amplitude *= worldGen.persistance;
                    frequency *= worldGen.lacunarity;
                }
                y *= worldGen.height; //* chunkHeight;
                y *= WorldGeneration.singleton.curve.Evaluate(y); //* worldGen.height;
                //Store the height, x, and z of the new vertice
                vertices[i] = new float3(x, y, z);
                i++;
            }
        }

        //Normalization: Just lowers map to y = 0
        //TODO: Map y from 0-1 and apply curve
        // for(int i = 0, z = 0; z <= size; z++)
        // {
        //     for(int x = 0; x <= size; x++)
        //     {
        //         vertices[i].y *= WorldGeneration.singleton.curve.Evaluate(vertices[i].y) * height;
        //         i++;
        //     }
        // }

        this.vertices = vertices;
    
    }

    /// <summary>
    /// Generate an array to tell the computer how to render the mesh
    /// </summary>
    /// <returns>Array of integers representing the triangles of the chunk</returns>
    public void GenerateTriangles(float resolution)
    {
        int chunkSize = (int)(WorldGeneration.singleton.chunkSize/resolution);
        int[] triangles = new int[chunkSize * chunkSize * 6];

        int vert = 0;
        int triNum = 0;
        for(int z = 0; z < chunkSize; z++)
        {
            for(int x = 0; x < chunkSize; x++)
            {
                triangles[triNum] = vert + 0;
                triangles[triNum + 1] = vert + chunkSize + 1;
                triangles[triNum + 2] = vert + 1;
                triangles[triNum + 3] = vert + 1;
                triangles[triNum + 4] = vert + chunkSize + 1;
                triangles[triNum + 5] = vert + chunkSize + 2;
                
                vert++;
                triNum += 6;
            }
            vert++;
        }
        this.triangles = triangles;
    }

    public void TrimWater()
    {
        int trisToRemove = 0;
        
        for(int i = 0; i < triangles.Length - 3; i+=3)
        {
            bool deleteTriangle = true;
            for(int j = 0; j < 3; j++)
            {
                if(vertices[triangles[i + j]].y > WorldGeneration.singleton.oceanLevel)
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

        this.triangles = newTriangles;
    }
}

public struct GenerateVertexJob : IJobParallelFor
{
    int xOff;
    int zOff;

    NativeArray<float> heights;

    public GenerateVertexJob(int _x, int _z, NativeArray<float> _heights)
    {
        xOff = _z;
        zOff = _z;
        heights = _heights;
    }

    public void Execute(int i)
    {
        WorldGeneration worldGen = WorldGeneration.singleton;
        //Height of the vertice
        float y = 0;

        //Used to decrease the affect of each layer of noise
        float amplitude = .5f;
        float frequency = 2;

        int x = i/worldGen.chunkSize;
        int z = i%worldGen.chunkSize;

        //Add layers of noise with decreasing amplitudes and increasing frequencies for extra detail
        for(int j = 0; j < worldGen.octaves; j++)
        {
            y += Mathf.PerlinNoise((float)(xOff) / worldGen.scale * frequency, (float)(zOff) / worldGen.scale * frequency) * amplitude * worldGen.height;

            amplitude *= worldGen.persistance;
            frequency *= worldGen.lacunarity;
        }

        y = WorldGeneration.singleton.curve.Evaluate(heights[i]) * worldGen.height;
        //Store the height, x, and z of the new vertice
        heights[i] = y;
        i++;
    }
}