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
        // Pretty bad way to do this but it works for now, maybe someone can redo this with tags or something
        // Just checking that spawns exist and if not sets to one in scene
        if(!player1Spawn){
            player1Spawn = GameObject.Find("spawn1");
        }
        if(!player2Spawn){
            player2Spawn = GameObject.Find("spawn2");
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
            // Request the server to set the spawn position
            RequestSpawnPositionServerRpc(OwnerClientId);
        }
    }

    [ServerRpc]
    private void RequestSpawnPositionServerRpc(ulong clientId)
    {
        Debug.Log($"Server received spawn request from client {clientId}");

        Vector3 spawnPosition = GetSpawnPosition(clientId);


        SetSpawnPositionClientRpc(spawnPosition, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        // Check the OwnerClientId to decide the spawn position
        if (clientId == 0){ // Assuming the host has an ID of 0
            return player1Spawn.transform.position;
        }
        else if (clientId == 1){ // Assuming the client has an ID of 1
            return player2Spawn.transform.position;
        }
        else { // For any extras
            return new Vector3(-1f, 1f, 3f);
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

