using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class peaShooter : Weapon
{
    // Variables to define weapon behavior
    //[SerializeField] public GameObject bullet;

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Shoot()
    {
        base.Shoot();
        if (gameObject.name == "peaShooter" && MusicManager.AudioManager != null)
        {
            MusicManager.AudioManager.basicGun();
        }

        if (gameObject.name == "Bouncy" && MusicManager.AudioManager != null)
        {
            MusicManager.AudioManager.bouncyGun();
        }
    }
}

