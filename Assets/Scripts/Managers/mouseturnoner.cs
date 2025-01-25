using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseturnoner : MonoBehaviour
{
    public bool on;
    // Start is called before the first frame update
    void Start()
    {
        if (on) {
            Cursor.lockState = CursorLockMode.None;
        }
        else {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
