// using System.Collections;
// using Unity.Netcode;
// using UnityEngine;

// public class PlayerNetwork : NetworkBehaviour
// {
//     private readonly NetworkVariable<Vector3> netpos = new(writePerm: NetworkVariableWritePermission.Owner);
//     private readonly NetworkVariable<Quaternion> netrot = new(writePerm: NetworkVariableWritePermission.Owner);

//     private bool hasSpawned = false; // Track if the player has spawned

//     void Update()
//     {
//         if (IsOwner)
//         {
//             netpos.Value = transform.position;
//             netrot.Value = transform.rotation;
//         }
//         else
//         {
//             transform.position = netpos.Value;
//             transform.rotation = netrot.Value;
//         }
//     }

//     public override void OnNetworkSpawn()
//     {
//         Debug.Log($"Player spawned with OwnerClientId: {OwnerClientId}, IsServer: {IsServer}, IsClient: {IsClient}");

//         if (!hasSpawned)
//         {
//             // Request the server to set the spawn position
//             RequestSpawnPositionServerRpc();
//         }
//     }

//     [ServerRpc]
//     private void RequestSpawnPositionServerRpc()
//     {
//         Debug.Log($"Server received spawn request from client {OwnerClientId}");

//         Vector3 spawnPosition;

//         // Check the OwnerClientId to decide the spawn position
//         if (OwnerClientId == 0) // Assuming the host has an ID of 0
//         {
//             spawnPosition = new Vector3(1f, 1f, -3f);
//         }
//         else // For other clients
//         {
//             spawnPosition = new Vector3(-1f, 1f, 5f);
//         }

//         // Adjust Y position to match the ground level
//         spawnPosition.y = GetGroundHeight(spawnPosition);

//         // Send the spawn position back to the client
//         SetSpawnPositionClientRpc(spawnPosition, new ClientRpcParams
//         {
//             Send = new ClientRpcSendParams
//             {
//                 TargetClientIds = new ulong[] { OwnerClientId }
//             }
//         });
//     }


//     [ClientRpc]
//     private void SetSpawnPositionClientRpc(Vector3 spawnPosition, ClientRpcParams rpcParams = default)
//     {
//         if (IsOwner)
//         {
//             transform.position = spawnPosition;
//             hasSpawned = true; // Mark the player as spawned
//             Debug.Log($"Player {OwnerClientId} spawned at: {spawnPosition}");
//         }
//     }

//     private float GetGroundHeight(Vector3 position)
//     {
//         RaycastHit hit;
//         if (Physics.Raycast(new Vector3(position.x, 10f, position.z), Vector3.down, out hit, Mathf.Infinity))
//         {
//             return hit.point.y + 0.1f; // Add slight offset to prevent clipping
//         }
//         return position.y; // Default if no ground detected
//     }
// }

using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // NetworkVariables to sync position and rotation
    private readonly NetworkVariable<Vector3> netpos = new(writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<Quaternion> netrot = new(writePerm: NetworkVariableWritePermission.Owner);

    private bool hasSpawned = false; // Tracks if the player has been spawned

    void Update()
    {
        if (IsOwner)
        {
            // Update the networked position/rotation if this is the owner
            netpos.Value = transform.position;
            netrot.Value = transform.rotation;
        }
        else
        {
            // Otherwise, update based on networked values
            transform.position = netpos.Value;
            transform.rotation = netrot.Value;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Player spawned with OwnerClientId: {OwnerClientId}, IsServer: {IsServer}, IsClient: {IsClient}");

        if (!hasSpawned)
        {
            // Request the server to assign the spawn position
            RequestSpawnPositionServerRpc();
        }
    }

    [ServerRpc]
    private void RequestSpawnPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        // Use the SenderClientId to differentiate which client requested the spawn
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Server received spawn request from client {senderClientId}");

        Vector3 spawnPosition;

        // For this example, we assume the host has ClientId 0
        if (senderClientId == 0)
        {
            spawnPosition = new Vector3(1f, 1f, -3f);
        }
        else if (senderClientId == 1)
        {
            spawnPosition = new Vector3(1f, 1f, 3f);
        }
        else
        {
            spawnPosition = new Vector3(-1f, 1f, 3f);
        }

        // Adjust Y position based on the ground level
        spawnPosition.y = GetGroundHeight(spawnPosition);

        // Send the spawn position back only to the client that requested it
        SetSpawnPositionClientRpc(spawnPosition, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { senderClientId }
            }
        });
    }

    [ClientRpc]
    private void SetSpawnPositionClientRpc(Vector3 spawnPosition, ClientRpcParams rpcParams = default)
    {
        if (IsOwner)
        {
            transform.position = spawnPosition;
            hasSpawned = true;
            Debug.Log($"Player {OwnerClientId} spawned at: {spawnPosition}");
        }
    }

    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        // Cast a ray downwards from a height (10f) at the x/z location to find the ground
        if (Physics.Raycast(new Vector3(position.x, 10f, position.z), Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.point.y + 0.1f; // Add a slight offset to prevent clipping
        }
        return position.y; // Return the original y if no ground is detected
    }
}
