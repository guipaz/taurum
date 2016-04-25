using UnityEngine;
using System.Collections;

public enum Command
{
    None,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
}

public class KeyboardController : MonoBehaviour {

    public delegate void CommandPressedEvent(Command key);
    public event CommandPressedEvent CommandPressed;

	void Update ()
    {
        Command command = Command.None;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            command = Command.MoveUp;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            command = Command.MoveDown;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            command = Command.MoveLeft;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            command = Command.MoveRight;

        if (command != Command.None && CommandPressed != null)
            CommandPressed(command);
    }
}
