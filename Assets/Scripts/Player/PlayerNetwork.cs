using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Hardcoded spawn points
    private readonly Vector3[] spawnPoints = new Vector3[]
    {
        new Vector3(1, 1, -3), // Spawn point for player 1
        new Vector3(1, 1, 3)   // Spawn point for player 2
    };

    private Rigidbody rb;
    private PlayerModelSwitcher modelSwitcher;

    // NetworkVariables for animation states - shared between both models
    public NetworkVariable<bool> netMovingLeft = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public NetworkVariable<bool> netMovingRight = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // NetworkVariables for jumping animations
    public NetworkVariable<bool> netJumpUp = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netJumpDown = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netLeftStrafeJump = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netRightStrafeJump = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public NetworkVariable<bool> netJumping = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> netClownMovingLeft = new NetworkVariable<bool>(false, 
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> netClownMovingRight = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> netClownMovingLeftJump = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> netClownMovingRightJump = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Disable physics simulation initially to prevent issues
            rb.isKinematic = true;
        }
        
        modelSwitcher = GetComponent<PlayerModelSwitcher>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Assign spawn point based on player ID
            int playerId = (int)OwnerClientId;
            if (playerId < spawnPoints.Length)
            {
                transform.position = spawnPoints[playerId];

                if (playerId == 0) // Player 1
                {
                    transform.rotation = Quaternion.LookRotation(Vector3.forward); 
                }
                else if (playerId == 1) // Player 2
                {
                    transform.rotation = Quaternion.LookRotation(Vector3.back); 
                }
            }
        }

        // Owner enables physics when spawned
        if (IsOwner && rb != null)
        {
            rb.isKinematic = false;
        }

        // Register callbacks for animation NetworkVariables
        netMovingLeft.OnValueChanged += OnNetworkValueChanged;
        netMovingRight.OnValueChanged += OnNetworkValueChanged;
        netJumpUp.OnValueChanged += OnNetworkValueChanged;
        netJumpDown.OnValueChanged += OnNetworkValueChanged;
        netLeftStrafeJump.OnValueChanged += OnNetworkValueChanged;
        netRightStrafeJump.OnValueChanged += OnNetworkValueChanged;
        netJumping.OnValueChanged += OnNetworkValueChanged;

        // Register callbacks for clown-specific animation NetworkVariables
        netClownMovingLeft.OnValueChanged += OnNetworkValueChanged;
        netClownMovingRight.OnValueChanged += OnNetworkValueChanged;
        netClownMovingLeftJump.OnValueChanged += OnNetworkValueChanged;
        netClownMovingRightJump.OnValueChanged += OnNetworkValueChanged;

        base.OnNetworkSpawn();
    }

    // Single callback to handle all animation network variable changes
    private void OnNetworkValueChanged(bool previousValue, bool newValue)
    {
        UpdateLocalAnimationState();
    }
    
    // Update local animation state based on network variables
    private void UpdateLocalAnimationState()
    {
        // Try to get the model switcher if it's not already assigned
        if (modelSwitcher == null)
        {
            modelSwitcher = GetComponent<PlayerModelSwitcher>();
            if (modelSwitcher == null)
            {
                return;
            }
        }
        
        try
        {
            bool movingLeft = netMovingLeft.Value;
            bool movingRight = netMovingRight.Value;
            
            // Determine the jump state
            JumpState jumpState = JumpState.None;
            
            if (netJumping.Value)
            {
                jumpState = JumpState.Generic; // Used by Clown
            }
            else if (netLeftStrafeJump.Value)
            {
                jumpState = JumpState.LeftStrafeJump;
            }
            else if (netRightStrafeJump.Value)
            {
                jumpState = JumpState.RightStrafeJump;
            }
            else if (netJumpUp.Value)
            {
                jumpState = JumpState.JumpUp;
            }
            else if (netJumpDown.Value)
            {
                jumpState = JumpState.JumpDown;
            }
            
            // Update the model switcher with the current animation state
            modelSwitcher.UpdateAnimationState(movingLeft, movingRight, jumpState);

            // Update clown-specific animation states
            if (modelSwitcher.IsClownModel)
            {
                modelSwitcher.UpdateAnimationState(netClownMovingLeft.Value, netClownMovingRight.Value, jumpState);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating animation state: {e.Message}", this);
        }
    }

    public void UpdateAnimationState(bool movingLeft, bool movingRight, JumpState jumpState)
    {
        if (!IsOwner) return;
        
        try
        {
            // Log animation state changes for debugging
            bool isClownModel = modelSwitcher != null && modelSwitcher.IsClownModel;
            string modelType = isClownModel ? "Clown" : "Wizard";
            
            // Only send network updates when values actually change
            if (netMovingLeft.Value != movingLeft)
            {
                netMovingLeft.Value = movingLeft;
            }
            
            if (netMovingRight.Value != movingRight)
            {
                netMovingRight.Value = movingRight;
            }

            // Update jump animation states
            bool isJumpUp = jumpState == JumpState.JumpUp;
            bool isJumpDown = jumpState == JumpState.JumpDown;
            bool isLeftStrafeJump = jumpState == JumpState.LeftStrafeJump;
            bool isRightStrafeJump = jumpState == JumpState.RightStrafeJump;
            bool isGenericJump = jumpState == JumpState.Generic;
            bool isJumping = jumpState != JumpState.None;
            
            // For Clown model, prioritize generic jumping
            if (isClownModel && isJumping && !isGenericJump)
            {
                isGenericJump = true;
            }
            
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
            
            if (netJumping.Value != isJumping)
            {
                netJumping.Value = isJumping;
            }

            // Update clown-specific animation states
            if (isClownModel)
            {
                if (netClownMovingLeft.Value != movingLeft)
                {
                    netClownMovingLeft.Value = movingLeft;
                }
                
                if (netClownMovingRight.Value != movingRight)
                {
                    netClownMovingRight.Value = movingRight;
                }
                
                if (netClownMovingLeftJump.Value != (isJumping && movingLeft))
                {
                    netClownMovingLeftJump.Value = isJumping && movingLeft;
                }
                
                if (netClownMovingRightJump.Value != (isJumping && movingRight))
                {
                    netClownMovingRightJump.Value = isJumping && movingRight;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating network animation state: {e.Message}");
        }
    }
    
    public enum JumpState
    {
        None,
        JumpUp,
        JumpDown,
        LeftStrafeJump,
        RightStrafeJump,
        Generic // For Clown's single jump animation
    }
        
    public override void OnNetworkDespawn()
    {
        // Unregister callbacks to prevent memory leaks
        netMovingLeft.OnValueChanged -= OnNetworkValueChanged;
        netMovingRight.OnValueChanged -= OnNetworkValueChanged;
        netJumpUp.OnValueChanged -= OnNetworkValueChanged;
        netJumpDown.OnValueChanged -= OnNetworkValueChanged;
        netLeftStrafeJump.OnValueChanged -= OnNetworkValueChanged;
        netRightStrafeJump.OnValueChanged -= OnNetworkValueChanged;
        netJumping.OnValueChanged -= OnNetworkValueChanged;

        // Unregister callbacks for clown-specific animation NetworkVariables
        netClownMovingLeft.OnValueChanged -= OnNetworkValueChanged;
        netClownMovingRight.OnValueChanged -= OnNetworkValueChanged;
        netClownMovingLeftJump.OnValueChanged -= OnNetworkValueChanged;
        netClownMovingRightJump.OnValueChanged -= OnNetworkValueChanged;
        
        base.OnNetworkDespawn();
    }
}