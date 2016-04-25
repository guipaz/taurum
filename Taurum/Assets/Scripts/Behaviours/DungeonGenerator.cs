using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DungeonGenerator : MonoBehaviour
{
    List<GameObject> roomList = new List<GameObject>();

    List<RoomTemplate> templates;

    Dungeon dungeon;

    public string seed;
    public int minimumRooms;
    public int maximumRooms;

    void Awake()
    {
        RoomTemplateReader reader = new RoomTemplateReader();
        templates = reader.ReadTemplateFile("rooms");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (GameObject obj in roomList)
                Destroy(obj);
            roomList.Clear();
            Generate();
        }
    }

    public DungeonManager Generate()
    {
        if (seed == "")
            seed = null;

        dungeon = new Dungeon(templates, minimumRooms, maximumRooms, seed);

        GameObject obj = InstantiateCanvas(dungeon);
        DungeonManager manager = obj.AddComponent<DungeonManager>();
        manager.dungeon = dungeon;

        return manager;
    }

    GameObject InstantiateCanvas(Dungeon d)
    {
        GameObject cObj = new GameObject("Canvas");
        for (int y = 0; y < d.height; y++)
        {
            for (int x = 0; x < d.width; x++)
            {
                char tile = d.canvas[x, y];
                
                GameObject prefab = GetTilePrefab(tile);
                if (prefab == null)
                    continue;

                GameObject obj = Instantiate(prefab);
                obj.transform.position = new Vector3(x, d.height - y - 1, 0);
                obj.transform.parent = cObj.transform;
            }
        }

        roomList.Add(cObj);

        return cObj;
    }

    GameObject GetTilePrefab(char c)
    {
        string name = "blank";
        switch (c)
        {
            case '+':
                name = "wall";
                break;
            case '-':
                name = "wall";
                break;
            case '|':
                name = "wall";
                break;
            case '{':
                name = "door";
                break;
            case 'x':
                name = "floor";
                break;
            case 'o':
                name = "floor";
                break;
        }

        if (name != "blank")
            return Resources.Load<GameObject>("prefabs/" + name);
        return null;
    }
}
