using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_GameMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("S_MainGame");
    }
}
