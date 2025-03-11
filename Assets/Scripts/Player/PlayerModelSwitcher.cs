using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerModelSwitcher : NetworkBehaviour
{
    [SerializeField] private GameObject player1Model; // Reference to the Player 1 model (Wizard)
    [SerializeField] private GameObject player2Model; // Reference to the Player 2 model (Clown)
    
    [SerializeField] private Animator player1Animator; // Wizard animator
    [SerializeField] private Animator player2Animator; // Clown animator
    
    private void Start()
    {
        InitializeAnimators();
    }
    
    private void InitializeAnimators()
    {
        // Find Player 1 animator if not assigned
        if (player1Animator == null && player1Model != null)
        {
            player1Animator = player1Model.GetComponentInChildren<Animator>();
            if (player1Animator == null)
            {
                Debug.LogWarning("Player 1 animator not found in model hierarchy", this);
            }
            else if (player1Animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("Player 1 animator doesn't have a controller assigned", this);
            }
        }
        
        // Find Player 2 animator if not assigned
        if (player2Animator == null && player2Model != null)
        {
            player2Animator = player2Model.GetComponentInChildren<Animator>();
            if (player2Animator == null)
            {
                Debug.LogWarning("Player 2 animator not found in model hierarchy", this);
            }
            else if (player2Animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("Player 2 animator doesn't have a controller assigned", this);
            }
        }
    }

    // Network variable to track which model should be active
    private NetworkVariable<byte> activeModelId = new NetworkVariable<byte>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    // Property to check if we're using the clown model
    public bool IsClownModel => activeModelId.Value == 1;
    
    // Property to get the active animator with validation
    public Animator ActiveAnimator 
    { 
        get
        {
            // Check if the model is active before returning its animator
            if (IsClownModel)
            {
                if (player2Model != null && player2Model.activeSelf)
                {
                    // If the animator wasn't assigned directly, try to find it in the model
                    if (player2Animator == null)
                    {
                        player2Animator = player2Model.GetComponentInChildren<Animator>();
                        
                        // Log a warning if still null
                        if (player2Animator == null)
                        {
                            Debug.LogWarning("Player 2 animator not found in model hierarchy", this);
                        }
                        else if (player2Animator.runtimeAnimatorController == null)
                        {
                            Debug.LogWarning("Player 2 animator doesn't have a controller assigned", this);
                        }
                    }
                    return player2Animator;
                }
            }
            else
            {
                if (player1Model != null && player1Model.activeSelf)
                {
                    // If the animator wasn't assigned directly, try to find it in the model
                    if (player1Animator == null)
                    {
                        player1Animator = player1Model.GetComponentInChildren<Animator>();
                        
                        // Log a warning if still null
                        if (player1Animator == null)
                        {
                            Debug.LogWarning("Player 1 animator not found in model hierarchy", this);
                        }
                        else if (player1Animator.runtimeAnimatorController == null)
                        {
                            Debug.LogWarning("Player 1 animator doesn't have a controller assigned", this);
                        }
                    }
                    return player1Animator;
                }
            }
            
            // Fallback
            return null;
        }
    }

    public override void OnNetworkSpawn()
    {
        // Initialize animators
        InitializeAnimators();
        
        // Register the callback for when the model ID changes
        activeModelId.OnValueChanged += OnActiveModelChanged;
        
        // If we're the server, set the initial model
        if (IsServer)
        {
            // Player 1 (OwnerClientId = 0) gets model 0, Player 2 (OwnerClientId = 1) gets model 1
            byte modelId = (byte)(OwnerClientId == 0 ? 0 : 1);
            activeModelId.Value = modelId;
            Debug.Log($"Server setting Player {OwnerClientId} to model ID {modelId}");
        }
        
        // Apply the current model state immediately
        ApplyModelChange(activeModelId.Value);
        
        // Force update animator references after a short delay to ensure everything is set up
        StartCoroutine(DelayedAnimatorSetup());
    }
    
    private System.Collections.IEnumerator DelayedAnimatorSetup()
    {
        // Wait for a frame to ensure all components are properly initialized
        yield return null;
        
        // Re-initialize animators
        InitializeAnimators();
        
        // Force apply model change again
        ApplyModelChange(activeModelId.Value);
        
        // Log debug info about current setup
        LogAnimatorDebugInfo();
    }
    
    private void LogAnimatorDebugInfo()
    {
        Debug.Log($"--- Player {OwnerClientId} Animation Debug Info ---");
        Debug.Log($"Active Model ID: {activeModelId.Value} (IsClownModel: {IsClownModel})");
        
        if (player1Model != null)
        {
            Debug.Log($"Player 1 Model: {player1Model.name}, Active: {player1Model.activeSelf}");
            if (player1Animator != null)
            {
                Debug.Log($"Player 1 Animator Found: {player1Animator.name}, Has Controller: {player1Animator.runtimeAnimatorController != null}");
            }
            else
            {
                Debug.Log("Player 1 Animator: NULL");
            }
        }
        
        if (player2Model != null)
        {
            Debug.Log($"Player 2 Model: {player2Model.name}, Active: {player2Model.activeSelf}");
            if (player2Animator != null)
            {
                Debug.Log($"Player 2 Animator Found: {player2Animator.name}, Has Controller: {player2Animator.runtimeAnimatorController != null}");
            }
            else
            {
                Debug.Log("Player 2 Animator: NULL");
            }
        }
        
        Debug.Log($"Active Animator: {(ActiveAnimator != null ? ActiveAnimator.name : "NULL")}");
        Debug.Log("-------------------------------------");
    }

    // Callback when the active model value changes
    private void OnActiveModelChanged(byte previousValue, byte newValue)
    {
        ApplyModelChange(newValue);
    }

    // Apply the model change based on the model ID
    private void ApplyModelChange(byte modelId)
    {
        if (player1Model == null || player2Model == null)
        {
            Debug.LogError("Models not assigned in inspector!", this);
            return;
        }

        // Activate the correct model based on ID
        if (modelId == 0)
        {
            player1Model.SetActive(true);
            player2Model.SetActive(false);
            Debug.Log($"Player {OwnerClientId} is using Player 1 model (Wizard)");
            
            // Get the animator if it's not already assigned
            if (player1Animator == null)
            {
                player1Animator = player1Model.GetComponentInChildren<Animator>();
                if (player1Animator == null)
                {
                    Debug.LogError("Player 1 model doesn't have an Animator component!", this);
                }
                else if (player1Animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("Player 1 animator doesn't have an AnimatorController assigned!", this);
                }
            }
        }
        else
        {
            player1Model.SetActive(false);
            player2Model.SetActive(true);
            Debug.Log($"Player {OwnerClientId} is using Player 2 model (Clown)");
            
            // Get the animator if it's not already assigned
            if (player2Animator == null)
            {
                player2Animator = player2Model.GetComponentInChildren<Animator>();
                if (player2Animator == null)
                {
                    Debug.LogError("Player 2 model doesn't have an Animator component!", this);
                }
                else if (player2Animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("Player 2 animator doesn't have an AnimatorController assigned!", this);
                }
            }
        }
        
        // Notify other components that the model has changed
        var movementManager = GetComponent<MovementManager>();
        if (movementManager != null)
        {
            movementManager.UpdateAnimatorReference();
        }
    }

    // Method to update animation parameters based on the active model
    public void UpdateAnimationState(bool movingLeft, bool movingRight, PlayerNetwork.JumpState jumpState)
    {
        if (IsClownModel)
        {
            // Get or find the animator if it's not assigned
            if (player2Animator == null && player2Model != null && player2Model.activeSelf)
            {
                player2Animator = player2Model.GetComponentInChildren<Animator>();
                
                if (player2Animator == null)
                {
                    Debug.LogError("Player 2 model doesn't have an Animator component!", this);
                    return;
                }
                
                if (player2Animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("Player 2 animator doesn't have an AnimatorController assigned!", this);
                    return;
                }
            }
            
            // Handle Clown animations
            if (player2Animator != null && player2Animator.runtimeAnimatorController != null)
            {
                try
                {
                    // Convert standard movement states to Clown-specific states
                    player2Animator.SetBool("clownMovingLeft", movingLeft);
                    player2Animator.SetBool("clownMovingRight", movingRight);
                    
                    // Handle jump states for Clown
                    bool isJumping = jumpState != PlayerNetwork.JumpState.None;
                    bool isJumpingLeft = isJumping && movingLeft;
                    bool isJumpingRight = isJumping && movingRight;
                    
                    player2Animator.SetBool("clownJumping", isJumping);
                    player2Animator.SetBool("clownMovingLeftJump", isJumpingLeft);
                    player2Animator.SetBool("clownMovingRightJump", isJumpingRight);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error setting Clown animation parameters: {e.Message}", this);
                }
            }
        }
        else
        {
            // Get or find the animator if it's not assigned
            if (player1Animator == null && player1Model != null && player1Model.activeSelf)
            {
                player1Animator = player1Model.GetComponentInChildren<Animator>();
                
                if (player1Animator == null)
                {
                    Debug.LogError("Player 1 model doesn't have an Animator component!", this);
                    return;
                }
                
                if (player1Animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("Player 1 animator doesn't have an AnimatorController assigned!", this);
                    return;
                }
            }
            
            // Handle Wizard animations
            if (player1Animator != null && player1Animator.runtimeAnimatorController != null)
            {
                try
                {
                    player1Animator.SetBool("movingLeft", movingLeft);
                    player1Animator.SetBool("movingRight", movingRight);
                    player1Animator.SetBool("jumpUp", jumpState == PlayerNetwork.JumpState.JumpUp);
                    player1Animator.SetBool("jumpDown", jumpState == PlayerNetwork.JumpState.JumpDown);
                    player1Animator.SetBool("leftStrafeJump", jumpState == PlayerNetwork.JumpState.LeftStrafeJump);
                    player1Animator.SetBool("rightStrafeJump", jumpState == PlayerNetwork.JumpState.RightStrafeJump);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error setting Wizard animation parameters: {e.Message}", this);
                }
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        // Clean up when the network object is despawned
        activeModelId.OnValueChanged -= OnActiveModelChanged;
    }
}