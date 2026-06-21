using UnityEngine;

public class AliceCaptureTrigger : MonoBehaviour
{
    public PredatorAgent predator;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("AliceCaptureTrigger: OnTriggerEnter with " + other.name);
        if (other.CompareTag("Rabbit"))
        {
            predator.NotifyCapture();
        }
    }
}
