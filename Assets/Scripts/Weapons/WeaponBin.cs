using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WeaponBin : NetworkBehaviour
{
    [SerializeField] private GameObject[] dummyWeaponPrefabs;  // Assign dummy prefabs in inspector
    [SerializeField] private float pickupRange = 3f;  // How far player can pickup from
    private List<GameObject> weaponsInBin = new List<GameObject>();

    // Spawn weapon in bin when purchased
    public void SpawnDummyWeapon(int weaponIndex)
    {
        if (weaponIndex >= dummyWeaponPrefabs.Length) return;
        
        // Spawn weapon at a random position within the bin
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            0.5f,  // Slightly above bin floor
            Random.Range(-0.5f, 0.5f)
        );
        
        Vector3 spawnPos = transform.position + randomOffset;
        GameObject dummyWeapon = Instantiate(dummyWeaponPrefabs[weaponIndex], spawnPos, Quaternion.identity);
        dummyWeapon.GetComponent<NetworkObject>().Spawn();
        
        DummyWeapon dummyComponent = dummyWeapon.GetComponent<DummyWeapon>();
        if (dummyComponent == null)
            dummyComponent = dummyWeapon.AddComponent<DummyWeapon>();
            
        dummyComponent.WeaponIndex = weaponIndex;
        weaponsInBin.Add(dummyWeapon);
    }

    // Called when weapon is dropped back in bin
    public void ReturnWeapon(int weaponIndex, Vector3 dropPosition)
    {
        SpawnDummyWeapon(weaponIndex);
    }
}