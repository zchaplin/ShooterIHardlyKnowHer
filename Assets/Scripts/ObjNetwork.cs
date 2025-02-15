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
    void Start()
    {
        // Instantiate the object at the specified position and rotation
        if(!IsOwner){}
        model = Instantiate(models[0], netObjpos.Value, netObjrot.Value);
    }
    void Update()
    {
        if(IsOwner){
            netObjpos.Value = gameObject.transform.position;
            netObjrot.Value = gameObject.transform.rotation;
            
            for(int i = 0; i < transform.childCount; i++){
                if(transform.GetChild(i).gameObject.activeSelf){
                    if(i != indexNum){
                        Destroy(model);
                        indexNum = i;
                        model = Instantiate(models[indexNum], netObjpos.Value, netObjrot.Value);
                    }
                    
                    break;
                }
            }
            Debug.Log("obj position: " + model.transform.position);
        }else{
            model.transform.position = netObjpos.Value;
            model.transform.rotation = netObjrot.Value;
            Debug.Log("model position: " + netObjpos.Value);
        }
        
    }
}
