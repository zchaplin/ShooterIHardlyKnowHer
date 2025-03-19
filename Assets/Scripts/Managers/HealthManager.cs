using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class HealthManager : NetworkBehaviour
{
    // public NetworkVariable<int> totalHealth = new NetworkVariable<int>(15, 
    //     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<int> remainingHealth = new NetworkVariable<int>(15, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int totalHealth = 15;
    // private int remainingHealth;

    // Event system for player damage
    public delegate void OnPlayerDamaged();
    public event OnPlayerDamaged PlayerDamaged;
    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            remainingHealth.Value = 15; // Set initial value only on the server
        }
    }


    void Start()
    {
        // remainingHealth.OnValueChanged += playerTakeDamage;
        // remainingHealth = totalHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // if (remainingHealth.Value <= 0) {
        //     //lose game
        //     SceneManager.LoadScene(2); 
        // }
    }

    public void playerTakeDamage(int dmg) {
        if (IsServer)
        {
            if (remainingHealth.Value > 0) {
                remainingHealth.Value -= dmg;
                PlayerDamaged?.Invoke();
            }
            if (remainingHealth.Value <= 0) {
                remainingHealth.Value = 0;
                GameOverAllClients();
            }
        }
        else
        {
            SubmitHealthServerRpc(dmg);
        }
    }

    public int getRemainingPlayerHealth() {
        return remainingHealth.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitHealthServerRpc(int dmg)
    {
        Debug.Log($"SubmitHealthServerRpc called with dmg: {dmg}. Current health: {remainingHealth.Value}");

         if (remainingHealth.Value > 0) {
                remainingHealth.Value -= dmg;
                PlayerDamaged?.Invoke();
            }
        if (remainingHealth.Value <= 0) {
            remainingHealth.Value = 0;
            GameOverAllClients();
        }
         Debug.Log($"Health after damage: {remainingHealth.Value}");

       
    }


    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsHost) // Prevent the host from running this twice
        {
            Debug.Log("Game Over! Loading scene on all clients...");
            SceneManager.LoadScene(2);
        }
    }

    private void GameOverAllClients()
{
    if (IsServer) // Only the server can load the scene for all clients
    {
        Debug.Log("Game Over! Syncing scene for all clients...");
        NetworkManager.SceneManager.LoadScene("EndScreen", LoadSceneMode.Single);
    }
}
}