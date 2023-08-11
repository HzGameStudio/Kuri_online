using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerUIManager : NetworkBehaviour
{
    private GameData gameManagerGameData;

    private GameObject startGameButton;

    private TextMeshProUGUI playerIDText;

    private TextMeshProUGUI winnerText;

    private void Start()
    {
        gameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();

        startGameButton = gameManagerGameData.startButton;
        playerIDText = gameManagerGameData.playerIDText;
        winnerText = gameManagerGameData.winnerText;

        if (IsServer)
        {
            startGameButton.SetActive(true);
            gameManagerGameData.CalcNumPlayersInGame();
        }

        if (IsClient && IsOwner)
        {
            playerIDText.text = "player num" + GetComponent<PlayerData>().playerID.Value.ToString();
        }

        // this shouldnt be here, make player spawner
        //Shange position to a Spawn postion
        if (IsServer)
        {
            transform.position = gameManagerGameData.GetSpawnPosition();
        }

        gameManagerGameData.isGameRunning.OnValueChanged += OnIsGameRunningChanged;
        GetComponent<PlayerData>().placeInGame.OnValueChanged += OnPlaceInGameChanged;
    }

    private void OnIsGameRunningChanged(bool previous, bool current)
    {
         startGameButton.SetActive(false);
    }

    private void OnPlaceInGameChanged(int previous, int current)
    {
        if (GetComponent<PlayerData>().placeInGame.Value != -1)
        {
            winnerText.gameObject.SetActive(true);
            winnerText.text = "YOU WON " + GetComponent<PlayerData>().placeInGame.Value.ToString() + " PLACE!!!";
        }
    }
}
