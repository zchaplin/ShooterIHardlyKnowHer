using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [Header("Player Spawning")]
    [SerializeField] private bool useCustomPlayerSpawner = true;
    [SerializeField] private PlayerPrefabSpawner playerSpawner; // Reference to the player spawner
    
    private bool hostPlayerSpawned = false;
    private bool isConnected = false;

    private void Start()
    {
        InitializeUnityServices();
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private async void InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartRelay()
    {
        string joinCode = await StartHostWithRelay();
        joinCodeText.text = joinCode;

        GUIUtility.systemCopyBuffer = joinCode;
        Debug.Log($"Join code copied to clipboard: {joinCode}");
    }

    public async void JoinRelay()
    {
        await StartClientWithRelay(joinCodeInputField.text);
    }

    private async Task<string> StartHostWithRelay(int maxConnections = 3)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        if (NetworkManager.Singleton.StartHost())
        {
            isConnected = true;
            
            // IMPORTANT: Only do this if not using custom spawner
            if (!useCustomPlayerSpawner && !hostPlayerSpawned)
            {
                SetHostTransform();
                hostPlayerSpawned = true;
            }
            
            return joinCode;
        }
        
        return null;
    }


   private void SetHostTransform()
    {
        // This will only be called if we're not using the custom player spawner
        // Wait a short time to ensure the player has spawned
        StartCoroutine(DelayedHostTransform());
    }
    
    private IEnumerator DelayedHostTransform()
    {
        // Wait a frame to ensure player has spawned
        yield return null;
        
        NetworkObject hostObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (hostObject != null)
        {
            hostObject.transform.position = new Vector3(1, 1, -3);
            hostObject.name = "Player1";
            Debug.Log("Set host transform");
        }
        else
        {
            Debug.LogWarning("Host object not found for transform setting");
        }
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        bool isClientStarted = !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();

        if (isClientStarted)
        {
            isConnected = true;
            
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                if (clientId == NetworkManager.Singleton.LocalClientId)
                {
                    // If using default NetworkManager spawning
                    if (!useCustomPlayerSpawner)
                    {
                        SetClientTransform();
                    }
                    
                    // Always try to connect spring joint
                    StartCoroutine(DelayedSpringJointConnection());
                }
            };
        }

        return isClientStarted;
    }

    private void SetClientTransform()
    {
        // This will only be called if we're not using the custom player spawner
        StartCoroutine(DelayedClientTransform());
    }
    
    private IEnumerator DelayedClientTransform()
    {
        // Wait a frame to ensure player has spawned
        yield return null;
        
        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject != null)
        {
            playerObject.transform.position = new Vector3(1, 1, 3);
            playerObject.name = "Player2";
            Debug.Log("Set client transform");
        }
        else
        {
            Debug.LogWarning("Client object not found for transform setting");
        }
    }
    
    private IEnumerator DelayedSpringJointConnection()
    {
        // Wait a moment to ensure both players are fully spawned and initialized
        yield return new WaitForSeconds(0.5f);
        ConnectClientToHostWithSpring();
    }

    private void ConnectClientToHostWithSpring()
    {
        // Only the host should create the spring joint
        if (!NetworkManager.Singleton.IsHost)
        {
            return; // Exit if this is not the host
        }

        // Find the players
        GameObject player1 = PlayerPrefabSpawner.GetPlayer1Instance();
        GameObject player2 = PlayerPrefabSpawner.GetPlayer2Instance();

        if (player1 != null && player2 != null)
        {
            // Add a delay before activating the spring
            StartCoroutine(DelayedSpringSetup(player1, player2));
        }
    }

    private IEnumerator DelayedSpringSetup(GameObject player1, GameObject player2)
    {
        // Wait for both players to settle into position
        yield return new WaitForSeconds(2f);
        
        // Add a Spring Joint component to player 2
        SpringJoint springJoint = player2.GetComponent<SpringJoint>();
        if (springJoint == null)
        {
            springJoint = player2.AddComponent<SpringJoint>();
        }

        // Configure the Spring Joint with gentler initial settings
        springJoint.connectedBody = player1.GetComponent<Rigidbody>();
        springJoint.spring = 20f; // Lower spring strength
        springJoint.damper = 5f;
        springJoint.minDistance = 2f;
        springJoint.maxDistance = 10f;
        springJoint.tolerance = 0.25f;
        
        // Gradually increase spring strength for a smoother transition
        StartCoroutine(GraduallyIncreaseSpringStrength(springJoint));
        
        Debug.Log("Spring Joint added between players with delayed activation");
    }

    private IEnumerator GraduallyIncreaseSpringStrength(SpringJoint joint)
    {
        float targetStrength = 50f;
        float currentStrength = joint.spring;
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            joint.spring = Mathf.Lerp(currentStrength, targetStrength, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        joint.spring = targetStrength;
    }
    void Update()
    {
        // Update logic (if needed)
    }
    // private void OnClientConnected(ulong clientId)
    // {
    //     // Check if this is the server (host) or a client
    //     if (NetworkManager.Singleton.IsServer)
    //     {
    //         ConnectClientToHostWithSpring();

    //     }
    // }
}