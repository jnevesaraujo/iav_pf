using UnityEngine;

public class AliceCaptureTrigger : MonoBehaviour
{
    public AliceAgent alice;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("AliceCaptureTrigger: OnTriggerEnter with " + other.name);
        if (other.CompareTag("Rabbit"))
        {
            alice.NotifyCapture();
        }
    }
}
