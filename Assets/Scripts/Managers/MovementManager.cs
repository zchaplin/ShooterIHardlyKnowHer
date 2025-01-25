using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] private GameObject player1; // The player GameObject
    [SerializeField] private Camera playerCamera; // Player's camera
    [SerializeField] private float speed = 10f; // Movement speed
    [SerializeField] private float acceleration = 20f; // Movement acceleration
    [SerializeField] private float mouseSensitivity = 2f; // Sensitivity for mouse movement

    private CharacterController characterControllerP1;
    private Vector3 currentVelocityP1 = Vector3.zero;
    private float verticalLookRotation = 0f; // Tracks up/down camera rotation

    void Start()
    {
        // Ensure the player GameObject and CharacterController are assigned
        if (player1 == null)
        {
            Debug.LogError("Player1 is not assigned in the MovementManager script.");
            return;
        }

        characterControllerP1 = player1.GetComponent<CharacterController>();
        if (characterControllerP1 == null)
        {
            Debug.LogError("Player1 does not have a CharacterController component.");
            return;
        }

        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
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

        // Calculate movement direction (only along the X-axis, ignoring rotation)
        Vector3 moveDirection = Vector3.right * moveX;

        // Apply acceleration to the current velocity
        currentVelocityP1 = Vector3.Lerp(currentVelocityP1, moveDirection * speed, acceleration * Time.deltaTime);

        // Lock the player's Y and Z positions to enforce movement along the X-axis only
        Vector3 newPosition = player1.transform.position + currentVelocityP1 * Time.deltaTime;
        newPosition.y = player1.transform.position.y; // Lock Y position
        newPosition.z = player1.transform.position.z; // Lock Z position

        // Move the player using the CharacterController
        characterControllerP1.Move(newPosition - player1.transform.position);
    }

    void RotatePlayerWithMouse()
    {
        // Get mouse movement input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player around the Y-axis (left/right movement)
        player1.transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up/down, clamping to prevent over-rotation
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // Restricts the camera to 90 degrees up/down
        playerCamera.transform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
    }
}