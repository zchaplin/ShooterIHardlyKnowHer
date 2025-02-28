using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomarang : Weapon
{
    // Variables to define weapon behavior
    [SerializeField] private float maxPoint = 50;
    private bool isShot;
    private GameObject bullet;
    private Vector3 startPosition;
    private bool isReturning;

    public override void Start() {
        base.Start();
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (isShot && bullet) {
            if (Vector3.Distance(bullet.transform.position, startPosition) >= maxPoint) {
                Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
                bulletRigidbody.velocity = Vector3.zero;
                Vector3 velocity = CalculateVelocityToHitTarget(bullet.transform.position, gameObject.transform.position, bulletRigidbody);
                bulletRigidbody.velocity = velocity;
                isReturning = true;
                isShot = false;
                
            }
            
        }
        if (isReturning && bullet && Vector3.Distance(bullet.transform.position, gameObject.transform.position) < 1f) {
            Destroy(bullet);
            isReturning = false;
        }
    }

    public override void Shoot()
    {
        isShot = true;
        // Calculate the direction from the weapon to the crosshair
        Vector3 shootDirection = GetShootDirection();

        // Spawn bullet with the calculated direction
        bullet = Instantiate(baseBullet, gameObject.transform.position, Quaternion.identity);
        bullet.transform.forward = shootDirection; // Set the bullet's forward direction
        startPosition = bullet.transform.position;
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
}