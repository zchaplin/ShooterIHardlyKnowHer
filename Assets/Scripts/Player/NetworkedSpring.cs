using Unity.Netcode;
using UnityEngine;

public class NetworkedSpring : NetworkBehaviour
{
    public float springConstant = 10f; // Spring stiffness
    public float damping = 1f; // Damping factor
    public float restLength = 2f; // Rest length of the spring

    private Rigidbody rb;

    //private NetworkVariable<FixedString64Bytes> networkName = new NetworkVariable<FixedString64Bytes>(); // Synchronized name
    private string otherPlayerName = "p1(Clone)";
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
            Debug.Log("Testing");
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

        // Update network variables with local values
    }

    void Update()
    {
        // if (!IsOwner)
        // {
        //     // Apply network values to local object
        //     transform.position = networkPosition.Value;
        //     rb.velocity = networkVelocity.Value;
        // }
    }
}