using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerInventory : NetworkBehaviour
{
    public Transform weapons;
    private Shop shop;
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
        if (shop != null)
        {
            shop.addWeapons(weapons);
        }

        // Start with default weapon
        ownedWeapons.Add(0);
    }

    void Update()
    {
        if (!IsOwner) return;

        CheckWeaponHighlight();

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
        Debug.DrawRay(playerCamera.position, playerCamera.forward, Color.red);
    }

    private void CheckWeaponHighlight()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 10f, weaponLayer))
        {
            //Debug.Log($"Raycast hit: {hit.collider.name}"); // Log the name of the object hit
            DummyWeapon dummyWeapon = hit.collider.GetComponentInParent<DummyWeapon>();
            if (dummyWeapon != null)
            {
                //Debug.Log("Highlighting weapon"); // Log when a weapon is highlighted
                dummyWeapon.Highlight();
            }
        }
        else
        {
            //Debug.Log("Raycast did not hit anything"); // Log when nothing is hit
            RemoveAllHighlights();
        }
    }

    private void RemoveAllHighlights()
    {
        // Find all dummy weapons in the scene and remove their highlights
        DummyWeapon[] allWeapons = FindObjectsOfType<DummyWeapon>();
        foreach (DummyWeapon weapon in allWeapons)
        {
            weapon.RemoveHighlight();
        }
    }

    private void TryPickupWeapon()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 10f, weaponLayer))
        {
            //Debug.Log("Hit something with raycast");
            DummyWeapon dummyWeapon = hit.collider.GetComponentInParent<DummyWeapon>();
            if (dummyWeapon != null)
            {
                //Debug.Log($"Found dummy weapon with index {dummyWeapon.WeaponIndex.Value}");
                PickupWeapon(dummyWeapon.WeaponIndex.Value);
                weaponBin.PickupWeapon(dummyWeapon.gameObject);  // Remove the dummy weapon from the bin
            }
        }
    }

    private void TryDropWeapon()
    {
        if (currentWeaponIndex == 0) return;  // Can't drop default weapon

        if (weaponBin != null)
        {
            // Spawn the dummy weapon in the bin
            weaponBin.ReturnWeapon(currentWeaponIndex, transform.position);

            // Remove from inventory
            ownedWeapons.Remove(currentWeaponIndex);

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