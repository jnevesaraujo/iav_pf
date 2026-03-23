using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public enum CubeFace { Front, Back, Top, Bottom, Left, Right }
    public Vector3 position;
    public bool isSolid;
    // Os 8 vertices (mesmos da aula01)
    static readonly Vector3 v0 = new Vector3(-0.5f, -0.5f, 0.5f);
    static readonly Vector3 v1 = new Vector3(0.5f, -0.5f, 0.5f);
    static readonly Vector3 v2 = new Vector3(0.5f, -0.5f, -0.5f);
    static readonly Vector3 v3 = new Vector3(-0.5f, -0.5f, -0.5f);
    static readonly Vector3 v4 = new Vector3(-0.5f, 0.5f, 0.5f);
    static readonly Vector3 v5 = new Vector3(0.5f, 0.5f, 0.5f);
    static readonly Vector3 v6 = new Vector3(0.5f, 0.5f, -0.5f);
    static readonly Vector3 v7 = new Vector3(-0.5f, 0.5f, -0.5f);

    public enum BlockType { GRASS, DIRT, STONE, AIR, CHESS_RED, CHESS_WHITE }
    public BlockType type;

    public Block(BlockType type, Vector3 position)
    {
        this.type = type;
        this.position = position;
        isSolid = (type != BlockType.AIR);
    }
    // TODO: implementar

    Vector3[] GetFaceVertices(CubeFace face)
    {
        switch (face)
        {
            case CubeFace.Front: return new Vector3[] { v4, v5, v1, v0 };
            case CubeFace.Bottom: return new Vector3[] { v0, v1, v2, v3 };
            case CubeFace.Top: return new Vector3[] { v7, v6, v5, v4 };
            case CubeFace.Left: return new Vector3[] { v7, v4, v0, v3 };
            case CubeFace.Right: return new Vector3[] { v5, v6, v2, v1 };
            case CubeFace.Back: return new Vector3[] { v6, v7, v3, v2 };
            default: return new Vector3[] { v0, v0, v0, v0 };
        }
    }

    public void AddFaceToMeshData(CubeFace face,
    List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        // 1. int vertexIndex = vertices.Count; ← para quê?
        int vertexIndex = vertices.Count;

        // 2. Obter os 4 vértices da face (ver tabela da aula01)
        // 3. Somar this.position a cada vértice
        vertices.AddRange(GetFaceVertices(face));

        vertices[vertexIndex + 0] = vertices[vertexIndex + 0] + this.position;
        vertices[vertexIndex + 1] = vertices[vertexIndex + 1] + this.position;
        vertices[vertexIndex + 2] = vertices[vertexIndex + 2] + this.position;
        vertices[vertexIndex + 3] = vertices[vertexIndex + 3] + this.position;

        // 4. Adicionar vértices e UVs às listas
    /*     uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1)); */

        uvs.AddRange(GetUVs(face, type));

        // 5. Adicionar triângulos COM OFFSET (vertexIndex + ...)
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);

    }

    public static Vector2[] GetUVs(CubeFace face, BlockType type)
    {
        // Canto inferior-esquerdo de cada textura no atlas (coluna, linha) / 16
        Vector2 lbc;
        if (type == BlockType.GRASS)
        {
            //coluna 2, linha 6
            if (face == CubeFace.Top) lbc = new Vector2(2f, 6f) / 16;
            // coluna 2, linha 15
            else if (face == CubeFace.Bottom) lbc = new Vector2(2f, 15f) / 16;
            // coluna 3, linha 15
            else lbc = new Vector2(3f, 15f) / 16;
        }
        // coluna 2, linha 15
        else if (type == BlockType.DIRT) lbc = new Vector2(2f, 15f) / 16;
        // coluna 0, linha 2
        else if (type == BlockType.CHESS_RED) 
            lbc = new Vector2(0f, 13f) / 16;
        // coluna 1, linha 2
        else if (type == BlockType.CHESS_WHITE) 
            lbc = new Vector2(1f, 13f) / 16;
        else lbc = new Vector2(0f, 14f) / 16;
        Vector2 uv00 = lbc; // inferior-esquerdo
        Vector2 uv10 = lbc + new Vector2(1f, 0f) / 16; // inferior-direito
        Vector2 uv01 = lbc + new Vector2(0f, 1f) / 16; // superior-esquerdo
        Vector2 uv11 = lbc + new Vector2(1f, 1f) / 16; // superior-direito
        return new[] { uv11, uv01, uv00, uv10 };
    }

}
