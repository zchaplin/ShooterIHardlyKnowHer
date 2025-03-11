using Unity.Netcode;
using UnityEngine;

public class NetworkedSpring : NetworkBehaviour
{
    public float springConstant = 100f; // Spring stiffness
    public float damping = 4f; // Damping factor
    public float restLength = 2f; // Rest length of the spring

    public GameObject StartChainPre; // Start chain prefab
    public GameObject EndChainPre; // End chain prefab

    private Rigidbody rb;
    private string otherPlayerName = "p1(Clone)";
    private bool hasChain = false;
    private GameObject chainStart;
    private GameObject chainEnd;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return; // Only the owner calculates forces

        // Find the other object by name (this assumes the other object has the same script)
        GameObject otherObject = GameObject.Find(otherPlayerName);

        if (otherObject != null && otherObject != gameObject)
        {
            // if (!hasChain)
            // {
            //     CreateChain(otherObject);
            //     hasChain = true; // Ensure the chain is only created once
            // }
            // else{
            //     Debug.Log(otherObject.transform);
            //     chainStart.GetComponent<CableProceduralSimple>().endPointTransform = otherObject.transform;
            // }

            // Debug.Log("Testing");

            // Calculate the spring force
            Vector3 displacement = otherObject.transform.position - transform.position;
            float distance = displacement.magnitude;
            Vector3 springForce = springConstant * (distance - restLength) * displacement.normalized;

            // Calculate damping force
            Vector3 relativeVelocity = otherObject.GetComponent<Rigidbody>().velocity - rb.velocity;
            Vector3 dampingForce = damping * relativeVelocity;

            // Apply the net force to both objects
            rb.AddForce(springForce + dampingForce);
            otherObject.GetComponent<Rigidbody>().AddForce(-(springForce + dampingForce));
        }
    }

    // void CreateChain(GameObject otherObject)
    // {
    //     if (StartChainPre == null || EndChainPre == null)
    //     {
    //         Debug.LogError("StartChainPre or EndChainPre is not assigned!");
    //         return;
    //     }

    //     // Instantiate the start chain and set it as a child of the current object
    //     chainStart = Instantiate(StartChainPre, transform.position, Quaternion.identity);
    //     chainStart.transform.SetParent(transform); // Set parent to the current object
    //     chainStart.name = "ChainStart";

    //     // Instantiate the end chain and set it as a child of the other object
    //     // chainEnd = Instantiate(EndChainPre, otherObject.transform.position, Quaternion.identity);
    //     // chainEnd.transform.SetParent(otherObject.transform); // Set parent to the other object
    //     // chainEnd.name = "ChainEnd";
    //     chainStart.GetComponent<CableProceduralSimple>().endPointTransform = otherObject.transform;

    // }
}