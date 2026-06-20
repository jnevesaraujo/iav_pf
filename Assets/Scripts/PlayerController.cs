using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float mouseSensitivity = 2f;

    float rotationY = 0f;

    PlayerInputActions inputActions;
    InputAction moveAction;
    InputAction upDownAction;
    InputAction lookAction;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        moveAction   = inputActions.Player.Move;
        upDownAction = inputActions.Player.UpDown;
        lookAction   = inputActions.Player.Look;
    }

    void OnEnable()  => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    void Update()
    {
        // WASD movement
        Vector2 move = moveAction.ReadValue<Vector2>();
        float up     = upDownAction.ReadValue<float>();

        transform.Translate(new Vector3(move.x, up, move.y) * speed * Time.deltaTime);

        // Mouse look
        Vector2 look = lookAction.ReadValue<Vector2>();
        rotationY += look.x * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
}