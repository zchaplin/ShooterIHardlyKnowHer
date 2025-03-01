using UnityEngine;
using Unity.Netcode;

public class DummyWeapon : NetworkBehaviour
{
    // Network-synchronized weapon index
    public NetworkVariable<int> WeaponIndex = new NetworkVariable<int>();

    // Visual feedback
    private Material[] originalMaterials; // Array to store original materials of all child renderers
    [SerializeField] private Material highlightMaterial; // Assign a highlight material in the Inspector
    private MeshRenderer[] meshRenderers; // Array to store all MeshRenderers in children
    private bool isHighlighted = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Initialize materials on all clients
        InitializeMaterials();
    }
    
    void Start()
    {
        // Make sure materials are initialized even if OnNetworkSpawn wasn't called
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            InitializeMaterials();
        }
    }
    
    private void InitializeMaterials()
    {
        try
        {
            // Get all MeshRenderers in children
            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                // Store the original materials of all child renderers
                originalMaterials = new Material[meshRenderers.Length];
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    if (meshRenderers[i] != null && meshRenderers[i].material != null)
                    {
                        originalMaterials[i] = meshRenderers[i].material;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No MeshRenderers found in children of DummyWeapon");
            }

            // Subscribe to changes in the WeaponIndex
            WeaponIndex.OnValueChanged += OnWeaponIndexChanged;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing DummyWeapon materials: {e.Message}\n{e.StackTrace}");
        }
    }

    private void OnWeaponIndexChanged(int oldValue, int newValue)
    {
        Debug.Log($"Weapon index changed from {oldValue} to {newValue}");
    }

    // Highlight the weapon when a player looks at it - client-side only
    public void Highlight()
    {
        if (isHighlighted) return; // Already highlighted
        
        if (meshRenderers != null && highlightMaterial != null)
        {
            try
            {
                foreach (var renderer in meshRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.material = highlightMaterial;
                    }
                }
                isHighlighted = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error highlighting weapon: {e.Message}");
            }
        }
    }

    // Remove the highlight - client-side only
    public void RemoveHighlight()
    {
        if (!isHighlighted) return; // Not highlighted
        
        if (meshRenderers != null && originalMaterials != null)
        {
            try
            {
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    if (meshRenderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
                    {
                        meshRenderers[i].material = originalMaterials[i];
                    }
                }
                isHighlighted = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error removing highlight: {e.Message}");
            }
        }
    }

    // Cleanup
    public override void OnDestroy()
    {
        try
        {
            if (IsSpawned)
            {
                WeaponIndex.OnValueChanged -= OnWeaponIndexChanged;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnDestroy: {e.Message}");
        }
        
        base.OnDestroy();
    }
}