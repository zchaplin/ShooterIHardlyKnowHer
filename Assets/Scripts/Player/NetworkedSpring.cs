using Unity.Netcode;
using UnityEngine;

public class NetworkedSpring : NetworkBehaviour
{
    [Header("Spring Properties")]
    public float springConstant = 10f; // Spring stiffness
    public float damping = 1f; // Damping factor
    public float restLength = 2f; // Rest length of the spring
    public float maxForce = 100f; // Maximum force to apply (prevents extreme forces)

    [Header("Chain Visualization")]
    public GameObject StartChainPre; // Start chain prefab
    public GameObject EndChainPre; // End chain prefab
    
    // NetworkVariable to track if players are connected
    private NetworkVariable<bool> playersConnected = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Rigidbody rb;
    private GameObject otherPlayer;
    private Rigidbody otherRb;
    private bool hasChain = false;
    private GameObject chainStart;
    private GameObject chainEnd;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Find the other player based on client ID
        if (IsOwner)
        {
            FindOtherPlayer();
        }
    }

    void FindOtherPlayer()
    {
        // First try to find via PlayerPrefabSpawner if available
        if (OwnerClientId == 0 && PlayerPrefabSpawner.GetPlayer2Instance() != null)
        {
            otherPlayer = PlayerPrefabSpawner.GetPlayer2Instance();
            otherRb = otherPlayer.GetComponent<Rigidbody>();
            return;
        }
        else if (OwnerClientId == 1 && PlayerPrefabSpawner.GetPlayer1Instance() != null)
        {
            otherPlayer = PlayerPrefabSpawner.GetPlayer1Instance();
            otherRb = otherPlayer.GetComponent<Rigidbody>();
            return;
        }
        
        // Dynamically find the other player based on NetworkObjectId
        ulong myClientId = OwnerClientId;
        ulong otherClientId = myClientId == 0 ? 1UL : 0UL;
        
        // Find all NetworkObjects in the scene
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (NetworkObject netObj in networkObjects)
        {
            if (netObj.OwnerClientId == otherClientId && netObj != NetworkObject)
            {
                otherPlayer = netObj.gameObject;
                otherRb = otherPlayer.GetComponent<Rigidbody>();
                break;
            }
        }
        
        if (otherPlayer == null)
        {
            // Retry in a moment since the other player might not be spawned yet
            Invoke("FindOtherPlayer", 0.5f);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return; // Only the owner calculates forces
        
        // If we haven't found the other player yet, try again
        if (otherPlayer == null)
        {
            FindOtherPlayer();
            return;
        }
        
        if (otherRb != null)
        {
            // Optional: Visualize the spring/chain
            if (StartChainPre != null && !hasChain)
            {
                CreateChain();
            }
            
            // Calculate the spring force
            Vector3 displacement = otherPlayer.transform.position - transform.position;
            float distance = displacement.magnitude;
            
            // Only apply forces if we're beyond the rest length
            if (distance > restLength)
            {
                Vector3 direction = displacement.normalized;
                float stretchLength = distance - restLength;
                
                // Calculate the spring force (F = -kx)
                Vector3 springForce = springConstant * stretchLength * direction;
                
                // Calculate damping force (F = -bv)
                Vector3 relativeVelocity = otherRb.velocity - rb.velocity;
                Vector3 dampingForce = damping * Vector3.Dot(relativeVelocity, direction) * direction;
                
                // Combine forces and apply force limit
                Vector3 totalForce = springForce + dampingForce;
                if (totalForce.magnitude > maxForce)
                {
                    totalForce = totalForce.normalized * maxForce;
                }
                
                // Apply the forces to both objects
                rb.AddForce(totalForce);
                
                // We need to be careful applying forces to the other player if it's not locally owned
                // For simplicity, we'll apply forces directly, but in a production game,
                // you'd want to request the server to apply these forces instead using RPCs
                if (IsServer || otherPlayer.GetComponent<NetworkBehaviour>().IsOwner)
                {
                    otherRb.AddForce(-totalForce);
                }
                else if (IsClient)
                {
                    // Request the server to apply force via RPC
                    ApplyForceServerRpc(-totalForce, otherPlayer.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
            
            // Update chain visualization if it exists
            UpdateChainVisualization();
        }
    }
    
    [ServerRpc]
    void ApplyForceServerRpc(Vector3 force, ulong targetNetworkId)
    {
        // Find the target object by network ID
        NetworkObject targetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkId];
        if (targetObj != null)
        {
            Rigidbody targetRb = targetObj.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                targetRb.AddForce(force);
            }
        }
    }

    void CreateChain()
    {
        if (StartChainPre == null)
        {
            Debug.LogWarning("StartChainPre is not assigned!");
            return;
        }

        // Instantiate the chain visualization
        chainStart = Instantiate(StartChainPre, transform.position, Quaternion.identity);
        chainStart.transform.SetParent(transform);
        
        // If you have a component that handles the rope visualization
        // (Like LineRenderer or a custom rope script), configure it here
        var ropeComponent = chainStart.GetComponent<LineRenderer>();
        if (ropeComponent != null)
        {
            // Configure the line renderer
            ropeComponent.positionCount = 2;
            ropeComponent.SetPosition(0, transform.position);
            ropeComponent.SetPosition(1, otherPlayer.transform.position);
        }
        
        hasChain = true;
    }
    
    void UpdateChainVisualization()
    {
        // Update chain visualization if it exists
        if (hasChain && chainStart != null && otherPlayer != null)
        {
            var ropeComponent = chainStart.GetComponent<LineRenderer>();
            if (ropeComponent != null)
            {
                ropeComponent.SetPosition(0, transform.position);
                ropeComponent.SetPosition(1, otherPlayer.transform.position);
            }
        }
    }
}