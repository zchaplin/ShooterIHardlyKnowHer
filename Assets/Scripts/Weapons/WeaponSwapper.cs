using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwapper : MonoBehaviour
{
    [SerializeField] public GameObject peaShooter1;
    [SerializeField] public GameObject peaShooter2;
    [SerializeField] public GameObject launcher1;
    [SerializeField] public GameObject launcher2;
    // Start is called before the first frame update
    void Start()
    {
        launcher1.SetActive(true);
        launcher2.SetActive(false);
        peaShooter1.SetActive(false);
        peaShooter2.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (launcher1.activeInHierarchy) {
                peaShooter1.SetActive(true);
                peaShooter2.SetActive(false);
                launcher1.SetActive(false);
                launcher2.SetActive(true);
            }
            else {
                peaShooter1.SetActive(false);
                peaShooter2.SetActive(true);
                launcher1.SetActive(true);
                launcher2.SetActive(false);
            }
        }
        
    }
}
