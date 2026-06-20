using UnityEngine;
public class WonderlandArena : MonoBehaviour
{
    [Header("Mundo Procedimental")]
    public TrainingArenaGenerator arenaBuilder;
    
    [Header("Agentes")]
    public AliceAgent alice;
    public RabbitAgent rabbit;

    [Header("Limites")]
    public int maxEpisodeSteps = 2000;
    //public float arenaHalfSize = 4.5f;

    [Header("Feedback visual (opcional)")]
    public MeshRenderer floorRenderer;
    public Material defaultMaterial;
    public Material predatorWinMaterial;
    public Material preyWinMaterial;
    private int stepCount;
    public void StartEpisode()
    {
        // Respawnar ambos os agentes em lados opostos da arena
        // Presa: Z positivo; Predador: Z negativo
        // ...
        stepCount = 0;

        if (arenaBuilder != null)
        {
            arenaBuilder.RebuildArenaRandomly();
        }

        alice.Place(new Vector3(Random.Range(8f, 24f), 25f, Random.Range(8f, 24f)));
        
        rabbit.gameObject.SetActive(true);
        rabbit.Place(new Vector3(Random.Range(8f, 24f), 25f, Random.Range(8f, 24f)));
    }


    private void FixedUpdate()
    {
        stepCount++;
        // Se stepCount >= maxEpisodeSteps → timeout (presa ganha)
        // ...
        if (stepCount >= maxEpisodeSteps)
        {
            // timeout: presa sobreviveu, presa ganha
            alice.AddReward(-1f);
            rabbit.AddReward(1f);

            EndAndReset();
        }
    }
    public void OnRabbitCaught()
    {
        // Predador tocou na presa: +1 predador, ‐1 presa
        // ...
        alice.AddReward(1f);
        rabbit.AddReward(-1f);

        EndAndReset();
    }

    public void EndAndReset()
    {
        // Chamar EndEpisode em ambos os agentes
        // ...
        Debug.Log("Ending episode. Step count: " + stepCount);
        alice.EndEpisode();
        rabbit.EndEpisode();
    }
}