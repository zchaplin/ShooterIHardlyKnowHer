using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject objectPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnObject();
        }
    }

    void SpawnObject()
    {
        // Instantiate the objectPrefab at the current position
        GameObject objectToSpawn = Instantiate(objectPrefab, transform.position, Quaternion.identity);

        // Set the layer of the spawned object to the name of the GameObject the script is attached to
        string layerName = gameObject.name;  // This will get the name of the GameObject the script is attached to
        int layer = LayerMask.NameToLayer(layerName);
        objectToSpawn.layer = layer;

        // Enable MeshRenderer if it exists
        MeshRenderer meshRenderer = objectToSpawn.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Use a NetworkVariable or RPC to synchronize visibility across clients
            meshRenderer.enabled = true;
        }

        // Get NetworkObject component
        NetworkObject networkObject = objectToSpawn.GetComponent<NetworkObject>();
        
        // Ensure the NetworkObject is spawned on both server and client
        networkObject.Spawn();

        // Use NetworkObject's ownership system to assign ownership to the client that spawned the object if needed
        if (IsOwner) 
        {
            // Assign ownership to the client that should own the object
            networkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        }
    }
}
