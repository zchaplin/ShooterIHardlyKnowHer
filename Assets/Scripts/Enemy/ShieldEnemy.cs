using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShieldEnemy : NetworkBehaviour
{
    [Header("Shield Properties")]
    public float shieldRadius = 5f;
    public int damageReduction = 15; // Damage reduced per hit
    public int shieldHealth = 15;    // Total damage a shield can absorb before breaking
    
    [Header("Visual")]
    public GameObject shieldVisual; // Assign this in inspector - a child object with a sphere mesh
    
    // Track enemies in shield radius and their shield health
    private Dictionary<ulong, int> shieldedEnemies = new Dictionary<ulong, int>();
    
    void OnEnable()
    {
        // Ensure shield visual is enabled
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Start checking for enemies in range
            StartCoroutine(CheckForEnemiesInRange());
        }
    }
    
    private IEnumerator CheckForEnemiesInRange()
    {
        while (isActiveAndEnabled)
        {
            // Find all enemies in range with the "Enemy" tag
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, shieldRadius);
            
            // Track currently shielded enemies to find those that left
            HashSet<ulong> currentEnemies = new HashSet<ulong>();
            
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy") && hitCollider.gameObject != gameObject)
                {
                    NetworkObject enemyObj = hitCollider.GetComponent<NetworkObject>();
                    if (enemyObj != null)
                    {
                        ulong enemyId = enemyObj.NetworkObjectId;
                        currentEnemies.Add(enemyId);
                        
                        // Add new enemies to shielded list
                        if (!shieldedEnemies.ContainsKey(enemyId))
                        {
                            shieldedEnemies[enemyId] = shieldHealth; // Set initial shield health
                            EnableEnemyShield_ClientRpc(enemyId);
                            Debug.Log($"Added shield to enemy {enemyId} with health {shieldHealth}");
                        }
                    }
                }
            }
            
            // Find enemies that left the shield radius
            List<ulong> leftEnemies = new List<ulong>();
            foreach (var enemyId in shieldedEnemies.Keys)
            {
                if (!currentEnemies.Contains(enemyId))
                {
                    leftEnemies.Add(enemyId);
                }
            }
            
            // Remove shields from enemies that left
            foreach (var enemyId in leftEnemies)
            {
                RemoveShield(enemyId);
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void RemoveShield(ulong enemyId)
    {
        if (shieldedEnemies.ContainsKey(enemyId))
        {
            shieldedEnemies.Remove(enemyId);
            DisableEnemyShield_ClientRpc(enemyId);
            Debug.Log($"Removed shield from enemy {enemyId}");
        }
    }
    
    [ClientRpc]
    private void EnableEnemyShield_ClientRpc(ulong enemyNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyObj))
        {
            // Find the "Shield" child object on this enemy
            Transform shieldTransform = enemyObj.transform.Find("Shield");
            if (shieldTransform != null)
            {
                shieldTransform.gameObject.SetActive(true);
            }
        }
    }
    
    [ClientRpc]
    private void DisableEnemyShield_ClientRpc(ulong enemyNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyObj))
        {
            // Find the "Shield" child object on this enemy
            Transform shieldTransform = enemyObj.transform.Find("Shield");
            if (shieldTransform != null)
            {
                shieldTransform.gameObject.SetActive(false);
            }
        }
    }
    
    // Called by NetworkMoveEnemy to check if it's shielded
    public bool IsShielded(ulong enemyNetId)
    {
        return shieldedEnemies.ContainsKey(enemyNetId);
    }
    
    // Called by NetworkMoveEnemy to reduce damage - returns the actual damage to apply
    public int ProcessDamage(ulong enemyNetId, int damage)
    {
        if (!shieldedEnemies.ContainsKey(enemyNetId))
            return damage;
            
        int remainingShieldHealth = shieldedEnemies[enemyNetId];
        int damageToAbsorb = Mathf.Min(damageReduction, damage);
        
        // Calculate how much damage the shield can absorb
        damageToAbsorb = Mathf.Min(damageToAbsorb, remainingShieldHealth);
        
        // Reduce shield health
        remainingShieldHealth -= damageToAbsorb;
        shieldedEnemies[enemyNetId] = remainingShieldHealth;
        
        // Calculate remaining damage to pass through
        int remainingDamage = damage - damageToAbsorb;
        
        // Show shield hit effect
        ShowShieldHitEffect_ClientRpc(enemyNetId);
        
        // If shield is depleted, remove it
        if (remainingShieldHealth <= 0)
        {
            RemoveShield(enemyNetId);
            Debug.Log($"Shield depleted for enemy {enemyNetId}");
        }
        else
        {
            Debug.Log($"Shield for enemy {enemyNetId} absorbed {damageToAbsorb} damage, {remainingShieldHealth} shield health remaining");
        }
        
        return remainingDamage;
    }
    
    [ClientRpc]
    private void ShowShieldHitEffect_ClientRpc(ulong enemyNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyObj))
        {
            // Find the "Shield" child object
            Transform shieldTransform = enemyObj.transform.Find("Shield");
            if (shieldTransform != null && shieldTransform.gameObject.activeSelf)
            {
                // Flash shield
                StartCoroutine(FlashShield(shieldTransform.gameObject));
            }
        }
    }
    
    private IEnumerator FlashShield(GameObject shield)
    {
        if (shield != null)
        {
            // Store original material and color
            Renderer renderer = shield.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                
                // Set to bright flash color
                Color flashColor = new Color(1f, 1f, 1f, originalColor.a);
                renderer.material.color = flashColor;
                
                // Wait briefly
                yield return new WaitForSeconds(0.1f);
                
                // Return to original color
                renderer.material.color = originalColor;
            }
        }
    }
    
    // When this enemy is destroyed, tell all clients to disable shields
    public override void OnDestroy()
    {
        if (IsServer)
        {
            DisableAllShields_ClientRpc();
        }
    }
    
    [ClientRpc]
    private void DisableAllShields_ClientRpc()
    {
        // Find all objects with Enemy tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            // Find and disable shield child
            Transform shieldTransform = enemy.transform.Find("Shield");
            if (shieldTransform != null)
            {
                shieldTransform.gameObject.SetActive(false);
            }
        }
    }
    
    // Show shield radius in editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shieldRadius);
    }
    
    // Show shield radius when selected (more visible)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, shieldRadius);
    }
}