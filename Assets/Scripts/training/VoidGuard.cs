using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VoidGuard : MonoBehaviour
{
    [Header("Proteção contra queda no vazio")]
    public bool enableGuard = false; // deixar desligado na arena de treino
    public float fallThreshold = -10f;
    public float safetyMargin = 2f;

    private CharacterController controller;
    private Vector3 lastSafePosition;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        lastSafePosition = transform.position;
    }

    void LateUpdate()
    {
        if (!enableGuard) return;

        if (controller.isGrounded)
        {
            lastSafePosition = transform.position;
        }
        else if (transform.position.y < fallThreshold)
        {
            controller.enabled = false;
            transform.position = lastSafePosition + Vector3.up * safetyMargin;
            controller.enabled = true;
        }
    }
}