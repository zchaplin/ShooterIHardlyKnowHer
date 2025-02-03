using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerInventory : NetworkBehaviour
{
    public Transform weapons;
    private Shop shop;

    // Activate once the player joins the game
    public override void OnNetworkSpawn() {
        if (OwnerClientId != NetworkManager.Singleton.LocalClientId) return;
        shop = FindObjectOfType<Shop>();
        if (shop) {
            shop.addWeapons(weapons);
        }
    }
}
