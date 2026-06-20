using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class RabbitAgent : Agent
{
    [Header("Referências")]
    public Transform alice;
    public WonderlandArena arena;

    [Header("Movimento")]
    public float moveSpeed = 8f;
    public float gravity = -20f;
    private CharacterController controller;
    private Vector3 velocity;
    private float moveX, moveZ;
    private InputAction moveAction;

    void Start()
    {
        // WASD for prey
        moveAction = new InputAction("PreyMove", binding: "<Keyboard>/w");
        var moveComposite = moveAction.AddCompositeBinding("2DVector");
        moveComposite.With("Up", "<Keyboard>/i");
        moveComposite.With("Down", "<Keyboard>/k");
        moveComposite.With("Left", "<Keyboard>/j");
        moveComposite.With("Right", "<Keyboard>/l");
        moveAction.Enable();

    }
    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        controller.stepOffset = 1.1f;
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
    public override void OnEpisodeBegin()
    {
        velocity = Vector3.zero;
        // O Coelho dita quando a arena reinicia
        arena.StartEpisode();
    }

    public void Place(Vector3 localPosition)
    {
        controller.enabled = false;
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        controller.enabled = true;
        velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observa apenas a própria velocidade. Os "Olhos" (Raycasts) farão o resto do trabalho.
        sensor.AddObservation(velocity.x / moveSpeed);
        sensor.AddObservation(velocity.z / moveSpeed);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Ganha uma micro-recompensa por cada momento que sobrevive vivo a fugir
        AddReward(0.001f);

        moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        /* 
                Vector3 horizontal = new Vector3(moveX, 0f, moveZ) * moveSpeed;

                if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;

                velocity.y += gravity * Time.deltaTime;
                velocity.x = horizontal.x;
                velocity.z = horizontal.z;

                controller.Move(velocity * Time.deltaTime);
         */
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        Vector2 move = moveAction.ReadValue<Vector2>(); 
        ca[0] = move.x;
        ca[1] = move.y;
    }
}