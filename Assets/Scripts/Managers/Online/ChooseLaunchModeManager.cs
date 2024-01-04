using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseLaunchModeManager : MonoBehaviour
{
    public void StartOnline()
    {
        SceneManager.LoadScene("O_Lobby");
    }

    public void StartSingleplayer()
    {
        SceneManager.LoadScene("S_GameMenu");
    }
}
