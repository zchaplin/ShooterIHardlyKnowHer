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
}
