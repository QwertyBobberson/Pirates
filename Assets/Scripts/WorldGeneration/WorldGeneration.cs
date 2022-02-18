using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

using Random = UnityEngine.Random;

public class WorldGeneration : MonoBehaviour
{
    public float chunkShreshold;
    public float chunkNoiseScale;
    public bool multiThread;
    /// <summary>
    /// Prefab that holds a single chunk
    /// </summary>
    public GameObject chunkObj;

    /// <summary>
    /// Size of an individual chunk
    /// </summary>
    public int chunkSize;

    /// <summary>
    /// Amount of octaves to use when generating terrain
    /// </summary>
    public int octaves;

    /// <summary>
    /// Scale of terrain to generate
    /// </summary>
    public float scale;

    /// <summary>
    /// How quickly octaves increase in detail
    /// </summary>
    public float lacunarity;

    /// <summary>
    /// How quickly octaves decrease in affect
    /// </summary>
    public float persistance;

    /// <summary>
    /// Height of the final terrain
    /// </summary>
    public float height;

    /// <summary>
    /// Seed for the random number generator
    /// </summary>
    public int seed;
    /// <summary>
    /// Minimum y level to draw triangles at
    /// </summary>
    public float oceanLevel;

    /// <summary>
    /// Starting coords of world
    /// </summary>
    public Vector2 offset;

    /// <summary>
    /// Curve to apply to terrain
    /// </summary>
    public AnimationCurve curve;

    /// <summary>
    /// Object to follow when keeping chunks loaded
    /// </summary>
    public GameObject player;

    /// <summary>
    /// List of all chunks currently loaded
    /// </summary>
    public Chunk[] loadedChunks;
    /// <summary>
    /// The chunk the player is currently on
    /// </summary>
    private Vector3 currentChunk;
    /// <summary>
    /// The chunk the player was in during the previous frame
    /// </summary>
    private Vector3 prevChunk;

    public GameObject debugSphere;

    /// <summary>
    /// Max distance from the player a chunk can be before it is unloaded
    /// </summary>
    public int renderDistance;

    public static WorldGeneration singleton;

    public int resolution;

