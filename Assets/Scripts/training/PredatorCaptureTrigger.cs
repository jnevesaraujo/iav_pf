using UnityEngine;

public class PredatorCaptureTrigger : MonoBehaviour
{
    public PredatorAgent predator;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PredatorCaptureTrigger: OnTriggerEnter with " + other.name);
        if (other.CompareTag("Prey"))
        {
            predator.NotifyCapture();
        }
    }
}
