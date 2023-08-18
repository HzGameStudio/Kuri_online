using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Runtime.InteropServices;
using Unity.Collections;

public class PlayerUIManager : NetworkBehaviour
{
    private GameData gameManagerGameData;

    private GameObject startGameButton;

    private TextMeshProUGUI playerIDText;

    private TextMeshProUGUI winnerText;

    private TextMeshProUGUI RunTimeText;

    private TextMeshProUGUI lobbyIDText;

    private TextMeshProUGUI kuraStatetext;

    private void Start()
    {
        gameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();

        startGameButton = gameManagerGameData.startButton;
        playerIDText = gameManagerGameData.playerIDText;
        winnerText = gameManagerGameData.winnerText;
        RunTimeText = gameManagerGameData.playerRunTimeText;
        lobbyIDText = gameManagerGameData.lobbyIDText;
        kuraStatetext = gameManagerGameData.kuraStatetext;

        if (IsHost)
        {
            startGameButton.SetActive(true);
        }

        if (IsServer)
        {
            transform.position = gameManagerGameData.GetSpawnPosition();
        }

        if (IsClient && IsOwner)
        {
            playerIDText.gameObject.SetActive(true);
            playerIDText.text = GetComponent<PlayerData>().playerID.Value.ToString();

            RunTimeText.gameObject.SetActive(true);

            lobbyIDText.gameObject.SetActive(true);
            lobbyIDText.text = gameManagerGameData.m_LobbyCode.Value.Value;

            kuraStatetext.gameObject.SetActive(true);
        }

        gameManagerGameData.isGameRunning.OnValueChanged += OnIsGameRunningChanged;
        GetComponent<PlayerData>().placeInGame.OnValueChanged += OnPlaceInGameChanged;
        GetComponent<PlayerData>().state.OnValueChanged += OnKuraStateChanged;
    }

    private void OnIsGameRunningChanged(bool previous, bool current)
    {
        if (current == true)
        {
            startGameButton.SetActive(false);
        }
    }

    private void OnPlaceInGameChanged(int previous, int current)
    {
        if (IsClient && IsOwner)
        {
            if (GetComponent<PlayerData>().placeInGame.Value != -1)
            {
                winnerText.gameObject.SetActive(true);
                winnerText.text = "YOU WON " + GetComponent<PlayerData>().placeInGame.Value.ToString() + " PLACE!!!";

                playerIDText.gameObject.SetActive(false);
                lobbyIDText.gameObject.SetActive(false);
                kuraStatetext.gameObject.SetActive(false);
            }
        }
    }

    private void OnKuraStateChanged(PlayerData.KuraState previous, PlayerData.KuraState current)
    {
        if (IsClient && IsOwner)
        {
            kuraStatetext.text = GetComponent<PlayerData>().state.Value.ToString();
        }
    }
}
