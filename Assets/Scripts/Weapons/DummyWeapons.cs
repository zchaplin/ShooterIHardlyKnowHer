using Unity.Netcode;
using UnityEngine;

public class DummyWeapons : NetworkBehaviour
{
    public int weaponIndex; // Set this in Inspector (matches real weapon index)
    private WeaponBin weaponBin;

    private void Start()
    {
        weaponBin = FindObjectOfType<WeaponBin>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerInventory>();
        if (player != null && Input.GetKey(KeyCode.E))
        {
            player.PickupWeaponServerRpc(weaponIndex);
            gameObject.SetActive(false);
        }
    }
}