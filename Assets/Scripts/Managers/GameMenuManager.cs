using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameMenuManager : SingletonNetwork<GameMenuManager>
{
    [SerializeField]
    private GameObject m_StartGameButton;

    [SerializeField]
    private TextMeshProUGUI m_LobbyCodeText;

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
    }

    public void StartGame()
    {
        LoadingSceneManager.Instance.LoadScene(SceneName.MainGame, true);
    }
}
