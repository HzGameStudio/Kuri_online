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
using System.Threading.Tasks;

// Class that creates the relay, starts host / client
public static class Relay
{
    public static async Task SignInAnonymously()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
            await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public static async Task<bool> CreateRelay()
    {
        try
        { 
            // MaxPlayers -1, because it asks for amount of players not including host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameManager.Instance.maxPlayers - 1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            RelayServerData relayServerData = new (allocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            GameManager.Instance.lobbyCode.Value = joinCode;

            return true;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);

            return false;
        }
    }

    public static async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with " +  joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            return true;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex);

            return false;
        }
    }
}