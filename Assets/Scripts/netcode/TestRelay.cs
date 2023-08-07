using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Threading.Tasks;

public class TestRelay : MonoBehaviour
{
    public const int m_MaxPlayers = 4;

    string m_EnterLobbyCode = "Enter code";

    string m_LobbyCode;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CreateRelay()
    {
        try
        { 
            // -1, not including host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(m_MaxPlayers - 1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            m_LobbyCode = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with " +  joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("1");

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");

            Debug.Log("2");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            Debug.Log("3");

            NetworkManager.Singleton.StartClient();

            Debug.Log("4");
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host")) CreateRelay();
            m_EnterLobbyCode = GUILayout.TextField(m_EnterLobbyCode);
            if (GUILayout.Button("Client")) JoinRelay(m_EnterLobbyCode);
        }

        if (NetworkManager.Singleton.IsClient)
        {
            GUILayout.Label(m_LobbyCode);
        }

        GUILayout.EndArea();
    }
}