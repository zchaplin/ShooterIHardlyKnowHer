using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerPrefabSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject player1Prefab; // Original player
    [SerializeField] private GameObject player2Prefab; // Clown player
    
    // References to the spawned player objects
    private static GameObject player1Instance;
    private static GameObject player2Instance;
    private Dictionary<ulong, bool> clientsSpawned = new Dictionary<ulong, bool>();


    // To prevent conflicts with RelayManager
    [SerializeField] private bool replaceNetworkManagerSpawning = false;

    void Awake()
    {
        // Only replace the NetworkManager's player prefab if specified
        if (replaceNetworkManagerSpawning && NetworkManager.Singleton != null)
        {
            // Remove the default player prefab from NetworkManager
            var networkManagerField = typeof(NetworkManager).GetField("_playerPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (networkManagerField != null)
            {
                networkManagerField.SetValue(NetworkManager.Singleton, null);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && replaceNetworkManagerSpawning)
        {
            // Register to handle connections
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            
            // If this is the host, spawn player 1 for ourselves immediately
            if (IsHost)
            {
                SpawnPlayer(NetworkManager.Singleton.LocalClientId);
            }
        }
        
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && replaceNetworkManagerSpawning)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        
        base.OnNetworkDespawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        // Only handle client connections if we're replacing the NetworkManager spawning
        if (replaceNetworkManagerSpawning)
        {
            SpawnPlayer(clientId);
        }
    }

    // This can be called directly from RelayManager to manually spawn different player prefabs
    public void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;

        // Check if this client already has a player spawned
        if (clientsSpawned.ContainsKey(clientId) && clientsSpawned[clientId])
        {
            Debug.LogWarning($"Player already spawned for client {clientId}, skipping spawn");
            return;
        }

        // Determine which player prefab to use based on connection order
        GameObject prefabToSpawn;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        
        if (clientId == NetworkManager.ServerClientId) // Host/Server (Player 1)
        {
            prefabToSpawn = player1Prefab;
            spawnPosition = new Vector3(1, 1, -3);
            spawnRotation = Quaternion.LookRotation(Vector3.forward);
        }
        else // Client (Player 2)
        {
            prefabToSpawn = player2Prefab;
            spawnPosition = new Vector3(1, 1, 3);
            spawnRotation = Quaternion.LookRotation(Vector3.back);
        }

        // Spawn the player object
        GameObject playerObject = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
        
        // Spawn on the network and give ownership to the appropriate client
        networkObject.SpawnAsPlayerObject(clientId);
        
        // Store reference to the player instance
        if (clientId == NetworkManager.ServerClientId)
        {
            player1Instance = playerObject;
            playerObject.name = "Player1";
        }
        else
        {
            player2Instance = playerObject;
            playerObject.name = "Player2";
        }
        
        Debug.Log($"Spawned player {clientId} with prefab {prefabToSpawn.name}");
        clientsSpawned[clientId] = true;
    }
    
    // Helper methods to get player instances
    public static GameObject GetPlayer1Instance() => player1Instance;
    public static GameObject GetPlayer2Instance() => player2Instance;
}