    public void Start()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Debug.LogError("There are two world generators");
        }
        DestroyWorld();
        InitialLoad();
    }


    public void Update()
    {
        currentChunk = GetChunk(player.transform.position);
        if (prevChunk != currentChunk)
        {
            NativeList<JobHandle> handles = new NativeList<JobHandle>(Allocator.TempJob);
            List<int> chunksToReload = new List<int>();

            for (int i = 0; i < loadedChunks.Length; i++)
            {
                Vector3 chunkDist = GetChunk(loadedChunks[i].transform.position) - currentChunk;
                if (Mathf.Abs(chunkDist.x) > renderDistance || Mathf.Abs(chunkDist.z) > renderDistance)
                {
                    chunksToReload.Add(i);
                    Vector3 newChunkDist = GetChunk(loadedChunks[i].transform.position) - prevChunk;
                    Destroy(loadedChunks[i].gameObject);
                    Vector3 newChunkPos = new Vector3(currentChunk.x - newChunkDist.x, 0, currentChunk.z - newChunkDist.z);

                    if (multiThread)
                    {
                        JobHandle handle;
                        bool exists = (CreateGenerateChunkJob(newChunkPos.x, newChunkPos.z, i, out handle));
                        if(exists)
                        {
                            handles.Add(handle);
                        }
                    }
                    else
                    {
                        DateTime time = DateTime.Now;
                        loadedChunks[i] = GenerateChunk(newChunkPos.x, newChunkPos.z);
                        Debug.Log($"{loadedChunks.Length} chunks loaded in {(DateTime.Now - time).TotalMilliseconds} milliseconds");
                    }
                }
            }
            if (chunksToReload.Count > 0 && multiThread)
            {
                DateTime time = DateTime.Now;
                JobHandle.CompleteAll(handles);
                Debug.Log($"{chunksToReload.Count} chunks loaded in {(DateTime.Now - time).TotalMilliseconds} milliseconds");
            }
            handles.Dispose();

            for (int i = 0; i < chunksToReload.Count; i++)
            {
                loadedChunks[chunksToReload[i]].UpdateMesh();
            }
        }
        prevChunk = currentChunk;
    }

    /// <summary>
    /// Loads an area renderdistance x renderdistance large
    /// Updates when the player moves
    /// </summary>
    public void InitialLoad()
    {
        loadedChunks = new Chunk[(int)Mathf.Pow(renderDistance * 2 + 1, 2)];
        Random.InitState(seed);
        offset.x = Random.Range(-100000, 100000);
        offset.y = Random.Range(-100000, 100000);

        NativeList<JobHandle> handles = new NativeList<JobHandle>(Allocator.TempJob);

        DateTime time = DateTime.Now;

        for (int i = 0, x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++, i++)
            {
                if (multiThread)
                {
                    JobHandle handle;
                    bool exists = CreateGenerateChunkJob(x, z, i, out handle);
                    if(exists) handles.Add(handle);
                }
                else
                {
                    loadedChunks[i] = (GenerateChunk(x, z));
                }
            }
        }

        if (multiThread)
        {
            JobHandle.CompleteAll(handles);
        }

        for (int i = 0; i < loadedChunks.Length; i++)
        {
            loadedChunks[i].UpdateMesh();
        }
        
        Debug.Log($"{handles.Length} chunks loaded in {(DateTime.Now - time).TotalMilliseconds} milliseconds");
        handles.Dispose();
    }

    /// <summary>
    /// Translates coordinates to chunk coordinates
    /// </summary>
    /// <param name="position">Position in world space</param>
    /// <returns>Position in chunk space</returns>
    public static Vector3 GetChunk(Vector3 position)
    {
        return new Vector3(Mathf.Floor(position.x / WorldGeneration.singleton.chunkSize),
                           0,
                           Mathf.Floor(position.z / WorldGeneration.singleton.chunkSize));
    }

    /// <summary>
    /// Destroy all chunks that are a child of this world generator
    /// </summary>
    public void DestroyWorld()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public Chunk GenerateChunk(float x, float z)
    {
        Chunk chunk = GameObject.Instantiate(chunkObj, new Vector3(x * chunkSize, 0, z * chunkSize), Quaternion.identity, this.transform).GetComponent<Chunk>();
        
        float chunkVal = Mathf.PerlinNoise(x * chunkSize * chunkNoiseScale + offset.x/chunkSize, z * chunkSize * chunkNoiseScale);
        if(chunkVal > chunkShreshold)
        {
            chunk.hasIsland = true;
            chunk.meshData.GenerateChunk(x * chunkSize + offset.x, z * chunkSize + offset.y, chunkVal, resolution);
        }
        else
        {
            chunk.hasIsland = false;
            Debug.Log($"Chunk at ({x},{z}) had a height of {chunkVal}, too low to generate a chunk");
        }

        return chunk;
    }

    public bool CreateGenerateChunkJob(float x, float z, int i, out JobHandle handle)
    {
        Chunk chunk = GameObject.Instantiate(chunkObj, new Vector3(x * chunkSize, 0, z * chunkSize), Quaternion.identity, this.transform).GetComponent<Chunk>();
        loadedChunks[i] = chunk;
        chunk.gameObject.name = $"{x}, {z}";
        chunk.meshData = new MeshData();
        float chunkVal = Mathf.PerlinNoise((x * chunkSize  + offset.x)/chunkSize/chunkNoiseScale, (z * chunkSize + offset.y)/chunkSize/chunkNoiseScale);
        
        chunk.gameObject.GetComponent<Renderer>().material.color = new Color(chunkVal, chunkVal, chunkVal);
        if(chunkVal > chunkShreshold)
        {
            chunk.hasIsland = true;
            Structs.ChunkInfo info = new Structs.ChunkInfo(x * chunkSize + offset.x, z * chunkSize + offset.y, chunkVal, i, resolution);
            handle = new GenerateChunkJob(info).Schedule();
            return true;
        }
        else
        {
            chunk.hasIsland = false;
            handle = new JobHandle();
            return false;
        }
    }
}

public struct GenerateChunkJob : IJob
{
    private Structs.ChunkInfo info;

    public GenerateChunkJob(Structs.ChunkInfo _info)
    {
        info = _info;
    }

    public void Execute()
    {
        WorldGeneration.singleton.loadedChunks[info.index].meshData.GenerateChunk(info.offsetX, info.offsetZ, info.chunkHeight, info.resolution);
    }
}