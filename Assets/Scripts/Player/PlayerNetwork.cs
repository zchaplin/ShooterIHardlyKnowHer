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

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            // Start with CharacterController disabled
            characterController.enabled = false;
        }
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

        base.OnNetworkSpawn();
    }
}


