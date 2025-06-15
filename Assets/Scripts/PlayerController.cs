using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("Jump Settings")]
    public LayerMask groundLayer;
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    // Components
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;

    // Internal State
    private Vector2 moveInput;
    private bool isGhost = false;

    // Input Buffering & State Tracking
    private bool jumpInputBuffer = false;
    private bool interactInputBuffer = false;
    private bool wasJumpPressedLastFrame = false;
    private bool wasInteractPressedLastFrame = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 壁に張り付かないように、摩擦ゼロの物理マテリアルを動的に作成して割り当てる
        PhysicsMaterial2D noFrictionMaterial = new PhysicsMaterial2D("NoFriction");
        noFrictionMaterial.friction = 0f;
        // RigidbodyとColliderの両方に設定して確実にする
        rb.sharedMaterial = noFrictionMaterial;
        boxCollider.sharedMaterial = noFrictionMaterial;

        if (TryGetComponent<PlayerInput>(out playerInput))
        {
            isGhost = false;
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            interactAction = playerInput.actions["Interact"];
        }
        else
        {
            isGhost = true;
        }
    }

    void Update()
    {
        // All input and physics are handled in FixedUpdate for consistency.
    }

    void FixedUpdate()
    {
        if (!isGhost)
        {
            ReadPlayerInput();
        }

        HandleMovement();
        HandleInteraction();
    }

    private void ReadPlayerInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        // --- Reliable trigger detection in FixedUpdate ---
        bool isJumpPressed = jumpAction.IsPressed();
        if (isJumpPressed && !wasJumpPressedLastFrame)
        {
            jumpInputBuffer = true;
        }
        wasJumpPressedLastFrame = isJumpPressed;

        bool isInteractPressed = interactAction.IsPressed();
        if (isInteractPressed && !wasInteractPressedLastFrame)
        {
            interactInputBuffer = true;
        }
        wasInteractPressedLastFrame = isInteractPressed;
    }

    public void SetGhostInput(Vector2 move, bool jumpTriggered, bool interactTriggered)
    {
        moveInput = move;
        if (jumpTriggered)
        {
            jumpInputBuffer = true;
        }
        if (interactTriggered)
        {
            interactInputBuffer = true;
        }
    }

    private void HandleMovement()
    {
        // rb.linearVelocity を直接操作する
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // --- Sprite Flipping ---
        // 入力方向に応じてキャラクターの向きを反転させる
        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = true; // 右向き
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = false; // 左向き
        }

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        if (jumpInputBuffer && coyoteTimeCounter > 0f)
        {
            // ジャンプ時もrb.linearVelocityを直接操作
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            jumpInputBuffer = false; // Consume buffer only on successful jump
        }
    }

    private void HandleInteraction()
    {
        if (interactInputBuffer)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach (var collider in colliders)
            {
                LeverController lever = collider.GetComponent<LeverController>();
                if (lever != null)
                {
                    lever.PullLever();
                    break;
                }
            }
            interactInputBuffer = false; // Consume buffer after attempting interaction
        }
    }

    private bool IsGrounded()
    {
        float extraHeightText = 0.1f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, extraHeightText, groundLayer);
        return raycastHit.collider != null;
    }

    // GameManagerから呼ばれ、プレイヤーの操作を有効/無効にする
    public void SetControllable(bool isControllable)
    {
        // isGhostフラグは、キーボード入力を読むかどうかを判定する
        // enabledフラグは、コンポーネント全体の動作（Update, FixedUpdate）を制御する
        enabled = isControllable;

        // 操作不能になったときに、慣性をリセットする
        if (!isControllable)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
