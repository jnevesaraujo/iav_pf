using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class AliceAgent : Agent
{
    [Header("Scene References")]
    public Transform rabbitTransform;
/*     public Transform cakeTransform;
    public Transform doorTransform; */
    public TrainingArenaGenerator arenaBuilder;

    [Header("Visual Feedback")]
    public Material winMaterial;
    public Material loseMaterial;

    [Header("Parameters")]
    public float moveSpeed = 10f;
    public float gravity = -20f;
    private bool isSmall = false;
    private CharacterController controller;
    private Vector3 velocity;
    float moveX, moveZ;
    private PlayerInputActions inputActions;
    private InputAction moveAction;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        // Allows the agent to step over small obstacles without getting stuck
        controller.stepOffset = 1.1f;
    }
    void Start()
    {
        inputActions = new PlayerInputActions();
        moveAction = inputActions.Player.Move;
        moveAction.Enable();
    }


    void FixedUpdate()
    {
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.fixedDeltaTime;
        move.y = velocity.y;

        controller.Move(move * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Resets the agent and environment state at the start of each episode.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (arenaBuilder != null)
        {
            arenaBuilder.RebuildArenaRandomly();
        }

        isSmall = false;
        transform.localScale = Vector3.one;
        velocity = Vector3.zero;

        // Alice Spawn Turn off controller to teleport correctly
        controller.enabled = false;
        // Spawn Alice in the center of the arena and drop from the sky (Y=25)
        transform.localPosition = new Vector3(Random.Range(8f, 24f), 25f, Random.Range(8f, 24f));
        transform.localRotation = Quaternion.identity;
        controller.enabled = true;

        // Objects Spawn
        rabbitTransform.localPosition = new Vector3(Random.Range(4f, 28f), 25f, Random.Range(4f, 28f));

        cakeTransform.gameObject.SetActive(true);
        cakeTransform.localPosition = new Vector3(Random.Range(4f, 28f), 25f, Random.Range(4f, 28f));

        doorTransform.localPosition = new Vector3(Random.Range(4f, 28f), 25f, Random.Range(4f, 28f));
    }

    /// <summary>
    /// Returns a random position for the cake inside the valid scene bounds.
    /// </summary>
    /// <returns>A random local position for the cake.</returns>
    private Vector3 RandomFoodPosition()
    {
        return new Vector3(Random.Range(-3.5f, 3.5f), 0.5f, Random.Range(1.0f, 10.0f));
    }

    /// <summary>
    /// Collects observations needed by the learning algorithm.
    /// </summary>
    /// <param name="sensor">The sensor used to collect observations.</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isSmall ? 1.0f : 0.0f); // 1 float
        sensor.AddObservation(transform.localPosition); // 3 floats
        sensor.AddObservation(rabbitTransform.localPosition); // 3 floats

        if (cakeTransform.gameObject.activeSelf)
        {
            sensor.AddObservation(cakeTransform.localPosition);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        sensor.AddObservation(doorTransform.localPosition); // 3 floats
    }

    /// <summary>
    /// Applies the actions chosen by the policy to move the agent.
    /// </summary>
    /// <param name="actions">The action buffers containing continuous and discrete actions.</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.001f);

        var ca = actions.ContinuousActions;
        moveX = ca[0];
        moveZ = ca[1];

        if (transform.localPosition.y < -5f)
        {
            AddReward(-1f);
            EndEpisode();
        }
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
    }

    /// <summary>
    /// Handles trigger collisions with walls, cake, and the final door.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }

        var raySensor = GetComponent<RayPerceptionSensorComponent3D>();
        if (other.CompareTag("Cake"))
        {
            if (!isSmall)
            {
                isSmall = true;
                transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                // Scale down the ray sensor's length to match the new size of the agent
                if (raySensor != null) raySensor.RayLength = 40f;

                other.gameObject.SetActive(false);
                AddReward(0.1f);
            }
            return;
        }

        if (other.CompareTag("DoorFrame"))
        {
            if (!isSmall)
            {
                AddReward(-1f);
            }
            else
            {
                AddReward(1f);
            }

            EndEpisode();
        }
    }
}
