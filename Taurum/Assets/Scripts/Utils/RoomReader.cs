using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class RoomTemplateReader
{
    List<RoomTemplate> processedRooms;
    
    bool openedRoom = false;
    bool openedTiles = false;

    RoomTemplate currentRoom;
    List<char[]> currentRows;
    int currentWidthConstraint;

    static char[] objectList = new char[] { 'o', '{' };

    public List<RoomTemplate> ReadTemplateFile(string filename)
    {
        processedRooms = new List<RoomTemplate>();

        string fullPath = Application.dataPath + "/Resources/rooms/" + filename + ".txt";
        if (!File.Exists(fullPath))
        {
            Debug.LogError(string.Format("File not found: {0}", fullPath));
            return null;
        }

        string[] content = File.ReadAllText(fullPath).Split(new char[] { '\n' }); // not performatic
        foreach (string cn in content)
        {
            string s = cn.Replace("\r", "");

            if (s.StartsWith("#")) // comment
                continue;

            if (s.StartsWith(":")) // start/end commands
            {
                if (s.StartsWith(":start_room"))
                {
                    if (!openedRoom && !openedTiles)
                    {
                        openedRoom = true;
                        currentRoom = new RoomTemplate();
                    } else
                    {
                        Debug.LogError(string.Format("Already opened room or tiles in line: {0}", s));
                        return null;
                    }
                } else if (s.StartsWith(":end_room"))
                {
                    if (openedRoom && !openedTiles)
                    {
                        openedRoom = false;
                        processedRooms.Add(currentRoom);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Didn't open room or didn't close tiles in line: {0}", s));
                        return null;
                    }
                } else if (s.StartsWith(":start_tiles"))
                {
                    if (openedRoom && !openedTiles)
                    {
                        openedTiles = true;
                        currentRows = new List<char[]>();
                    }
                    else
                    {
                        Debug.LogError(string.Format("Didn't open room ir already opened tiles in line: {0}", s));
                        return null;
                    }
                } else if (s.StartsWith(":end_tiles"))
                {
                    if (openedRoom && openedTiles && currentRows.Count > 0)
                    {
                        openedTiles = false;
                        char[,] tiles = new char[currentWidthConstraint, currentRows.Count];
                        for (int y = 0; y < currentRows.Count; y++)
                        {
                            char[] line = currentRows[y];
                            for (int x = 0; x < line.Length; x++)
                            {
                                tiles[x, y] = line[x];
                            }
                        }

                        currentRoom.tiles = tiles;
                        currentRoom.width = currentWidthConstraint;
                        currentRoom.height = currentRows.Count;
                    }
                    else
                    {
                        Debug.LogError(string.Format("Didn't open room or tiles in line: {0}", s));
                        return null;
                    }
                }

                continue;
            }

            if (openedTiles) // map tiles
            {
                if (s.Length == 0)
                {
                    Debug.LogError("Empty line in room");
                    return null;
                }

                char[] tiles = s.ToCharArray();
                currentRows.Add(tiles);

                // search for objects
                for (int i = 0; i < tiles.Length; i++)
                {
                    char c = tiles[i];
                    if (!objectList.Contains(c))
                        continue;

                    int x = i % currentWidthConstraint;
                    int y = currentRows.Count - 1;
                    switch (c)
                    {
                        case '{': // exit
                        case 'o': // exit
                            currentRoom.exits.Add(new Vector2(x, y));
                            break;
                    }
                }

                if (currentRows.Count == 1)
                    currentWidthConstraint = tiles.Length;

                continue;
            }

            if (openedRoom && s.Length > 0) // tags and other info
            {
                if (s.StartsWith("tags"))
                {
                    string[] tags = s.Split(new char[] { ' ' });
                    foreach (string tag in tags)
                    {
                        if (tag == "tags")
                            continue;

                        currentRoom.tags.Add(tag);
                    }
                }
            }
        }

        return processedRooms;
    }
}
