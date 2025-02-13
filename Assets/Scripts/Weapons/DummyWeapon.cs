using UnityEngine;
using Unity.Netcode;

public class DummyWeapon : NetworkBehaviour
{
    public int WeaponIndex { get; set; }
    private bool isHighlighted = false;
    private Material originalMaterial;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
        }
    }

    // Optional: Highlight the weapon when looking at it
    public void Highlight(bool shouldHighlight)
    {
        if (isHighlighted == shouldHighlight) return;
        isHighlighted = shouldHighlight;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = shouldHighlight ? Color.yellow : Color.white;
        }
    }
}