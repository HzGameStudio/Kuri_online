using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JetBrains.Annotations;
using System.Net.NetworkInformation;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.VisualScripting;





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

    public MapObject(string itype, Vector3 iposition, Quaternion irotation)
    {
        type = itype;
        position = iposition;
        rotation = irotation;
    }
}

[Serializable]
public class Map
{
    public List<MapObject> Objects = new();
}

public class MapManager : Singleton<MapManager>
{
    [SerializeField]
    private string m_MapName;

    [SerializeField]
    private AssetLabelReference m_ObjectLabel;

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
            map.Objects.Add(new MapObject(name, ob.position, ob.rotation));
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

    public void LoadMap(string map_name)
    {
        string json;

        try
        {
            json = File.ReadAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Maps" + Path.DirectorySeparatorChar + map_name + ".map");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        Map map = JsonUtility.FromJson<Map>(json);

        Dictionary<string, GameObject> mapObjects = new ();

        Addressables.LoadAssetsAsync<GameObject>(m_ObjectLabel, (sprite) => { }).Completed += (asyncOperationHandle) =>
        {
            foreach(GameObject ob in asyncOperationHandle.Result)
            {
                Debug.Log("Adding to loaded " + ob.name);
                mapObjects.Add(ob.name, ob);
            }

            foreach (MapObject ob in map.Objects)
            {
                Instantiate(mapObjects[ob.type], ob.position, ob.rotation, transform);
            }
        };
    }

    public void MLoadMap()
    {
        LoadMap(m_MapName);
    }
}
