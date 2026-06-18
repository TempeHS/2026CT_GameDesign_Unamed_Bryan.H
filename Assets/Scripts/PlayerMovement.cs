using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    public float normalSpeed = 5f;
    public float carryingSpeed = 2.5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private bool isCarrying = false;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentSpeed = normalSpeed;
    }

    void Update()
    {
        rb.linearVelocity = moveInput * currentSpeed;
        animator.SetBool("isCarrying", isCarrying);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat("inputx", moveInput.x);
        animator.SetFloat("inputy", moveInput.y);

        if (context.performed)
        {
            animator.SetBool("isWalking", true);
        }

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("lastinputx", moveInput.x);
            animator.SetFloat("lastinputy", moveInput.y);
        }
    }

    public void PickUpPackage()
    {
        isCarrying = true;
        currentSpeed = carryingSpeed;
    }
}
