using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class LobbyManager : Singleton<LobbyManager>
{
    [SerializeField]
    private TextMeshProUGUI m_LobbyCodeInputField;

    private bool m_Started = false;

    public async void Create()
    {
        if (m_Started) return;

        m_Started = true;

        await Relay.SignInAnonymously();

        bool res = await Relay.CreateRelay();

        if (res)
            LoadingSceneManager.Instance.LoadScene(SceneName.O_GameMenu, true);
        else
            m_Started = false;
    }

    public async void Join()
    {
        if (m_Started) return;

        m_Started = true;

        await Relay.SignInAnonymously();

        bool res = await Relay.JoinRelay(m_LobbyCodeInputField.text.Substring(0, 6));

        if (res)
            LoadingSceneManager.Instance.LoadScene(SceneName.O_GameMenu, true);
        else
            m_Started = false;
    }
}
