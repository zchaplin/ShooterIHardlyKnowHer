using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class GunUI : NetworkBehaviour
{
    // public GameObject weaponsParent;
    
    public Color notSelectedColor = Color.black;
    public Color availableColor = Color.gray;
    public Color selectedColor = Color.white;

    public PlayerInventory playerInventory;

    // Start is called before the first frame update

    public void getInventory()
{
    if (playerInventory == null)
    {
        foreach (var player in FindObjectsOfType<PlayerInventory>())
        {
            if (player.IsOwner) // Only get the inventory of the local player
            {
                playerInventory = player;
                break;
            }
        }
    }
}

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeSelectedGunColor(int childIndex)
    {
        getInventory();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Image childImage = child.GetChild(0).GetComponent<Image>();
            TMP_Text childText = child.GetChild(1).GetComponent<TMP_Text>();
            Debug.Log("childIndex: " + childIndex + " i: " + i + "child anme: " + child.name + "childImage: " + childImage + "childText: " + childText); 

            if (playerInventory.ownedWeapons.Contains(i)) {
                childImage.color = notSelectedColor;
                childText.color = notSelectedColor;
            } else {
                childImage.color = availableColor;
                childText.color = availableColor;
            }
            if (i == childIndex) {
                childImage.color = Color.white;
                childText.color = Color.white;
            }
        }
    }

    public void ChangeAvailableGunColor(int childIndex)
    {
        getInventory();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Image childImage = child.GetChild(0).GetComponent<Image>();
            TMP_Text childText = child.GetChild(1).GetComponent<TMP_Text>();
            //Debug.Log("childIndex: " + childIndex + " i: " + i + "child anme: " + child.name + "childImage: " + childImage + "childText: " + childText); 
            //Debug.Log("playerInventory" + playerInventory);
            //Debug.Log("playerInventory.ownedWeapons:" + playerInventory.ownedWeapons);
            if (playerInventory.ownedWeapons.Contains(i)) {
                childImage.color = notSelectedColor;
                childText.color = notSelectedColor;
            } else {
                childImage.color = availableColor;
                childText.color = availableColor;
            }
        }
    }
}
