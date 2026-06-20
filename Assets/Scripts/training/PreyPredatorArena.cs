using UnityEngine;
public class PreyPredatorArena : MonoBehaviour
{
    [Header("Agentes")]
    public PreyAgent prey;
    public PredatorAgent predator;
    [Header("Limites")]
    public int maxEpisodeSteps = 2000;
    public float arenaHalfSize = 4.5f;
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

        if (floorRenderer != null)
            floorRenderer.material = defaultMaterial;

        // presa: Z positivo
        prey.Place(new Vector3(
            Random.Range(-arenaHalfSize, arenaHalfSize), 0.5f, Random.Range(1f, arenaHalfSize)));
        // predador: Z negativo
        predator.Place(new Vector3(
            Random.Range(-arenaHalfSize, arenaHalfSize), 0.5f, Random.Range(-arenaHalfSize, -1f)));
        /*         prey.transform.localPosition = new Vector3(
                    Random.Range(-arenaHalfSize, arenaHalfSize), 0.5f, Random.Range(1f, arenaHalfSize));
         */

        /*         predator.transform.localPosition = new Vector3(
                    Random.Range(-arenaHalfSize, arenaHalfSize), 0.5f, Random.Range(-arenaHalfSize, -1f));
         */
    }


    private void FixedUpdate()
    {
        stepCount++;
        // Se stepCount >= maxEpisodeSteps → timeout (presa ganha)
        // ...
        if (stepCount >= maxEpisodeSteps)
        {
            // timeout: presa sobreviveu, presa ganha
            prey.AddReward(1f);
            predator.AddReward(-1f);

            if (floorRenderer != null)
                floorRenderer.material = preyWinMaterial;

            EndAndReset();
        }
    }
    public void OnPreyCaptured()
    {
        // Predador tocou na presa: +1 predador, ‐1 presa
        // ...
        predator.AddReward(1f);
        prey.AddReward(-1f);

        if (floorRenderer != null)
            floorRenderer.material = predatorWinMaterial;

        EndAndReset();
    }

    private void EndAndReset()
    {
        // Chamar EndEpisode em ambos os agentes
        // ...
        Debug.Log("Ending episode. Step count: " + stepCount);
        prey.EndEpisode();
        predator.EndEpisode();
    }
}