using UnityEngine;

using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class relay_manager : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI joinCodeText;
	[SerializeField] private TMP_InputField joinCodeInputField;

	public string joinCode;

	// Start is called before the first frame update
	async void Start()
	{
		await UnityServices.InitializeAsync();

		await AuthenticationService.Instance.SignInAnonymouslyAsync();
	}

	public async void StartRelay()
	{
		await StartHostingWithRelay();

		gameObject.SetActive(false);
	}

	public async void JoinRelay()
	{
		await StartClientWithRelay(joinCodeInputField.text);

		gameObject.SetActive(false);
	}

	private async Task StartHostingWithRelay(int maxConnections = 3)
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

			joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			joinCodeText.text = joinCode;
			
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
				allocation.RelayServer.IpV4,
				(ushort)allocation.RelayServer.Port,
				allocation.AllocationIdBytes,
				allocation.Key,
				allocation.ConnectionData
			);

			NetworkManager.Singleton.StartHost();
		}
		catch (RelayServiceException e)
		{
			Debug.Log(e);
		}

	}

	private async Task<bool> StartClientWithRelay(string lobbyCode)
	{
		JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(lobbyCode);

		NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
				joinAllocation.RelayServer.IpV4,
				(ushort)joinAllocation.RelayServer.Port,
				joinAllocation.AllocationIdBytes,
				joinAllocation.Key,
				joinAllocation.ConnectionData,
				joinAllocation.HostConnectionData
			);
		NetworkManager.Singleton.StartClient();
		return !string.IsNullOrEmpty(lobbyCode);
	}
}
