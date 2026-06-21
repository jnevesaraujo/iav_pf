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
    private bool episodeEnded = false;

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
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.fixedDeltaTime;
        move.y = velocity.y;

        if (move.x != 0 || move.z != 0)
            transform.rotation = Quaternion.LookRotation(new Vector3(move.x, 0, move.z));

        controller.Move(move * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Resets the agent state at the start of each episode.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reinicar booelano de fim de episódio e velocidade da alice
        velocity = Vector3.zero;
        episodeEnded = false;
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
        sensor.AddObservation(transform.localPosition); // 3 floats
        sensor.AddObservation(rabbitTransform.localPosition); // 3 floats

        // TOTAL = 6 floats. O resto é visto pelos olhos (Raycasts).
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
        ca[0] = move.x;
        ca[1] = move.y;
        ca[2] = 0f; // No rotation input for now
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

    internal void NotifyCapture()
    {
        if (!episodeEnded)
        {
            episodeEnded = true;
            arena.OnRabbitCaught();
        }
    }
}
