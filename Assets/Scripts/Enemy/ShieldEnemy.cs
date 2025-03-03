using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShieldEnemy : NetworkBehaviour
{
    public float shieldRadius = 5f;
    public int damageReduction = 5;
    public GameObject shieldVisualPrefab;
    private GameObject mainShieldVisual;
    
    // Dictionary to keep track of enemy NetworkObjectIds and their shield GameObjects
    private Dictionary<ulong, GameObject> shieldedEnemies = new Dictionary<ulong, GameObject>();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Create the main shield visual around this enemy
            SpawnMainShield();
            
            // Start checking for enemies in range
            StartCoroutine(CheckForEnemiesInRange());
        }
    }
    
    private void SpawnMainShield()
    {
        if (shieldVisualPrefab != null)
        {
            // Create the shield at our exact position
            mainShieldVisual = Instantiate(shieldVisualPrefab, transform.position, Quaternion.identity);
            mainShieldVisual.transform.parent = transform;
            mainShieldVisual.transform.localPosition = Vector3.zero;
            mainShieldVisual.transform.localScale = new Vector3(shieldRadius * 2, shieldRadius * 2, shieldRadius * 2);
            
            // Spawn on network
            NetworkObject shieldNetObj = mainShieldVisual.GetComponent<NetworkObject>();
            if (shieldNetObj != null)
            {
                shieldNetObj.Spawn();
            }
        }
    }
    
    private IEnumerator CheckForEnemiesInRange()
    {
        while (IsServer && isActiveAndEnabled)
        {
            // Store current NetworkObjectIds to check which ones left the radius
            HashSet<ulong> currentIds = new HashSet<ulong>(shieldedEnemies.Keys);
            
            // Find all enemies in range with the "Enemy" tag
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, shieldRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy") && hitCollider.gameObject != gameObject)
                {
                    NetworkObject enemyNetObj = hitCollider.GetComponent<NetworkObject>();
                    if (enemyNetObj != null)
                    {
                        ulong enemyNetId = enemyNetObj.NetworkObjectId;
                        
                        if (!shieldedEnemies.ContainsKey(enemyNetId))
                        {
                            // New enemy entered the radius
                            SpawnShieldAroundEnemy(enemyNetObj);
                        }
                        else
                        {
                            // Enemy was already in radius, update shield position just to be safe
                            UpdateShieldPosition(enemyNetId);
                            
                            // Remove from current set so we don't remove it later
                            currentIds.Remove(enemyNetId);
                        }
                    }
                }
            }
            
            // Any enemies left in currentIds have left the radius
            foreach (var enemyNetId in currentIds)
            {
                RemoveShieldFromEnemy(enemyNetId);
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void SpawnShieldAroundEnemy(NetworkObject enemyNetObj)
    {
        // Create shield at the exact position of the enemy
        GameObject enemyShield = Instantiate(shieldVisualPrefab, enemyNetObj.transform.position, Quaternion.identity);
        enemyShield.name = "EnemyShield";
        
        // Scale the shield to be slightly larger than the enemy
        enemyShield.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        // Create a NetworkObject for the shield
        NetworkObject shieldNetObj = enemyShield.GetComponent<NetworkObject>();
        if (shieldNetObj != null)
        {
            // Spawn it on the network
            shieldNetObj.Spawn();
            
            // Store the shield in our dictionary
            shieldedEnemies[enemyNetObj.NetworkObjectId] = enemyShield;
            
            // Tell clients to parent the shield to the enemy
            ParentShieldToEnemy_ClientRpc(enemyNetObj.NetworkObjectId, shieldNetObj.NetworkObjectId);
        }
    }
    
    private void UpdateShieldPosition(ulong enemyNetId)
    {
        if (shieldedEnemies.TryGetValue(enemyNetId, out GameObject shield) && 
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyObj))
        {
            // Update shield position on server
            shield.transform.position = enemyObj.transform.position;
        }
    }
    
    private void RemoveShieldFromEnemy(ulong enemyNetId)
    {
        if (shieldedEnemies.TryGetValue(enemyNetId, out GameObject shield))
        {
            // Despawn the shield
            NetworkObject shieldNetObj = shield.GetComponent<NetworkObject>();
            if (shieldNetObj != null)
            {
                shieldNetObj.Despawn(true);
            }
            
            // Remove from dictionary
            shieldedEnemies.Remove(enemyNetId);
        }
    }
    
    [ClientRpc]
    private void ParentShieldToEnemy_ClientRpc(ulong enemyNetId, ulong shieldNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyObj) &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(shieldNetId, out NetworkObject shieldObj))
        {
            // Parent the shield to the enemy
            shieldObj.transform.SetParent(enemyObj.transform);
            shieldObj.transform.localPosition = Vector3.zero;
        }
    }
    
    // Called from NetworkMoveEnemy to check if enemy is shielded
    public bool IsEnemyShielded(ulong enemyNetId)
    {
        return shieldedEnemies.ContainsKey(enemyNetId);
    }
    
    // Called from NetworkMoveEnemy when an enemy in the shield radius takes damage
    public int ReduceDamage(int damage)
    {
        // Reduce damage by the damage reduction amount, minimum damage is 1
        return Mathf.Max(1, damage - damageReduction);
    }
    
    public override void OnNetworkDespawn()
    {
        // Cleanup all created shields when this enemy is despawned
        if (IsServer)
        {
            foreach (var shield in shieldedEnemies.Values)
            {
                if (shield != null)
                {
                    NetworkObject shieldNetObj = shield.GetComponent<NetworkObject>();
                    if (shieldNetObj != null)
                    {
                        shieldNetObj.Despawn(true);
                    }
                }
            }
            
            shieldedEnemies.Clear();
        }
    }
    
    public override void OnDestroy()
    {
        // Additional cleanup in case OnNetworkDespawn wasn't called
        StopAllCoroutines();
    }
    
    // Show shield radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shieldRadius);
    }
}