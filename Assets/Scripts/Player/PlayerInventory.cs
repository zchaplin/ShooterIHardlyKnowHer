using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public List<GameObject> realWeapons; // Assign real weapon GameObjects
    private GameObject currentWeapon;
    private int currentWeaponIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            realWeapons[0].SetActive(true); // Enable default weapon (pea shooter)
            currentWeapon = realWeapons[0];
        }
    }

    [ServerRpc]
    public void PickupWeaponServerRpc(int weaponIndex)
    {
        if (!realWeapons[weaponIndex].activeSelf)
        {
            // Disable current weapon
            currentWeapon.SetActive(false);

            // Enable new weapon
            realWeapons[weaponIndex].SetActive(true);
            currentWeapon = realWeapons[weaponIndex];
            currentWeaponIndex = weaponIndex;

            // Sync state
            UpdateWeaponClientRpc(weaponIndex);
        }
    }

    [ClientRpc]
    private void UpdateWeaponClientRpc(int weaponIndex)
    {
        if (IsOwner) return; // Owner already handled
        realWeapons[weaponIndex].SetActive(true);
        currentWeapon = realWeapons[weaponIndex];
        currentWeaponIndex = weaponIndex;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReturnWeapon();
        }
    }

    private void ReturnWeapon()
    {
        if (currentWeaponIndex == 0) return; // Can't return default weapon

        // Re-enable dummy weapon in bin
        FindObjectOfType<WeaponBin>().UnlockWeaponServerRpc(currentWeaponIndex);

        // Switch back to default weapon
        currentWeapon.SetActive(false);
        currentWeapon = realWeapons[0];
        currentWeapon.SetActive(true);
        currentWeaponIndex = 0;
    }
}