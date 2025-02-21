using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private GameObject player1Spawn;
    [SerializeField] private GameObject player2Spawn;
    private readonly NetworkVariable<Vector3> netpos = new(writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<Quaternion> netrot = new(writePerm: NetworkVariableWritePermission.Owner);

    private bool hasSpawned = false;

    private void Awake()
    {
        // Check for spawn points and log their positions
        if (!player1Spawn)
        {
            player1Spawn = GameObject.Find("spawn1");
            Debug.Log($"Player 1 Spawn Position: {player1Spawn?.transform.position}");
        }
        if (!player2Spawn)
        {
            player2Spawn = GameObject.Find("spawn2");
            Debug.Log($"Player 2 Spawn Position: {player2Spawn?.transform.position}");
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            netpos.Value = transform.position;
            netrot.Value = transform.rotation;
        }
        else
        {
            transform.position = netpos.Value;
            transform.rotation = netrot.Value;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Player spawned with OwnerClientId: {OwnerClientId}, IsServer: {IsServer}, IsClient: {IsClient}");

        if (IsOwner && !hasSpawned) // Check for ownership and if not spawned
        {
            RequestSpawnPositionServerRpc();
        }
    }

    [ServerRpc]
    private void RequestSpawnPositionServerRpc()
    {
        Debug.Log($"Server received spawn request from client {OwnerClientId}");

        Vector3 spawnPosition = GetSpawnPosition(OwnerClientId);

        // Set spawn position for the owner
        SetSpawnPositionClientRpc(spawnPosition);
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        Debug.Log(clientId);
        // Check the OwnerClientId to decide the spawn position
        if (clientId == 0) // Assuming the host has an ID of 0
        {
            return player1Spawn.transform.position;
        }
        else // Assuming the client is not ID 0
        {
            return player2Spawn.transform.position;
        }
    }

    [ClientRpc]
    private void SetSpawnPositionClientRpc(Vector3 spawnPosition, ClientRpcParams rpcParams = default)
    {
        if (IsOwner)
        {
            transform.position = spawnPosition;
            netpos.Value = transform.position; // Update player spawn position
            hasSpawned = true; // Mark the player as spawned
            Debug.Log($"Player {OwnerClientId} spawned at: {transform.position}");
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
