using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; 
    private Rigidbody2D rb;     
    private Vector2 moveInput; 
    private Animator animator;


    [Header("Jumping")]
    public float jumpForce = 5f;
    public float jumpDuration = 0.4f;
    private bool isJumping = false;
    private float jumpTimer;

     [Header("Visuals (Fake 3D)")]
    public Transform shadowSprite; 
    private Vector3 originalShadowScale;

        void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (shadowSprite != null)
        {
            originalShadowScale = shadowSprite.localScale;
        }
    }

    // Handles Input System "Move" action
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Handles Input System "Jump" action
    public void OnJump(InputValue value)
    {
        if (value.isPressed && !isJumping)
        {
            {
            isJumping = true;
            jumpTimer = jumpDuration; 
            }
            
        
        }
    }

    void FixedUpdate()
    {
        
        rb.linearVelocity = moveInput * moveSpeed;

       
        if (isJumping)
        {
            if (jumpTimer > 0)
            {
                
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimer -= Time.fixedDeltaTime;
            }
            else
            {
                isJumping = false;
            }
        }
        
        UpdateShadow();
    }

    void UpdateShadow()
    {
        if (shadowSprite != null)
        {
        
        }
    }


    
    void Start()    
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)

    {
        animator.SetBool("isWalking", true);

        if(context.canceled)
        {
            animator.SetBool("isWalking", false); 
            animator.SetFloat("lastinputx", moveInput.x);
            animator.SetFloat("lastinputy", moveInput.y);
        
        }
        
        moveInput = context.ReadValue<Vector2>(); 
        animator.SetFloat("inputx", moveInput.x);
        animator.SetFloat("inputy", moveInput.y);
    }
}
