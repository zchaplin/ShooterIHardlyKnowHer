using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerModelSwitcher : NetworkBehaviour
{
    [SerializeField] private GameObject player1Model; // Reference to the Player 1 model (Wizard)
    [SerializeField] private GameObject player2Model; // Reference to the Player 2 model (Clown)
    
    // Network variable to track which model should be active
    private NetworkVariable<byte> activeModelId = new NetworkVariable<byte>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    // Property to check if we're using the clown model
    public bool IsClownModel => activeModelId.Value == 1;

    public override void OnNetworkSpawn()
    {
        // Register the callback for when the model ID changes
        activeModelId.OnValueChanged += OnActiveModelChanged;
        
        // If we're the server, set the initial model
        if (IsServer)
        {
            // Player 1 (OwnerClientId = 0) gets model 0, Player 2 (OwnerClientId = 1) gets model 1
            byte modelId = (byte)(OwnerClientId == 0 ? 0 : 1);
            activeModelId.Value = modelId;
            Debug.Log($"Server setting Player {OwnerClientId} to model ID {modelId}");
        }
        
        // Apply the current model state immediately
        ApplyModelChange(activeModelId.Value);
    }
    
    // Callback when the active model value changes
    private void OnActiveModelChanged(byte previousValue, byte newValue)
    {
        ApplyModelChange(newValue);
    }

    // Apply the model change based on the model ID
    private void ApplyModelChange(byte modelId)
    {
        if (player1Model == null || player2Model == null)
        {
            Debug.LogError("Models not assigned in inspector!", this);
            return;
        }

        // Activate the correct model based on ID
        if (modelId == 0)
        {
            player1Model.SetActive(true);
            player2Model.SetActive(false);
            Debug.Log($"Player {OwnerClientId} is using Player 1 model (Wizard)");
        }
        else
        {
            player1Model.SetActive(false);
            player2Model.SetActive(true);
            Debug.Log($"Player {OwnerClientId} is using Player 2 model (Clown)");
        }
    }

    public override void OnNetworkDespawn()
    {
        // Clean up when the network object is despawned
        activeModelId.OnValueChanged -= OnActiveModelChanged;
    }
}