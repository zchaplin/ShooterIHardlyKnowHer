using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Weapon : MonoBehaviour
{
    // Variables to define weapon behavior
    public float fireRate;// = 1f;
    //public float range = 100f;
    //public float damage = 50f;

    // [SerializeField] public int player; // This is now unused but kept for consistency with the existing structure.

    // OVERRIDE THIS VARIABLE TO THE PROJECTILE BEING SHOT IN WEAPON SCRIPTS
    public GameObject baseBullet;
    public int initialBullets;
    [SerializeField] private int bullets;
    [SerializeField] private bool isLimitedBullet = true;

    private float nextTimeToFire = 0f;
    protected Quaternion bulletRotation;
    [SerializeField] private TMP_Text rechargeText;

    // Reference to player camera (for crosshair)
    public Camera playerCamera;

    public virtual void Start()
    {
        bullets = initialBullets;
        // Ensure the player camera is assigned
        if (playerCamera == null)
        {
            Debug.LogError("Player camera is not assigned in the Weapon script.");
            return;
        }
        rechargeText = GameObject.Find("Canvas/WeaponStats/Manager/Recharge").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        // Check if it's time to fire
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire && !ShowWeaponStats.isPaused) // 0 = Left Mouse Button
        {
            if (isLimitedBullet && bullets > 0) {
                nextTimeToFire = Time.time + 1f / fireRate;
                bullets -= 1;
                Shoot();
            } else if (!isLimitedBullet) {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }
        rechargeText.text = "Time until next shot: " + Mathf.Max(0f, (nextTimeToFire - Time.time)).ToString("F2") + "\nBullets: " + bullets;
    }

    public virtual void Shoot()
    { 
        // Calculate the direction from the weapon to the crosshair
        Vector3 shootDirection = GetShootDirection();


        // Spawn bullet with the calculated direction
        GameObject bullet = Instantiate(baseBullet, gameObject.transform.position, Quaternion.identity);
        bullet.transform.forward = shootDirection; // Set the bullet's forward direction

        // If the bullet has a rigidbody, set its velocity
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
        if (bulletRigidbody != null)
        {
            // Check if the crosshair is aiming at something
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // If aiming at something, calculate velocity to hit the target (with gravity)
                Vector3 targetPosition = hit.point;
                Vector3 velocity = CalculateVelocityToHitTarget(bullet.transform.position, targetPosition, bulletRigidbody);
                bulletRigidbody.velocity = velocity;
            }
            else
            {
                // If not aiming at anything, shoot straight forward (no gravity)
                bulletRigidbody.velocity = shootDirection * 75f; // Adjust speed as needed
            }
        }
    }

    protected Vector3 GetShootDirection()
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
            return transform.forward;
        }
    }

    protected Vector3 CalculateVelocityToHitTarget(Vector3 origin, Vector3 target, Rigidbody rb)
    {
        // Calculate the direction to the target
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        // Calculate the time of flight based on distance and bullet speed
        float bulletSpeed = 75f; // Adjust this value to control bullet speed
        float timeOfFlight = distance / bulletSpeed;

        // Calculate the vertical velocity required to compensate for gravity
        float gravity = Physics.gravity.magnitude;
        float verticalVelocity = (target.y - origin.y) / timeOfFlight + 0.5f * gravity * timeOfFlight;

        // Combine horizontal and vertical velocities
        Vector3 velocity = direction.normalized * bulletSpeed;
        velocity.y = verticalVelocity;

        return velocity;
    }

    public virtual void RefillBullets() {
        bullets = initialBullets;
        //Debug.Log("refill: " + bullets + " name: " + gameObject.name);
    }
    public virtual bool hasBullets() {
        if (bullets >= initialBullets) {
            return true;
        }
        return false;
    }
}
