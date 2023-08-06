using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button executePhysiqueButton;

    private bool hasServerStarted = false;
    // Start is called before the first frame update
    void Start()
    {
       

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };

        executePhysiqueButton.onClick.AddListener(() =>
        {

            if (hasServerStarted)
            {

            }

        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
