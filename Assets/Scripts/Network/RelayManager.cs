using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void StartRelay()
    {
        string joinCode = await StartHostWithRelay(3, "dtls");
        joinCodeText.text = joinCode;
    }

    public async void JoinRelay()
    {
        await StartClientWithRelay(joinCodeInputField.text, "dtls");
    }

    public async Task<string> StartHostWithRelay(int maxConnections, string connectionType)
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

        var relayServerData = AllocationUtils.ToRelayServerData(allocation, connectionType);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, connectionType);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        return NetworkManager.Singleton.StartClient();
    }
}
