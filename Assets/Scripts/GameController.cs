﻿using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public PlayerController player;
    public static bool isEnemiesTurn = false;   //монстры могут менять эту перменную
    public float flagTimer = 0.5f;  //время на которое становится доступен флаг хода монстра

    //ход противника закончился
    public void EnemyTurnEnd()
    {
        isEnemiesTurn = false;
    }

    //ход противника не закончился
    public void EnemyTurnIsNotEnd()
    {
        isEnemiesTurn = true;
    }

    //пока все противники не скажут, что их ход закончился - переменная в конечном итоге будет оставаться true

    public void PlayerTurnEnd()
    {
        StartCoroutine(SetEnemiesTurn());
    }

    //раздача флагов действий монстрам
    IEnumerator SetEnemiesTurn()
    {
        Enemy.isEnemiesTurn = true; //даем флажок для всех enemy
        isEnemiesTurn = true;   //считаем, что сейчас ход противника
        yield return new WaitForSeconds(flagTimer); //ждём
        isEnemiesTurn = false;  //считаем, что ход противника как бы кончился, но если есть живые противники, они выставят флаг заново
        Enemy.isEnemiesTurn = false; //забираем флажок у всех enemy - за это время каждый enemy должен успеть взять свой флаг
        StartCoroutine(ReturnToPlayer()); //начинаем слушать
    }

    //ожидание возвращение всех флагов
    IEnumerator ReturnToPlayer()
    {
        yield return null; //пропускаем первый фрейм, т.к. в нём ход противника сбросился автоматически
        while (isEnemiesTurn)
        {   //ждём пока монстры не сделают ход
            yield return null;
        }
        //восстанавливаем энергию
        player.TurnStart();
    }

}
