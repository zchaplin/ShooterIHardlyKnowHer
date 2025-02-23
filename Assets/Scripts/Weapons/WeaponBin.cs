using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WeaponBin : NetworkBehaviour
{
    [SerializeField] private GameObject[] dummyWeaponPrefabs;  // Assign dummy prefabs in inspector
    private List<GameObject> weaponsInBin = new List<GameObject>();

    // Spawn weapon in bin when purchased
    public void SpawnDummyWeapon(int weaponIndex)
    {
        if (weaponIndex >= dummyWeaponPrefabs.Length) return;

        // Only the server can spawn weapons
        if (!IsServer) return;

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
        networkObject.Spawn();

        // Add DummyWeapon component if it doesn't exist
        DummyWeapon dummyComponent = dummyWeapon.GetComponent<DummyWeapon>();
        if (dummyComponent == null)
            dummyComponent = dummyWeapon.AddComponent<DummyWeapon>();

        dummyComponent.WeaponIndex.Value = weaponIndex;
        weaponsInBin.Add(dummyWeapon);
    }

    // Called when weapon is dropped back in bin
    [ServerRpc(RequireOwnership = false)]
    public void ReturnWeaponServerRpc(int weaponIndex, Vector3 dropPosition)
    {
        SpawnDummyWeapon(weaponIndex);
    }

    // Called when a player picks up a weapon
    [ServerRpc(RequireOwnership = false)]
    public void PickupWeaponServerRpc(ulong weaponNetworkId)
    {
        // Find the weapon in the bin
        foreach (var weapon in weaponsInBin)
        {
            NetworkObject networkObject = weapon.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.NetworkObjectId == weaponNetworkId)
            {
                // Remove the weapon from the bin
                weaponsInBin.Remove(weapon);
                networkObject.Despawn();
                break;
            }
        }
    }

    // Client-side method to pick up a weapon
    public void PickupWeapon(GameObject weapon)
    {
        NetworkObject networkObject = weapon.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            PickupWeaponServerRpc(networkObject.NetworkObjectId);
        }
    }

    // Client-side method to return a weapon
    public void ReturnWeapon(int weaponIndex, Vector3 dropPosition)
    {
        ReturnWeaponServerRpc(weaponIndex, dropPosition);
    }
}