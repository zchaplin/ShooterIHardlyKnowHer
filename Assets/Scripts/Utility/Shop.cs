using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Shop : MonoBehaviour
{
    [SerializeField] public GameObject canvasShop;
    [SerializeField] public GameObject panel1;
    [SerializeField] public GameObject panel2;
    [SerializeField] public GameObject panel3;
    [SerializeField] public GameObject panel4;
    [SerializeField] public GameObject panel5;
    [SerializeField] public GameObject panel6;

    private List<Image> panels;
    private List<GameObject> player1Weapons;
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

        manageButtonImages();
        SetupButtonListeners();
       
        canvasShop.SetActive(false);
        panels = new List<Image>();
        panels.Add(panel1.GetComponent<Image>());
        panels.Add(panel2.GetComponent<Image>());
        panels.Add(panel3.GetComponent<Image>());
        panels.Add(panel4.GetComponent<Image>());

        Debug.Log("weapons stats: "+showWeaponStats);
    }

    private void SetupButtonListeners()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (int.TryParse(button.name, out int weaponIndex))
            {
                int index = weaponIndex; // Create a local copy for the lambda
                button.onClick.AddListener(() => PurchaseWeapon(index));
            }
        }
    }

    private void PurchaseWeapon(int weaponNum)
    {
        Debug.Log($"Attempting to purchase weapon {weaponNum}");
        
        if (player1Weapons.Count == 0)
        {
            Debug.LogWarning("No weapons available to purchase!");
            return;
        }
        
        // Check if client can afford it and doesn't already own it
        if (weapons1Bought[weaponNum] == 0 && ScoreTracker.score >= weaponsPrice[weaponNum])
        {
            Debug.Log($"Purchasing weapon {weaponNum} for {weaponsPrice[weaponNum]} points");
            
            // Deduct score locally
            ScoreTracker.score -= weaponsPrice[weaponNum];
            weapons1Bought[weaponNum] = 1;

            // Request server to spawn the weapon (handle both host and client case)
            if (networkManager != null && networkManager.IsClient)
            {
                // Find local player to get client ID
                ulong localClientId = networkManager.LocalClientId;
                Debug.Log($"Local client ID: {localClientId}, requesting purchase from server");
                
                if (weaponBin != null)
                {
                    // Call server RPC to purchase the weapon
                    weaponBin.PurchaseWeaponServerRpc(weaponNum, localClientId);
                }
            }
            else
            {
                Debug.LogError("NetworkManager not found or not connected as client!");
            }
        }
        else if (weapons1Bought[weaponNum] == 1 && weaponNum < player1Weapons.Count && player1Weapons[weaponNum].activeInHierarchy)
        {
            // Weapon is already bought and equipped: refill bullets
            Weapon weaponScript = player1Weapons[weaponNum].GetComponent<Weapon>();
            if (weaponScript != null)
            {
                Debug.Log($"Refilling bullets for weapon {weaponNum}");
                weaponScript.RefillBullets();
                if (showWeaponStats != null)
                {
                    showWeaponStats.updateText(weaponNum); // Update UI to reflect new ammo count
                }
            }
            else
            {
                Debug.LogError($"Weapon script not found on weapon {weaponNum}");
            }
        }
        else if (weapons1Bought[weaponNum] == 1 && weaponNum < player1Weapons.Count && !player1Weapons[weaponNum].activeInHierarchy)
        {
            Debug.Log($"Weapon {weaponNum} is already bought but not equipped. Cannot buy again.");
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
        Debug.Log("First weapon activated: " + player1Weapons[0]);
    }

    void Update()
    {
        for (int i=0; i<panels.Count; i+=1) 
        {
            if (ScoreTracker.score >= weaponsPrice[i]) 
            {
                panels[i].color = Color.white;
            }
            else
            {
                panels[i].color = Color.gray;
            }
        }
        
        // Toggle shop menu with B key
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!canvasShop.activeInHierarchy)
            {
                canvasShop.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                canvasShop.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        ShowCorrectImage();
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
            Debug.Log($"Activating weapon {weaponNum}");
            for (int i = 0; i < player1Weapons.Count; i++)
            {
                if (player1Weapons[i] != null)
                {
                    player1Weapons[i].SetActive(false);
                }
            }
            
            player1Weapons[weaponNum].SetActive(true);
            
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

    public void DeactivateWeapon(int weaponNum)
    {
        if (weaponNum <= 0 || weaponNum >= weapons1Bought.Count) return;
        
        weapons1Bought[weaponNum] = 0;
        
        if (weaponNum < player1Weapons.Count)
        {
            player1Weapons[weaponNum].SetActive(false);
            player1Weapons[0].SetActive(true);
        }
    }

    // Mark weapon as purchased (called from server via ClientRpc)
    public void MarkWeaponAsPurchased(int weaponNum)
    {
        if (weaponNum < 0 || weaponNum >= weapons1Bought.Count) return;
        
        weapons1Bought[weaponNum] = 1;
        Debug.Log($"Weapon {weaponNum} marked as purchased");
    }

    public void manageButtonImages() 
    {
        buttonImagesMap = new Dictionary<Button, RawImage[]>();

        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            RawImage[] images = button.GetComponentsInChildren<RawImage>();
            List<RawImage> childImages = new List<RawImage>();
            foreach (RawImage image in images)
            {
                if (image.gameObject != button.gameObject)
                {
                    childImages.Add(image);
                }
            }
            buttonImagesMap[button] = childImages.ToArray();
        }
    }

    public void ShowCorrectImage() 
    {
        if (player1Weapons.Count == 0) return;
        
        foreach (var kvp in buttonImagesMap)
        {
            Button button = kvp.Key;
            RawImage[] images = kvp.Value;
            
            if (!int.TryParse(button.name, out int i)) continue;
            if (i >= weapons1Bought.Count) continue;
            
            foreach (var image in images)
            {
                // Image for owned weapons
                if (weapons1Bought[i] == 1) 
                {
                    if (image.name == "emptyRedGun") 
                    {
                        image.enabled = true;
                    }
                } 
                else 
                {
                    if (image.name == "emptyRedGun") 
                    {
                        image.enabled = false;
                    }
                }

                // Image for the activated weapon
                if (i < player1Weapons.Count && player1Weapons[i] != null && player1Weapons[i].activeInHierarchy) 
                {
                    if (image.name == "filledRedGun") 
                    {
                        image.enabled = true;
                    }
                } 
                else 
                {
                    if (image.name == "filledRedGun") 
                    {
                        image.enabled = false;
                    }
                }
            }
        }
    }
}