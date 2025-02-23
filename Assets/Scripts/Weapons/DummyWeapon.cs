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

    void Start()
    {
        // Get all MeshRenderers in children
        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            // Store the original materials of all child renderers
            originalMaterials = new Material[meshRenderers.Length];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                originalMaterials[i] = meshRenderers[i].material;
            }
        }
        else
        {
            Debug.LogError("No MeshRenderers found in children of DummyWeapon");
        }

        // Subscribe to changes in the WeaponIndex
        WeaponIndex.OnValueChanged += OnWeaponIndexChanged;
    }

    private void OnWeaponIndexChanged(int oldValue, int newValue)
    {
        Debug.Log($"Weapon index changed from {oldValue} to {newValue}");
    }

    // Highlight the weapon when a player looks at it
    public void Highlight()
    {
        if (meshRenderers != null && highlightMaterial != null)
        {
            foreach (var renderer in meshRenderers)
            {
                renderer.material = highlightMaterial;
            }
        }
    }

    // Remove the highlight
    public void RemoveHighlight()
    {
        if (meshRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = originalMaterials[i];
            }
        }
    }

    // Cleanup
    public override void OnDestroy()
    {
        base.OnDestroy();
        WeaponIndex.OnValueChanged -= OnWeaponIndexChanged;
    }
}