using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MovementManager : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera; // Player's camera
    [SerializeField] private float speed = 10f; // Movement speed
    [SerializeField] private float acceleration = 20f; // Movement acceleration
    [SerializeField] private float mouseSensitivity = 2f; // Sensitivity for mouse movement

    private CharacterController characterControllerP1;
    private Vector3 currentVelocityP1 = Vector3.zero;
    private float verticalLookRotation = 0f; // Tracks up/down camera rotation
    private float horizontalLookRotation = 0f; // Tracks left/right camera rotation
    private Vector3 startingForwardDirection; // Tracks the player's initial forward direction
    void Start()
    {
        characterControllerP1 = GetComponent<CharacterController>();
        if (characterControllerP1 == null)
        {
            Debug.LogError("Player1 does not have a CharacterController component.");
            return;
        }

        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsLocalPlayer){
            playerCamera.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if(!IsOwner) return;
        // Handle sideways movement (A/D)
        HandleSidewaysMovement();

        // Handle mouse movement for camera and ensure it won't happen while in shop
        if (Cursor.lockState != CursorLockMode.Confined) {
            RotatePlayerWithMouse();
        }
    }

    void HandleSidewaysMovement()
    {
        // Get input for sideways movement (A/D keys)
        float moveX = Input.GetAxis("Horizontal");

        // Reverse movement for Player 2 
        if (OwnerClientId == 1) 
        {
            moveX *= -1;
        }

        // Calculate movement direction (only along the X-axis, ignoring rotation)
        Vector3 moveDirection = Vector3.right * moveX;

        // Apply acceleration to the current velocity
        currentVelocityP1 = Vector3.Lerp(currentVelocityP1, moveDirection * speed, acceleration * Time.deltaTime);

        // Lock the player's Y and Z positions to enforce movement along the X-axis only
        Vector3 newPosition = transform.position + currentVelocityP1 * Time.deltaTime;
        newPosition.y = transform.position.y; // Lock Y position
        newPosition.z = transform.position.z; // Lock Z position

        // Move the player using the CharacterController
        characterControllerP1.Move(newPosition - transform.position);
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
    transform.rotation = Quaternion.Euler(0f, horizontalLookRotation, 0f);

        // Rotate the camera up/down, clamping to prevent over-rotation
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // Restricts the camera to 90 degrees up/down
        playerCamera.transform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
    }
}