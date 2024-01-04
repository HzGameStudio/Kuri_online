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
using TMPro;
using System;

// Class that creates the relay, starts host / client
public class TestRelay : SingletonNetwork<TestRelay>
{
    [SerializeField]
    private TextMeshProUGUI m_LobbyCodeInputField;

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
            // MaxPlayers -1, because it asks for amount of players not including host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameManager.Instance.maxPlayers - 1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            GameManager.Instance.lobbyCode.Value = joinCode;

            LoadingSceneManager.Instance.LoadScene(SceneName.O_GameMenu, true);
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

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    public void ButtonCreateRelay()
    {
        // You can Manually change the <Protocol Type> parameter in the <Unity Transform> component on the <NetworkManager> object,
        // <Relay Unity Transform> is for playing online with the relay, <Unity Transform> is for testing offline, only works when playing on one computer
        if (NetworkManager.Singleton.GetComponent<UnityTransport>().Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
            CreateRelay();
        else
        {
            AuthenticationService.Instance.SignOut();

            NetworkManager.Singleton.StartHost();
        }
    }

    public void ButtonJoinRelay()
    {
        if (NetworkManager.Singleton.GetComponent<UnityTransport>().Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
            JoinRelay(m_LobbyCodeInputField.text.Substring(0,6));
        else
        {
            AuthenticationService.Instance.SignOut();

            NetworkManager.Singleton.StartClient();
        }
    }
}