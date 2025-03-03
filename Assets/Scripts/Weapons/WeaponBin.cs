using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WeaponBin : NetworkBehaviour
{
    [SerializeField] private GameObject[] dummyWeaponPrefabs;  // Assign dummy prefabs in inspector
    private Dictionary<ulong, GameObject> weaponsInBin = new Dictionary<ulong, GameObject>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //Debug.Log($"WeaponBin spawned. IsServer: {IsServer}, IsHost: {IsHost}, IsClient: {IsClient}");
    }

    // New ServerRpc for weapon purchase
    [ServerRpc(RequireOwnership = false)]
    public void PurchaseWeaponServerRpc(int weaponIndex, ulong clientId)
    {
        //Debug.Log($"Server received purchase request for weapon {weaponIndex} from client {clientId}");
        
        if (!IsServer)
        {
            //Debug.LogError("PurchaseWeaponServerRpc execution on client - this should never happen!");
            return;
        }
        
        // Spawn the weapon in the bin for all clients to see
        SpawnDummyWeapon(weaponIndex);
        
        // Notify clients that this weapon was purchased successfully
        NotifyWeaponPurchasedClientRpc(weaponIndex, clientId);
    }
    
    // ClientRpc to notify all clients about a purchase
    [ClientRpc]
    private void NotifyWeaponPurchasedClientRpc(int weaponIndex, ulong clientId)
    {
        //Debug.Log($"Client notified that weapon {weaponIndex} was purchased by client {clientId}");
        
        // Find all clients' Shop instances
        Shop[] shops = FindObjectsOfType<Shop>();
        foreach (Shop shop in shops)
        {
            if (shop != null)
            {
                shop.MarkWeaponAsPurchased(weaponIndex);
            }
        }
    }

    // Spawn weapon in bin when purchased - only called on server
    public void SpawnDummyWeapon(int weaponIndex)
    {
        if (!IsServer)
        {
            //Debug.LogWarning("SpawnDummyWeapon called on client - should only be called on server!");
            return;
        }

        if (weaponIndex >= dummyWeaponPrefabs.Length) 
        {
            //Debug.LogError($"Invalid weapon index: {weaponIndex}");
            return;
        }

        try
        {
            // Spawn weapon at a random position within the bin
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0.5f,  // Slightly above bin floor
                Random.Range(-0.5f, 0.5f)
            );

            Vector3 spawnPos = transform.position + randomOffset;
            GameObject dummyWeapon = Instantiate(dummyWeaponPrefabs[weaponIndex], spawnPos, Quaternion.identity);

            // Spawn the weapon on the network
            NetworkObject networkObject = dummyWeapon.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
                
                // Add DummyWeapon component if it doesn't exist
                DummyWeapon dummyComponent = dummyWeapon.GetComponent<DummyWeapon>();
                if (dummyComponent == null)
                    dummyComponent = dummyWeapon.AddComponent<DummyWeapon>();

                dummyComponent.WeaponIndex.Value = weaponIndex;
                
                // Track the weapon by its network ID
                weaponsInBin[networkObject.NetworkObjectId] = dummyWeapon;
                
                // Debug.Log($"Weapon {weaponIndex} spawned successfully with NetworkObjectId: {networkObject.NetworkObjectId}");
            }
            else
            {
                //Debug.LogError("NetworkObject component missing on dummy weapon prefab!");
                Destroy(dummyWeapon);
            }
        }
        catch (System.Exception)
        {
            //Debug.LogError($"Error spawning weapon: {e.Message}\n{e.StackTrace}");
        }
    }

    // Called when weapon is dropped back in bin
    [ServerRpc(RequireOwnership = false)]
    public void ReturnWeaponServerRpc(int weaponIndex, Vector3 dropPosition)
    {
        //Debug.Log($"Server received request to return weapon {weaponIndex}");
        
        if (!IsServer)
        {
            //Debug.LogError("ReturnWeaponServerRpc execution on client - this should never happen!");
            return;
        }
        
        // Server-side spawn
        SpawnDummyWeapon(weaponIndex);
    }

    // Called when a player picks up a weapon
    [ServerRpc(RequireOwnership = false)]
    public void PickupWeaponServerRpc(ulong weaponNetworkId)
    {
        //Debug.Log($"Server received request to pickup weapon with ID: {weaponNetworkId}");
        
        if (!IsServer)
        {
            //Debug.LogError("PickupWeaponServerRpc execution on client - this should never happen!");
            return;
        }

        try {
            // Check if we can find this object in our spawn manager
            if (NetworkManager.Singleton != null && 
                NetworkManager.Singleton.SpawnManager != null && 
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(weaponNetworkId))
            {
                NetworkObject weaponNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[weaponNetworkId];
                
                if (weaponNetObj != null)
                {
                    // Remove from our tracking dictionary
                    if (weaponsInBin.ContainsKey(weaponNetworkId))
                    {
                        weaponsInBin.Remove(weaponNetworkId);
                    }
                    
                    // Despawn from network (this will remove it for all clients)
                    weaponNetObj.Despawn();
                    
                    //Debug.Log($"Weapon with ID {weaponNetworkId} successfully despawned");
                }
                else
                {
                    //Debug.LogError($"Found NetworkObjectId: {weaponNetworkId} but object reference is null");
                }
            }
            else
            {
                //Debug.LogError($"Could not find weapon with NetworkObjectId: {weaponNetworkId} in SpawnedObjects dictionary");
            }
        }
        catch (System.Exception)
        {
            //Debug.LogError($"Error during pickup: {e.Message}\n{e.StackTrace}");
        }
    }
}