using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEngine;

// SCRIPT NO LONGER NEEDED. INGORE IGNORE IGNORE
public class Proj_Shoot : MonoBehaviour
{

    public Transform proj_point;
    public GameObject proj_defualt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Shoot(proj_defualt);
        }
    }

    // Shoot()1 spawns a projectile at the Proj_point with its default velocity and acceleration
    public void Shoot(GameObject projectile) {
        if (projectile == null) return;

        // spawn the projectile and move it to the right position.
        GameObject proj = Instantiate(projectile);
        proj.transform.position = proj_point.position;
        proj.transform.rotation = transform.rotation;
    }

    // Shoot()3 spawns a projectile at the Proj_point while overriding its starting velocity and acceleration
    public void Shoot(GameObject projectile, Vector3 vel_start, Vector3 accel) {

    }
    
}
