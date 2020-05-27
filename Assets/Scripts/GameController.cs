using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    
    public PlayerController player;
    public string EnemyTag = "Enemy";
    public float FlagTimer = 0.5f;  //время на которое становится доступен флаг хода монстра
    public float Length = 100f; //длина игрового поля
    public float Witdh = 100f; //ширина игрового поля
    public long Score = 0;

    private GameObject[] Enemies;  //все монстры на карте

    private void Start()
    {   //получаем список всех enemy на карте
        Enemies = GameObject.FindGameObjectsWithTag(EnemyTag);
    }

    public void AddScore(int score)
    {
        this.Score += score;
    }


    public void PlayerTurnEnd()
    {
        StartCoroutine(SetEnemiesTurn());
    }

    //раздача флагов действий монстрам
    IEnumerator SetEnemiesTurn()
    {
        //сообщаем каждому мобу по очереди разрешение на ход
        foreach (GameObject e in Enemies)
        {
            Enemy enemy = e.GetComponent<Enemy>();
            if (enemy.GetAlive())
            {
                e.GetComponent<Enemy>().TurnAllowed();
                yield return null;  //пропускаем два фрейма
                yield return null;
            }
        }

        StartCoroutine(ReturnToPlayer());
        /*
        Enemy.isEnemiesTurn = true; //даем флажок для всех enemy
        isEnemiesTurn = true;   //считаем, что сейчас ход противника
        yield return new WaitForSeconds(flagTimer); //ждём
        isEnemiesTurn = false;  //считаем, что ход противника как бы кончился, но если есть живые противники, они выставят флаг заново
        Enemy.isEnemiesTurn = false; //забираем флажок у всех enemy - за это время каждый enemy должен успеть взять свой флаг
        StartCoroutine(ReturnToPlayer()); //начинаем слушать
        */
    }

    //ожидание возвращение всех флагов
    IEnumerator ReturnToPlayer()
    {
        bool isEnemiesTurn = true;
        while (isEnemiesTurn)
        {
            isEnemiesTurn = false;
            foreach (GameObject e in Enemies)
            {
                if (e.GetComponent<Enemy>().GetTurnFlag())
                {   //если хотя бы у одного моба включен флаг ->
                    isEnemiesTurn = true;
                    yield return null;
                    continue;
                }
            }
            
        }
        //восстанавливаем энергию
        player.TurnStart();
    }

}
