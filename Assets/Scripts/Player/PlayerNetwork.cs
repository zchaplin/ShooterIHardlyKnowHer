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
            Debug.Log("current position: " + transform.position);
        }else{
            transform.position = netpos.Value;
            transform.rotation = netrot.Value;
        }
        
    }

    // public override void OnNetworkSpawn()
    // {
    //     if (IsOwner)
    //     {
    //         Vector3 spawnPosition;

    //         //modified to be inclusive of more than 2 players. 
    //         if (OwnerClientId % 2 == 0) // any even number clientID
    //         {
    //             Debug.Log(" == 0");
    //             spawnPosition = new Vector3(1f, 1f, -3f + OwnerClientId);
    //         }
    //         else // any odd number clientID
    //         {
    //             Debug.Log(" != 0");
    //             spawnPosition = new Vector3(-1f, 1f, 3f + OwnerClientId);
    //         }

    //         // Adjust Y position to match the ground level
    //         spawnPosition.y = GetGroundHeight(spawnPosition);
            
    //         transform.position = spawnPosition;
    //         Debug.Log("position: " + spawnPosition);
    //     }
    // }

    // private float GetGroundHeight(Vector3 position)
    // {
    //     RaycastHit hit;
    //     if (Physics.Raycast(new Vector3(position.x, 10f, position.z), Vector3.down, out hit, Mathf.Infinity))
    //     {
    //         return hit.point.y + 0.1f; // Add slight offset to prevent clipping
    //     }
    //     return position.y; // Default if no ground detected
    // }

}
