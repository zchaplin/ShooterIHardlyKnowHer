using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

public class ObjNetwork : NetworkBehaviour
{

    private readonly NetworkVariable<Vector3> netpos = new (writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<Quaternion> netrot = new (writePerm: NetworkVariableWritePermission.Owner);

    void Update()
    {
        if(IsOwner){
            netpos.Value = transform.position;
            netrot.Value = transform.rotation;
            Debug.Log("owner obj position: " + transform.position);
        }else{
            transform.position = netpos.Value;
            transform.rotation = netrot.Value;
            Debug.Log("client obj position: " + transform.position);
        }
    }
}
