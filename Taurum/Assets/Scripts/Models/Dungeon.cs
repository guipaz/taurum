using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Dungeon
{
    // data
    public List<Room> rooms;
    public System.Random random;
    
    // working variables
    Room initialRoom;
    Queue<Room> queue;
    List<Node> positioned;
    List<Node> unwrapped;

    // result canvas
    public char[,] canvas { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }
    public Vector2 initialPosition { get; private set; }

    public Dungeon(List<RoomTemplate> templates, int minRooms, int maxRooms, string seed = null)
    {
        if (seed == null)
            seed = DateTime.Now.Ticks.ToString();

        random = new System.Random(seed.GetHashCode());

        Debug.Log("Dungeon seed: " + seed);

        // Instantiate the rooms based on the templates
        rooms = new List<Room>();
        int nR = random.Next(minRooms, maxRooms + 1);
        int id = 0;
        for (int i = 0; i < nR; i++)
            rooms.Add(new Room(templates[random.Next(0, templates.Count)], id++));

        Connect();
        Position();
        Unwrap();
        Offset();
        PaintCanvas();
        SetInitialPosition();

        CorridorTracer tracer = new CorridorTracer();
        tracer.Trace(this);

        CloseWalls();
    }
    
    void Connect()
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("No room to create canvas, aborting");
            return;
        }
        
        // registers all the rooms remaining to be connected
        List<Room> remainingRooms = new List<Room>(rooms);

        // creates a queue for connecting the rooms
        queue = new Queue<Room>();

        // chooses the initial room and enqueues it
        initialRoom = remainingRooms[random.Next(0, remainingRooms.Count)];
        queue.Enqueue(initialRoom);
        remainingRooms.Remove(initialRoom);

        // connects ALL rooms!!11!
        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();

            while (current.AvailableExits.Count > 0)
            {
                // local variable to iterate every room if necessary without damaging the external variable
                List<Room> aux = new List<Room>(remainingRooms);
                Room adjacent = null;
                while (aux.Count > 0)
                {
                    //TODO constraints for a good room instead of random
                    Room r = aux[random.Next(0, aux.Count)];
                    aux.Remove(r);

                    if (r.AvailableExits.Count == 0)
                        continue;
                    
                    adjacent = r;

                    // if has only one entrance, continue to search for another one
                    // if not, just chooses this one
                    if (r.AvailableExits.Count > 1)
                        break;
                }

                // didn't found any room, get outta here
                if (adjacent == null)
                    break;

                bool connected = current.Connect(adjacent);
                if (!connected)
                {
                    Debug.LogAssertion("Couldn't connect room!");
                    break;
                }

                queue.Enqueue(adjacent);
                remainingRooms.Remove(adjacent);
            }   
        }

        // removes the ones that weren't connected
        foreach (Room r in remainingRooms)
            rooms.Remove(r);
    }

    public Vector2 GetSpawnPosition()
    {
        Room r = rooms[random.Next(0, rooms.Count)];
        List<Vector2> positions = new List<Vector2>();
        for (int y = 0; y < r.size.y; y++)
            for (int x = 0; x < r.size.x; x++)
                if (r.template.tiles[x, y] == 'x')
                    positions.Add(new Vector2(x, y));

        //TODO check collision for objects, player, enemies, items

        return positions[random.Next(0, positions.Count)] + r.position;
    }

    void Position()
    {
        positioned = new List<Node>();
        PositionInner(initialRoom, Vector2.zero);
    }

    void PositionInner(Node n, Vector2 pos)
    {
        n.position = pos;
        positioned.Add(n);

        //Debug.Log(string.Format("Positioned node {0} at {1}", n.id, pos));

        foreach (KeyValuePair<NodeExit, NodeConnection> c in n.connections)
        {
            if (c.Value != null && !positioned.Contains(c.Value.node))
            {
                Vector2 nodePos = pos;
                int axisChange = 0;

                NodeExit exit = c.Key;
                NodeExit adjExit = c.Value.localExit;
                Node adjNode = c.Value.node;

                switch (exit.direction)
                {
                    case Direction.Right:
                        //if (adjExit.direction == Direction.Up)
                        //    axisChange = (int)(Math.Abs(n.size.y - adjNode.size.y) + exit.position.y + 1);

                        nodePos += new Vector2(n.size.x + 1, axisChange);
                        break;

                    case Direction.Left:
                        //if (adjExit.direction == Direction.Up)
                        //    axisChange = (int)(Math.Abs(n.size.y - adjNode.size.y) + exit.position.y + 1);

                        nodePos += new Vector2(-adjNode.size.x - 1, axisChange);
                        break;

                    case Direction.Down:
                        //if (adjExit.direction == Direction.Left)
                        //    axisChange = (int)(Math.Abs(n.size.x - adjNode.size.x) + exit.position.x + 1);
                        //else if (adjExit.direction == Direction.Right)
                        //    axisChange = (int)(Math.Abs(n.size.x - adjNode.size.x) - exit.position.x - 1);

                        nodePos += new Vector2(axisChange, n.size.y + 1);
                        break;

                    case Direction.Up:
                        //if (adjExit.direction == Direction.Left)
                        //    axisChange = (int)(Math.Abs(n.size.x - adjNode.size.x) + exit.position.x + 1);
                        //else if (adjExit.direction == Direction.Right)
                        //    axisChange = (int)(Math.Abs(n.size.x - adjNode.size.x) - exit.position.x - 1);

                        nodePos += new Vector2(axisChange, -adjNode.size.y - 1);
                        break;
                }

                PositionInner(c.Value.node, nodePos);
            }
        }

        //Debug.Log(string.Format("Finished node {0} children", n.id));
    }

    void Unwrap()
    {
        unwrapped = new List<Node>();
        UnwrapInner(initialRoom);
    }

    void UnwrapInner(Node r)
    {
        if (unwrapped.Contains(r))
            return;

        bool overlapped = true;
        Direction direction = Direction.None;
        int safetyLock = 10;

        while (overlapped && safetyLock > 0)
        {
            safetyLock--;
            overlapped = false;

            foreach (Node adj in unwrapped)
            {
                if (r.bounds.Overlaps(adj.bounds))
                {
                    overlapped = true;

                    //Debug.Log(string.Format("Overlapped ({0} and {1}: {2} and {3}", r.id, adj.id, r.bounds, adj.bounds));

                    //TODO random x or y
                    if (direction == Direction.Left || (direction == Direction.None && r.position.x < adj.position.x))
                    {
                        r.position = new Vector2(adj.position.x - Math.Max(r.bounds.width, adj.bounds.width) - 1, r.position.y);
                        direction = Direction.Left;
                    }
                    else // right
                    {
                        r.position = new Vector2(adj.position.x + Math.Max(r.bounds.width, adj.bounds.width) + 1, r.position.y);
                        direction = Direction.Right;
                    }

                    //Debug.Log(string.Format("Moved {0} to {1}", r.id, r.bounds));
                }
            }
        }

        //if (safetyLock == 0)
        //    Debug.LogError("Safety lock activated!");

        unwrapped.Add(r);

        foreach (NodeConnection c in r.connections.Values)
            if (c != null)
                UnwrapInner(c.node);
    }

    void Offset()
    {
        int xOffset = 0;
        int yOffset = 0;
        
        foreach (Room r in rooms)
        {
            if (r.position.x < xOffset)
                xOffset = (int)r.position.x;
            if (r.position.y < yOffset)
                yOffset = (int)r.position.y;
        }

        xOffset *= -1;
        yOffset *= -1;

        foreach (Room r in rooms)
        {
            r.position = new Vector2(r.position.x + xOffset, r.position.y + yOffset);
        }
    }

    void SetInitialPosition()
    {
        initialPosition = GetSpawnPosition();
    }

    void PaintCanvas()
    {
        int w = 0;
        int h = 0;
        
        foreach (Room r in rooms)
        {
            int rW = (int)(r.bounds.x + r.bounds.width);
            int rH = (int)(r.bounds.y + r.bounds.height);

            if (rW > w)
                w = rW;
            if (rH > h)
                h = rH;
        }
        
        width = w + 4;
        height = h + 4;
        canvas = new char[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                canvas[x, y] = ' ';

        foreach (Room r in rooms)
        {
            int xOff = (int)r.position.x + 2;
            int yOff = (int)r.position.y + 2;

            for (int y = 0; y < r.template.height; y++)
            {
                for (int x = 0; x < r.template.width; x++)
                {
                    char tile = r.template.tiles[x, y];
                    canvas[xOff + x, yOff + y] = tile;
                }
            }

            r.position = new Vector2(xOff, yOff);

            // remove unused doors
            foreach (KeyValuePair<NodeExit, NodeConnection> pair in r.connections)
            {
                if (pair.Value == null)
                    canvas[(int)(r.position.x + pair.Key.position.x), (int)(r.position.y + pair.Key.position.y)] = '-';
            }
        }

    }

    void CloseWalls()
    {
        Vector2[] adjs = new Vector2[]
        {
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(-1, -1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(1, 1),
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = canvas[x, y];
                Vector2 curPos = new Vector2(x, y);

                if (c == 'c') // corridor
                {
                    foreach (Vector2 adj in adjs)
                    {
                        Vector2 pos = curPos + adj;
                        if (pos.x >= 0 && pos.x < width &&
                            pos.y >= 0 && pos.y < height)
                        {
                            char a = canvas[(int)pos.x, (int)pos.y];
                            if (a == ' ')
                                canvas[(int)pos.x, (int)pos.y] = '-'; // turns into wall
                        }
                    }

                    canvas[x, y] = 'x'; // turns into normal floor
                }
            }
        }
    }

    public enum Direction
    {
        None, Up, Down, Left, Right
    }

    public class NodeExit
    {
        public Vector2 position;
        public Direction direction;

        public NodeExit(Vector2 pos, Direction dir)
        {
            position = pos;
            direction = dir;
        }

        public Direction GetOpposite()
        {
            switch (direction)
            {
                case Direction.Down:
                    return Direction.Up;
                case Direction.Up:
                    return Direction.Down;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
            }

            return direction;
        }
    }

    public class NodeConnection
    {
        public Node node;
        public NodeExit localExit;

        public NodeConnection(Node n, NodeExit conn)
        {
            node = n;
            localExit = conn;
        }
    }

    public class Node
    {
        public int id;

        Vector2 _pos;
        public Vector2 position
        {
            get
            {
                return _pos;
            }

            set
            {
                _pos = value;
                bounds = new Rect(value.x, value.y, size.x, size.y);
            }
        }

        Vector2 _size;
        public Vector2 size
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;
                bounds = new Rect(position.x, position.y, value.x, value.y);
            }
        }

        public Rect bounds;
        public Dictionary<NodeExit, NodeConnection> connections;

        public Node()
        {
            id = 0;
            position = Vector2.zero;
            size = Vector2.zero;
            bounds = new Rect();
            connections = new Dictionary<NodeExit, NodeConnection>();
        }
    }

    public class Room : Node
    {
        public RoomTemplate template;
        
        public List<NodeExit> AvailableExits
        {
            get
            {
                List<NodeExit> available = new List<NodeExit>();
                foreach (KeyValuePair<NodeExit, NodeConnection> pair in connections)
                    if (pair.Value == null)
                        available.Add(pair.Key);
                return available;
            }
        }

        public Room(RoomTemplate template, int id) : base()
        {
            this.template = template;
            this.id = id;
            this.size = new Vector2(template.width, template.height);
            this.bounds = new Rect(0, 0, size.x, size.y);

            // sets the available exits
            foreach (Vector2 exit in template.exits)
            {
                Direction direction = Direction.Up;
                if (exit.x == 0)
                    direction = Direction.Left;
                else if (exit.y == 0)
                    direction = Direction.Up;
                else if (exit.x == template.width - 1)
                    direction = Direction.Right;
                else if (exit.y == template.height - 1)
                    direction = Direction.Down;
                else
                    continue;

                NodeExit nExit = new NodeExit(exit, direction);
                connections[nExit] = null;
            }
        }

        public bool Connect(Room adjacent)
        {
            List<NodeExit> curExits = AvailableExits;
            List<NodeExit> adjExits = adjacent.AvailableExits;
            
            NodeExit curChosen = null;
            NodeExit adjChosen = null;
            int quality = 0;
            foreach (NodeExit v1 in curExits)
            {
                foreach (NodeExit v2 in adjExits)
                {
                    int q = 0; // bad
                    if (v1.GetOpposite() == v2.direction)
                    {
                        q = 2; // good
                    } else if (v1.direction != v2.direction)
                    {
                        q = 1; // ok
                    }
                    
                    if (curChosen == null || q > quality)
                    {
                        curChosen = v1;
                        adjChosen = v2;
                        quality = q;
                    }
                }
            }

            if (curChosen != null && adjChosen != null)
            {
                NodeConnection c1 = new NodeConnection(adjacent, adjChosen);
                connections[curChosen] = c1;

                NodeConnection c2 = new NodeConnection(this, curChosen);
                adjacent.connections[adjChosen] = c2;

                return true;
            }

            return false;
        }

        public bool IsExitAvailable(Vector2 pos)
        {
            List<NodeExit> exits = AvailableExits;
            foreach (NodeExit exit in exits)
                if (exit.position == pos)
                    return true;
            return false;
        }
    }
}
