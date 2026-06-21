using System.Collections;
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
    public Material defaultSkybox;
    public Material aliceWinSkybox; 
    public Material rabbitWinSkybox; 
    
    private bool isEpisodeEnding = false;
    private int stepCount;
    public void StartEpisode()
    {
        // Respawnar ambos os agentes em lados opostos da arena
        // Presa: Z positivo; Predador: Z negativo
        // ...
        stepCount = 0;
        isEpisodeEnding = false;

        if (defaultSkybox != null) RenderSettings.skybox = defaultSkybox;

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
        if (isEpisodeEnding) return;

        stepCount++;

        // Regra 1: O tempo acabou (Coelho sobreviveu à perseguição!)
        if (stepCount >= maxEpisodeSteps)
        {
            StartCoroutine(EndGameSequence(rabbitWinSkybox, 1f, -1f));
            return;
        }

        // Regra 2: Alice caiu da montanha para o abismo (Coelho ganha!)
        if (alice.transform.localPosition.y < -5f)
        {
            StartCoroutine(EndGameSequence(rabbitWinSkybox, 1f, -1f));
            return;
        }

        // Regra 3: Coelho escorregou e caiu no abismo (Alice ganha!)
        if (rabbit.transform.localPosition.y < -5f)
        {
            StartCoroutine(EndGameSequence(aliceWinSkybox, -1f, 1f));
            return;
        }

        float distance = Vector3.Distance(alice.transform.localPosition, rabbit.transform.localPosition);

        if (distance < 1.5f)
        {
            StartCoroutine(EndGameSequence(aliceWinSkybox, -1f, 1f));
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

    private IEnumerator EndGameSequence(Material winningSkybox, float rabbitReward, float aliceReward)
    {
        isEpisodeEnding = true; 

        rabbit.AddReward(rabbitReward);
        alice.AddReward(aliceReward);

        if (winningSkybox != null) RenderSettings.skybox = winningSkybox;

        yield return new WaitForSeconds(3f);

        EndAndReset();
    }
}