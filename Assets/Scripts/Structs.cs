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
        public int size;
        public float offsetX;
        public float offsetZ;
        public int octaves;
        public float scale;
        public float lacunarity;
        public float persistance;
        public float height;
        public float oceanHeight;

        public ChunkInfo(int _size, float _offsetX, float _offsetZ, int _octaves, float _scale, float _lacunarity, float _persistance, float _height, float _oceanHeight, int _index)
        {
            index = _index;
            size = _size;
            offsetX = _offsetX;
            offsetZ = _offsetZ;
            octaves = _octaves;
            scale = _scale;
            lacunarity = _lacunarity;
            persistance = _persistance;
            height = _height;
            oceanHeight = _oceanHeight;
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
}
