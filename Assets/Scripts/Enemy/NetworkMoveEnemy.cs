using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkMoveEnemy : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if(IsServer) {
            // Move the enemy forward
            if (gameObject.layer == LayerMask.NameToLayer("SpawnerHost")) {
                transform.position += transform.forward * -2f * Time.deltaTime;
            } else if (gameObject.layer == LayerMask.NameToLayer("SpawnerGuest")) {
                transform.position += transform.forward * 2f * Time.deltaTime;
            }
        }
    }
}
