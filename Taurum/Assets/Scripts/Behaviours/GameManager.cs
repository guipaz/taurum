using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DungeonGenerator))]
[RequireComponent(typeof(KeyboardController))]
public class GameManager : MonoBehaviour {
    
    // managers, controllers and generators
    DungeonGenerator generator;
    KeyboardController controller;
    CameraController cameraController;
    DungeonManager dungeonManager;
    HUDManager hudManager;

    // game flow
    Command playerAction;
    bool waitForAction;
    public int turn;

    void Awake()
    {
        generator = GetComponent<DungeonGenerator>();
        cameraController = Camera.main.GetComponent<CameraController>();
        hudManager = GetComponent<HUDManager>();

        controller = GetComponent<KeyboardController>();
        controller.CommandPressed += KeyboardController_CommandPressed;
    }

    void Start()
    {
        // creates the dungeon ambient
        dungeonManager = generator.Generate();
        dungeonManager.PlayerMoved += DungeonManager_PlayerMoved;
        dungeonManager.Setup();

        // registers events
        dungeonManager.player.EntityTick += Player_EntityTick;
        
        // sets the player data
        dungeonManager.player.data = CreatePlaceholderPlayer();

        // updates the HUD
        hudManager.UpdateName(dungeonManager.player.data.name);
        hudManager.UpdateHP(dungeonManager.player.data.Current(TStat.HP));
        hudManager.UpdateMP(dungeonManager.player.data.Current(TStat.MP));

        // prepares the turn
        waitForAction = true;
        turn = 0;
        hudManager.UpdateTurn(turn);
    }

    PlayerData CreatePlaceholderPlayer()
    {
        PlayerData pData = new PlayerData();
        pData.name = "Stryfe";

        pData.attributes[TValue.Base, TAttribute.Strenght] = 1;
        pData.attributes[TValue.Base, TAttribute.Agility] = 1;
        pData.attributes[TValue.Base, TAttribute.Intelligence] = 1;
        
        pData.Calculate();

        return pData;
    }

    private void DungeonManager_PlayerMoved(Vector2 newPosition)
    {
        cameraController.UpdatePosition(dungeonManager.CanvasToWorld(newPosition));
    }

    private void KeyboardController_CommandPressed(Command key)
    {
        if (!waitForAction)
            return;

        playerAction = key;

        waitForAction = false;
        Tick();
    }

    void Tick()
    {
        // simulates the whole turn
        List<BaseVisual> entities = dungeonManager.GetEntities();

        foreach (BaseVisual e in entities)
            e.Tick();

        waitForAction = true;
        turn++;
        hudManager.UpdateTurn(turn);
    }

    private void Player_EntityTick(BaseVisual visual)
    {
        switch (playerAction)
        {
            case Command.MoveUp:
                dungeonManager.MovePlayer(new Vector2(0, -1));
                break;
            case Command.MoveDown:
                dungeonManager.MovePlayer(new Vector2(0, 1));
                break;
            case Command.MoveLeft:
                dungeonManager.MovePlayer(new Vector2(-1, 0));
                break;
            case Command.MoveRight:
                dungeonManager.MovePlayer(new Vector2(1, 0));
                break;
        }
    }
}
