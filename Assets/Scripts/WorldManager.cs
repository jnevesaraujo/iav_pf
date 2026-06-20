using System;
using System.Collections;
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
    // variaveis para coroutines
    public int chunksPerFrame = 2;
    private Coroutine buildRoutine;

    [Header("Treino RL")]
    public bool isTrainingArena = false;
    internal Vector2Int physicalOffset = Vector2Int.zero;

    void Start()
    {
        if (isTrainingArena) return;

        for (int cx = 0; cx < gridSize; cx++)
            for (int cz = 0; cz < gridSize; cz++)
            {
                SpawnChunk(new Vector2Int(cx, cz));
            }
    }

    void Update()
    {
        if (isTrainingArena) return;
        Vector2Int current = GetPlayerChunk();

        if (current != lastPlayerChunk)
        {
            lastPlayerChunk = current;
            // Cancelar a coroutine anterior (se ainda estiver a correr)
            if (buildRoutine != null)
                StopCoroutine(buildRoutine);
            // Remover chunks fora do range (isto continua síncrono)
            RemoveDistantChunks(current);

            // Lançar nova coroutine para gerar os novos
            buildRoutine = StartCoroutine(BuildChunks(GetNeededChunks(current)));
        }
    }

    private HashSet<Vector2Int> GetNeededChunks(Vector2Int current)
    {
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
        for (int cx = current.x - renderDistance; cx <= current.x + renderDistance; cx++)
        {
            for (int cz = current.y - renderDistance; cz <= current.y + renderDistance; cz++)
            {
                neededChunks.Add(new Vector2Int(cx, cz));
            }
        }
        
        return neededChunks;
    }

    private void RemoveDistantChunks(Vector2Int current)
    {
        HashSet<Vector2Int> neededChunks = GetNeededChunks(current);
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
    }

    Vector2Int GetPlayerChunk()
    {
        Vector3 pos = player.position;
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkSize), Mathf.FloorToInt(pos.z / chunkSize));
    }

    public void SpawnChunk(Vector2Int coord)
    {
        // TODO: Calcular posição world: coord* chunkSize
        Vector3 worldPosition = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        // TODO: Instantiate do prefab, obter Chunk, chamar Initialize
        GameObject chunkObject = Instantiate(chunkPrefab, worldPosition, Quaternion.identity);
        chunkObject.name = $"Chunk_{coord.x}_{coord.y}";

        // Obtem componente chunk e chama initialize
        Chunk chunk = chunkObject.GetComponent<Chunk>();

        chunk.Initialize(coord, chunkMaterial, this);
        chunk.DrawChunk();

        // TODO: Registar no Dictionary activeChunks
        activeChunks[coord] = chunkObject;
    }

    public Chunk GetChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject go))
            return go.GetComponent<Chunk>();

        return null;
    }

    IEnumerator BuildChunks(HashSet<Vector2Int> needed)
    {
        int count = 0;
        foreach (var coord in needed)
        {
            if (!activeChunks.ContainsKey(coord))
            {
                SpawnChunk(coord);
                count++;
                if (count % chunksPerFrame == 0)
                    yield return null; // pausa até ao próximo frame
            }
        }
    }


    /// <summary>
    /// Remove todos os chunks ativos da cena de forma síncrona.
    /// </summary>
    public void ClearAllChunks()
    {
        foreach (var chunk in activeChunks.Values)
        {
            if (chunk != null) Destroy(chunk);
        }
        activeChunks.Clear();
    }

}