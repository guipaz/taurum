using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CorridorTracer
{
    Dungeon dungeon;
    Node[,] nodes;
    NodeTrace[,] traces;

    public void Trace(Dungeon d)
    {
        dungeon = d;
        nodes = new Node[dungeon.width, dungeon.height];
        traces = new NodeTrace[dungeon.width, dungeon.height];

        PopulateNodes();
        PopulateAdjacents();

        List<Node> steps = new List<Node>();

        foreach (Dungeon.Room r in dungeon.rooms)
        {
            foreach (KeyValuePair<Dungeon.NodeExit, Dungeon.NodeConnection> pair in r.connections)
            {
                // no connection here
                if (pair.Value == null)
                    continue;

                // grab the exit
                Dungeon.NodeExit exit = pair.Key;
                Node n = nodes[(int)(r.position.x + exit.position.x), (int)(r.position.y + exit.position.y)];

                // acquire the steps from the exit to the respective door
                List<Node> localSteps = Walk(r.position + exit.position, pair.Value.node.position + pair.Value.localExit.position);
                if (localSteps != null)
                    steps.AddRange(localSteps);
            }
        }

        foreach (Node step in steps)
        {
            dungeon.canvas[(int)step.position.x, (int)step.position.y] = 'c';
        }
    }

    void PopulateNodes()
    {
        for (int y = 0; y < dungeon.height; y++)
        {
            for (int x = 0; x < dungeon.width; x++)
            {
                int v = 1;

                switch (dungeon.canvas[x, y])
                {
                    case ' ':
                        v = 0;
                        break;
                    case 'o':
                        v = 2;
                        break;
                }

                nodes[x, y] = new Node(v, new Vector2(x, y));
                traces[x, y] = new NodeTrace(nodes[x, y]);
            }
        }
    }

    void PopulateAdjacents()
    {
        for (int y = 0; y < dungeon.height; y++)
        {
            for (int x = 0; x < dungeon.width; x++)
            {
                NodeTrace n = traces[x, y];

                if (x - 1 >= 0)
                    n.adjacents.Add(traces[x - 1, y]);
                if (x + 1 < dungeon.width)
                    n.adjacents.Add(traces[x + 1, y]);
                if (y - 1 >= 0)
                    n.adjacents.Add(traces[x, y - 1]);
                if (y + 1 < dungeon.height)
                    n.adjacents.Add(traces[x, y + 1]);
            }
        }
    }

    void ClearTraces()
    {
        foreach (NodeTrace n in traces)
        {
            n.parent = null;
            n.FCost = 0;
            n.HCost = 0;
            n.GCost = 0;
        }
    }

    List<Node> Walk(Vector2 start, Vector2 end)
    {
        //Debug.Log(string.Format("Walking from {0} to {1}", start, end));

        ClearTraces();

        Queue<NodeTrace> opened = new Queue<NodeTrace>();
        List<NodeTrace> closed = new List<NodeTrace>();

        NodeTrace startTrace = traces[(int)start.x, (int)start.y];
        startTrace.HCost = GetHCost(start, end);
        startTrace.FCost = startTrace.HCost;
        opened.Enqueue(startTrace);

        NodeTrace current = null;
        int safetyLock = 10000;

        while (opened.Count > 0 && safetyLock > 0)
        {
            safetyLock--;

            current = opened.Dequeue();
            closed.Add(current);

            if (current.node.position == end)
                break;

            foreach (NodeTrace adj in current.adjacents)
            {
                if (adj.node.position == end)
                {
                    adj.parent = current;
                    opened.Clear();

                    opened.Enqueue(adj);
                    break;
                }

                if (adj.node.value != 0 || closed.Contains(adj))
                    continue;

                int GCost = current.GCost + 1;
                int HCost = GetHCost(adj.node.position, end);
                int FCost = GCost + HCost;

                if (FCost < adj.FCost || !opened.Contains(adj))
                {
                    adj.GCost = GCost;
                    adj.HCost = HCost;
                    adj.FCost = FCost;
                    
                    adj.parent = current;
                    if (!opened.Contains(adj))
                        opened.Enqueue(adj);
                }
            }
        }

        if (safetyLock == 0)
            Debug.LogError("Safety lock activated!");

        NodeTrace firstStep = current.parent;
        if (firstStep == null)
        {
            Debug.LogError("No space for corridor!");
            return null;
        }

        List<Node> steps = new List<Node>();
        steps.Add(firstStep.node);

        safetyLock = 100;
        while (firstStep.parent != null && safetyLock > 0)
        {
            safetyLock--;

            firstStep = firstStep.parent;
            steps.Add(firstStep.node);
        }

        if (safetyLock == 0)
            Debug.LogError("Safety lock activated!");

        steps.Reverse();
        steps.RemoveAt(0);
        
        return steps;
    }

    int GetHCost(Vector2 start, Vector2 end)
    {
        return (int)(Math.Abs((start - end).x) + Math.Abs((start - end).y));
    }

    class NodeTrace
    {
        public NodeTrace parent;
        public Node node;

        public List<NodeTrace> adjacents { get; private set; }

        public int FCost; // total
        public int GCost; // start -> here
        public int HCost; // here -> end

        public NodeTrace(Node node)
        {
            this.adjacents = new List<NodeTrace>();
            this.node = node;
        }
    }

    class Node
    {
        public Vector2 position { get; private set; }
        public int value { get; private set; }

        public Node(int value, Vector2 position)
        {
            this.value = value;
            this.position = position;
        }
    }
}