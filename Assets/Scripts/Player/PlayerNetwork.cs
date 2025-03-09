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

    private CharacterController characterController;
    private Animator playerAnimator;

    // NetworkVariables for animation states
    public NetworkVariable<bool> netMovingLeft = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    public NetworkVariable<bool> netMovingRight = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            // Start with CharacterController disabled
            characterController.enabled = false;
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

        // Owner enables CharacterController when spawned
        if (IsOwner && characterController != null)
        {
            characterController.enabled = true;
        }

        // Register callbacks for animation NetworkVariables
        netMovingLeft.OnValueChanged += OnMovingLeftChanged;
        netMovingRight.OnValueChanged += OnMovingRightChanged;

        base.OnNetworkSpawn();
    }

        // Animation network callbacks
    private void OnMovingLeftChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("movingLeft", newValue);
            Debug.Log($"Player {OwnerClientId} animation: movingLeft = {newValue}");
        }
    }
    
    private void OnMovingRightChanged(bool previousValue, bool newValue)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("movingRight", newValue);
            Debug.Log($"Player {OwnerClientId} animation: movingRight = {newValue}");
        }
    }

    // Call this method from your movement script to update animation states
    public void UpdateAnimationState(bool movingLeft, bool movingRight)
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
    }
}


