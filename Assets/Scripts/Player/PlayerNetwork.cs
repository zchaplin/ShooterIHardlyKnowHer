using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> netpos = new (writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<Quaternion> netrot = new (writePerm: NetworkVariableWritePermission.Owner);

    void Update()
    {
        if(IsOwner){
            netpos.Value = transform.position;
            netrot.Value = transform.rotation;
        }else{
            transform.position = netpos.Value;
            transform.rotation = netrot.Value;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Vector3 spawnPosition;

            if (OwnerClientId == 0) // Player 1
            {
                spawnPosition = new Vector3(1f, 1f, -5f);
            }
            else // Player 2
            {
                spawnPosition = new Vector3(-1f, 1f, 5f);
            }

            // Adjust Y position to match the ground level
            spawnPosition.y = GetGroundHeight(spawnPosition);

            transform.position = spawnPosition;
        }
    }

    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(position.x, 10f, position.z), Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.point.y + 0.1f; // Add slight offset to prevent clipping
        }
        return position.y; // Default if no ground detected
    }

}
