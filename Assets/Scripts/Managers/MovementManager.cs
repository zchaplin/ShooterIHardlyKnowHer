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

    // Add a flag to identify the character type
    [SerializeField] private bool isClownCharacter = false;

    private Rigidbody rb; // Rigidbody component for physics-based movement
    private Collider playerCollider; // Player's collider
    private Vector3 currentVelocityP1 = Vector3.zero;
    private float verticalLookRotation = 0f; // Tracks up/down camera rotation
    private float horizontalLookRotation = 0f; // Tracks left/right camera rotation
    private bool isGrounded = false; // Check if the player is grounded
    private PlayerNetwork.JumpState currentJumpState = PlayerNetwork.JumpState.None; // Track current jump state
    private float verticalVelocity = 0f; // Track vertical velocity for jump state transitions
    private bool isPlayer2 = false; // Flag to identify if this is Player 2

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        
        // Determine if this is Player 2 based on client ID
        isPlayer2 = OwnerClientId != NetworkManager.ServerClientId;
        isClownCharacter = isPlayer2; // Player 2 is the clown

        if (rb == null)
        {
            Debug.LogError("Player does not have a Rigidbody component.");
            return;
        }

        // Configure the Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Auto-detect if this is the clown character by checking animator parameters
        if (playerAnimator != null)
        {
            // Check if clown parameters exist
            foreach (AnimatorControllerParameter param in playerAnimator.parameters)
            {
                if (param.name == "clownMovingLeft" || param.name == "clownMovingRight" || param.name == "clownJumping")
                {
                    isClownCharacter = true;
                    Debug.Log("Detected clown character based on animator parameters");
                    break;
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
        }
        
        // Determine if this is Player 2 based on client ID
        isPlayer2 = OwnerClientId != NetworkManager.ServerClientId;
        isClownCharacter = isPlayer2; // Player 2 is the clown
    }

    void Update()
    {
        if(!IsOwner) return;
        
        bool movingLeft = Input.GetKey(KeyCode.A);
        bool movingRight = Input.GetKey(KeyCode.D);

        verticalVelocity = rb.velocity.y;
        
        UpdateJumpState(movingLeft, movingRight);
        
        // Update animator parameters based on character type
        if (playerAnimator != null)
        {
            if (isClownCharacter)
            {
                // Use clown parameter names
                SafeSetBool("clownMovingLeft", movingLeft);
                SafeSetBool("clownMovingRight", movingRight);
                
                // Handle jump animations for clown
                bool isJumping = currentJumpState != PlayerNetwork.JumpState.None;
                bool isLeftJumping = isJumping && movingLeft;
                bool isRightJumping = isJumping && movingRight;
                bool isNormalJumping = isJumping && !movingLeft && !movingRight;
                
                SafeSetBool("clownJumping", isNormalJumping);
                SafeSetBool("clownMovingLeftJump", isLeftJumping);
                SafeSetBool("clownMovingRightJump", isRightJumping);
            }
            else
            {
                // Use original parameter names
                SafeSetBool("movingLeft", movingLeft);
                SafeSetBool("movingRight", movingRight);
                SafeSetBool("jumpUp", currentJumpState == PlayerNetwork.JumpState.JumpUp);
                SafeSetBool("jumpDown", currentJumpState == PlayerNetwork.JumpState.JumpDown);
                SafeSetBool("leftStrafeJump", currentJumpState == PlayerNetwork.JumpState.LeftStrafeJump);
                SafeSetBool("rightStrafeJump", currentJumpState == PlayerNetwork.JumpState.RightStrafeJump);
            }
        }

        // Sync animation states over the network
        PlayerNetwork playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork != null)
        {
            playerNetwork.UpdateAnimationState(movingLeft, movingRight, currentJumpState, isClownCharacter);
        }
        
        HandleSidewaysMovement();
        
        if (Cursor.lockState != CursorLockMode.Confined)
        {
            RotatePlayerWithMouse();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }
    
    // Helper method to safely set bool parameters
    private void SafeSetBool(string paramName, bool value)
    {
        if (playerAnimator == null) return;
        
        // Check if the parameter exists by iterating through parameters
        foreach (AnimatorControllerParameter param in playerAnimator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                playerAnimator.SetBool(paramName, value);
                return;
            }
        }
        // Parameter not found - silently ignore
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
            // Player 2 should rotate between 90° and 270° to match Player 1's perspective
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
            if (isClownCharacter)
            {
                // For clown character, we just need to know if jumping or not
                // and whether moving left/right during jump
                if (movingLeft)
                {
                    currentJumpState = PlayerNetwork.JumpState.LeftStrafeJump;
                }
                else if (movingRight)
                {
                    currentJumpState = PlayerNetwork.JumpState.RightStrafeJump;
                }
                else if (currentJumpState == PlayerNetwork.JumpState.None)
                {
                    currentJumpState = PlayerNetwork.JumpState.JumpUp; // Any non-None state will work for clown
                }
                // Else keep current state
            }
            else
            {
                // For original character with multiple jump animations
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
            if (isClownCharacter)
            {
                // For clown character
                bool isJumping = currentJumpState != PlayerNetwork.JumpState.None;
                bool movingLeft = Input.GetKey(KeyCode.A);
                bool movingRight = Input.GetKey(KeyCode.D);
                bool isLeftJumping = isJumping && movingLeft;
                bool isRightJumping = isJumping && movingRight;
                bool isNormalJumping = isJumping && !movingLeft && !movingRight;
                
                SafeSetBool("clownJumping", isNormalJumping);
                SafeSetBool("clownMovingLeftJump", isLeftJumping);
                SafeSetBool("clownMovingRightJump", isRightJumping);
            }
            else
            {
                // For original character
                SafeSetBool("jumpUp", currentJumpState == PlayerNetwork.JumpState.JumpUp);
                SafeSetBool("jumpDown", currentJumpState == PlayerNetwork.JumpState.JumpDown);
                SafeSetBool("leftStrafeJump", currentJumpState == PlayerNetwork.JumpState.LeftStrafeJump);
                SafeSetBool("rightStrafeJump", currentJumpState == PlayerNetwork.JumpState.RightStrafeJump);
            }
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
                    bool movingLeft = Input.GetKey(KeyCode.A);
                    bool movingRight = Input.GetKey(KeyCode.D);
                    playerNetwork.UpdateAnimationState(
                        movingLeft, 
                        movingRight, 
                        PlayerNetwork.JumpState.None,
                        isClownCharacter
                    );
                }
            }
        }
    }
}