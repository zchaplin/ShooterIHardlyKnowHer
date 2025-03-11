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
        bulletRotation = gameObject.transform.rotation;

        base.muzzleVFX();
        //spawn bullet
        Instantiate(baseBullet, gameObject.transform.position + Vector3.forward*2, bulletRotation, transform);
    }
}
