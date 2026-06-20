using UnityEngine;

/// <summary>
/// Controla a geração de ambientes controlados para treino de Reinforcement Learning.
/// Isola a lógica de treino da lógica nativa de geração procedimental do mundo.
/// </summary>
public class TrainingArenaGenerator : MonoBehaviour
{
    [Header("Configuração da Arena")]
    public WorldManager worldManager;
    public int arenaSizeInChunks = 2;

    /// <summary>
    /// Reconstrói a arena de treino, garantindo um terreno diferente a cada episódio.
    /// </summary>
    public void RebuildArenaRandomly()
    {
        if (worldManager == null) return;

        // 1. Limpa os chunks da iteração anterior.
        worldManager.ClearAllChunks();

        // 2. Define um deslocamento aleatório para obter uma topografia nova a cada iteração.
        int randomOffsetX = Random.Range(-10000, 10000);
        int randomOffsetZ = Random.Range(-10000, 10000);
        worldManager.physicalOffset = new Vector2Int(randomOffsetX, randomOffsetZ);
//teste
        // 3. Gera a arena de forma síncrona para que esteja disponível no primeiro frame do episódio.
        for (int cx = 0; cx < arenaSizeInChunks; cx++)
        {
            for (int cz = 0; cz < arenaSizeInChunks; cz++)
            {
                Vector2Int coord = new Vector2Int(randomOffsetX + cx, randomOffsetZ + cz);
                worldManager.SpawnChunk(coord);
            }
        }
    }
}