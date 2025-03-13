using Unity.Netcode;
using UnityEngine;

public class ClownAnimatorController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody playerRigidbody;
    private bool isGrounded = true;

    private enum ClownState
    {
        Idle = 0,
        MoveLeft = 2,
        MoveRight = 5
    }

    private ClownState lastAnimationState = ClownState.Idle;
    private bool wasGrounded = true;
    private bool wasKeyA = false;
    private bool wasKeyD = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponentInParent<Rigidbody>();

        if (animator == null)
        {
            Debug.LogError("Animator component not found on the clown model!", this);
        }

        if (playerRigidbody == null)
        {
            Debug.LogError("Rigidbody not found in parent objects!", this);
        }

        // Set initial state
        animator.SetInteger("clownState", (int)ClownState.Idle);
        animator.SetBool("clownJumping", false);
        lastAnimationState = ClownState.Idle;
    }

    void Update()
    {
        if (animator == null) return;

        // Get movement inputs
        bool keyA = Input.GetKey(KeyCode.A);
        bool keyD = Input.GetKey(KeyCode.D);
        bool spacePressed = Input.GetKeyDown(KeyCode.Space);

        // Check grounded state
        if (transform.parent != null)
        {
            MovementManager movementManager = transform.parent.GetComponent<MovementManager>();
            if (movementManager != null)
            {
                isGrounded = movementManager.IsGrounded;
            }
            else
            {
                isGrounded = playerRigidbody != null && Mathf.Abs(playerRigidbody.velocity.y) < 0.1f;
            }
        }

        bool isJumping = !isGrounded;
        
        // Set clownJumping bool when space is pressed
        if (spacePressed && isGrounded)
        {
            animator.SetBool("clownJumping", true);
        }
        
        // Handle Jump Animation (Trigger)
        if (isJumping && wasGrounded) // Jump just started
        {
            animator.SetTrigger("clownJumpTrigger");
            animator.ResetTrigger("clownJumpTrigger"); // Reset the trigger
        }
        
        // Reset clownJumping bool when landing
        if (isGrounded && !wasGrounded)
        {
            animator.SetBool("clownJumping", false);
        }

        // Determine new movement animation state
        ClownState animationState = ClownState.Idle;

        if (isGrounded)
        {
            if (keyA && !keyD)
            {
                animationState = ClownState.MoveLeft;
            }
            else if (keyD && !keyA)
            {
                animationState = ClownState.MoveRight;
            }
        }

        // Only set the animation state if it has changed
        if (lastAnimationState != animationState)
        {
            // Store new state
            lastAnimationState = animationState;

            // Apply new animation state
            animator.SetInteger("clownState", (int)animationState);

            #if UNITY_EDITOR
            Debug.Log("Animation changed to: " + animationState);
            #endif
        }

        // Store current state for next frame
        wasGrounded = isGrounded;
        wasKeyA = keyA;
        wasKeyD = keyD;
    }
}