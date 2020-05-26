using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    
    public PlayerController player;
    public string EnemyTag = "Enemy";
    public float flagTimer = 0.5f;  //время на которое становится доступен флаг хода монстра
    public float length = 100f; //длина игрового поля
    public float witdh = 100f; //ширина игрового поля

    private GameObject[] Enemies;  //все монстры на карте

    private void Start()
    {   //получаем список всех enemy на карте
        Enemies = GameObject.FindGameObjectsWithTag(EnemyTag);
    }

    //пока все противники не скажут, что их ход закончился - переменная в конечном итоге будет оставаться true

    public void PlayerTurnEnd()
    {
        StartCoroutine(SetEnemiesTurn());
    }

    //раздача флагов действий монстрам
    IEnumerator SetEnemiesTurn()
    {
        Debug.Log("EnemiesTurn");
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
                Debug.Log(isEnemiesTurn);
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
