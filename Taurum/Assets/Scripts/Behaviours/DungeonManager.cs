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

        // debug
        int enemyN = dungeon.random.Next(5, 10);
        for (int i = 0; i < enemyN; i++)
            SpawnEnemy("orc");
    }

    public void MovePlayer(Vector2 movement)
    {
        Vector2 newPosition = player.position + movement;

        if (CanMove(newPosition))
        {
            SetPlayerPosition(newPosition);
            return;
        }

        // found something, check if it's an object
        BaseVisual obj = GetObject(newPosition);
        if (obj != null)
            obj.Action();
    }

    public bool CanMove(Vector2 position)
    {
        if (position.x < 0 || position.x >= dungeon.width || position.y < 0 || position.y >= dungeon.height)
            return false;
        
        char c = dungeon.canvas[(int)position.x, (int)position.y];
        if (c == 'x' || c == 'o' || c == '{')
        {
            BaseVisual v = GetObject(position);
            if (v != null)
                return false;

            if (player.position == position)
                return false;

            return true;
        }

        return false;
    }

    public BaseVisual GetObject(Vector2 position)
    {
        // checks for objects
        foreach (EnemyVisual e in enemies)
            if (e.position == position)
                return e;
        return null;
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

    public List<BaseVisual> GetEntities()
    {
        List<BaseVisual> entities = new List<BaseVisual>();
        foreach (EnemyVisual v in enemies)
            entities.Add(v);

        entities.Add(player);
        return entities.OrderByDescending(o => o.GetLevel()).OrderByDescending(o => o.GetSpeed()).ToList();
    }

    public void SpawnEnemy(string enemy)
    {
        GameObject enemyObj = Instantiate(Resources.Load<GameObject>("prefabs/" + enemy));
        EnemyVisual visual = enemyObj.GetComponent<EnemyVisual>();
        visual.data = LoadEnemyData(enemy);

        visual.EntityTick += Visual_EntityTick;
        visual.EnemyAttacked += EnemyVisual_EnemyAttacked;
        visual.EnemyKilled += EnemyVisual_EnemyKilled;

        enemies.Add(visual);

        // gets a place to spawn it
        SetEnemyPosition(visual, dungeon.GetSpawnPosition());
    }

    private void EnemyVisual_EnemyKilled(EnemyVisual visual)
    {
        //TODO give player loot
        Debug.Log(string.Format("{0} died!", visual.data.name));
        Destroy(visual.gameObject);
        enemies.Remove(visual);
    }

    private void EnemyVisual_EnemyAttacked(EnemyVisual enemy)
    {
        EnemyData enemyData = enemy.data;
        PlayerData playerData = player.data;

        Debug.Log(string.Format("{0} is being attacked!", enemyData.name));
        
        bool hit = BattleCalculator.GetHit(playerData.stats, enemyData.stats);
        if (hit)
        {
            int damage = BattleCalculator.GetDamage(playerData.stats, enemyData.stats, TAttackType.Physical);
            Debug.Log(string.Format("{0} took {1} damage!", enemyData.name, damage));
            enemy.ReceiveDamage(damage);
            Debug.Log(string.Format("{0} has {1} HP left!", enemyData.name, enemyData.Current(TStat.HP)));
        } else
        {
            Debug.Log("Player missed!");
        }
    }

    EnemyData LoadEnemyData(string enemy)
    {
        //TODO
        EnemyData data = new EnemyData();
        data.name = "Orc";
        data.stats[TValue.Base, TStat.HP] = 3;
        data.stats[TValue.Base, TStat.MP] = 1;
        data.stats[TValue.Base, TStat.SPD] = 5;
        data.stats[TValue.Base, TStat.DOD] = 1;
        data.stats[TValue.Base, TStat.PAtq] = 3;
        data.stats[TValue.Base, TStat.RAtq] = 1;
        data.stats[TValue.Base, TStat.MAtq] = 1;
        data.stats[TValue.Base, TStat.PDef] = 1;
        data.stats[TValue.Base, TStat.RDef] = 1;
        data.stats[TValue.Base, TStat.MDef] = 1;

        data.Calculate();

        return data;
    }

    private void Visual_EntityTick(BaseVisual visual)
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
