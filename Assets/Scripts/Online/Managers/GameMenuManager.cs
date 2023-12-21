using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameMenuManager : SingletonNetwork<GameMenuManager>
{
    [SerializeField]
    private GameObject m_StartGameButton;

    [SerializeField]
    private TextMeshProUGUI m_LobbyCodeText;

    [SerializeField]
    private TextMeshProUGUI m_NumPlayersText;

    private IEnumerator Start()
    {
        // Wait for the network Scene Manager to start
        yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);

        // Set the events on the loading manager
        // Doing this because every time the network session ends the loading manager stops
        // detecting the events
        LoadingSceneManager.Instance.Init();

        if (IsHost)
        {
            m_StartGameButton.SetActive(true);
        }

        m_LobbyCodeText.text = GameManager.Instance.lobbyCode.Value.ToString();

        UpdateNumPlayersText(0, GameManager.Instance.connectedPlayers.Value);

        GameManager.Instance.connectedPlayers.OnValueChanged += UpdateNumPlayersText;
    }

    public void StartGame()
    {
        LoadingSceneManager.Instance.LoadScene(SceneName.O_MainGame, true);
    }

    private void UpdateNumPlayersText(int previous, int current)
    {
        m_NumPlayersText.text = "Connected players: " + current.ToString();
    }
}
