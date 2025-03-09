using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerInventory : NetworkBehaviour
{
    public Transform weapons;
    private Shop shop;
    [SerializeField] private Transform playerCamera;  // Assign the player's camera in inspector
    [SerializeField] private LayerMask weaponLayer;  // Set this to the layer your dummy weapons are on
    [SerializeField] private GameObject pickupText;  // Assign the pickup text in inspector

    
    // Network variables for weapon management
    private NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);
    
    public List<int> ownedWeapons = new List<int>();
    private WeaponBin weaponBin;
    private bool isInitialized = false;
    private GunUI gunUI;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Debug.Log($"PlayerInventory spawned. IsOwner: {IsOwner}, IsServer: {IsServer}, ClientId: {OwnerClientId}");
        pickupText = GameObject.Find("Canvas/WeaponStats/PickUpText");
        pickupText.SetActive(false);
        // Find required components
        if (shop == null) shop = FindObjectOfType<Shop>();
        gunUI = FindObjectOfType<GunUI>();
        if (weaponBin == null) weaponBin = FindObjectOfType<WeaponBin>();
        
        if (IsOwner)
        {
            if (shop != null)
            {
                shop.addWeapons(weapons);
                // Start with default weapon
                ownedWeapons.Add(0);
                currentWeaponIndex.Value = 0;
                
                Debug.Log("Player inventory initialized for owner");
            }
            else
            {
                Debug.LogError("Shop script not found!");
            }
        }
        
        // Subscribe to network variable changes
        currentWeaponIndex.OnValueChanged += OnWeaponIndexChanged;
        
        isInitialized = true;
    }
    
    private void OnWeaponIndexChanged(int oldValue, int newValue)
    {
       //Debug.Log($"Network weapon change: {oldValue} -> {newValue}");
        
        // Make sure we have a weapons transform
        if (weapons == null || weapons.childCount == 0)
        {
            Debug.LogError("No weapons available after weapon change!");
            return;
        }
        
        // Update owned weapons list if not already in it
        if (!ownedWeapons.Contains(newValue))
        {
            ownedWeapons.Add(newValue);
        }
        
        // Update the weapon visibility directly
        for (int i = 0; i < weapons.childCount; i++)
        {
            if (i < weapons.childCount && weapons.GetChild(i) != null)
            {
                weapons.GetChild(i).gameObject.SetActive(i == newValue);
            }
        }
        // reload weapon
        // Transform weaponTransform = weapons.GetChild(newValue);
        // Weapon weaponScript = weaponTransform.gameObject.GetComponent<Weapon>();
        // weaponScript.RefillBullets();

        
        // If we're the owner, also update the shop UI
        if (IsOwner && shop != null)
        {
            // shop.activateWeapons(newValue);
            //.Log($"Weapon {newValue} activated via Shop for owner");
        }
    }

    void Update()
    {
        if (!IsOwner || !isInitialized) return;

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
    }

    private void CheckWeaponHighlight()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 10f, weaponLayer))
        {
            DummyWeapon dummyWeapon = hit.collider.GetComponentInParent<DummyWeapon>();
            if (dummyWeapon != null)
            {
                pickupText.SetActive(true);
                dummyWeapon.Highlight();
            }
        }
        else
        {
            RemoveAllHighlights();
            pickupText.SetActive(false);
        }
    }

    private void RemoveAllHighlights()
    {
        // Find all dummy weapons in the scene and remove their highlights
        DummyWeapon[] allWeapons = FindObjectsOfType<DummyWeapon>();
        foreach (DummyWeapon weapon in allWeapons)
        {
            if (weapon != null)
            {
                weapon.RemoveHighlight();
            }
        }
    }

    private void TryPickupWeapon()
    {
        if (weaponBin == null)
        {
            weaponBin = FindObjectOfType<WeaponBin>();
            if (weaponBin == null)
            {
                Debug.LogError("Cannot find WeaponBin in scene!");
                return;
            }
        }
        
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 10f, weaponLayer))
        {
            //Debug.Log("Hit something with raycast");
            
            DummyWeapon dummyWeapon = hit.collider.GetComponentInParent<DummyWeapon>();
            pickupText.SetActive(false);
            if (dummyWeapon != null)
            {
                int weaponIndex = dummyWeapon.WeaponIndex.Value;
                //Debug.Log($"Found dummy weapon with index {weaponIndex}");
                
                NetworkObject networkObj = dummyWeapon.GetComponent<NetworkObject>();
                if (networkObj != null)
                {
                    ulong networkId = networkObj.NetworkObjectId;
                    
                    // Update our local weapon index first
                    currentWeaponIndex.Value = weaponIndex;
                    Transform weaponTransform = weapons.GetChild(weaponIndex);
                    Weapon weaponScript = weaponTransform.gameObject.GetComponent<Weapon>();
                    weaponScript.RefillBullets();

                    // Then tell the server to remove the dummy weapon
                    weaponBin.PickupWeaponServerRpc(networkId);
    
                    gunUI.ChangeAvailableGunColor(weaponIndex);
                    
                    //Debug.Log($"Requested pickup of weapon {weaponIndex} with networkID {networkId}");
                }
                else
                {
                    Debug.LogError("DummyWeapon has no NetworkObject component!");
                }
            }
        }
    }

    private void TryDropWeapon()
    {
        if (currentWeaponIndex.Value == 0) return;  // Can't drop default weapon
        
        if (weaponBin == null)
        {
            weaponBin = FindObjectOfType<WeaponBin>();
            if (weaponBin == null)
            {
                Debug.LogError("Cannot find WeaponBin in scene!");
                return;
            }
        }

        // Request server to spawn the dummy weapon in the bin
        weaponBin.ReturnWeaponServerRpc(currentWeaponIndex.Value, transform.position);

        // Remove from inventory
        ownedWeapons.Remove(currentWeaponIndex.Value);

        // Switch back to default weapon
        currentWeaponIndex.Value = 0;
    }

    private void SwitchWeapon(int index)
    {
        if (ownedWeapons.Contains(index))
        {
            currentWeaponIndex.Value = index;
            gunUI.ChangeSelectedGunColor(index);
            // Transform weaponTransform = weapons.GetChild(index);
            // Weapon weaponScript = weaponTransform.gameObject.GetComponent<Weapon>();
            // weaponScript.RefillBullets();

        }
    }

    public override void OnDestroy()
    {
        // Clean up subscription
        if (IsSpawned)
        {
            currentWeaponIndex.OnValueChanged -= OnWeaponIndexChanged;
        }
        base.OnDestroy();
    }
}