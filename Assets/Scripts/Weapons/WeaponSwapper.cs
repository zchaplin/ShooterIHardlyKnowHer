using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwapper : MonoBehaviour
{
    [SerializeField] public GameObject peaShooter;
    [SerializeField] public GameObject launcher;
    // Start is called before the first frame update
    void Start()
    {
        launcher.SetActive(true);
        peaShooter.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (launcher.activeInHierarchy) {
                peaShooter.SetActive(true);
                launcher.SetActive(false);
            }
            else {
                peaShooter.SetActive(false);
                launcher.SetActive(true);
            }
        }
        
    }
}
