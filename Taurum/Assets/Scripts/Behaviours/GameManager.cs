using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DungeonGenerator))]
[RequireComponent(typeof(KeyboardController))]
public class GameManager : MonoBehaviour {
    
    // Managers, controllers and generators
    DungeonGenerator generator;
    KeyboardController controller;
    CameraController cameraController;
    DungeonManager dungeonManager;

    // Game flow
    Command playerAction;
    bool waitForAction;
    int turn;

    void Awake()
    {
        generator = GetComponent<DungeonGenerator>();
        cameraController = Camera.main.GetComponent<CameraController>();

        controller = GetComponent<KeyboardController>();
        controller.CommandPressed += KeyboardController_CommandPressed;
    }

    void Start()
    {
        dungeonManager = generator.Generate();
        dungeonManager.PlayerMoved += DungeonManager_PlayerMoved;
        dungeonManager.Setup();

        dungeonManager.player.EntityTick += Player_EntityTick;

        waitForAction = true;
        turn = 0;
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
        List<TickableVisual> entities = dungeonManager.GetEntities();

        foreach (TickableVisual e in entities)
            e.Tick();

        waitForAction = true;
    }

    private void Player_EntityTick(TickableVisual visual)
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
