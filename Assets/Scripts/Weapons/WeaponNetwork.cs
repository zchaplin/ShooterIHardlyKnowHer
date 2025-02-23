using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;

public class WeaponNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> netObjpos = new (writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<Quaternion> netObjrot = new (writePerm: NetworkVariableWritePermission.Owner);
    //private readonly NetworkVariable<bool> weaponSwaped = new (writePerm: NetworkVariableWritePermission.Server);
    //private bool Swaped = false;
    private readonly NetworkVariable<int> i = new (writePerm: NetworkVariableWritePermission.Owner);
    int indexNum = 0;
    public List<GameObject> models; //This is assigned in the unity engine. MAKE SURE TO DRAG THE PREFAB MODEL. AVOID USING THE DROPDOWN LIST.  
    private GameObject model;
    void Start()
    {
        // Instantiate the visual model at the specified position and rotation for other players to see
        if(!IsOwner){model = Instantiate(models[0], netObjpos.Value, netObjrot.Value);}
    }
    void Update()
    {
        if(IsOwner){
            for(i.Value = 0; i.Value < transform.childCount; i.Value++){ //checks active children (weapons that are being held)
                if(transform.GetChild(i.Value).gameObject.activeSelf){
                    if(i.Value != indexNum){ //If the player changes weapon
                        Destroy(model); //remove model from client side
                    }
                    break;
                }
            }
            netObjpos.Value = transform.GetChild(indexNum).position;
            netObjrot.Value = transform.GetChild(indexNum).rotation;
        }else{
            if(indexNum != i.Value){ //if the weapon is swapped
                Destroy(model); //removes model for server side
                model = Instantiate(models[i.Value], netObjpos.Value, netObjrot.Value); 
                Debug.Log("new weapon: " + model);
                indexNum = i.Value;
            }
            model.transform.position = netObjpos.Value;
            model.transform.rotation = netObjrot.Value;
        }
    }
    void OnDisable()
    {
        Destroy(model);
    }
    void Oestroy()
    {
        Destroy(model);
    }
}