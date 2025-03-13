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

        base.OnNetworkSpawn();
    }
}