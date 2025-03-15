using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonToggle : MonoBehaviour
{
    [SerializeField] private GameObject toggleItem;
    void Start()
    {
        
    }

    public void toggleButton(){
        if(toggleItem.activeSelf == true){
            toggleItem.SetActive(false);
        } else {
            toggleItem.SetActive(true);
        }
    }
}
