using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShieldEnemy : NetworkBehaviour
{
    [Header("Shield Properties")]
    public float shieldRadius = 5f;
    public int shieldHealth = 15; // Shield absorbs this much damage before breaking
    
    [Header("Visual")]
    public GameObject shieldVisual; // Assign this in inspector - a child object with a sphere mesh
    
    // Dictionary to track enemies and their remaining shield health
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
            // Store current enemies to check which ones left the radius
            HashSet<ulong> currentEnemies = new HashSet<ulong>(shieldedEnemies.Keys);
            
            // Find all enemies in range with the "Enemy" tag
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, shieldRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy") && hitCollider.gameObject != gameObject)
                {
                    NetworkObject enemyObj = hitCollider.GetComponent<NetworkObject>();
                    if (enemyObj != null)
                    {
                        ulong enemyNetId = enemyObj.NetworkObjectId;
                        
                        if (!shieldedEnemies.ContainsKey(enemyNetId))
                        {
                            // New enemy entered the radius
                            shieldedEnemies[enemyNetId] = shieldHealth;
                            EnableEnemyShield_ClientRpc(enemyNetId);
                        }
                        else
                        {
                            // Enemy was already in radius
                            currentEnemies.Remove(enemyNetId);
                        }
                    }
                }
            }
            
            // Any enemies left in currentEnemies have left the radius
            foreach (var enemyNetId in currentEnemies)
            {
                RemoveShieldFromEnemy(enemyNetId);
            }
            
            yield return new WaitForSeconds(0.5f);
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
    
    private void RemoveShieldFromEnemy(ulong enemyNetId)
    {
        if (shieldedEnemies.ContainsKey(enemyNetId))
        {
            shieldedEnemies.Remove(enemyNetId);
            DisableEnemyShield_ClientRpc(enemyNetId);
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
    
    // Called by NetworkMoveEnemy when a shielded enemy takes damage
    public int ProcessDamage(ulong enemyNetId, int damage)
    {
        // If enemy isn't shielded, return full damage
        if (!shieldedEnemies.ContainsKey(enemyNetId))
        {
            return damage;
        }
        
        int remainingShieldHealth = shieldedEnemies[enemyNetId];
        
        // If damage is less than remaining shield, absorb it all
        if (damage <= remainingShieldHealth)
        {
            // Update remaining shield health
            shieldedEnemies[enemyNetId] = remainingShieldHealth - damage;
            
            // Show shield hit effect
            ShowShieldHitEffect_ClientRpc(enemyNetId);
            
            Debug.Log($"Shield absorbed all {damage} damage. Remaining shield: {shieldedEnemies[enemyNetId]}");
            
            // No damage passes through
            return 0;
        }
        else
        {
            // Shield breaks, calculate remaining damage
            int remainingDamage = damage - remainingShieldHealth;
            
            // Remove shield
            RemoveShieldFromEnemy(enemyNetId);
            
            // Show shield break effect
            ShieldBrokenEffect_ClientRpc(enemyNetId);
            
            Debug.Log($"Shield broke! Absorbed {remainingShieldHealth} damage. {remainingDamage} damage passes through.");
            
            // Return remaining damage that passes through
            return remainingDamage;
        }
    }
    
    [ClientRpc]
    private void ShowShieldHitEffect_ClientRpc(ulong enemyNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyObj))
        {
            Transform shieldTransform = enemyObj.transform.Find("Shield");
            if (shieldTransform != null)
            {
                // Flash the shield
                StartCoroutine(FlashShield(shieldTransform.gameObject));
            }
        }
    }
    
    [ClientRpc]
    private void ShieldBrokenEffect_ClientRpc(ulong enemyNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyObj))
        {
            Transform shieldTransform = enemyObj.transform.Find("Shield");
            if (shieldTransform != null)
            {
                // Show break effect before disabling
                StartCoroutine(ShieldBreakEffect(shieldTransform.gameObject));
            }
        }
    }
    
    private IEnumerator FlashShield(GameObject shield)
    {
        Renderer renderer = shield.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            
            // Flash white
            Color flashColor = new Color(1f, 1f, 1f, originalColor.a);
            renderer.material.color = flashColor;
            
            yield return new WaitForSeconds(0.1f);
            
            // Return to original color
            renderer.material.color = originalColor;
        }
    }
    
    private IEnumerator ShieldBreakEffect(GameObject shield)
    {
        Renderer renderer = shield.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            
            // Flash red
            renderer.material.color = new Color(1f, 0f, 0f, originalColor.a);
            
            // Quickly shrink
            Vector3 originalScale = shield.transform.localScale;
            float duration = 0.3f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                shield.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                
                // Also fade out
                Color fadeColor = renderer.material.color;
                fadeColor.a = Mathf.Lerp(originalColor.a, 0f, t);
                renderer.material.color = fadeColor;
                
                yield return null;
            }
            
            // Disable the shield
            shield.SetActive(false);
            
            // Reset scale and color for next time
            shield.transform.localScale = originalScale;
            renderer.material.color = originalColor;
        }
        else
        {
            // If no renderer, just disable
            shield.SetActive(false);
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