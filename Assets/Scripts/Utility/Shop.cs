using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void Start()
    {
        player1Weapons = new List<GameObject>();
        weaponsPrice = new List<int> {0,5,15,30,40,40};
        weapons1Bought = new List<int> {1,0,0,0,0,0};

        manageButtonImages();
        SetupButtonListeners(); // Add this line to setup button clicks
       
        canvasShop.SetActive(false);
        panels = new List<Image>();
        panels.Add(panel1.GetComponent<Image>());
        panels.Add(panel2.GetComponent<Image>());
        panels.Add(panel3.GetComponent<Image>());
        panels.Add(panel4.GetComponent<Image>());

        Debug.Log("weapons stats: "+showWeaponStats);
    }

    // Add this new method to setup button listeners
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

    // New method to handle weapon purchase
    private void PurchaseWeapon(int weaponNum)
    {
        Debug.Log($"Attempting to purchase weapon {weaponNum}");
        
        if (weapons1Bought[weaponNum] == 0 && ScoreTracker.score >= weaponsPrice[weaponNum])
        {
            Debug.Log($"Purchasing weapon {weaponNum} for {weaponsPrice[weaponNum]} points");
            ScoreTracker.score -= weaponsPrice[weaponNum];
            weapons1Bought[weaponNum] = 1;

            // Spawn weapon in bin
            if (weaponNum > 0)
            {
                GameObject bin = GameObject.FindGameObjectWithTag("WeaponBin");
                if (bin)
                {
                    Debug.Log($"Found bin, spawning weapon {weaponNum}");
                    WeaponBin binScript = bin.GetComponent<WeaponBin>();
                    if (binScript != null)
                    {
                        binScript.SpawnDummyWeapon(weaponNum);
                    }
                    else
                    {
                        Debug.LogError("WeaponBin script not found on bin object");
                    }
                }
                else
                {
                    Debug.LogError("Could not find object with WeaponBin tag");
                }
            }
        }
        else if (weapons1Bought[weaponNum] == 1 && player1Weapons[weaponNum].activeInHierarchy)
        {
            // Weapon is already bought and equipped: refill bullets
            Weapon weaponScript = player1Weapons[weaponNum].GetComponent<Weapon>();
            if (weaponScript != null)
            {
                Debug.Log($"Refilling bullets for weapon {weaponNum}");
                weaponScript.RefillBullets();
                showWeaponStats.updateText(weaponNum); // Update UI to reflect new ammo count
            }
            else
            {
                Debug.LogError($"Weapon script not found on weapon {weaponNum}");
            }
        }
        else if (weapons1Bought[weaponNum] == 1 && !player1Weapons[weaponNum].activeInHierarchy)
        {
            Debug.Log($"Weapon {weaponNum} is already bought but not equipped. Cannot buy again.");
        }
    }

    public void addWeapons(Transform weapons) 
    {
        for (int i=0; i<weapons.childCount; i+=1) 
        {
            player1Weapons.Add(weapons.GetChild(i).gameObject);
        }

        // for (int i=1; i<weapons.childCount; i+=1) 
        // {
        //     player1Weapons[i].SetActive(false);
        // }

        player1Weapons[0].SetActive(true);
        //Debug.Log(player1Weapons[0]);
    }

    void Update()
    {
        for (int i=0; i<panels.Count; i+=1) 
        {
            if (ScoreTracker.score >= weaponsPrice[i]) 
            {
                panels[i].color = Color.white;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!canvasShop.activeInHierarchy)
            {
                canvasShop.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                ShowWeaponStats.isPaused = true;
            }
            else
            {
                canvasShop.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
            }
        }

        ShowCorrectImage();
    }

    // This is now only used when picking up weapons from the bin
    public void activateWeapons(int weaponNum)
    {
        if (weapons1Bought[weaponNum] == 1)
        {
            for (int i = 0; i < player1Weapons.Count; i++)
            {
                player1Weapons[i].SetActive(false);
            }
            player1Weapons[weaponNum].SetActive(true);
            showWeaponStats.updateText(weaponNum);
        }
    }

    public void DeactivateWeapon(int weaponNum)
    {
        if (weaponNum <= 0 || weaponNum >= weapons1Bought.Count) return;
        
        weapons1Bought[weaponNum] = 0;
        player1Weapons[weaponNum].SetActive(false);
        player1Weapons[0].SetActive(true);
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
        foreach (var kvp in buttonImagesMap)
        {
            Button button = kvp.Key;
            RawImage[] images = kvp.Value;
            int i = int.Parse(button.name);
            
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
                if (player1Weapons.Count > 0 && player1Weapons[i].activeInHierarchy) 
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