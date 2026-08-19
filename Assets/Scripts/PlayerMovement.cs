using System.Collections;
using UnityEngine;



[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float csgoMaxSpeed = 14f;
    public float csgoGroundAccel = 120f;
    public float csgoAirAccel = 80f;
    public float csgoFriction = 8f;

    [Header("Jump")]
    public float jumpForce = 18f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
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
    public float slideFriction = 0.25f;

    [Header("Wall Slide / Wall Jump")]
    public bool enableWallSlide = true;
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 18f;
    public float wallJumpHorizontalBoost = 10f;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask wallLayer;

    public bool isFrozen = false;

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

    private bool jumpPressed;
    private bool jumpHeld;
    private bool dashPressed;
    private bool slidePressed;

    private SpriteRenderer spriteRenderer;
    private Animator anim;

    void Awake()
    {
    rb = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }


    void Update()
    {
        if (isFrozen) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
        dashPressed = Input.GetButtonDown("Fire3");
        slidePressed = Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.S);

        if (jumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter = Mathf.Max(jumpBufferCounter - Time.deltaTime, 0f);

        UpdateAnimationParameters();
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool touchingWall = Physics2D.Raycast(
            wallCheck.position,
            facingRight ? Vector2.right : Vector2.left,
            wallCheckDistance,
            wallLayer
        );

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            airJumpsUsed = 0;
            airDashesUsed = 0;
        }
        else
        {
            coyoteCounter -= Time.fixedDeltaTime;
        }

        isWallSliding =
            enableWallSlide &&
            !isGrounded &&
            touchingWall &&
            rb.linearVelocity.y < 0f &&
            Mathf.Abs(moveInput) > 0.1f &&
            !isDashing;

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }

        if (isWallSliding && jumpPressed)
        {
            float dir = facingRight ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir * wallJumpHorizontalBoost, 0f);
            rb.AddForce(new Vector2(dir * wallJumpHorizontalBoost, wallJumpForce), ForceMode2D.Impulse);
            isWallSliding = false;
            return;
        }

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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier);
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

        if (!isDashing && !isSliding && !isWallSliding)
        {
            if (isGrounded)
            {
                ApplyGroundFriction();
                CSGOAccelerate(moveInput, csgoMaxSpeed, csgoGroundAccel);
            }
            else
            {
                CSGOAirAccelerate(moveInput, csgoMaxSpeed, csgoAirAccel);
            }

            if (moveInput > 0 && !facingRight) Flip();
            else if (moveInput < 0 && facingRight) Flip();
        }
    }

    void Jump()
    {
        coyoteCounter = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        anim.SetTrigger("Jump");
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        anim.SetTrigger("Dash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 dashDir = new Vector2(x, y);

        if (dashDir == Vector2.zero)
            dashDir = facingRight ? Vector2.right : Vector2.left;

        dashDir.Normalize();

        float t = 0f;
        while (t < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    IEnumerator SlideCoroutine()
    {
        isSliding = true;
        anim.SetBool("Sliding", true);

        float slideDir = facingRight ? 1f : -1f;
        float t = 0f;

        while (t < slideDuration && isGrounded)
        {
            float x = Mathf.Lerp(slideDir * slideSpeed, 0f, slideFriction * t);
            rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        anim.SetBool("Sliding", false);
        isSliding = false;
    }

    void CSGOAccelerate(float wishDir, float wishSpeed, float accel)
    {
        float currentSpeed = rb.linearVelocity.x * wishDir;
        float addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.fixedDeltaTime * wishSpeed;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        rb.linearVelocity += new Vector2(accelSpeed * wishDir, 0);
    }

    void CSGOAirAccelerate(float wishDir, float wishSpeed, float accel)
    {
        float currentSpeed = rb.linearVelocity.x * wishDir;
        float addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.fixedDeltaTime * wishSpeed;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        rb.linearVelocity += new Vector2(accelSpeed * wishDir, 0);
    }

    void ApplyGroundFriction()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);
        if (speed < 0.1f) return;

        float drop = speed * csgoFriction * Time.fixedDeltaTime;
        float newSpeed = Mathf.Max(speed - drop, 0);

        rb.linearVelocity = new Vector2(newSpeed * Mathf.Sign(rb.linearVelocity.x), rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }


    void UpdateAnimationParameters()
    {
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("WallSlide", isWallSliding);
        anim.SetBool("Dashing", isDashing);

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
    }
 
  
}
