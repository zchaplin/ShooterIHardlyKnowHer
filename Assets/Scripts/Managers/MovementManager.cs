using Unity.Netcode;
using UnityEngine;

public class MovementManager : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera; // Player's camera
    [SerializeField] private Animator playerAnimator; // Player's animator
    [SerializeField] private float speed = 10f; // Movement speed
    [SerializeField] private float acceleration = 20f; // Movement acceleration
    [SerializeField] private float mouseSensitivity = 2f; // Sensitivity for mouse movement
    [SerializeField] private float jumpForce = 5f; // Force applied when jumping

    private Rigidbody rb; // Rigidbody component for physics-based movement
    private Collider playerCollider; // Player's collider
    private Vector3 currentVelocityP1 = Vector3.zero;
    private float verticalLookRotation = 0f; // Tracks up/down camera rotation
    private float horizontalLookRotation = 0f; // Tracks left/right camera rotation
    private bool isGrounded = false; // Check if the player is grounded
    private PlayerNetwork.JumpState currentJumpState = PlayerNetwork.JumpState.None; // Track current jump state
    private float verticalVelocity = 0f; // Track vertical velocity for jump state transitions

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>(); // Get the player's collider

        if (rb == null)
        {
            Debug.LogError("Player1 does not have a Rigidbody component.");
            return;
        }

        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ignore collisions with objects tagged as "Enemy"
    }


    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if(!IsOwner) return;
        // Handle sideways movement (A/D)
        bool movingLeft = Input.GetKey(KeyCode.A);
        bool movingRight = Input.GetKey(KeyCode.D);

        verticalVelocity = rb.velocity.y;

        UpdateJumpState(movingLeft, movingRight);

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("movingLeft", movingLeft);
            playerAnimator.SetBool("movingRight", movingRight);
            playerAnimator.SetBool("jumpUp", currentJumpState == PlayerNetwork.JumpState.JumpUp);
            playerAnimator.SetBool("jumpDown", currentJumpState == PlayerNetwork.JumpState.JumpDown);
            playerAnimator.SetBool("leftStrafeJump", currentJumpState == PlayerNetwork.JumpState.LeftStrafeJump);
            playerAnimator.SetBool("rightStrafeJump", currentJumpState == PlayerNetwork.JumpState.RightStrafeJump);
        }

        PlayerNetwork playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork != null)
        {
            playerNetwork.UpdateAnimationState(movingLeft, movingRight, currentJumpState);
        }

        HandleSidewaysMovement();
        // Handle mouse movement for camera and ensure it won't happen while in shop
        if (Cursor.lockState != CursorLockMode.Confined)
        {
            RotatePlayerWithMouse();
        }

        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // Handle sideways movement (A/D)
        HandleSidewaysMovement();
    }

    void HandleSidewaysMovement()
    {
        // Get input for sideways movement (A/D keys)
        float moveX = Input.GetAxis("Horizontal");

        // Reverse movement for Player 2 
        if (!IsServer)
        {
            moveX *= -1;
        }

        // Calculate movement direction (only along the X-axis, ignoring rotation)
        Vector3 moveDirection = Vector3.right * moveX;

        // Apply acceleration to the current velocity
        currentVelocityP1 = Vector3.Lerp(currentVelocityP1, moveDirection * speed, acceleration * Time.fixedDeltaTime);

        // Lock the player's Y and Z positions to enforce movement along the X-axis only
        Vector3 newVelocity = currentVelocityP1;
        newVelocity.y = rb.velocity.y; // Preserve existing Y velocity (e.g., gravity)
        newVelocity.z = 0f; // Lock Z velocity

        // Apply the velocity to the Rigidbody
        rb.velocity = newVelocity;
    }

    void RotatePlayerWithMouse()
    {
        // Get mouse movement input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Adjust horizontal rotation based on input
        horizontalLookRotation += mouseX;

        // Apply different rotation clamping based on player ID
        if (OwnerClientId == 0) // Player 1
        {
            horizontalLookRotation = Mathf.Clamp(horizontalLookRotation, -90f, 90f);
        }
        else if (OwnerClientId == 1) // Player 2
        {
            // Player 2 should rotate between 90° and 270° to match Player 1’s perspective
            if (horizontalLookRotation < 90f) horizontalLookRotation = 90f;
            if (horizontalLookRotation > 270f) horizontalLookRotation = 270f;
        }

        // Apply rotation
        rb.rotation = Quaternion.Euler(0f, horizontalLookRotation, 0f);

        // Rotate the camera up/down, clamping to prevent over-rotation
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // Restricts the camera to 90 degrees up/down
        playerCamera.transform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
    }

    void Jump()
    {
        // Apply jump force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;

        // Set initial jump state based on horizontal movement
        bool movingLeft = Input.GetKey(KeyCode.A);
        bool movingRight = Input.GetKey(KeyCode.D);
        
        if (movingLeft)
        {
            currentJumpState = PlayerNetwork.JumpState.LeftStrafeJump;
        }
        else if (movingRight)
        {
            currentJumpState = PlayerNetwork.JumpState.RightStrafeJump;
        }
        else
        {
            // Always start with JumpUp for vertical jumps
            // JumpDown will happen automatically when velocity becomes negative
            currentJumpState = PlayerNetwork.JumpState.JumpUp;
        }
        
        // Update animator directly
        UpdateJumpAnimator();       
    }

    void UpdateJumpState(bool movingLeft, bool movingRight)
    {
        if (!isGrounded)
        {
            // If we're in the air
            if (verticalVelocity < -0.5f) // Falling threshold - transition from rising to falling
            {
                // We're falling, update to the appropriate state
                if (movingLeft)
                {
                    currentJumpState = PlayerNetwork.JumpState.LeftStrafeJump;
                }
                else if (movingRight)
                {
                    currentJumpState = PlayerNetwork.JumpState.RightStrafeJump;
                }
                else if (currentJumpState == PlayerNetwork.JumpState.JumpUp)
                {
                    // If we were in JumpUp, transition to JumpDown
                    currentJumpState = PlayerNetwork.JumpState.JumpDown;
                }
                else
                {
                    currentJumpState = PlayerNetwork.JumpState.JumpDown;
                }
            }
            else if (verticalVelocity > 0.1f) // Rising threshold
            {
                // We're rising, update to the appropriate state
                if (movingLeft)
                {
                    currentJumpState = PlayerNetwork.JumpState.LeftStrafeJump;
                }
                else if (movingRight)
                {
                    currentJumpState = PlayerNetwork.JumpState.RightStrafeJump;
                }
                else
                {
                    currentJumpState = PlayerNetwork.JumpState.JumpUp;
                }
            }
        }
        else
        {
            // We're grounded, reset jump state
            currentJumpState = PlayerNetwork.JumpState.None;
        }
    }
    
    void UpdateJumpAnimator()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("jumpUp", currentJumpState == PlayerNetwork.JumpState.JumpUp);
            playerAnimator.SetBool("jumpDown", currentJumpState == PlayerNetwork.JumpState.JumpDown);
            playerAnimator.SetBool("leftStrafeJump", currentJumpState == PlayerNetwork.JumpState.LeftStrafeJump);
            playerAnimator.SetBool("rightStrafeJump", currentJumpState == PlayerNetwork.JumpState.RightStrafeJump);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the player is grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            
            // Only update jump state if we were in any jump state
            if (currentJumpState != PlayerNetwork.JumpState.None)
            {
                currentJumpState = PlayerNetwork.JumpState.None;
                
                // Update animator directly
                UpdateJumpAnimator();
                
                // Update network state
                PlayerNetwork playerNetwork = GetComponent<PlayerNetwork>();
                if (playerNetwork != null && IsOwner)
                {
                    playerNetwork.UpdateAnimationState(
                        playerAnimator.GetBool("movingLeft"), 
                        playerAnimator.GetBool("movingRight"), 
                        PlayerNetwork.JumpState.None
                    );
                }
            }
        }
    }
}