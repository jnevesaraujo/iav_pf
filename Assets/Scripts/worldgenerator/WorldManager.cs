using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("Referências")]
    public List<Transform> trackedTargets = new List<Transform>();
    public GameObject chunkPrefab;
    public Material chunkMaterial;

    [Header("Configuração")]
    public int renderDistance = 3;
    public int chunkSize = 16;
    private Dictionary<Vector2Int, GameObject> activeChunks = new();
    private Dictionary<Transform, Vector2Int> lastTargetChunks = new();
    public int gridSize = 5; // fallback se não houver trackedTargets
    public int chunksPerFrame = 2;
    private Coroutine buildRoutine;

    [Header("Treino RL")]
    public bool isTrainingArena = false;
    internal Vector2Int physicalOffset = Vector2Int.zero;

    void Start()
    {
        if (isTrainingArena) return;

        if (trackedTargets.Count > 0)
        {
            HashSet<Vector2Int> needed = GetNeededChunks();
            foreach (var coord in needed)
                SpawnChunk(coord);

            foreach (var target in trackedTargets)
                if (target != null)
                    lastTargetChunks[target] = GetChunkCoord(target.position);
        }
        else
        {
            for (int cx = 0; cx < gridSize; cx++)
                for (int cz = 0; cz < gridSize; cz++)
                    SpawnChunk(new Vector2Int(cx, cz));
        }
    }

    void Update()
    {
        if (isTrainingArena) return;
        if (trackedTargets.Count == 0) return;

        bool anyTargetMoved = false;

        foreach (var target in trackedTargets)
        {
            if (target == null) continue;

            Vector2Int currentChunk = GetChunkCoord(target.position);
            if (!lastTargetChunks.TryGetValue(target, out Vector2Int lastChunk) || lastChunk != currentChunk)
            {
                lastTargetChunks[target] = currentChunk;
                anyTargetMoved = true;
            }
        }

        if (anyTargetMoved)
        {
            if (buildRoutine != null)
                StopCoroutine(buildRoutine);

            HashSet<Vector2Int> needed = GetNeededChunks();
            RemoveDistantChunks(needed);
            buildRoutine = StartCoroutine(BuildChunks(needed));
        }
    }

    private HashSet<Vector2Int> GetNeededChunks()
    {
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

        foreach (var target in trackedTargets)
        {
            if (target == null) continue;

            Vector2Int targetChunk = GetChunkCoord(target.position);
            for (int cx = targetChunk.x - renderDistance; cx <= targetChunk.x + renderDistance; cx++)
                for (int cz = targetChunk.y - renderDistance; cz <= targetChunk.y + renderDistance; cz++)
                    neededChunks.Add(new Vector2Int(cx, cz));
        }

        return neededChunks;
    }

    private void RemoveDistantChunks(HashSet<Vector2Int> neededChunks)
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunkCoord in activeChunks.Keys)
            if (!neededChunks.Contains(chunkCoord))
                chunksToRemove.Add(chunkCoord);

        foreach (var chunkCoord in chunksToRemove)
        {
            Destroy(activeChunks[chunkCoord]);
            activeChunks.Remove(chunkCoord);
        }
    }

    Vector2Int GetChunkCoord(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / chunkSize), Mathf.FloorToInt(pos.z / chunkSize));
    }

    public void SpawnChunk(Vector2Int coord)
    {
        Vector2Int physicalCoord = coord - physicalOffset;
        Vector3 worldPosition = new Vector3(physicalCoord.x * chunkSize, 0, physicalCoord.y * chunkSize);

        GameObject chunkObject = Instantiate(chunkPrefab, worldPosition, Quaternion.identity);
        chunkObject.name = $"Chunk_{coord.x}_{coord.y}";

        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.Initialize(coord, chunkMaterial, this);
        chunk.DrawChunk();

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
                    yield return null;
            }
        }
    }

    public void ClearAllChunks()
    {
        foreach (var chunk in activeChunks.Values)
            if (chunk != null) Destroy(chunk);
        activeChunks.Clear();
    }
}