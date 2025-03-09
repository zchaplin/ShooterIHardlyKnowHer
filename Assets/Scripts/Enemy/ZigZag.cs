using System.Collections;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PushBackAndForth : NetworkBehaviour
{
    public float pushForce = 5f; // Force to apply for the push
    public float interval = 1f;   // Time interval between pushes

    [Header("Animation")]
    public Animator animator;
    public string sneakWalkParam = "sneakWalk";
    public string turnLeftParam = "turnLeft";
    public string turnRightParam = "turnRight";
    public float turnAnimationDuration = 0.5f; // Duration to play turn animation

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
            StartCoroutine(PushWithAnimation());
        }
    }
    
    IEnumerator PushWithAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // Start turning animation            
            if (pushingRight)
            {
                // Going to push left, so turn left
                PlayTurnAnimation(false);
                
                // Apply force after a small delay
                yield return new WaitForSeconds(turnAnimationDuration * 0.5f);
                rb.AddForce(Vector3.left * pushForce, ForceMode.Impulse);
            }
            else
            {
                // Going to push right, so turn right
                PlayTurnAnimation(true);
                
                // Apply force after a small delay
                yield return new WaitForSeconds(turnAnimationDuration * 0.5f);
                rb.AddForce(Vector3.right * pushForce, ForceMode.Impulse);
            }

            // Wait for the rest of the turn animation
            yield return new WaitForSeconds(turnAnimationDuration * 0.5f);
            
            // Reset turn animations and continue sneaking
            if (animator != null)
            {
                animator.SetBool(turnLeftParam, false);
                animator.SetBool(turnRightParam, false);
            }
            
            // Toggle direction for next push
            pushingRight = !pushingRight;
        }
    }
    
    private void PlayTurnAnimation(bool turnRight)
    {
        if (animator == null) return;
        
        // Stop the other turn animation if it's playing
        animator.SetBool(turnRight ? turnLeftParam : turnRightParam, false);
        
        // Start the new turn animation
        animator.SetBool(turnRight ? turnRightParam : turnLeftParam, true);
        
        // Sync animation across network
        if (IsServer)
        {
            SyncAnimationClientRpc(turnRight);
        }
    }
    
    [ClientRpc]
    private void SyncAnimationClientRpc(bool turnRight)
    {
        if (IsServer) return; // Server already did this locally
        
        if (animator != null)
        {
            // Reset both turn animations
            animator.SetBool(turnLeftParam, false);
            animator.SetBool(turnRightParam, false);
            
            // Play the correct one
            animator.SetBool(turnRight ? turnRightParam : turnLeftParam, true);
        }
    }
}

