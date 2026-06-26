using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using System;

public class AliceAgent : Agent
{
    [Header("Scene References")]
    public Transform rabbitTransform;
    /*     public Transform cakeTransform;
        public Transform doorTransform; */
    public WonderlandArena arena;

    [Header("Visual Feedback")]
    public Material winMaterial;
    public Material loseMaterial;

    [Header("Parameters")]
    public float moveSpeed = 10f;
    public float gravity = -20f;

    private CharacterController controller;
    private Vector3 velocity;
    float moveX, moveZ;
    private PlayerInputActions inputActions;
    private InputAction moveAction;
    private Vector3 smoothedMovement;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        // Allows the agent to step over small obstacles without getting stuck
        controller.stepOffset = 1.1f;
    }
    void Start()
    {
        // Arrow keys for predator
        moveAction = new InputAction("AliceMove");
        var moveComposite = moveAction.AddCompositeBinding("2DVector");
        moveComposite.With("Up", "<Keyboard>/upArrow");
        moveComposite.With("Down", "<Keyboard>/downArrow");
        moveComposite.With("Left", "<Keyboard>/leftArrow");
        moveComposite.With("Right", "<Keyboard>/rightArrow");
        moveAction.Enable();

    }


    void FixedUpdate()
    {
        Vector3 rawMove = new Vector3(moveX, 0, moveZ) * moveSpeed;

        smoothedMovement = Vector3.Lerp(smoothedMovement, rawMove, Time.fixedDeltaTime * 8f);
        //Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;
        Vector3 move = smoothedMovement;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.fixedDeltaTime;
        move.y = velocity.y;

        Vector3 direction = new Vector3(move.x, 0, move.z);
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // Slerp: Roda suavemente (interpolação)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 4f);
        }

        controller.Move(move * Time.fixedDeltaTime);

        /*         Vector3 beforePos = transform.position;
                controller.Move(move * Time.fixedDeltaTime);
                Vector3 actualDelta = transform.position - beforePos;
                Debug.Log($"requested: {move}, actual: {actualDelta}"); */
    }

    /// <summary>
    /// Resets the agent state at the start of each episode.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reinicar booelano de fim de episódio e velocidade da alice
        velocity = Vector3.zero;
    }

    public void Place(Vector3 localPosition)
    {
        controller.enabled = false;
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.identity;
        controller.enabled = true;
    }

    /// <summary>
    /// Collects observations needed by the learning algorithm.
    /// </summary>
    /// <param name="sensor">The sensor used to collect observations.</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 toRabbit = rabbitTransform.localPosition - transform.localPosition;
        sensor.AddObservation(toRabbit.normalized);
    }

    /// <summary>
    /// Applies the actions chosen by the policy to move the agent.
    /// </summary>
    /// <param name="actions">The action buffers containing continuous and discrete actions.</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        //AddReward(-0.001f);

        var ca = actions.ContinuousActions;
        moveX = ca[0];
        moveZ = ca[1];

    }

    /// <summary>
    /// Supplies manual inputs for debugging and testing.
    /// </summary>
    /// <param name="actionsOut">The output action buffers for heuristic control.</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;

        Vector2 move = moveAction.ReadValue<Vector2>();

        // Se não houver câmara principal na cena, usa o movimento global normal como plano B
        if (Camera.main == null)
        {
            ca[0] = move.x;
            ca[1] = move.y;
            return;
        }

        // Verifica para onde a câmara está a olhar (ignorar o eixo Y / altura)
        Transform camTransform = Camera.main.transform;

        Vector3 camForward = camTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = camTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // converte o input local num vetor global
        Vector3 playerIntent = (camForward * move.y) + (camRight * move.x);

        ca[0] = playerIntent.x;
        ca[1] = playerIntent.z;
    }

    /// <summary>
    /// Handles trigger collisions with walls, cake, and the final door.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            return;
        }

        if (other.CompareTag("Rabbit"))
        {
            arena.OnRabbitCaught();
        }
    }
}
