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
    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartRelay() {
        string joinCode = await StartHostWithRelay();
        joinCodeText.text = joinCode;
    }

    public async void JoinRelay() {
        await StartClientWithRelay(joinCodeInputField.text);
    }
    
private async Task<string> StartHostWithRelay(int maxConnections = 3) {
    Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
    string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

    if (NetworkManager.Singleton.StartHost()) {
        SetHostTransform(); 
        return joinCode;
    }

    return null;
}

private void SetHostTransform() {
    NetworkObject hostObject = NetworkManager.Singleton.LocalClient.PlayerObject;
    if (hostObject != null) {
        hostObject.transform.position = new Vector3(0, 1, -5); 
    }
}


private async Task<bool> StartClientWithRelay(string joinCode) {
    JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

    bool isClientStarted = !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();

    if (isClientStarted) {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) => {
            if (clientId == NetworkManager.Singleton.LocalClientId) {
                SetClientTransform();
            }
        };
    }

    return isClientStarted;
}

private void SetClientTransform() {
    NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
    if (playerObject != null) {
        playerObject.transform.position = new Vector3(0,1,5); 
    }
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
