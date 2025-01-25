using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperLaser : Weapon
{
    // Variables to define weapon behavior
    //[SerializeField] public GameObject bullet;

    public override void Start() {
        base.Start();
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Shoot()
    {
        if (player == 1) {
            bulletRotation = gameObject.transform.rotation;
        }
        else {
            bulletRotation = gameObject.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
        }

        //spawn bullet
        Instantiate(baseBullet, gameObject.transform.position + ((player==1)? Vector3.forward*22 : Vector3.back*22), bulletRotation, transform);
    }
}
