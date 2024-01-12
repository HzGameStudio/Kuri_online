using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_GameMenuManager : MonoBehaviour
{
    private void Start()
    {
        MapManager.Instance.FillMapPanel(MapManager.Instance.ScanForMaps());
    }

    public void Back()
    {
        SceneManager.LoadScene("ChooseLaunchMode");
    }

    public void StartGame()
    {
        MapManager.Instance.StartGame();
        SceneManager.LoadScene("S_MainGame");
    }
}
