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
        }
    }


     void FixedUpdate()
    {
        checkAlive();
        
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

    public void TakeDamage(int x)
    {
        // Check if this is a regular enemy (not a shield enemy itself)
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
                if (shieldEnemy != null && shieldEnemy.IsEnemyShielded(GetComponent<NetworkObject>().NetworkObjectId))
                {
                    nearestShieldEnemy = shieldEnemy;
                    break;
                }
            }
            
            // If we're being shielded, reduce the damage
            if (nearestShieldEnemy != null)
            {
                x = nearestShieldEnemy.ReduceDamage(x);
            }
        }
        
        // Apply damage
        health -= x;
        checkAlive();
    }

    private void checkAlive(){
        if (health <= 0) {
            EnemyDrop();
            MusicManager.AudioManager.goopMusic();

            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn(true);
            Destroy(gameObject);
        }
        if (Math.Abs(transform.position.z) <= 5) {
            //Debug.Log("Enemy reached the player" + playerHealth);
            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn(true); 
            Destroy(gameObject); // Destroy the GameObject
            //take dmg
            playerHealth.playerTakeDamage(1);
        }
    }

    public void EnemyDrop() {
        // Chance of getting something
        int randomNumber = UnityEngine.Random.Range(0, 101);
        // if more than 5 percentage
        if (randomNumber <= 5) {
            // get wave number from waves to see which weapons are unlocked at the moment
            int weaponNum = enemySpawner.getAvailableWeapons();
            // Request server to spawn the weapon (handle both host and client case)
            if (networkManager != null && networkManager.IsClient)
            {
                // Find local player to get client ID
                ulong localClientId = networkManager.LocalClientId;
                // Debug.Log($"Local client ID: {localClientId}, requesting purchase from server");
                if (weaponBin != null)
                {
                    // Call server RPC to purchase the weapon
                    weaponBin.PurchaseWeaponServerRpc(weaponNum, localClientId);
                }
            }
        }
    }
}
