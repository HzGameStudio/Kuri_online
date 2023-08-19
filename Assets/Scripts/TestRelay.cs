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

// Class that creates the relay \starts host / client
public class TestRelay : NetworkBehaviour
{
    private string m_EnterLobbyCode = "Enter code";

    [SerializeField]
    private GameObject m_NetworkManager;

    [SerializeField]
    private GameData m_GameData;

    private async void Start()
    {
        // Async is to do stuff on other threads (copied from tutorial idk :P)
        // When function is await, it does not stop to complete it, but launches it on another thread and continues next in the code
        await UnityServices.InitializeAsync();

        // Little lambda 
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        // Sign in anonymously for now, but will have to do smth else to have accounts
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CreateRelay()
    {
        try
        { 
            // MaxPlayers -1, because it asks for amount of players not including host
            // Create the relay, a "kind of server" to transfer data between clients and host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(m_GameData.maxPlayers - 1);

            // Get the join code
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            // Tell the <NetworkManager> the details of the relay, this is the Server <NetworkManager>, so it will store the details of the host(client + server)
            RelayServerData relayServerData = new RelayServerData(allocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            m_GameData.lobbyCode.Value = joinCode;
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
            // Join relay and start client, everything basically the same as CreateRelay(), but for joining
            Debug.Log("Joining relay with " +  joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Tell the <NetworkManager> the details of the relay, this is each of the client's <NetworkManager>, so it will store the details that the client needs to join
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnGUI()
    {
        // Cheap and shit >TEMPORARY< UI :skull:
        GUILayout.BeginArea(new Rect(100, 100, 1000, 1000));

        GUI.skin.button.fontSize = 36;
        GUI.skin.textField.fontSize = 36;

        // This if means "if the game didn't create a client or a server", meaning that the player is still on the loading screen
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host", GUILayout.Width(600), GUILayout.Height(200)))
            {
                // You can Manually change the <Protocol Type> parameter in the <Unity Transform> component on the <NetworkManager> object,
                // <Relay Unity Transform> is for playing online with the relay, <Unity Transform> is for testing offline, only works when playing on one computer
                if (m_NetworkManager.GetComponent<UnityTransport>().Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
                    CreateRelay();
                else
                {
                    // When playing offline, we don't need the relay, so just sign out and start host / client
                    AuthenticationService.Instance.SignOut();

                    NetworkManager.Singleton.StartHost();
                }
            }
            m_EnterLobbyCode = GUILayout.TextField(m_EnterLobbyCode, GUILayout.Width(600), GUILayout.Height(200));
            if (GUILayout.Button("Client", GUILayout.Width(600), GUILayout.Height(200)))
            {
                if (m_NetworkManager.GetComponent<UnityTransport>().Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
                    JoinRelay(m_EnterLobbyCode);
                else
                {
                    AuthenticationService.Instance.SignOut();

                    NetworkManager.Singleton.StartClient();
                }
            }
        }

        GUILayout.EndArea();
    }
}