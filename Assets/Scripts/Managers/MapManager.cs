using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JetBrains.Annotations;


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
            mapManager.SaveMapToJSON();
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
    public List<MapObject> Objects = new ();
}

public class MapManager : Singleton<MapManager>
{
    [SerializeField]
    private string m_MapName;

    public void SaveMapToJSON()
    {
        if (m_MapName == "")
        {
            Debug.Log("Empty map name");
            return;
        }

        Map map = new ();

        foreach (Transform ob in transform)
        {
            string name;
            int index = ob.name.IndexOf(' ');
            if (index == -1)
                name = ob.name;
            else
                name = ob.name[..index];

            Debug.Log("Adding " + name);
            map.Objects.Add(new MapObject( name, ob.position, ob.rotation ));
        }

        string json = JsonUtility.ToJson(map, true);

        Debug.Log(Application.persistentDataPath + Path.DirectorySeparatorChar + "Maps" + Path.DirectorySeparatorChar + m_MapName + ".json");

        File.WriteAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Maps" + Path.DirectorySeparatorChar + m_MapName + ".json", json);
    }
}
