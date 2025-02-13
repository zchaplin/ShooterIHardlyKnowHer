using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerInventory : NetworkBehaviour
{
    public Transform weapons;
    private Shop shop;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private Transform playerCamera;  // Assign the player's camera in inspector
    [SerializeField] private LayerMask weaponLayer;  // Set this to the layer your dummy weapons are on
    private int currentWeaponIndex = 0;
    private List<int> ownedWeapons = new List<int>();
    private WeaponBin weaponBin;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        shop = FindObjectOfType<Shop>();
        weaponBin = FindObjectOfType<WeaponBin>();
        if (shop) {
            shop.addWeapons(weapons);
        }
        
        // Start with default weapon
        ownedWeapons.Add(0);
    }

    void Update()
    {
        if (!IsOwner) return;

        // Pickup weapon when looking at it and pressing E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupWeapon();
        }

        // Drop weapon when pressing T
        if (Input.GetKeyDown(KeyCode.T))
        {
            TryDropWeapon();
        }

        // Switch weapons with number keys
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
            }
        }

        // Debug raycast to see what we're looking at
        Debug.DrawRay(playerCamera.position, playerCamera.forward * pickupRange, Color.red);
    }

    private void TryPickupWeapon()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickupRange, weaponLayer))
        {
            Debug.Log("Hit something with raycast");
            DummyWeapon dummyWeapon = hit.collider.GetComponent<DummyWeapon>();
            if (dummyWeapon != null)
            {
                Debug.Log($"Found dummy weapon with index {dummyWeapon.WeaponIndex}");
                PickupWeapon(dummyWeapon.WeaponIndex);
                Destroy(dummyWeapon.gameObject);  // Remove the dummy weapon
            }
        }
    }

    private void TryDropWeapon()
    {
        if (currentWeaponIndex == 0) return;  // Can't drop default weapon

        if (weaponBin != null)
        {
            // Spawn the dummy weapon in the bin
            weaponBin.SpawnDummyWeapon(currentWeaponIndex);
            
            // Remove from inventory
            ownedWeapons.Remove(currentWeaponIndex);
            shop.DeactivateWeapon(currentWeaponIndex);
            
            // Switch back to default weapon
            currentWeaponIndex = 0;
            shop.activateWeapons(0);
        }
    }

    private void PickupWeapon(int weaponIndex)
    {
        if (!ownedWeapons.Contains(weaponIndex))
        {
            ownedWeapons.Add(weaponIndex);
        }
        currentWeaponIndex = weaponIndex;
        shop.activateWeapons(weaponIndex);
    }

    private void SwitchWeapon(int index)
    {
        if (ownedWeapons.Contains(index))
        {
            currentWeaponIndex = index;
            shop.activateWeapons(index);
        }
    }
}