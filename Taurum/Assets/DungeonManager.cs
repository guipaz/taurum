using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class DungeonManager : MonoBehaviour
{
    public Dungeon dungeon { get; set; }
    public PlayerVisual player { get; private set; }
    public List<EnemyVisual> enemies { get; private set; }

    // Events
    public delegate void PlayerMovedHandler(Vector2 newPosition);
    public event PlayerMovedHandler PlayerMoved;

    public void Setup()
    {
        GameObject playerObj = Instantiate(Resources.Load<GameObject>("prefabs/player"));
        player = playerObj.GetComponent<PlayerVisual>();
        SetPlayerPosition(dungeon.initialPosition);

        enemies = new List<EnemyVisual>();
        int enemyN = dungeon.random.Next(5, 10);
        for (int i = 0; i < enemyN; i++)
            Spawn("orc");
    }

    public void MovePlayer(Vector2 movement)
    {
        Vector2 newPosition = player.position + movement;

        if (CanMove(newPosition))
            SetPlayerPosition(newPosition);
    }

    public bool CanMove(Vector2 position)
    {
        if (position.x < 0 || position.x >= dungeon.width || position.y < 0 || position.y >= dungeon.height)
            return false;
        
        char c = dungeon.canvas[(int)position.x, (int)position.y];
        if (c == 'x' || c == 'o' || c == '{')
        {
            foreach (EnemyVisual e in enemies)
                if (e.position == position)
                    return false;

            if (player.position == position)
                return false;

            return true;
        }

        return false;
    }

    public void SetPlayerPosition(Vector2 position)
    {
        player.position = position;
        player.transform.position = CanvasToWorld(position);

        if (PlayerMoved != null)
            PlayerMoved(position);
    }

    public void SetEnemyPosition(EnemyVisual visual, Vector2 position)
    {
        visual.position = position;
        visual.transform.position = CanvasToWorld(position);
    }

    public List<TickableVisual> GetEntities()
    {
        List<TickableVisual> entities = new List<TickableVisual>();
        foreach (EnemyVisual v in enemies)
            entities.Add(v);

        entities.Add(player);
        return entities.OrderByDescending(o => o.speed).ToList();
    }

    public void Spawn(string enemy)
    {
        GameObject enemyObj = Instantiate(Resources.Load<GameObject>("prefabs/" + enemy));
        EnemyVisual visual = enemyObj.GetComponent<EnemyVisual>();
        visual.EntityTick += Visual_EntityTick;
        enemies.Add(visual);

        // gets a place to spawn it
        SetEnemyPosition(visual, dungeon.GetSpawnPosition());
    }

    private void Visual_EntityTick(TickableVisual visual)
    {
        //TODO make dynamic
        bool willMove = dungeon.random.Next(0, 3) >= 1; // 75%
        if (willMove)
        {
            bool horizontal = dungeon.random.Next(0, 2) == 0;
            int value = dungeon.random.Next(-1, 2);
            Vector2 movement = new Vector2(horizontal ? value : 0, horizontal ? 0 : value);

            if (CanMove(visual.position + movement))
                SetEnemyPosition((EnemyVisual)visual, visual.position + movement);
        }
    }

    public Vector3 CanvasToWorld(Vector2 canvasPosition)
    {
        return new Vector3(canvasPosition.x, dungeon.height - canvasPosition.y - 1, 0);
    }
}
