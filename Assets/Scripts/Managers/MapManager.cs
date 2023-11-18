using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MapManager : SingletonNetworkPersistent<MapManager>
{
    [SerializeField]
    GameObject mapFieldPrefab;

    GameObject mapPanel;

    List<GameObject> mapFields = new List<GameObject>();

    private int m_SelectedMapIndex = -1;

    public string SelectedMapName = "";

    private void Start()
    {
        ScanForMaps();
    }

    public void ScanForMaps()
    {
        mapPanel = GameObject.FindGameObjectWithTag("ChooseMapPanel");
        Debug.Log(mapPanel);

        Debug.Log("Scanning for maps");

        DirectoryInfo info = new DirectoryInfo("Assets/Resources/Maps");
        FileInfo[] fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
            Debug.Log(file.Extension);
            if (file.Extension != ".prefab")
                continue;

            // Add map toggle field to panel 
            GameObject mapField = Instantiate(mapFieldPrefab, mapPanel.transform);
            mapField.GetComponentInChildren<Text>().text = file.Name[..file.Name.IndexOf('.')];
            mapField.GetComponent<Toggle>().onValueChanged.AddListener(DisableAllOtherTogles);

            mapFields.Add(mapField);
        }
    }

    public void StartGame()
    {
        mapFields.Clear();
    }

    public void DisableAllOtherTogles(bool newValue)
    {
        if (newValue == false)
        {
            m_SelectedMapIndex = -1;
            SelectedMapName = "";
            return;
        }

        if (m_SelectedMapIndex != -1)
            mapFields[m_SelectedMapIndex].GetComponent<Toggle>().isOn = false;

        for (int i=0; i < mapFields.Count; i++)
        {
            if (mapFields[i].GetComponent<Toggle>().isOn)
            {
                m_SelectedMapIndex = i;
                SelectedMapName = mapFields[i].GetComponentInChildren<Text>().text;
                break;
            }
        }
    }
}
