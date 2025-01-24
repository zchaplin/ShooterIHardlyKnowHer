using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Variables to define weapon behavior
    public float fireRate = 1f;
    //public float range = 100f;
    //public float damage = 50f;

    [SerializeField] public int player; // This is now unused but kept for consistency with the existing structure.

    // OVERRIDE THIS VARIABLE TO THE PROJECTILE BEING SHOT IN WEAPON SCRIPTS
    public GameObject baseBullet;

    private float nextTimeToFire = 0f;
    protected Quaternion bulletRotation;

    public virtual void Start()
    {
    }

    // Update is called once per frame
    public virtual void Update()
    {
        // Check if it's time to fire
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire) // 0 = Left Mouse Button
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    public virtual void Shoot()
    {
        // Spawn bullet
        Instantiate(baseBullet, gameObject.transform.position, gameObject.transform.rotation);
    }
}
