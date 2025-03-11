using System.Collections;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PushBackAndForth : NetworkBehaviour
{
    public float pushForce = 5f; // Force to apply for the push
    public float interval = 1f;   // Time interval between pushes
    public float rotationAmount = 15f; // Amount to rotate when changing direction

    [Header("Animation")]
    public Animator animator;
    public string sneakWalkParam = "sneakWalk";

    private Rigidbody rb;
    private bool pushingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Find animator if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Set sneak walk animation
        if (animator != null)
        {
            animator.SetBool(sneakWalkParam, true);
        }
              
        if (IsServer)
        {
            StartCoroutine(PushRoutine());
        }
    }
    
    IEnumerator PushRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (pushingRight)
            {              
                // Apply force
                rb.AddForce(Vector3.left * pushForce, ForceMode.Impulse);
            }
            else
            {            
                // Apply force
                rb.AddForce(Vector3.right * pushForce, ForceMode.Impulse);
            }
            
            // Toggle direction for next push
            pushingRight = !pushingRight;
        }
    }
}