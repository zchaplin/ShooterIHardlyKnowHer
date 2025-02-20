using UnityEngine;
using Unity.Netcode;

public class DummyWeapon : NetworkBehaviour
{
    public int WeaponIndex { get; set; }
    private Material originalMaterial;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
        }
    }

}