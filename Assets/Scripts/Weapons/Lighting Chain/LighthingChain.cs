using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LighthingChain : Weapon
{

    public int maxEnemies = 3;
    private Vector3 initialForward;

     public override void Start() {
        base.Start();
        initialForward = new Vector3(0.0f, 0.0f, 1.0f);
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Shoot()
    {        
        // Calculate the direction from the weapon to the crosshair
        Vector3 shootDirection = GetShootDirection();
        
        base.muzzleVFX();

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
                if (hit.collider.CompareTag("Enemy")||hit.collider.CompareTag("Player")) { // w/o it hitting walls will aslo hit enemeis
                    // If aiming at something, calculate velocity to hit the target (with gravity)
                    Vector3 targetPosition = hit.point;
                    FindClosetEnemies(hit.point);
                    Vector3 velocity = CalculateVelocityToHitTarget(bullet.transform.position, targetPosition, bulletRigidbody);
                    bulletRigidbody.velocity = velocity;
                }
            }
            else
            {
                // If not aiming at anything, shoot straight forward (no gravity)
                bulletRigidbody.velocity = shootDirection * 75f; // Adjust speed as needed
            }
        }

    }


    new protected Vector3 GetShootDirection()
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

    public Material lineMaterial; // Will be used to connect hit enemies 
    public Color lineColor = Color.blue;
    public float lineWidth = 0.05f; 
    private float fieldOfView = 180f; // since each player only sees in 180 degrees
    private void FindClosetEnemies(Vector3 hitPoint) {
        // Look for only colliders in a certain radius from the hit
        float searchRadius = 20f;
         
        Collider[] colliders = Physics.OverlapSphere(hitPoint, searchRadius);

        // Filter only enemies, then sort by distance
        GameObject[] closestEnemies = colliders
            .Select(c => c.gameObject)
            .Where(c => c.CompareTag("Enemy")) 
            .Concat(GameObject.FindGameObjectsWithTag("Player")) 
            .OrderBy(enemy => Vector3.Distance(hitPoint, enemy.transform.position)) // Sort by distance
            .Take(maxEnemies-1)
            .Where(enemy => IsEnemyInFieldOfView(enemy))
            .ToArray();
        
        if (closestEnemies.Length == 0) return; // No enemies

       StartCoroutine(SetUpLineRenderer(closestEnemies, hitPoint));
    }
    
    private bool IsEnemyInFieldOfView(GameObject enemy)
    {
        // Calculate the direction from the player to the enemy
        Vector3 directionToEnemy = enemy.transform.position - gameObject.transform.position;

        // Calculate the angle between the player's forward direction and the direction to the enemy
        float angle = Vector3.Angle(initialForward, directionToEnemy);

        // Check if the enemy is within the player's field of view
        return angle < fieldOfView / 2f;
    }

    IEnumerator SetUpLineRenderer(GameObject[] closestEnemies, Vector3 hitPoint) {
        GameObject lineObj = new GameObject("EnemyLineRenderer");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = closestEnemies.Length + 1;

        // Connect line reneder between all enemies
        lineRenderer.SetPosition(0, gameObject.transform.position);
        for (int i = 0; i < closestEnemies.Length; i++)
        {
            if (closestEnemies[i] != null)
            {
                NetworkMoveEnemy enemy = closestEnemies[i].GetComponent<NetworkMoveEnemy>();
                enemy.Deactivate();
                lineRenderer.SetPosition(i + 1, closestEnemies[i].transform.position);
            }
        }

        yield return new WaitForSeconds(1f);
        Destroy(lineRenderer);
        for (int i = 0; i < closestEnemies.Length; i++) {
            Destroy(closestEnemies[i]);
        }

    }

    new protected Vector3 CalculateVelocityToHitTarget(Vector3 origin, Vector3 target, Rigidbody rb)
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
    // Idea: if projectile hits enemy, check the numebr of gameobjects with the enwmy tag and imapct x of them 
}
