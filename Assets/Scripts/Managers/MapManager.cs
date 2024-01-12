using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(MapManager)), CanEditMultipleObjects]
public class EditorMapManager : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapManager mapManager = (MapManager)target;

        if (GUILayout.Button("SaveMapToJSON"))
            mapManager.MSaveMap();
        if (GUILayout.Button("UnloadMap"))
            mapManager.UnloadMap();
        if (GUILayout.Button("LoadMap"))
            mapManager.MLoadMap();
    }
}

#endif

[Serializable]
public struct MapObject
{
    public string type;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public MapObject(string itype, Vector3 iposition, Quaternion irotation, Vector3 iscale)
    {
        type = itype;
        position = iposition;
        rotation = irotation;
        scale = iscale;
    }
}

[Serializable]
public class Map
{
    public List<MapObject> Objects = new();
}

public class MapManager : SingletonPersistent<MapManager>
{
    [SerializeField]
    private string m_MapName;

    [SerializeField]
    private AssetLabelReference m_ObjectLabel;

    [SerializeField]
    GameObject mapFieldPrefab;

    List<GameObject> mapFields = new();

    private int m_SelectedMapIndex = -1;

    [HideInInspector]
    public string SelectedMapName = "";

    public void SaveMap(string map_name)
    {
        if (map_name == "")
        {
            Debug.Log("Empty map name");
            return;
        }

        Map map = new();

        foreach (Transform ob in transform)
        {
            string name;
            int index = ob.name.IndexOf(' ');
            if (index == -1)
                name = ob.name;
            else
                name = ob.name[..index];

            Debug.Log("Adding " + name);
            map.Objects.Add(new MapObject(name, ob.position, ob.rotation, ob.localScale));
        }

        string json = JsonUtility.ToJson(map, true);

        Debug.Log(Application.persistentDataPath + Path.DirectorySeparatorChar + "Maps" + Path.DirectorySeparatorChar + map_name + ".map");

        File.WriteAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Maps" + Path.DirectorySeparatorChar + map_name + ".map", json);
    }

    public void MSaveMap()
    {
        SaveMap(m_MapName);
    }

    public void UnloadMap()
    {
        foreach(Transform ob in transform)
        {
            Debug.Log("Destroying " + ob.name);
            Destroy(ob.gameObject);
        }
    }

    public AsyncOperationHandle<IList<GameObject>> LoadMap(string map_name)
    {
        string json;

        try
        {
            json = File.ReadAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Maps" + Path.DirectorySeparatorChar + map_name + ".map");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return new ();
        }

        Map map = JsonUtility.FromJson<Map>(json);

        Dictionary<string, GameObject> mapObjects = new ();

        AsyncOperationHandle<IList<GameObject>> operation = Addressables.LoadAssetsAsync<GameObject>(m_ObjectLabel, (sprite) => { });
            
        operation.Completed += (asyncOperationHandle) =>
        {
            foreach(GameObject ob in asyncOperationHandle.Result)
            {
                Debug.Log("Adding to loaded " + ob.name);
                mapObjects.Add(ob.name, ob);
            }

            foreach (MapObject ob in map.Objects)
            {
                GameObject gob = Instantiate(mapObjects[ob.type], ob.position, ob.rotation, transform);
                gob.transform.localScale = ob.scale;
            }
        };

        return operation;
    }

    public void MLoadMap()
    {
        LoadMap(m_MapName);
    }

    public List<string> ScanForMaps()
    {
        Debug.Log("Scanning for maps");

        DirectoryInfo info = new (Application.persistentDataPath + Path.DirectorySeparatorChar + "Maps");
        FileInfo[] fileInfo = info.GetFiles();

        List<string> list = new ();

        foreach (var file in fileInfo)
        {
            Debug.Log(file.Extension);
            if (file.Extension != ".map")
                continue;

            list.Add(file.Name[..file.Name.IndexOf('.')]);
        }

        return list;
    }

    public void FillMapPanel(List<string> names)
    {
        GameObject mapPanel = GameObject.FindGameObjectWithTag("ChooseMapPanel");
        Debug.Log(mapPanel);

        foreach (string name in names)
        {
            // Add map toggle field to panel 
            GameObject mapField = Instantiate(mapFieldPrefab, mapPanel.transform);
            mapField.GetComponentInChildren<Text>().text = name;
            mapField.GetComponent<Toggle>().onValueChanged.AddListener(DisableAllOtherTogles);

            mapFields.Add(mapField);
        }
    }

    public void StartGame()
    {
        mapFields.Clear();
    }

    private void DisableAllOtherTogles(bool newValue)
    {
        if (newValue == false)
        {
            m_SelectedMapIndex = -1;
            SelectedMapName = "";
            return;
        }

        if (m_SelectedMapIndex != -1)
            mapFields[m_SelectedMapIndex].GetComponent<Toggle>().isOn = false;

        for (int i = 0; i < mapFields.Count; i++)
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
