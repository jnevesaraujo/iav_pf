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

        bool isTraining = arenaBuilder != null
        && arenaBuilder.worldManager != null
        && arenaBuilder.worldManager.isTrainingArena;

        if (isTraining)
        {
            arenaBuilder.RebuildArenaRandomly();

            alice.Place(new Vector3(Random.Range(8f, 24f), 15f, Random.Range(8f, 24f)));
            rabbit.Place(new Vector3(Random.Range(8f, 24f), 15f, Random.Range(8f, 24f)));
        }
        alice.Place(new Vector3(Random.Range(0f, 64f), 15f, Random.Range(0f, 64f)));
        rabbit.Place(new Vector3(Random.Range(0f, 64f), 15f, Random.Range(0f, 64f)));
    }


    private void FixedUpdate()
    {
        stepCount++;

        // Regra 1: O tempo acabou (Coelho sobreviveu à perseguição!)
        if (stepCount >= maxEpisodeSteps)
        {
            Debug.Log("Max steps reached. Ending episode. Rabbit wins by survival.");
            alice.AddReward(-1f);
            rabbit.AddReward(1f);
            EndAndReset();
            return;
        }

        // Regra 2: Alice caiu da montanha para o abismo (Coelho ganha!)
        if (alice.transform.localPosition.y < -5f)
        {
            Debug.Log("Alice fell off the cliff. Rabbit wins!");
            alice.AddReward(-1f);
            rabbit.AddReward(1f);
            EndAndReset();
            return;
        }

        // Regra 3: Coelho escorregou e caiu no abismo (Alice ganha!)
        if (rabbit.transform.localPosition.y < -5f)
        {
            Debug.Log("Rabbit fell off the cliff. Alice wins!");
            rabbit.AddReward(-1f);
            alice.AddReward(1f);
            EndAndReset();
            return;
        }

        float distance = Vector3.Distance(alice.transform.localPosition, rabbit.transform.localPosition);

        if (distance < 1.5f)
        {
            Debug.Log("Alice caught the rabbit! Distance: " + distance);
            alice.AddReward(1f);
            rabbit.AddReward(-1f);
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