using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RoomTemplate
{
    public int width;
    public int height;

    public List<string> tags;
    public List<Vector2> exits;

    public char[,] tiles;

    public RoomTemplate()
    {
        exits = new List<Vector2>();
        tags = new List<string>();
    }

    public void Print()
    {
        string print = "";

        string tags = "";
        foreach (char s in tags)
            tags += s + " ";

        print += "tags: " + tags;
        print += "\nexists (" + exits.Count + "): ";

        foreach (Vector2 v in exits)
            print += "[" + v.x + ", " + v.y + "] ";

        for (int y = 0; y < height; y++)
        {
            print += "\n";
            for (int x = 0; x < width; x++)
                print += tiles[x,y];
        }

        Debug.Log(print);
    }
}
