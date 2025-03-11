using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ShieldEnemy : NetworkBehaviour
{
    [Header("Shield Properties")]
    public float shieldRadius = 5f;
    public int shieldStrength = 15; // How much damage the shield absorbs before breaking
    
    [Header("Visual")]
    public GameObject shieldVisual; // The visual effect of the shield generator
    
    // Server-side list of currently shielded enemies
    private HashSet<ulong> shieldedEnemies = new HashSet<ulong>();
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Make sure shield visual is visible
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
        
        if (IsServer)
        {
            // Only the server manages shield detection
            StartCoroutine(UpdateShieldedEnemies());
        }
    }
    
    private IEnumerator UpdateShieldedEnemies()
    {
        while (true)
        {
            // Clear current list
            // shieldedEnemies.Clear();
            
            // Find all enemies in radius
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, shieldRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy") && hitCollider.gameObject != gameObject)
                {
                    NetworkObject enemyNetObj = hitCollider.GetComponent<NetworkObject>();
                    if (enemyNetObj != null)
                    {
                        // Add to shielded enemies list
                        shieldedEnemies.Add(enemyNetObj.NetworkObjectId);
                    }
                }
            }
            
            // Update shield visuals on all clients
            UpdateShieldVisualsClientRpc(shieldedEnemies.ToArray());
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    [ClientRpc]
    private void UpdateShieldVisualsClientRpc(ulong[] shieldedIds)
    {
        // First disable all shield visuals
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in allEnemies)
        {
            Transform shieldTransform = enemy.transform.Find("Shield");
            if (shieldTransform != null)
            {
                shieldTransform.gameObject.SetActive(false);
            }
        }
        
        // Then enable shields only for protected enemies
        foreach (ulong enemyId in shieldedIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out NetworkObject enemyObj))
            {
                Transform shieldTransform = enemyObj.transform.Find("Shield");
                if (shieldTransform != null)
                {
                    shieldTransform.gameObject.SetActive(true);
                }
            }
        }
    }
    
    // Check if an enemy is shielded
    public bool IsShielded(ulong enemyNetId)
    {
        return shieldedEnemies.Contains(enemyNetId);
    }
    
    // Process damage for a shielded enemy
    public int ProcessDamage(ulong enemyNetId, int damage)
    {
        // If enemy is shielded and damage is less than shield strength, absorb all damage
        if (IsShielded(enemyNetId) && damage <= shieldStrength)
        {
            // Debug.Log($"Shield absorbed {damage} damage");
            return 0;
        }
        // If damage exceeds shield strength, shield breaks and remaining damage goes through
        else if (IsShielded(enemyNetId))
        {
            int remainingDamage = damage - shieldStrength;
            
            // Remove enemy from shielded list
            shieldedEnemies.Remove(enemyNetId);
            
            // Update shield visuals
            UpdateShieldVisualsClientRpc(shieldedEnemies.ToArray());
            
            // Debug.Log($"Shield broke! {remainingDamage} damage passes through");
            return remainingDamage;
        }
        else
        {
            // Enemy not shielded, full damage applies
            return damage;
        }
    }
    
    // When this shield enemy is destroyed, remove all shields
    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (IsServer)
        {
            // Clear all shields when this shield enemy is destroyed
            shieldedEnemies.Clear();
            UpdateShieldVisualsClientRpc(new ulong[0]);
        }
    }
    
    // Visualize shield radius in editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shieldRadius);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, shieldRadius);
    }
}