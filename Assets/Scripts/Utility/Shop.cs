using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject canvasShop;
    [SerializeField] private GameObject panel1;
    [SerializeField] private GameObject panel2;
    [SerializeField] private GameObject panel3;
    [SerializeField] private GameObject panel4;
    [SerializeField] private GameObject panel5;
    [SerializeField] private GameObject panel6;

    private List<Image> panels;

    private List<GameObject> player1Weapons;

    private List<int> weaponsPrice;
    private List<int> weapons1Bought;
    private Dictionary<Button, RawImage[]> buttonImagesMap;
    [SerializeField] private ShowWeaponStats showWeaponStats; // used for debugging


    void Start()
    {
        player1Weapons = new List<GameObject>();
        weaponsPrice = new List<int> {0,5,15,30,40,40};
        weapons1Bought = new List<int> {1,0,0,0,0,0};

        manageButtonImages();
       
        canvasShop.SetActive(false);
        // Panels colors
        panels = new List<Image>();
        panels.Add(panel1.GetComponent<Image>());
        panels.Add(panel2.GetComponent<Image>());
        panels.Add(panel3.GetComponent<Image>());
        panels.Add(panel4.GetComponent<Image>());

        Debug.Log("weapons stats: "+showWeaponStats);
    }

    public void addWeapons(Transform weapons) {
        for (int i=0; i<weapons.childCount; i+=1) {
            player1Weapons.Add(weapons.GetChild(i).gameObject);
        }

        for (int i=1; i<weapons.childCount; i+=1) {
            player1Weapons[i].SetActive(false);
        }

        player1Weapons[0].SetActive(true);
        Debug.Log(player1Weapons[0]);
    }

    void Update()
    {
        // Original idea: panels and outlines change color based on who owns the weapon
        for (int i=0; i<panels.Count; i+=1) {
            // check if have enough money to buy a weapon
            // color panel white if no one owns it yet
            if (ScoreTracker.score >= weaponsPrice[i]) {
                panels[i].color = Color.white;
            }
        }
        
        // check for player's turn
        if (Input.GetKeyDown(KeyCode.S)) {
            canvasShop.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        // Close shop when the player starts shotting again
        if (Input.GetKeyDown(KeyCode.W)) {
            canvasShop.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }


        ShowCorrectImage();
    }

    public void activateWeapons(int weaponNum) {
        // Show canvas and cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        canvasShop.SetActive(false);
        
        // if we bought the weapon, but no one uses it at the moment
        if (weapons1Bought[weaponNum] == 1)  {
            // Deactivate the player's current weapons and activate that one
            for (int i=0; i<player1Weapons.Count; i+=1) {
                player1Weapons[i].SetActive(false);
            }
            player1Weapons[weaponNum].SetActive(true);
            if (ScoreTracker.score >= weaponsPrice[weaponNum]) {
                player1Weapons[weaponNum].GetComponent<Weapon>().RefillBullets();
                GameObject.FindWithTag("Score").GetComponent<ScoreTracker>().addScore(-weaponsPrice[weaponNum]/2);
            }
        }
        // if not, that means we didn't buy the weapon
        else {
            // have enough money to buy
            if (ScoreTracker.score >= weaponsPrice[weaponNum]) {
                for (int i=0; i<player1Weapons.Count; i+=1) {
                    player1Weapons[i].SetActive(false);
                }
                player1Weapons[weaponNum].SetActive(true);
                GameObject.FindWithTag("Score").GetComponent<ScoreTracker>().addScore(-weaponsPrice[weaponNum]);
                weapons1Bought[weaponNum] = 1;
            }
        }

        showWeaponStats.updateText(weaponNum);
    }


    // Finds which images correspond to which buttons and save it in a dict
   public void manageButtonImages() {
        buttonImagesMap = new Dictionary<Button, RawImage[]>();

        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            // Get all images that are children of the button
            RawImage[] images = button.GetComponentsInChildren<RawImage>();

            // Exclude the Image component attached to the Button itself
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

   public void ShowCorrectImage() {
    foreach (var kvp in buttonImagesMap)
    {
        Button button = kvp.Key;
        RawImage[] images = kvp.Value;
        int i = int.Parse(button.name);

        
        foreach (var image in images)
        {
            // Image for owned weapons
            if (weapons1Bought[i] == 1) {
                if (image.name == "emptyRedGun") {
                    image.enabled = true;
                }
            } else {
                if (image.name == "emptyRedGun") {
                    image.enabled = false;
                }
            }

            // Image for the activated weapon
            if (player1Weapons.Count > 0 && player1Weapons[i].activeInHierarchy) {
                if (image.name == "filledRedGun") {
                    image.enabled = true;
                }
            } else {
                if (image.name == "filledRedGun") {
                    image.enabled = false;
                }
            }
        }
    }
   }
}
