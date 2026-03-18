using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public GameObject chunkPrefab;
    public Material chunkMaterial;

    [Header("Configuração")]
    public int renderDistance = 3;
    public int chunkSize = 16;
    private Dictionary<Vector2Int, GameObject> activeChunks = new();
    private Vector2Int lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
    public int gridSize = 5; // 5×5 = 25 chunks

    void Start()
    {
        for (int cx = 0; cx < gridSize; cx++)
            for (int cz = 0; cz < gridSize; cz++)
            {
                SpawnChunk(new Vector2Int(cx, cz));
            }
    }

    void Update()
    {
        Vector2Int current = GetPlayerChunk();
        if (current != lastPlayerChunk)
        {
            lastPlayerChunk = current;
            UpdateChunks();
        }
    }

    Vector2Int GetPlayerChunk()
    {
        Vector3 pos = player.position;
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkSize), Mathf.FloorToInt(pos.z / chunkSize));
    }
    void UpdateChunks()
    {

        Vector2Int playerChunk = GetPlayerChunk();

        // TODO: 1.Construir HashSet<Vector2Int> com os chunks necessarios
        // (todos os (cx,cz) dentro de renderDistance do centro)
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
        for (int cx = playerChunk.x - renderDistance; cx <= playerChunk.x + renderDistance; cx++)
        {
            for (int cz = playerChunk.y - renderDistance; cz <= playerChunk.y + renderDistance; cz++)
            {
                neededChunks.Add(new Vector2Int(cx, cz));
            }
        }

        // TODO: 2.Remover chunks que já não são necessarios
        // Atenção: não modificar o Dictionary enquanto se itera!
        // Sugestão: recolher as chaves a remover numa lista separada
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunkCoord in activeChunks.Keys)
        {
            if (!neededChunks.Contains(chunkCoord))
            {
                chunksToRemove.Add(chunkCoord);
            }
        }

        // Remover chunks fora da distancia segurança
        foreach (var chunkCoord in chunksToRemove)
        {
            Destroy(activeChunks[chunkCoord]);
            activeChunks.Remove(chunkCoord);
            Debug.Log($"Chunk removido em {chunkCoord}");
        }

        // TODO: 3.Spawnar os chunks de 'needed' que ainda não existem
        foreach (var chunkCoord in neededChunks)
        {
            if (!activeChunks.ContainsKey(chunkCoord))
            {
                SpawnChunk(chunkCoord);
                activeChunks[chunkCoord].GetComponent<Chunk>().DrawChunk();
            }
        }

    }

    void SpawnChunk(Vector2Int coord)
    {
        // TODO: Calcular posição world: coord* chunkSize
        Vector3 worldPosition = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        // TODO: Instantiate do prefab, obter Chunk, chamar Initialize
        GameObject chunkObject = Instantiate(chunkPrefab, worldPosition, Quaternion.identity);
        chunkObject.name = $"Chunk_{coord.x}_{coord.y}";

        // Obtem componente chunk e chama initialize
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        if (chunk == null)
        {
            Debug.LogError("Chunk prefab não tem componente chunk!");
            Destroy(chunkObject);
            return;
        }
        chunk.Initialize(coord, chunkMaterial, this);

        // TODO: Registar no Dictionary activeChunks
        activeChunks[coord] = chunkObject;
        Debug.Log($"Spawned chunk at {coord}");
    }

    public Chunk GetChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject go))
            return go.GetComponent<Chunk>();

        return null;
    }
}