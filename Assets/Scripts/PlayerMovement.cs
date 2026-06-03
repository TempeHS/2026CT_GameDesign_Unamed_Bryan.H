using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour

    {
        [SerializeField] private float moveSpeed = 5f; 
        private Rigidbody2D rb;     
        private Vector2 moveInput; 
        private Animator animator;

    
        
        void Start()    
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>(); 
        }

        
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
            
            ///// pls dont edit i think this is good for idles 
            

            moveInput = context.ReadValue<Vector2>(); 
            animator.SetFloat("inputx", moveInput.x);
            animator.SetFloat("inputy", moveInput.y);
        }
    }
