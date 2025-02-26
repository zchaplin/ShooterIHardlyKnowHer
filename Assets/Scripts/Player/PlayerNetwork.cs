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

        if (IsOwner && !hasSpawned)
        {
            RequestSpawnPositionServerRpc(OwnerClientId);
        }
    }

    [ServerRpc]
    private void RequestSpawnPositionServerRpc(ulong clientId)
    {
        Debug.Log($"Server received spawn request from client {clientId}");

        Vector3 spawnPosition = GetSpawnPosition(clientId);

        // Apply position on the server-side instance as well
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            networkClient.PlayerObject.transform.position = spawnPosition;
        }

        SetSpawnPositionClientRpc(spawnPosition);
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        // Assign spawn based on connected clients' order
        if (NetworkManager.Singleton.ConnectedClientsList[0].ClientId == clientId)
        {
            return player1Spawn.transform.position;
        }
        else
        {
            return player2Spawn.transform.position;
        }
    }

    [ClientRpc]
    private void SetSpawnPositionClientRpc(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        netpos.Value = spawnPosition;
        hasSpawned = true;
        Debug.Log($"Player {OwnerClientId} spawned at: {spawnPosition}");
    }
}
