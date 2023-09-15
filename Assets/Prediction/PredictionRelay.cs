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

public class PredictionRelay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_LobbyCodeInputField;

    public string lobbyCode;

    [SerializeField]
    public GameObject createButton;
    [SerializeField]
    public GameObject joinButton;
    [SerializeField]
    public GameObject joinfield;
    [SerializeField]
    public TextMeshProUGUI lobbyCodeField;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        Debug.Log("aaaaaaaaaaaaaaaaaaaaaaa");

        try
        {
            Debug.Log("aaaa");

            // MaxPlayers -1, because it asks for amount of players not including host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            lobbyCode = joinCode;

            lobbyCodeField.text = joinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(m_LobbyCodeInputField.text.Substring(0,6));

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);
        }
    }
}
