using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public const int chunkSize = 16;
    public Block[,,] chunkData;
    public Material chunkMaterial;
    public float scale = 0.1f;
    public int octaves = 4;
    public float exponent = 0.5f;
    public float densityScale = -0.3f;
    public float densityThreshold = -0.1f;
    public int maxSolidHeight = 12;
    public Vector2Int worldOffset;
    public WorldManager worldManager;
    // variaveis para grutas
    public float caveScale = 0.1f;
    public float caveThreshold = 0.65f;
    // Multiplos layers
    public float seaLevel = 4f;
    public float maxHeight = 14f;
    public float detailAmplitude = 2f;

    void Start()
    {

    }

    public void Initialize(Vector2Int offset, Material mat, WorldManager manager)
    {
        worldOffset = offset;
        chunkMaterial = mat;
        worldManager = manager;
        InitializeChunk();
        // DrawChunk(); //NÃO é chamado aqui — ver D.4
    }
    void InitializeChunk()
    {
        chunkData = new Block[chunkSize, chunkSize, chunkSize];
        int[,] surfaceHeight = new int[chunkSize, chunkSize];

        // NOVO: Array para guardar o bloco que a classe WonderlandGenerator decidir para cada (x,z)
        Block.BlockType[,] surfaceTypes = new Block.BlockType[chunkSize, chunkSize];

        int margin = 2;
        float wx, wz, densityNoise;
        // Preencher os blocos baseados na densidade
        for (int x = 0; x < chunkSize; x++)
            for (int z = 0; z < chunkSize; z++)
            {
                wx = worldOffset.x * chunkSize + x;
                wz = worldOffset.y * chunkSize + z;

                WonderlandGenerator.GetBiomeData(wx, wz, out float finalHeight, out surfaceTypes[x, z]);
           
                // primeira passagem para definir altura da superficie
                for (int y = 0; y < chunkSize; y++)
                {
                    
                    //heightNoise = Mathf.Pow(NoiseUtils.FBm(wx, wz, 4, 0.05f), exponent) * chunkSize;
                    densityNoise = (NoiseUtils.Perlin3D(wx * densityScale, y * densityScale, wz * densityScale) - 0.5f) * 2f;
                    if (finalHeight - y + densityNoise > 0f)
                        surfaceHeight[x, z] = y;
                }

                // segunda passagem enche os blocos com carving
                for (int y = 0; y < chunkSize; y++)
                {
                    //heightNoise = Mathf.Pow(NoiseUtils.FBm(wx, wz, 4, 0.05f), exponent) * chunkSize;
                    densityNoise = (NoiseUtils.Perlin3D(wx * densityScale, y * densityScale, wz * densityScale) - 0.5f) * 2f;
                    bool solid = finalHeight - y + densityNoise > 0f;

                    if (solid && y > 1 && y < surfaceHeight[x, z] - margin)
                    {
                        float cx = (worldOffset.x * chunkSize + x) * caveScale;
                        float cy = y * caveScale;
                        float cz = (worldOffset.y * chunkSize + z) * caveScale;
                        if (NoiseUtils.Perlin3D(cx, cy, cz) > caveThreshold)
                            solid = false;
                    }

                    chunkData[x, y, z] = new Block(solid ? Block.BlockType.DIRT : Block.BlockType.AIR, new Vector3(x, y, z));
                }

            }

        // Inicio deterministico baseado na posição do chunk
        float wx0 = worldOffset.x * chunkSize;
        float wz0 = worldOffset.y * chunkSize;
        float startNoise = NoiseUtils.FBm(wx0, wz0, 4, 0.05f);
        int startX = Mathf.FloorToInt((startNoise * 0.5f + 0.5f) * chunkSize);
        int startZ = Mathf.FloorToInt((startNoise * 0.3f + 0.5f) * chunkSize);
        int startY = surfaceHeight[startX, startZ] / 2; // meio do chão

        CaveGenerator.CarveWorm(chunkData, chunkSize, worldOffset,
            new Vector3(wx0 + startX, startY, wz0 + startZ),
            steps: 200,
            radius: 2.5f,
            stepSize: 1f,
            directionScale: 0.05f);
        // refinar tipos com base nos vizinhos solidos refine types using HasSolidNeighbour
        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
                for (int z = 0; z < chunkSize; z++)
                {
                    if (!chunkData[x, y, z].isSolid) continue;

                    if (y <= 2)
                        chunkData[x, y, z].type = Block.BlockType.STONE;
                    else if (y < surfaceHeight[x, z] && (
                            !HasSolidNeighbour(x, y + 1, z) ||
                            !HasSolidNeighbour(x, y, z + 1) ||
                            !HasSolidNeighbour(x, y, z - 1) ||
                            !HasSolidNeighbour(x - 1, y, z) ||
                            !HasSolidNeighbour(x + 1, y, z)))
                        chunkData[x, y, z].type = Block.BlockType.STONE;
                    else if (!HasSolidNeighbour(x, y + 1, z))
                        chunkData[x, y, z].type = surfaceTypes[x, z];
                }
    }


    bool HasSolidNeighbour(int x, int y, int z)
    {
        // Desafio:
        // Modifiquem HasSolidNeighbour para que, quando x ou z saia fora de [0, chunkSize), 
        // consulte o chunk vizinho em vez de retornar false.
        // Dicas:
        // y fora dos limites → sem chunks acima/abaixo → retornar false (como antes)
        // x < 0 → vizinho em worldOffset + Vector2Int.left, coordenada local x = chunkSize - 1
        // x >= chunkSize → vizinho em worldOffset + Vector2Int.right , coordenada local x = 0
        // Mesma lógica para z (com up/down)
        // Se o vizinho não existir (borda do mundo) ou ainda não tiver dados → retornar false

        // y fora dos limites → sem chunks acima/abaixo → retornar false
        if (y < 0 || y >= chunkSize)
            return false;

        Vector2Int neighborOffset = Vector2Int.zero;

        // limites X
        if (x < 0)
        {
            neighborOffset += Vector2Int.left;
            x = chunkSize - 1;
        }
        else if (x >= chunkSize)
        {
            neighborOffset += Vector2Int.right;
            x = 0;
        }

        // limites Z
        if (z < 0)
        {
            neighborOffset += Vector2Int.down;
            z = chunkSize - 1;
        }
        else if (z >= chunkSize)
        {
            neighborOffset += Vector2Int.up;
            z = 0;
        }

        // Se não sai fora dos limites, verifica o próprio chunk
        if (neighborOffset == Vector2Int.zero)
            return chunkData[x, y, z].isSolid;

        // Se vizinho não existir → retornar false
        Vector2Int neighborCoord = worldOffset + neighborOffset;
        Chunk neighborChunk = worldManager.GetChunk(neighborCoord);
        if (neighborChunk == null)
            return false;

        return neighborChunk.chunkData[x, y, z].isSolid;
    }


    public void DrawChunk()
    {
        // 1. Criar listas partilhadas (vertices, triangles, uvs)
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // 2. Para cada bloco: adicionar TODAS as 6 faces
        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
                for (int z = 0; z < chunkSize; z++)
                {
                    Block block = chunkData[x, y, z];
                    if (!block.isSolid) continue; // blocos que não seja solidos, salta

                    if (!HasSolidNeighbour(x, y, z + 1))
                        block.AddFaceToMeshData(Block.CubeFace.Front, vertices, triangles, uvs);
                    if (!HasSolidNeighbour(x, y, z - 1))
                        block.AddFaceToMeshData(Block.CubeFace.Back, vertices, triangles, uvs);
                    if (!HasSolidNeighbour(x, y + 1, z))
                        block.AddFaceToMeshData(Block.CubeFace.Top, vertices, triangles, uvs);
                    if (!HasSolidNeighbour(x, y - 1, z))
                        block.AddFaceToMeshData(Block.CubeFace.Bottom, vertices, triangles, uvs);
                    if (!HasSolidNeighbour(x - 1, y, z))
                        block.AddFaceToMeshData(Block.CubeFace.Left, vertices, triangles, uvs);
                    if (!HasSolidNeighbour(x + 1, y, z))
                        block.AddFaceToMeshData(Block.CubeFace.Right, vertices, triangles, uvs);
                }

        // 3. Criar Mesh, atribuir arrays
        Mesh mesh = new Mesh();
        // usar indices 32-bit pq 16^3 blocks * 6 faces * 4 verts > 65535 (limite 16-bit)
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        // 4. RecalculateNormals + RecalculateBounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 5. Atribuir ao MeshFilter e MeshRenderer
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = chunkMaterial;
    }
}