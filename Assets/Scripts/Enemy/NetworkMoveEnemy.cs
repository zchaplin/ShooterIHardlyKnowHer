using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Unity.Netcode;

public class NetworkMoveEnemy : NetworkBehaviour
{
    public bool active = false;
    public float speed = 0.5f;
    public int health = 10;

    [Header("Shield")]
    public GameObject shieldVisual; 

    private HealthManager playerHealth;
    private WeaponBin weaponBin;
    private EnemySpawner enemySpawner;
    private NetworkManager networkManager;


    // Start is called before the first frame update
    void Start()
    {
        GameObject healthManagerObject = GameObject.Find("healthManager");
        
        playerHealth = healthManagerObject.GetComponent<HealthManager>();
        weaponBin = FindObjectOfType<WeaponBin>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        networkManager = NetworkManager.Singleton;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(IsServer) {
            // Move the enemy forward
            if (gameObject.layer == LayerMask.NameToLayer("SpawnerHost")) {
                transform.position += transform.forward * -2f * Time.deltaTime;
            } else if (gameObject.layer == LayerMask.NameToLayer("SpawnerGuest")) {
                transform.position += transform.forward * 2f * Time.deltaTime;
            }

            checkAlive();
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
    public void TakeDamage(int x)
    {
        // If we're not the server, request the server to apply damage
        if (!IsServer)
        {
            TakeDamageServerRpc(x);
            return;
        }
        
        // Server-side damage processing
        ApplyDamage(x);
    }
    
    // Server RPC to handle damage from clients
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        ApplyDamage(damage);
    }

        private void ApplyDamage(int x)
    {
        // Original damage value for logging
        int originalDamage = x;
        
        // Check if this is a shield enemy
        ShieldEnemy ownShieldComponent = GetComponent<ShieldEnemy>();
        if (ownShieldComponent == null)
        {
            // Look for nearby shield enemies
            ShieldEnemy nearestShieldEnemy = null;
            
            // Check for shield enemies in the vicinity
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);
            foreach (var hitCollider in hitColliders)
            {
                ShieldEnemy shieldEnemy = hitCollider.GetComponent<ShieldEnemy>();
                if (shieldEnemy != null && shieldEnemy.IsShielded(GetComponent<NetworkObject>().NetworkObjectId))
                {
                    nearestShieldEnemy = shieldEnemy;
                    break;
                }
            }
            
            // If we're being shielded, process the damage through the shield
            if (nearestShieldEnemy != null)
            {
                ulong myNetId = GetComponent<NetworkObject>().NetworkObjectId;
                x = nearestShieldEnemy.ProcessDamage(myNetId, x);
                Debug.Log($"Enemy {myNetId} - Shield reduced damage from {originalDamage} to {x}");
            }
        }
        
        // Apply damage to health
        health -= x;
        
        // Visual feedback for damage (optional)
        DamageTakenClientRpc(health);
    }

    [ClientRpc]
    private void DamageTakenClientRpc(int newHealth)
    {
        // Update local health value
        health = newHealth;
        
        // Could add visual feedback here like a flash
    }

    private void checkAlive()
    {
        // This method now only runs on the server
        if (!IsServer)
            return;
            
        if (health <= 0) {
            EnemyDrop();
            MusicManager.AudioManager.goopMusic();
            
            // Tell clients this enemy is dying for any visual effects
            EnemyDyingClientRpc();
            
            // Despawn and destroy (server-only)
            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn(true);
        }
        if (Math.Abs(transform.position.z) <= 5) {
            //Debug.Log("Enemy reached the player" + playerHealth);
            
            // Tell clients this enemy reached the player
            EnemyReachedPlayerClientRpc();
            
            // Despawn (server-only)
            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn(true);
            
            //take dmg
            playerHealth.playerTakeDamage(1);
        }
    }
    
    [ClientRpc]
    private void EnemyDyingClientRpc()
    {
        // Client-side death effects could go here
        // No need to destroy the object as the server will handle that through despawning
    }
    
    [ClientRpc]
    private void EnemyReachedPlayerClientRpc()
    {
        // Client-side effects for enemy reaching player could go here
    }

    public void EnemyDrop() {
        // This should only run on the server
        if (!IsServer)
            return;
            
        // Chance of getting something
        int randomNumber = UnityEngine.Random.Range(0, 101);
        // if more than 5 percentage
        if (randomNumber <= 5) {
            // get wave number from waves to see which weapons are unlocked at the moment
            int weaponNum = enemySpawner.getAvailableWeapons();
            
            // Since we're on the server, we can call directly
            if (weaponBin != null)
            {
                ulong localClientId = networkManager.LocalClientId;
                weaponBin.PurchaseWeaponServerRpc(weaponNum, localClientId);
            }
        }
    }
}
