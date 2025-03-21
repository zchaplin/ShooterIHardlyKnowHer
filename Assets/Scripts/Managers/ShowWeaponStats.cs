using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShowWeaponStats : MonoBehaviour
{
    public static bool isPaused = false;
    // Start is called before the first frame update
    [SerializeField] private GameObject weaponCanvas;
    [SerializeField] private GameObject hostCanvas;
    [SerializeField] private GameObject PauseCanvas;
    [SerializeField] private TMP_Text weaponText;


    // really ineffective way of doing this, will switch to a better way later mb mb (midterm tomorrow :/)
    private List<string> weaponsNames;
    private List<string> weaponsAbilities;
    private List<string> weaponsDmg;
    // private List<string> weaponsFireRate;
    private bool firstUpdate = true;

    void Start()
    {
        weaponCanvas.SetActive(true);
        weaponsNames = new List<string> {"PeaShooter", "Bouncy", "Lightning", "Laser", "Granade", "Boomarang"};
        weaponsAbilities = new List<string> {"Regular shooting", "Reflects from objects (7 bounces)", "Hits close 3 enemies", "More damage over time", "Explosion", "Comes back"};
        // weaponsFireRate = new List<string> {"2", "2.5", "1", "0.3", "1.5"};
        weaponsDmg = new List<string> {"4", "5", "4", "1 + 5 every sec", "7"};
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            weaponCanvas.SetActive(!weaponCanvas.activeSelf);
            if (firstUpdate) {
                updateText(0);
                firstUpdate = false;
            }
        }
        // Will also display the host/join code when pressing tab, for now just putting the code here
        // if (Input.GetKeyDown(KeyCode.Alpha1)) {
        //     hostCanvas.SetActive(!hostCanvas.activeSelf);
        // }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            PauseCanvas.SetActive(!PauseCanvas.activeSelf);
            if (PauseCanvas.activeSelf) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                isPaused = true;
            } else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
                isPaused = false;
            }
            
        }
    }

    public void updateText(int weaponNum) {
        weaponText.text = $"Name: {weaponsNames[weaponNum]}\nAbility: {weaponsAbilities[weaponNum]}\nDamage: {weaponsDmg[weaponNum]}";
    }

    public void Quit() {
        Application.Quit();
    }
}
