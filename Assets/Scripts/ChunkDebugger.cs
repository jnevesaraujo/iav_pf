using UnityEngine;
public class ChunkDebugger : MonoBehaviour
{
    [SerializeField] private Material chunkMaterial;
    void Start()
    {

        CreateChunk(Vector3.zero);
        CreateChunk(new Vector3(16, 0, 0));
        CreateChunk(new Vector3(32, 0, 0));
        CreateChunk(new Vector3(0, 0, 16));     
        CreateChunk(new Vector3(0, 0, 32));
        CreateChunk(new Vector3(16, 0, 16));
        CreateChunk(new Vector3(32, 0, 16));
        CreateChunk(new Vector3(16, 0, 32));
        CreateChunk(new Vector3(32, 0, 32));

    }

    void CreateChunk(Vector3 position)
    {
        GameObject go = new GameObject($"Chunk_{position.x}_{position.y}_{position.z}");
        go.transform.position = position; // use the passed position, not Vector3.zero
        Chunk chunk = go.AddComponent<Chunk>();
        chunk.chunkMaterial = chunkMaterial;
    }
}
