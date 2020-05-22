using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public void TurnEnd()
    {
        //TODO: Передача хода монстрам
        PlayerController pc = player.GetComponent<PlayerController>();
        //восстанавливаем энергию
        pc.TurnStart();
    }





}
