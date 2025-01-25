using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive_Proj : Proj_Move
{
    public float expl_radius = 5;
    private SphereCollider sc;
    private ParticleSystem ps;
    public override void Start()
    {
        base.Start();
        sc = gameObject.GetComponent<SphereCollider>();
        sc.radius = 0.3f;
        StartCoroutine("explode");
    }
    private void OnTriggerEnter(Collider other) {
        damageEntity(other.gameObject);
    }
    private IEnumerator explode()
    {
        while (sc.radius < expl_radius) {

            sc.radius += 0.2f;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("explody go by bye");
        yield return new WaitForFixedUpdate();
        Destroy(gameObject);
    }
}
