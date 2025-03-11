using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Weapon : MonoBehaviour
{
    // Variables to define weapon behavior
    public float fireRate;
    public float bulletSpeed = 75f; // Made this a configurable property
    
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

    public GameObject muzzleFlash;
    public Transform muzzlePosition;
    public virtual void Start()
    {
        bullets = initialBullets;
        // Ensure the player camera is assigned
        if (playerCamera == null)
        {
            Debug.LogError("Player camera is not assigned in the Weapon script.");
            return;
        }
        rechargeText = GameObject.Find("Canvas/WeaponStats/Manager/ammo/Recharge").GetComponent<TMP_Text>();
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
        if (gameObject.name == "peaShooter") {
            rechargeText.text = "inf";
        } else {
            rechargeText.text = "" + bullets;
        }
    }

    // Add back the GetShootDirection method for boomerang and other weapons
    protected Vector3 GetShootDirection()
    {
        // Create a ray from the camera through the center of the screen (crosshair)
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen
        RaycastHit hit;

        // If the ray hits something, shoot toward the hit point
        if (Physics.Raycast(ray, out hit))
        {
            return (hit.point - transform.position).normalized;
        }
        // If the ray doesn't hit anything, shoot in the camera's forward direction
        else
        {
            return playerCamera.transform.forward;
        }
    }
    
    public virtual void Shoot()
    { 
        // Get the shooting direction
        Vector3 shootDirection = GetShootDirection();
        Vector3? targetPoint = null;

        // Create a ray to find the target point
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // If the ray hits something, store the target point
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }

        muzzleVFX();
        
        // Calculate proper rotation for the bullet
        Quaternion bulletRotation = Quaternion.LookRotation(shootDirection);
        
        // Spawn bullet with the calculated rotation
        GameObject bullet = Instantiate(baseBullet, transform.position, bulletRotation);
        
        // Get bullet's rigidbody
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
        if (bulletRigidbody != null)
        {
            if (targetPoint.HasValue)
            {
                // If we have a target point, calculate a trajectory to hit it
                Vector3 velocity = CalculateVelocityToHitTarget(bullet.transform.position, targetPoint.Value, bulletRigidbody);
                bulletRigidbody.velocity = velocity;
            }
            else
            {
                // Otherwise shoot in a straight line
                bulletRigidbody.velocity = shootDirection * bulletSpeed;
            }
            
            // Debug info to help diagnose shooting issues
            Debug.DrawRay(transform.position, shootDirection * 10f, Color.red, 1f);
        }
    }

    public virtual void muzzleVFX(){
        if(muzzleFlash != null){
            GameObject Flash = Instantiate(muzzleFlash, muzzlePosition);
            Destroy(Flash, 0.1f);
        }
    }
    protected Vector3 CalculateVelocityToHitTarget(Vector3 origin, Vector3 target, Rigidbody rb)
    {
        // Calculate the direction to the target
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        // Calculate time of flight
        float timeOfFlight = distance / bulletSpeed;

        // Calculate the vertical velocity required to compensate for gravity
        float gravity = Physics.gravity.magnitude;
        float verticalVelocity = (target.y - origin.y) / timeOfFlight + 0.5f * gravity * timeOfFlight;

        // Combine horizontal and vertical velocities
        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
        Vector3 horizontalVelocity = horizontalDirection * bulletSpeed * (direction.magnitude / new Vector3(direction.x, 0, direction.z).magnitude);
        
        // Combine for final velocity
        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;

        return velocity;
    }

    public virtual void RefillBullets() {
        bullets = initialBullets;
    }
    
    public virtual bool hasBullets() {
        if (bullets >= initialBullets) {
            return true;
        }
        return false;
    }
}