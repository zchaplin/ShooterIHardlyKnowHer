using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;


public class NetworkMoveEnemy : NetworkBehaviour
{
    public GameObject vfxPrefab;
    public bool active = false;
    public float speed = 0.5f;
    public int health = 10;
    
    // Player detection settings - can be adjusted per enemy type
    public float playerDetectionDistance = 5f;
    public bool useColliderBounds = true; // Use collider bounds to detect player proximity

    [Header("Shield")]
    public GameObject shieldVisual; 

    private HealthManager playerHealth;
    private WeaponBin weaponBin;
    private EnemySpawner enemySpawner;
    private NetworkManager networkManager;
    private bool initialRotationSet = false;
    private Collider myCollider;

    void Start()
    {
        GameObject healthManagerObject = GameObject.Find("healthManager");
        
        playerHealth = healthManagerObject.GetComponent<HealthManager>();
        weaponBin = FindObjectOfType<WeaponBin>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        networkManager = NetworkManager.Singleton;
        myCollider = GetComponent<Collider>();

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        
        // Set initial rotation based on which layer the enemy is on
        SetInitialRotation();
    }
    
    private void SetInitialRotation()
    {
        if (initialRotationSet)
            return;
            
        if (gameObject.layer == LayerMask.NameToLayer("SpawnerHost"))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Facing negative Z
        }
        else if (gameObject.layer == LayerMask.NameToLayer("SpawnerGuest"))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Facing positive Z
        }
        
        initialRotationSet = true;
    }

    void Update()
    {
        if (!initialRotationSet) {
            SetInitialRotation();
        }
        
        if (IsServer) {
            // Move the enemy forward - with proper rotations, we can use the same movement code for both
            transform.position += transform.forward * 2f * Time.deltaTime;
            
            // Check for death or player proximity
            CheckStatus();
        }
    }

    public void Activate(float speedIn)
    {
        speed = speedIn;
        active = true;
    }

    public void Deactivate() {
        speed = 0f;
        active = false;
    }
    
    // This method will be called by weapons on both client and server
    public void TakeDamage(int damage)
    {
        if (!IsServer)
        {
            TakeDamageServerRpc(damage);
            return;
        }
        
        ApplyDamage(damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        // Get this enemy's NetworkObject ID
        ulong myNetId = GetComponent<NetworkObject>().NetworkObjectId;
        
        // Process shield protection if applicable
        ShieldEnemy ownShieldComponent = GetComponent<ShieldEnemy>();
        if (ownShieldComponent == null)
        {
            // Look for nearby shield enemies
            ShieldEnemy nearestShieldEnemy = null;
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);
            foreach (var hitCollider in hitColliders)
            {
                ShieldEnemy shieldEnemy = hitCollider.GetComponent<ShieldEnemy>();
                if (shieldEnemy != null && shieldEnemy.IsShielded(myNetId))
                {
                    nearestShieldEnemy = shieldEnemy;
                    break;
                }
            }
            
            if (nearestShieldEnemy != null)
            {
                damage = nearestShieldEnemy.ProcessDamage(myNetId, damage);
                
                if (damage <= 0)
                {
                    DamageTakenClientRpc(health);
                    return;
                }
            }
        }
        
        // Apply damage and update clients
        health -= damage;
        DamageTakenClientRpc(health);
        
        // Check if enemy should die from this damage
        if (health <= 0)
        {
            DestroyEnemy();
        }
    }

    [ClientRpc]
    private void DamageTakenClientRpc(int newHealth)
    {
        health = newHealth;
    }

    // Unified check for death or player proximity
    private void CheckStatus()
    {
        if (!IsServer)
            return;
        
        // Check for death
        if (health <= 0)
        {
            DestroyEnemy();
            return;
        }
        
        // Check for player proximity
        if (IsNearPlayer())
        {
            // Player damage and despawn
            // Debug.Log("player taking damage from enemy script");
            playerHealth.playerTakeDamage(1);
            EnemyReachedPlayerClientRpc();
            
            // Despawn
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
    
    // Better player proximity detection
    private bool IsNearPlayer()
    {
        float zDistanceToPlayer = Math.Abs(transform.position.z);
        
        // If we're using collider bounds for more accurate detection
        if (useColliderBounds && myCollider != null)
        {
            // Calculate the closest point of the collider to the Z=0 plane
            float closestZ;
            
            if (transform.forward.z > 0) // Moving toward positive Z
            {
                // Get the front-most point of the collider
                closestZ = myCollider.bounds.max.z;
            }
            else // Moving toward negative Z
            {
                // Get the front-most point of the collider (which is the minimum Z in this case)
                closestZ = myCollider.bounds.min.z;
            }
            
            // Calculate absolute distance to player plane (Z=0)
            zDistanceToPlayer = Math.Abs(closestZ);
        }
        
        return zDistanceToPlayer <= playerDetectionDistance;
    }
    
    // Centralized enemy destruction method
    private void DestroyEnemy()
    {
        Debug.Log("Boom");
        // Instantiate(vfxPrefab, transform.position, Quaternion.identity);
        EnemyDrop();
        
        if (MusicManager.AudioManager != null)
        {
            MusicManager.AudioManager.goopMusic();
        }
        
        // Notify clients that this enemy is dying
        EnemyDyingClientRpc();
        
        // Despawn and destroy
        GetComponent<NetworkObject>().Despawn(true);
    }
    
    // public AudioSource source;
    // public AudioClip enemyClip;

    [ClientRpc]
    private void EnemyDyingClientRpc()
    {
        // Optional death effects
        Instantiate(vfxPrefab, transform.position, Quaternion.identity);
        // play a sound
        // source.clip = enemyClip;
        // source.Play();
    }
    
    // public AudioClip[] damageClip;
    [ClientRpc]
    private void EnemyReachedPlayerClientRpc()
    {
        // Optional player reached effects
        
    }

    public void EnemyDrop()
    {
        if (!IsServer)
            return;
            
        // 5% chance of dropping a weapon
        int randomNumber = UnityEngine.Random.Range(0, 101);
        if (randomNumber <= 5 && weaponBin != null)
        {
            int weaponNum = enemySpawner.getAvailableWeapons();
            ulong localClientId = networkManager.LocalClientId;
            weaponBin.PurchaseWeaponServerRpc(weaponNum, localClientId);
        }
    }
}