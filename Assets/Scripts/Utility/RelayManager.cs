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
            SetHostTransform();
            return joinCode;
        }

        return null;
    }

    private void SetHostTransform()
    {
        NetworkObject hostObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (hostObject != null)
        {
            hostObject.transform.position = new Vector3(1, 1, -3);
            hostObject.name = "Player1";
        }
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        bool isClientStarted = !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();

        if (isClientStarted)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                if (clientId == NetworkManager.Singleton.LocalClientId)
                {
                    SetClientTransform();
                    ConnectClientToHostWithSpring();
                }
            };
        }

        return isClientStarted;
    }

    private void SetClientTransform()
    {
        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject != null)
        {
            playerObject.transform.position = new Vector3(1, 1, 3);
            playerObject.name = "Player2";
        }
    }

    private void ConnectClientToHostWithSpring()
{
    // Only the host should create the spring joint
    Debug.Log("Test");
    if (!NetworkManager.Singleton.IsHost)
    {
        return; // Exit if this is not the host
    }

    // Get the client's and host's NetworkObjects
    NetworkObject clientObject = NetworkManager.Singleton.LocalClient.PlayerObject;
    NetworkObject hostObject = NetworkManager.Singleton.ConnectedClients[NetworkManager.ServerClientId].PlayerObject;

    if (clientObject != null && hostObject != null)
    {
        // Add a Spring Joint component to the client
        SpringJoint springJoint = clientObject.gameObject.AddComponent<SpringJoint>();

        // Configure the Spring Joint
        springJoint.connectedBody = hostObject.GetComponent<Rigidbody>(); // Connect to the host's Rigidbody
        springJoint.spring = 50f; // Adjust the spring strength
        springJoint.damper = 5f; // Adjust the damping
        springJoint.minDistance = 2f; // Minimum distance between client and host
        springJoint.maxDistance = 10f; // Maximum distance between client and host
        springJoint.tolerance = 0.25f; // Tolerance for distance

        Debug.Log("Spring Joint added between client and host.");
    }
    else
    {
        Debug.LogError("Failed to find client or host objects for Spring Joint.");
    }
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