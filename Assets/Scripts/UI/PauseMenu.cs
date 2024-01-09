using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public static bool GameIsPaused = false;

    [SerializeField] private GameObject PauseMenuUI;

    public GameObject player;


    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1;
    }

    public void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("S_GameMenu");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("ChooseLaunchMode");
    }

    public void ResetCheckPoint()
    {
        player.GetComponent<S_PlayerMain>().Respawn();
        Resume();
    }
}
    
