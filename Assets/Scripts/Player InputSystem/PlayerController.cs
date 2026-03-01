using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 15f;
    [Min(1f)] public float fallGravityMultiplier = 3f;

    private Rigidbody rb;
    private Vector2 moveInput;

    private PlayerInputActions inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
    }

    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Disable();
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        ApplyExtraFallGravity();

        if (OpeningLock.IsLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (!GameStateManager.IsNormal)
        {
            moveInput = Vector2.zero;
            return;
        }

        // 固定相机 → 用世界坐标
        Vector3 move = new Vector3(-moveInput.x, 0f, -moveInput.y);

        if (move.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(move);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void ApplyExtraFallGravity()
    {
        if (fallGravityMultiplier <= 1f)
        {
            return;
        }

        if (rb.linearVelocity.y < 0f)
        {
            Vector3 extraGravity = Physics.gravity * (fallGravityMultiplier - 1f);
            rb.AddForce(extraGravity, ForceMode.Acceleration);
        }
    }
}
