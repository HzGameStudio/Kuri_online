using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerUIManager : NetworkBehaviour
{
    public NetworkVariable<int> playerID = new NetworkVariable<int>();
    public NetworkVariable<int> placeInGame = new NetworkVariable<int>(-1);

    [SerializeField]
    private GameData gameManagerGameData;

    private GameObject startGameButton;

    private TextMeshProUGUI playerIDText;

    private TextMeshProUGUI winerText;

    private void Start()
    {
        gameManagerGameData = GameObject.FindGameObjectWithTag("gameManager").GetComponent<GameData>();

        startGameButton = gameManagerGameData.startButton;
        playerIDText = gameManagerGameData.playerIDText;
        winerText = gameManagerGameData.winerText;

        if (IsServer)
        {
            startGameButton.SetActive(true);
            gameManagerGameData.CalcNumPlayersInGame();
            playerID.Value = gameManagerGameData.numPlayersInGame.Value;
        }

        if (IsClient && IsOwner)
        {
            playerIDText.text = "player num" + playerID.Value.ToString();
        }

        // this shouldnt be here, make player spawner
        //Shange position to a Spawn postion
        if (IsServer)
        {
            transform.position = gameManagerGameData.GetSpawnPosition();
        }

        gameManagerGameData.isGameRunning.OnValueChanged += OnIsGameRunningChanged;
    }

    private void OnIsGameRunningChanged(bool previous, bool current)
    {
         startGameButton.SetActive(false);
    }

    private void Update()
    {
        if (IsServer)
        {

        }
        else if (IsClient && IsOwner)
        {
            UpdateClient();
        }
    }

    private void UpdateClient()
    {
        if (placeInGame.Value != -1)
        {
            winerText.gameObject.SetActive(true);
            winerText.text = "YOU WON " + placeInGame.Value.ToString() + " PLACE!!!";
        }
    }
}
