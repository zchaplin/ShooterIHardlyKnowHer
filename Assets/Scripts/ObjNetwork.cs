//this script is meant for a parent class
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;

public class ObjNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> netObjpos = new (writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<Quaternion> netObjrot = new (writePerm: NetworkVariableWritePermission.Owner);
    int indexNum = 0;
    public List<GameObject> models; //This is assigned in the unity engine. MAKE SURE TO DRAG THE PREFAB MODEL. AVOID USING THE DROPDOWN LIST.  
    private GameObject model;
    private bool weaponSwaped = false;
    void Start()
    {
        // Instantiate the visual model at the specified position and rotation for other players to see
        if(!IsOwner){model = Instantiate(models[0], netObjpos.Value, netObjrot.Value);}
    }
    void Update()
    {
        if(IsOwner){
            for(int i = 0; i < transform.childCount; i++){ //checks if all the children are active
                if(transform.GetChild(i).gameObject.activeSelf){
                    if(i != indexNum){ //If the player changes weapon
                        Destroy(model); 
                        indexNum = i;
                        weaponSwaped = true;
                    }
                    break;
                }
            }
            netObjpos.Value = transform.GetChild(indexNum).position;
            netObjrot.Value = transform.GetChild(indexNum).rotation;
            Debug.Log("obj position: " + model.transform.position);
        }else{
            if(weaponSwaped){ 
                model = Instantiate(models[indexNum], netObjpos.Value, netObjrot.Value); 
            }
            model.transform.position = netObjpos.Value;
            model.transform.rotation = netObjrot.Value;
            
            Debug.Log("model position: " + netObjpos.Value);
        }
        
    }
}
