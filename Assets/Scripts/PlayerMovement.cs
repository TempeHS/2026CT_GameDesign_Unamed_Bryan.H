using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float acceleration = 40f;
    public float deceleration = 50f;
    public float airControlMultiplier = 0.6f;

    [Header("Jump")]
    public float jumpForce = 18f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Jump Limits")]
    public int maxAirJumps = 3;
    private int airJumpsUsed = 0;

    [Header("Dash")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.3f;
    public int maxAirDashes = 1;

    [Header("Slide (Ground)")]
    public float slideSpeed = 16f;
    public float slideDuration = 0.4f;
    public float slideFriction = 0.2f;

    [Header("Wall Slide")]
    public bool enableWallSlide = true;
    public float wallSlideSpeed = 2f;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask wallLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool facingRight = true;

    private bool isGrounded;
    private bool isSliding;
    private bool isDashing;
    private bool isWallSliding;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float lastDashTime;
    private int airDashesUsed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        bool jumpPressed = Input.GetButtonDown("Jump");
        bool jumpHeld = Input.GetButton("Jump");
        bool dashPressed = Input.GetButtonDown("Fire3");
        bool slidePressed = Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.S);

        // Ground check moved to Update but stable because wall slide no longer overrides it
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            airDashesUsed = 0;
            airJumpsUsed = 0;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (jumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter = Mathf.Max(jumpBufferCounter - Time.deltaTime, 0f);

        bool canGroundJump = coyoteCounter > 0f && !isWallSliding;
        bool canAirJump = !isGrounded && airJumpsUsed < maxAirJumps;

        if (jumpBufferCounter > 0f && (canGroundJump || canAirJump) && !isDashing)
        {
            Jump();
            if (!canGroundJump) airJumpsUsed++;
            jumpBufferCounter = 0f;
        }

        if (!jumpHeld && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (dashPressed && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            bool canDash = isGrounded || airDashesUsed < maxAirDashes;
            if (canDash)
            {
                if (!isGrounded) airDashesUsed++;
                StartCoroutine(DashCoroutine());
            }
        }

        if (slidePressed && isGrounded && Mathf.Abs(moveInput) > 0.1f && !isSliding && !isDashing)
        {
            StartCoroutine(SlideCoroutine());
        }

        // FIXED WALL SLIDE — no longer overrides grounded state
        if (enableWallSlide && !isGrounded && !isDashing)
        {
            bool touchingWall = Physics2D.Raycast(
                wallCheck.position,
                facingRight ? Vector2.right : Vector2.left,
                wallCheckDistance,
                wallLayer
            );

            isWallSliding = touchingWall && rb.linearVelocity.y < 0f && Mathf.Abs(moveInput) > 0.1f;

            if (isWallSliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
            }
        }
        else
        {
            isWallSliding = false;
        }

        if (!isDashing && !isSliding)
        {
            if (moveInput > 0 && !facingRight) Flip();
            else if (moveInput < 0 && facingRight) Flip();
        }
    }

    void FixedUpdate()
    {
        if (isDashing || isSliding || isWallSliding) return;

        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        if (!isGrounded) accelRate *= airControlMultiplier;

        float movement = accelRate * speedDiff;
        rb.AddForce(new Vector2(movement, 0f));
    }

    void Jump()
    {
        coyoteCounter = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        Vector2 dashDir = facingRight ? Vector2.right : Vector2.left;
        float t = 0f;

        while (t < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            t += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    IEnumerator SlideCoroutine()
    {
        isSliding = true;

        float slideDir = facingRight ? 1f : -1f;
        float timer = 0f;

        while (timer < slideDuration && isGrounded)
        {
            rb.linearVelocity = new Vector2(slideDir * slideSpeed, rb.linearVelocity.y);
            rb.linearVelocity = new Vector2(
                Mathf.Lerp(rb.linearVelocity.x, 0f, slideFriction * Time.deltaTime),
                rb.linearVelocity.y
            );
            timer += Time.deltaTime;
            yield return null;
        }

        isSliding = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(wallCheck.position,
                wallCheck.position + (facingRight ? Vector3.right : Vector3.left) * wallCheckDistance);
        }
    }
}
