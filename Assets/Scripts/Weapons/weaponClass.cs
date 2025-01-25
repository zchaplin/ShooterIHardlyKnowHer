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

    // Reference to player camera (for crosshair)
    [SerializeField] private Camera playerCamera;

    public virtual void Start()
    {
        // Ensure the player camera is assigned
        if (playerCamera == null)
        {
            Debug.LogError("Player camera is not assigned in the Weapon script.");
            return;
        }
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
        // Calculate the direction from the weapon to the crosshair
        Vector3 shootDirection = GetShootDirection();

        // Debug.Log("Shoot direction: " + shootDirection);

        // Spawn bullet with the calculated direction
        GameObject bullet = Instantiate(baseBullet, gameObject.transform.position, Quaternion.identity);
        bullet.transform.forward = shootDirection; // Set the bullet's forward direction

        // If the bullet has a rigidbody, set its velocity
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
        if (bulletRigidbody != null)
        {
            bulletRigidbody.velocity = shootDirection * 75f; // Adjust speed as needed
        }
    }

    private Vector3 GetShootDirection()
    {
        // Create a ray from the camera through the center of the screen (crosshair)
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen
        RaycastHit hit;

        // If the ray hits something, shoot toward the hit point
        if (Physics.Raycast(ray, out hit))
        {
            return (hit.point - gameObject.transform.position).normalized;
        }
        // If the ray doesn't hit anything, shoot in the camera's forward direction
        else
        {
            return playerCamera.transform.forward;
        }
    }
}
