using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace Structs
{
    public class ChunkJob
    {
        public Chunk chunk;
        public GenerateChunkJob job;

        public ChunkJob(Chunk _chunk, GenerateChunkJob _job)
        {
            chunk = _chunk;
            job = _job;
        }
    }

    public struct ChunkInfo
    {
        public int index;
        public float offsetX;
        public float offsetZ;
        public float chunkHeight;
        public int resolution;

        public ChunkInfo(float _offsetX, float _offsetZ, float _chunkHeight, int _index, int _resolution) 
        {
            index = _index;
            offsetX = _offsetX;
            offsetZ = _offsetZ;
            chunkHeight = _chunkHeight;
            resolution = _resolution;
        }
    }

    public struct ChunkIndex
    {
        public int index;
        public MeshData chunk;

        public ChunkIndex(int _index, MeshData _chunk)
        {
            index = _index;
            chunk = _chunk;
        }
    }

    public struct float3
    {
        public float x;
        public float y;
        public float z;

        public float3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }
}
