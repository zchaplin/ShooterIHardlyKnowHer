using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser_Proj : Proj_Move
{
    public override void Start()
    {
        base.Start();
        damage = 1;
        StartCoroutine(increaseDamage());
    }

    IEnumerator increaseDamage() {
        while (true) {
            damage += 5;
            yield return new WaitForSeconds(0.5f);
        }  
    }
}
