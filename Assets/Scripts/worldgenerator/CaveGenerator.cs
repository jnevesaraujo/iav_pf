using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator
{
    public static void CarveWorm(Block[,,] chunkData, int chunkSize, Vector2Int worldOffset, Vector3 start, int steps, float radius, float stepSize, float directionScale)
    {
        Vector3 pos = start;
        for (int i = 0; i < steps; i++)
        {
            // Direcção determinada por noise
            float nx = NoiseUtils.Perlin3D(pos.x * directionScale,
            pos.y * directionScale,
            pos.z * directionScale) * 2f - 1f;

            float ny = NoiseUtils.Perlin3D(pos.y * directionScale + 100f,
            pos.z * directionScale + 100f,
            pos.x * directionScale + 100f) * 2f - 1f;
            
            float nz = NoiseUtils.Perlin3D(pos.z * directionScale + 200f,
            pos.x * directionScale + 200f,
            pos.y * directionScale + 200f) * 2f - 1f;
            
            Vector3 dir = new Vector3(nx, ny * 0.5f, nz).normalized;
            pos += dir * stepSize;
            // Escavar esfera à volta da posição actual
            CarveAt(chunkData, chunkSize, worldOffset, pos, radius);
        }
    }
    static void CarveAt(Block[,,] chunkData, int chunkSize, Vector2Int worldOffset, Vector3 center, float radius)
    {
        // Converter coordenadas globais para locais

        int localX = Mathf.RoundToInt(center.x) - worldOffset.x * chunkSize;
        int localY = Mathf.RoundToInt(center.y);
        int localZ = Mathf.RoundToInt(center.z) - worldOffset.y * chunkSize;
        int r = Mathf.CeilToInt(radius);
        for (int dx = -r; dx <= r; dx++)
            for (int dy = -r; dy <= r; dy++)
                for (int dz = -r; dz <= r; dz++)
                {
                    if (dx * dx + dy * dy + dz * dz > radius * radius) continue;
                    int bx = localX + dx;
                    int by = localY + dy;
                    int bz = localZ + dz;
                    if (bx >= 0 && bx < chunkSize &&
                    by > 1 && by < chunkSize &&
                    bz >= 0 && bz < chunkSize)
                    {
                        chunkData[bx, by, bz] = new Block(Block.BlockType.AIR, new Vector3(bx, by, bz)); // ar
                    }
                }
    }
}