using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 15f;
    [Min(1f)] public float fallGravityMultiplier = 3f;

    [Header("Visual")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string runStateName = "Run";

    private Rigidbody rb;
    private Vector2 moveInput;

    private PlayerInputActions inputActions;
    private bool isRunning;

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

        if (OpeningLock.IsLocked || !GameStateManager.IsNormal)
        {
            moveInput = Vector2.zero;
            UpdateVisualState(0f);
            return;
        }

        // 角色在3D场景中移动，仅视觉对象负责2D动画与翻转
        Vector3 move = new Vector3(-moveInput.x, 0f, -moveInput.y);

        if (move.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(move);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        UpdateVisualState(moveInput.x);
    }

    void UpdateVisualState(float horizontalInput)
    {
        if (playerAnimator == null || playerSpriteRenderer == null)
        {
            return;
        }

        if (horizontalInput > 0f)
        {
            playerSpriteRenderer.flipX = false;
            if (!isRunning)
            {
                playerAnimator.Play(runStateName);
                isRunning = true;
            }
        }
        else if (horizontalInput < 0f)
        {
            playerSpriteRenderer.flipX = true;
            if (!isRunning)
            {
                playerAnimator.Play(runStateName);
                isRunning = true;
            }
        }
        else if (isRunning)
        {
            playerAnimator.Play(idleStateName);
            isRunning = false;
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
