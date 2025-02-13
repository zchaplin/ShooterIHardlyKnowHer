using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class WeaponBin : NetworkBehaviour
{
    public List<GameObject> dummyWeapons; // Assign dummy prefabs in Inspector
    public List<GameObject> realWeapons;  // Assign real weapon GameObjects
    private NetworkList<bool> weaponsUnlocked;

    private void Awake()
    {
        weaponsUnlocked = new NetworkList<bool>(
            new List<bool> { false, false, false, false, false }, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnlockWeaponServerRpc(int weaponIndex)
    {
        if (weaponIndex < weaponsUnlocked.Count)
        {
            weaponsUnlocked[weaponIndex] = true;
            UpdateDummyWeaponClientRpc(weaponIndex, true);
        }
    }

    [ClientRpc]
    private void UpdateDummyWeaponClientRpc(int weaponIndex, bool state)
    {
        dummyWeapons[weaponIndex].SetActive(state);
    }

    public GameObject GetRealWeapon(int weaponIndex)
    {
        if (weaponIndex < realWeapons.Count && weaponsUnlocked[weaponIndex])
            return realWeapons[weaponIndex];
        else
            return null;
    }
}