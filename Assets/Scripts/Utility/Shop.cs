using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Shop : MonoBehaviour
{

    public List<GameObject> player1Weapons;
    private List<int> weaponsPrice;
    private List<int> weapons1Bought;
    private Dictionary<Button, RawImage[]> buttonImagesMap;
    [SerializeField] private ShowWeaponStats showWeaponStats;
    
    // Reference to network manager for server RPCs
    private NetworkManager networkManager;
    // Reference to weapon bin for spawning weapons
    private WeaponBin weaponBin;

    void Start()
    {
        player1Weapons = new List<GameObject>();
        weaponsPrice = new List<int> {0,5,15,30,40,40};
        weapons1Bought = new List<int> {1,0,0,0,0,0};

        // Get network manager reference
        networkManager = NetworkManager.Singleton;
        // Get weapon bin reference
        weaponBin = FindObjectOfType<WeaponBin>();
        
        if (weaponBin == null)
        {
            Debug.LogError("WeaponBin not found in scene! Shop functionality will be limited.");
        }
        
      

    }
    public void addWeapons(Transform weapons) 
    {
        // Clear previous weapons
        player1Weapons.Clear();
        
        for (int i=0; i<weapons.childCount; i+=1) 
        {
            player1Weapons.Add(weapons.GetChild(i).gameObject);
        }

        player1Weapons[0].SetActive(true);
        //Debug.Log("First weapon activated: " + player1Weapons[0]);
    }

    void Update()
    {
        
    }

    public void activateWeapons(int weaponNum)
    {
        // Safety checks
        if (player1Weapons.Count == 0)
        {
            Debug.LogWarning("No weapons to activate!");
            return;
        }
        
        if (weaponNum >= player1Weapons.Count)
        {
            Debug.LogError($"Weapon index {weaponNum} out of range!");
            return;
        }
        
        if (weapons1Bought[weaponNum] == 1 || weaponNum == 0)  // Always allow activating the default weapon
        {
            //Debug.Log($"Activating weapon {weaponNum}");
            // for (int i = 0; i < player1Weapons.Count; i++)
            // {
            //     if (player1Weapons[i] != null)
            //     {
            //         player1Weapons[i].SetActive(false);
            //     }
            // }
            
            // player1Weapons[weaponNum].SetActive(true); 
            
            if (showWeaponStats != null)
            {
                showWeaponStats.updateText(weaponNum);
            }
        }
        else
        {
            Debug.LogWarning($"Tried to activate weapon {weaponNum} but it's not owned!");
        }
    }

    // public void DeactivateWeapon(int weaponNum)
    // {
    //     if (weaponNum <= 0 || weaponNum >= weapons1Bought.Count) return;
        
    //     weapons1Bought[weaponNum] = 0;
        
    //     if (weaponNum < player1Weapons.Count)
    //     {
    //         player1Weapons[weaponNum].SetActive(false);
    //         player1Weapons[0].SetActive(true);
    //     }
    // }

    // Mark weapon as purchased (called from server via ClientRpc)
    public void MarkWeaponAsPurchased(int weaponNum)
    {
        if (weaponNum < 0 || weaponNum >= weapons1Bought.Count) return;
        
        weapons1Bought[weaponNum] = 1;
        //Debug.Log($"Weapon {weaponNum} marked as purchased");
    }



}