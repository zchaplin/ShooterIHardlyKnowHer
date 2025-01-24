using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] public GameObject peaShooter1;
    [SerializeField] public GameObject peaShooter2;
    [SerializeField] public GameObject launcher1;
    [SerializeField] public GameObject launcher2;
    [SerializeField] public GameObject boomarang1;
    [SerializeField] public GameObject boomarang2;
    [SerializeField] public GameObject weapon41;
    [SerializeField] public GameObject weapon42;
    //[SerializeField] public GameObject weapon51;
    //[SerializeField] public GameObject weapon52;

    [SerializeField] public GameObject canvasShop;
    [SerializeField] public GameObject panel1;
    [SerializeField] public GameObject panel2;
    [SerializeField] public GameObject panel3;
    [SerializeField] public GameObject panel4;
    [SerializeField] public GameObject panel5;
    private List<Image> panels;

    private List<GameObject> player1Weapons;
    private List<GameObject> player2Weapons;

    private List<int> weaponsPrice;
    private List<int> weapons1Bought;
    private List<int> weapons2Bought;

    private int player = 0;

    private Dictionary<Button, RawImage[]> buttonImagesMap;


    void Start()
    {
        manageButtonImages();
       
        canvasShop.SetActive(false);

        player1Weapons = new List<GameObject>();
        player1Weapons.Add(peaShooter1);
        player1Weapons.Add(launcher1);
        player1Weapons.Add(boomarang1);
        player1Weapons.Add(weapon41);
        //player1Weapons.Add(weapon51);

        player2Weapons = new List<GameObject>();
        player2Weapons.Add(peaShooter2);
        player2Weapons.Add(launcher2);
        player2Weapons.Add(boomarang2
);
        player2Weapons.Add(weapon42);
        //player2Weapons.Add(weapon52);

        peaShooter1.SetActive(true);
        peaShooter2.SetActive(true);

        for (int i=1; i<player1Weapons.Count; i+=1) {
            player1Weapons[i].SetActive(false);
        }
        for (int i=1; i<player2Weapons.Count; i+=1) {
            player2Weapons[i].SetActive(false);
        }


        weaponsPrice = new List<int> {0,5,20,45,100};
        weapons1Bought = new List<int> {1,0,0,0,0};
        weapons2Bought = new List<int> {1,0,0,0,0};


        // Panels colors
        panels = new List<Image>();
        panels.Add(panel1.GetComponent<Image>());
        panels.Add(panel2.GetComponent<Image>());
        panels.Add(panel3.GetComponent<Image>());
        panels.Add(panel4.GetComponent<Image>());
        //panels.Add(panel5.GetComponent<Image>());
    }

    // Update is called once per frame
    void Update()
    {
        peaShooter1.SetActive(true);
        peaShooter2.SetActive(true);
        // Original idea: panels and outlines change color based on who owns the weapon
        for (int i=0; i<panels.Count; i+=1) {
            // check if have enough money to buy a weapon
            // color panel white if no one owns it yet
            if (ScoreTracker.score >= weaponsPrice[i]) {
                panels[i].color = Color.white;
            }
        }
        
        // check for player's turn
        if (Input.GetKeyDown(KeyCode.S))
        {
            player = 2;
            canvasShop.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            player = 1;
            canvasShop.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
        }

        // Close shop when the player starts shotting again
         if (Input.GetKeyDown(KeyCode.W) && player == 2)
        {
            canvasShop.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) && player == 1)
        {
            canvasShop.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
        }

        ShowCorrectImage();
    }
    public void activateWeapons(int weaponNum) {
        Debug.Log("clicked on button " + weaponNum);
        // Show canvas and cursor
        Cursor.lockState = CursorLockMode.None;
        canvasShop.SetActive(false);

        // Used to figure out which player will get the weapon based on who walked BACKWARDS
        List<GameObject> playerWeapons = (player == 1) ? player1Weapons : player2Weapons;
        List<GameObject> otherWeapons = (player == 1) ? player2Weapons : player1Weapons;
        List<int> playerBoughtWeapons = (player == 1) ? weapons1Bought : weapons2Bought;
        List<int> otherBoughtWeapons = (player == 1) ? weapons2Bought : weapons1Bought;

        
        // if we bought the weapon, but no one uses it at the moment
        if (playerBoughtWeapons[weaponNum] == 1)  {
            // if the other player has it
            if (otherWeapons[weaponNum].activeInHierarchy) {
                // activate current player's weapon and deactive the one that the other player is holding
                if (weaponNum != 0) {
                    otherWeapons[weaponNum].SetActive(false);
                    // Give a new weapon to the other player
                    otherWeapons[0].SetActive(true);
                }
            }
            // Deactivate the player's current weapons and activate that one
            for (int i=0; i<playerWeapons.Count; i+=1) {
                playerWeapons[i].SetActive(false);
            }
            playerWeapons[weaponNum].SetActive(true);
        }
        // if not, that means we didn't buy the weapon
        else {
            // have enough money to buy
            if (ScoreTracker.score >= weaponsPrice[weaponNum]) {
                for (int i=0; i<playerWeapons.Count; i+=1) {
                    playerWeapons[i].SetActive(false);
                }

                if (otherWeapons[weaponNum].activeInHierarchy) {
                    // activate current player's weapon and deactive the one that the other player is holding
                    if (weaponNum != 0) {
                        otherWeapons[weaponNum].SetActive(false);
                        // Give a new weapon to the other player
                        otherWeapons[0].SetActive(true);
                    }
                }

                playerWeapons[weaponNum].SetActive(true);
                ScoreTracker.score -= weaponsPrice[weaponNum];
                playerBoughtWeapons[weaponNum] = 1;
            }
        }
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
            if (weapons2Bought[i] == 1) {
                if (image.name == "emptyBlueGun") {
                    image.enabled = true;
                }
            } else {
                if (image.name == "emptyBlueGun") {
                    image.enabled = false;
                }
            }

            // Image for the activated weapon
            if (player1Weapons[i].activeInHierarchy) {
                if (image.name == "filledRedGun") {
                    image.enabled = true;
                }
            } else {
                if (image.name == "filledRedGun") {
                    image.enabled = false;
                }
            }
            if (player2Weapons[i].activeInHierarchy) {
                if (image.name == "filledBlueGun") {
                    image.enabled = true;
                }
            } else {
                if (image.name == "filledBlueGun") {
                    image.enabled = false;
                }
            }

            
        }
    }
   }
}
