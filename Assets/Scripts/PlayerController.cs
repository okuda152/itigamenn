using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 15f;

    [Header("Wall Kick")]
    public float wallKickXForce    = 16f;
    public float wallKickYForce    = 9f;
    public float wallSlideSpeed    = 2f;
    public float wallSlideMaxTime  = 1.2f;
    public float wallKickCooldown  = 0.5f;  // この間は入力で速度を上書きしない

    Rigidbody2D rb;
    CapsuleCollider2D col;

    public float  MoveInput      { get; private set; }
    public bool   IsGrounded     { get; private set; }
    public bool   IsWallSliding  { get; private set; }
    public bool   FacingRight    { get; private set; } = true;

    bool  isTouchingWall;
    int   wallSide;
    int   kickedFromSide;
    bool  jumpRequest;
    float wallSlideTimer;
    float wallKickCooldownTimer;
    bool  wasGrounded;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        rb.freezeRotation = true;
        rb.gravityScale   = 3f;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        if (kb.aKey.isPressed         || kb.leftArrowKey.isPressed)  x -= 1f;
        if (kb.dKey.isPressed         || kb.rightArrowKey.isPressed) x += 1f;
        MoveInput = x;
        if (x != 0f) FacingRight = x > 0f;

        if (kb.spaceKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            jumpRequest = true;
    }

    void FixedUpdate()
    {
        wallKickCooldownTimer -= Time.fixedDeltaTime;
        CheckGround();
        CheckWall();
        Move();
        WallSlide();
        Jump();

        // 着地した瞬間にほこり
        if (IsGrounded && !wasGrounded)
            EffectManager.LandDust((Vector2)col.bounds.center + Vector2.down * col.bounds.extents.y);
        wasGrounded = IsGrounded;
    }

    void CheckGround()
    {
        var b = col.bounds;
        var hits = Physics2D.OverlapBoxAll(
            new Vector2(b.center.x, b.min.y + 0.05f),
            new Vector2(b.size.x * 0.9f, 0.15f), 0f);
        IsGrounded = false;
        foreach (var h in hits)
            if (h != col) { IsGrounded = true; break; }
    }

    void CheckWall()
    {
        var b = col.bounds;
        const float dist = 0.15f;

        bool HitsWall(Vector2 dir)
        {
            foreach (var h in Physics2D.RaycastAll(b.center, dir, b.extents.x + dist))
                if (h.collider != col) return true;
            return false;
        }

        bool right = HitsWall(Vector2.right);
        bool left  = HitsWall(Vector2.left);
        // 壁キック直後はクールダウン中なので壁接触を無視
        isTouchingWall = (right || left) && !IsGrounded && wallKickCooldownTimer <= 0f;
        wallSide = right ? 1 : (left ? -1 : 0);
    }

    void Move()
    {
        // クールダウン中は入力で水平速度を上書きしない（キックの勢いをそのまま維持）
        if (wallKickCooldownTimer > 0f) return;

        float vx = MoveInput * moveSpeed;
        if (IsWallSliding && MoveInput * wallSide > 0f) vx = 0f;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    void WallSlide()
    {
        IsWallSliding = isTouchingWall && !IsGrounded && rb.linearVelocity.y < 0f;

        if (IsWallSliding)
        {
            wallSlideTimer += Time.fixedDeltaTime;
            // タイムアウト後は速度制限を解除して自然落下させる
            if (wallSlideTimer < wallSlideMaxTime)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                    Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            wallSlideTimer = 0f;
        }
    }

    void Jump()
    {
        if (!jumpRequest) return;
        jumpRequest = false;

        if (IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        else if (IsWallSliding)
        {
            float vx = -wallSide * wallKickXForce;
            rb.linearVelocity        = new Vector2(vx, wallKickYForce);
            FacingRight              = vx > 0f;
            kickedFromSide           = wallSide;
            wallKickCooldownTimer    = wallKickCooldown;
            wallSlideTimer           = 0f;
        }
    }
}
