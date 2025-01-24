using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] public GameObject Player1; // The player GameObject
    [SerializeField] public Camera playerCamera; // Player's camera
    [SerializeField] float speed = 10f;
    [SerializeField] float acceleration = 2f;
    [SerializeField] float mouseSensitivity = 2f; // Sensitivity for mouse movement
    private CharacterController characterControllerP1;
    private Vector3 currentVelocityP1 = Vector3.zero;
    private float verticalLookRotation = 0f; // Tracks up/down camera rotation

    void Start()
    {
        characterControllerP1 = Player1.GetComponent<CharacterController>();

        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Handle player movement (WASD)
        Vector3 inputDirectionP1 = (Player1.transform.right * Input.GetAxis("Vertical")); //+ 
                                   //(Player1.transform.forward * Input.GetAxis("Vertical"));
        currentVelocityP1 = Vector3.MoveTowards(currentVelocityP1, inputDirectionP1, acceleration * Time.deltaTime);
        characterControllerP1.Move(currentVelocityP1 * speed * Time.deltaTime);

        // 2. Handle mouse movement for camera
        RotatePlayerWithMouse();
    }

    void RotatePlayerWithMouse()
    {
        // Get mouse movement input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player around the Y-axis (left/right movement)
        Player1.transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up/down, clamping to prevent over-rotation
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // Restricts the camera to 90 degrees up/down
        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }
}
