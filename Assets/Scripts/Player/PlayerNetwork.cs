using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // NetworkVariables for animation states
    // Original player animations
    public NetworkVariable<bool> netMovingLeft = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public NetworkVariable<bool> netMovingRight = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Player 1 Jump animation states
    public NetworkVariable<bool> netJumpUp = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netJumpDown = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netLeftStrafeJump = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netRightStrafeJump = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Clown animation states
    public NetworkVariable<bool> netClownMovingLeft = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netClownMovingRight = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netClownJumping = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netClownMovingLeftJump = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netClownMovingRightJump = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody rb;
    private Animator playerAnimator;
    private bool isClownCharacter = false; // Flag to identify the clown model
    private bool isPlayer2 = false; // Flag to identify player 2

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Disable physics simulation initially to prevent issues
            rb.isKinematic = true;
        }
        
        playerAnimator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        // Determine if this is Player 2 (Clown)
        isPlayer2 = OwnerClientId != NetworkManager.ServerClientId;
        isClownCharacter = isPlayer2;  // Player 2 is the clown

        // Owner enables physics when spawned
        if (IsOwner && rb != null)
        {
            rb.isKinematic = false;
        }

        // Register callbacks for animation NetworkVariables
        RegisterCallbacks();

        base.OnNetworkSpawn();
    }

    private void RegisterCallbacks()
    {
        // Register appropriate callbacks based on character type
        if (isClownCharacter)
        {
            // Player 2 (Clown) callbacks
            netClownMovingLeft.OnValueChanged += OnClownMovingLeftChanged;
            netClownMovingRight.OnValueChanged += OnClownMovingRightChanged;
            netClownJumping.OnValueChanged += OnClownJumpingChanged;
            netClownMovingLeftJump.OnValueChanged += OnClownMovingLeftJumpChanged;
            netClownMovingRightJump.OnValueChanged += OnClownMovingRightJumpChanged;
        }
        else
        {
            // Player 1 (Original) callbacks
            netMovingLeft.OnValueChanged += OnMovingLeftChanged;
            netMovingRight.OnValueChanged += OnMovingRightChanged;
            netJumpUp.OnValueChanged += OnJumpUpChanged;
            netJumpDown.OnValueChanged += OnJumpDownChanged;
            netLeftStrafeJump.OnValueChanged += OnLeftStrafeJumpChanged;
            netRightStrafeJump.OnValueChanged += OnRightStrafeJumpChanged;
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
        // Parameter not found, idk why, do nothing, or fix your bools in animator
    }

    // Original player animation callbacks
    private void OnMovingLeftChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && !isClownCharacter)
        {
            SafeSetBool("movingLeft", newValue);
        }
    }
    
    private void OnMovingRightChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && !isClownCharacter)
        {
            SafeSetBool("movingRight", newValue);
        }
    }
    
    private void OnJumpUpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && !isClownCharacter)
        {
            SafeSetBool("jumpUp", newValue);
        }
    }
    
    private void OnJumpDownChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && !isClownCharacter)
        {
            SafeSetBool("jumpDown", newValue);
        }
    }
    
    private void OnLeftStrafeJumpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && !isClownCharacter)
        {
            SafeSetBool("leftStrafeJump", newValue);
        }
    }
    
    private void OnRightStrafeJumpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && !isClownCharacter)
        {
            SafeSetBool("rightStrafeJump", newValue);
        }
    }
    
    // Clown animation callbacks
    private void OnClownMovingLeftChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && isClownCharacter)
        {
            SafeSetBool("clownMovingLeft", newValue);
        }
    }
    
    private void OnClownMovingRightChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && isClownCharacter)
        {
            SafeSetBool("clownMovingRight", newValue);
        }
    }
    
    private void OnClownJumpingChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && isClownCharacter)
        {
            SafeSetBool("clownJumping", newValue);
        }
    }
    
    private void OnClownMovingLeftJumpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && isClownCharacter)
        {
            SafeSetBool("clownMovingLeftJump", newValue);
        }
    }
    
    private void OnClownMovingRightJumpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null && isClownCharacter)
        {
            SafeSetBool("clownMovingRightJump", newValue);
        }
    }

    // Call this method from your movement script to update animation states
    public void UpdateAnimationState(bool movingLeft, bool movingRight, JumpState jumpState, bool isClown = false)
    {
        if (!IsOwner) return;

        // Update our character type flag
        isClownCharacter = isClown;
        
        if (isClownCharacter)
        {
            // Clown animations
            if (netClownMovingLeft.Value != movingLeft)
            {
                netClownMovingLeft.Value = movingLeft;
            }
            
            if (netClownMovingRight.Value != movingRight)
            {
                netClownMovingRight.Value = movingRight;
            }
            
            bool isJumping = jumpState != JumpState.None;
            bool isLeftJumping = isJumping && movingLeft;
            bool isRightJumping = isJumping && movingRight;
            bool isNormalJumping = isJumping && !movingLeft && !movingRight;
            
            if (netClownJumping.Value != isNormalJumping)
            {
                netClownJumping.Value = isNormalJumping;
            }
            
            if (netClownMovingLeftJump.Value != isLeftJumping)
            {
                netClownMovingLeftJump.Value = isLeftJumping;
            }
            
            if (netClownMovingRightJump.Value != isRightJumping)
            {
                netClownMovingRightJump.Value = isRightJumping;
            }
        }
        else
        {
            // Original animations
            if (netMovingLeft.Value != movingLeft)
            {
                netMovingLeft.Value = movingLeft;
            }
            
            if (netMovingRight.Value != movingRight)
            {
                netMovingRight.Value = movingRight;
            }
            
            // Update jump states
            bool isJumpUp = jumpState == JumpState.JumpUp;
            bool isJumpDown = jumpState == JumpState.JumpDown;
            bool isLeftStrafeJump = jumpState == JumpState.LeftStrafeJump;
            bool isRightStrafeJump = jumpState == JumpState.RightStrafeJump;
            
            if (netJumpUp.Value != isJumpUp)
            {
                netJumpUp.Value = isJumpUp;
            }
            
            if (netJumpDown.Value != isJumpDown)
            {
                netJumpDown.Value = isJumpDown;
            }
            
            if (netLeftStrafeJump.Value != isLeftStrafeJump)
            {
                netLeftStrafeJump.Value = isLeftStrafeJump;
            }
            
            if (netRightStrafeJump.Value != isRightStrafeJump)
            {
                netRightStrafeJump.Value = isRightStrafeJump;
            }
        }
    }
    
    // Enum to represent different jump states
    public enum JumpState
    {
        None,
        JumpUp,
        JumpDown,
        LeftStrafeJump,
        RightStrafeJump
    }
}