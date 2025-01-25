using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser_Proj : Proj_Move
{
    public override void Start()
    {
        base.Start();
    }
    private void OnTriggerEnter(Collider other) {
        damageEntity(other.gameObject);
    }
}
