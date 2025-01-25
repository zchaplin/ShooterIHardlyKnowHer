using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grenade_Proj : Proj_Move
{
    public GameObject explosion;

    public override void damageEntity(GameObject other) {
        GameObject newExplosion = Instantiate(explosion);
        newExplosion.transform.position = transform.position;
        Debug.Log("exploding!!!");
        Destroy(gameObject);
    }
}
