using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Text playerName;
    public Text playerHp;
    public Text playerMp;
    public Text turnText;

    public void UpdateName(string name)
    {
        playerName.text = string.Format("{0}, the Mighty", name);
    }

    public void UpdateHP(int hp)
    {
        playerHp.text = string.Format("HP: {0}", hp);
    }

    public void UpdateMP(int mp)
    {
        playerMp.text = string.Format("MP: {0}", mp);
    }

    public void UpdateTurn(int turn)
    {
        turnText.text = string.Format("Turn: {0}", turn);
    }
}
