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
    private Animator playerAnimator;

    // NetworkVariables for animation states
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
        netMovingLeft.OnValueChanged += OnMovingLeftChanged;
        netMovingRight.OnValueChanged += OnMovingRightChanged;
        netJumpUp.OnValueChanged += OnJumpUpChanged;
        netJumpDown.OnValueChanged += OnJumpDownChanged;
        netLeftStrafeJump.OnValueChanged += OnLeftStrafeJumpChanged;
        netRightStrafeJump.OnValueChanged += OnRightStrafeJumpChanged;

        base.OnNetworkSpawn();
    }

        // Animation network callbacks
    private void OnMovingLeftChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("movingLeft", newValue);
        }
    }
    
    private void OnMovingRightChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("movingRight", newValue);
        }
    }

    private void OnJumpUpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("jumpUp", newValue);
        }
    }
    
    private void OnJumpDownChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("jumpDown", newValue);
        }
    }
    
    private void OnLeftStrafeJumpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("leftStrafeJump", newValue);
        }
    }
    
    private void OnRightStrafeJumpChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("rightStrafeJump", newValue);
        }
    }

    // Call this method from your movement script to update animation states
    public void UpdateAnimationState(bool movingLeft, bool movingRight, JumpState jumpState)
    {
        if (!IsOwner) return;
        
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
    
    public enum JumpState
    {
        None,
        JumpUp,
        JumpDown,
        LeftStrafeJump,
        RightStrafeJump
    }
}


