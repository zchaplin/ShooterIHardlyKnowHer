using Unity.Netcode;
using UnityEngine;

public class MovementManager : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera; // Player's camera
    [SerializeField] private float speed = 10f; // Movement speed
    [SerializeField] private float acceleration = 20f; // Movement acceleration
    [SerializeField] private float mouseSensitivity = 2f; // Sensitivity for mouse movement
    [SerializeField] private float jumpForce = 5f; // Force applied when jumping

    // Animation components
    [SerializeField] private Animator wizardAnimator; // Reference to wizard's animator

    // Network variables to sync animation states
    private NetworkVariable<bool> networkMovingRight = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    private NetworkVariable<bool> networkMovingLeft = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    private NetworkVariable<bool> networkJumpUp = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    private NetworkVariable<bool> networkJumpDown = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private Rigidbody rb; // Rigidbody component for physics-based movement
    private Collider playerCollider; // Player's collider
    private Vector3 currentVelocityP1 = Vector3.zero;
    private float verticalLookRotation = 0f; // Tracks up/down camera rotation
    private float horizontalLookRotation = 0f; // Tracks left/right camera rotation
    private bool isGrounded = false; // Check if the player is grounded
    private float verticalVelocity = 0f; // Track vertical velocity for jump state
    
    private PlayerModelSwitcher modelSwitcher; // Reference to the model switcher
    private bool wasJumping = false; // Track if we were jumping last frame
    private bool isMovingRight = false; // Track if we're moving right
    private bool isMovingLeft = false; // Track if we're moving left

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>(); // Get the player's collider
        modelSwitcher = GetComponent<PlayerModelSwitcher>();

        if (rb == null)
        {
            Debug.LogError("Player does not have a Rigidbody component.");
            return;
        }

        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Register network variable callbacks
        networkMovingRight.OnValueChanged += OnMovingRightChanged;
        networkMovingLeft.OnValueChanged += OnMovingLeftChanged;
        networkJumpUp.OnValueChanged += OnJumpUpChanged;
        networkJumpDown.OnValueChanged += OnJumpDownChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
        }
        
        // Apply initial states if we're not the owner
        if (!IsOwner)
        {
            OnMovingRightChanged(false, networkMovingRight.Value);
            OnMovingLeftChanged(false, networkMovingLeft.Value);
            OnJumpUpChanged(false, networkJumpUp.Value);
            OnJumpDownChanged(false, networkJumpDown.Value);
        }
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // Unregister callbacks
        networkMovingRight.OnValueChanged -= OnMovingRightChanged;
        networkMovingLeft.OnValueChanged -= OnMovingLeftChanged;
        networkJumpUp.OnValueChanged -= OnJumpUpChanged;
        networkJumpDown.OnValueChanged -= OnJumpDownChanged;
    }
    
    // Network callbacks
    private void OnMovingRightChanged(bool previousValue, bool newValue)
    {
        if (!IsOwner && !modelSwitcher.IsClownModel)
        {
            wizardAnimator.SetBool("movingRight", newValue);
        }
    }
    
    private void OnMovingLeftChanged(bool previousValue, bool newValue)
    {
        if (!IsOwner && !modelSwitcher.IsClownModel)
        {
            wizardAnimator.SetBool("movingLeft", newValue);
        }
    }
    
    private void OnJumpUpChanged(bool previousValue, bool newValue)
    {
        if (!IsOwner && !modelSwitcher.IsClownModel)
        {
            wizardAnimator.SetBool("jumpUp", newValue);
        }
    }
    
    private void OnJumpDownChanged(bool previousValue, bool newValue)
    {
        if (!IsOwner && !modelSwitcher.IsClownModel)
        {
            wizardAnimator.SetBool("jumpDown", newValue);
        }
    }

    void Update()
    {
        if(!IsOwner) return;
        
        verticalVelocity = rb.velocity.y;

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

        // Update animations based on current movement state
        UpdateAnimations();
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
        
        // Simply track raw input for animations - this works for wizard
        isMovingRight = moveX > 0.1f;
        isMovingLeft = moveX < -0.1f;
        
        // For debug
        // if (isMovingRight) Debug.Log("Raw Input: Moving RIGHT");
        // if (isMovingLeft) Debug.Log("Raw Input: Moving LEFT");
        
        // Reverse movement for Player 2 physics
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
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the player is grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void UpdateAnimations()
    {
        bool isJumping = !isGrounded;
        bool isJumpingUp = isJumping && verticalVelocity > 0;
        bool isJumpingDown = isJumping && verticalVelocity <= 0;
        
        // Determine if we just started jumping
        bool jumpStarted = !wasJumping && isJumping;
        wasJumping = isJumping;

        // Skip animation updates for clown model - now handled by ClownAnimatorController
        if (modelSwitcher.IsClownModel) return;

        // Reset all wizard animation flags
        wizardAnimator.SetBool("movingRight", false);
        wizardAnimator.SetBool("movingLeft", false);
        wizardAnimator.SetBool("jumpUp", false);
        wizardAnimator.SetBool("jumpDown", false);
        
        // Reset network variables
        networkMovingRight.Value = false;
        networkMovingLeft.Value = false;
        networkJumpUp.Value = false;
        networkJumpDown.Value = false;

        // Handle Wizard animations with simplified parameters
        if (isJumping)
        {
            // Set the appropriate jump animation based on vertical velocity
            if (isJumpingUp)
            {
                wizardAnimator.SetBool("jumpUp", true);
                networkJumpUp.Value = true;
            }
            else // jumping down
            {
                wizardAnimator.SetBool("jumpDown", true);
                networkJumpDown.Value = true;
            }
            
            // Also set movement flags if moving horizontally while jumping
            if (isMovingRight)
            {
                wizardAnimator.SetBool("movingRight", true);
                networkMovingRight.Value = true;
                // Debug.Log("Wizard jumping while moving right");
            }
            else if (isMovingLeft)
            {
                wizardAnimator.SetBool("movingLeft", true);
                networkMovingLeft.Value = true;
                // Debug.Log("Wizard jumping while moving left");
            }
        }
        else if (isMovingRight)
        {
            wizardAnimator.SetBool("movingRight", true);
            networkMovingRight.Value = true;
            // Debug.Log("Wizard moving right");
        }
        else if (isMovingLeft)
        {
            wizardAnimator.SetBool("movingLeft", true);
            networkMovingLeft.Value = true;
            // Debug.Log("Wizard moving left");
        }
        // Idle is default when no animation is set
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }
}