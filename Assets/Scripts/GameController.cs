using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public static bool isEnemiesTurnEnd = true;

    public void EnemiesTurnEnd()
    {
        isEnemiesTurnEnd = false;
    }

    public void EnemiesStillTurn()
    {
        isEnemiesTurnEnd = true;
    }

    public void TurnEnd()
    {
        //TODO: Передача хода монстрам
        PlayerController pc = player.GetComponent<PlayerController>();
        //восстанавливаем энергию
        pc.TurnStart();
    }

}
