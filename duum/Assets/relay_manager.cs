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

public class relay_manager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;
    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartRelay()
    {
        string joinCode = await StartHostingWithRelay();
        joinCodeText.text = joinCode;
    }

    public async void JoinRelay()
    {
        await StartClientWithRelay(joinCodeInputField.text);
    }

    private async Task<string> StartHostingWithRelay(int maxConnections = 3)
    {

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }
    
    private async Task<bool> StartClientWithRelay(string lobbyCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(lobbyCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        return !string.IsNullOrEmpty(lobbyCode) && NetworkManager.Singleton.StartClient();
    }
}
