using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static int MaxItemsListLength = 100;
    public Item[] ItemsList = new Item[MaxItemsListLength];
    public PlayerController player;
    public string EnemyTag = "Enemy";
    public float FlagTimer = 0.5f;  //время на которое становится доступен флаг хода монстра
    public float Length = 100f; //длина игрового поля
    public float Witdh = 100f; //ширина игрового поля
    public long Score = 0;
    public float EnemiesNotInRange; //расстояение на котором противники становятся невидимы

    private List<GameObject> Enemies = new List<GameObject>();  //все монстры на карте
    private HUD HUD;

    private void Start()
    {   //получаем список всех enemy на карте

        HUD = transform.Find("/HUD").GetComponent<HUD>();

        for(int i = 0; i < ItemsList.Length; i++)
        {   //проверка соотетствия id предметов с их позицией в массиве предметов
            if(ItemsList[i] != null)
            {
                if(ItemsList[i].id != i)
                {
                    throw new System.Exception("The id is not associated with position in array!");
                }
            }
        }


        Enemies.AddRange(GameObject.FindGameObjectsWithTag(EnemyTag));
    }

    //конец игры
    public void GameOver()
    {
        HUD.DisplayGameOver(Score);
    }

    public void AddScore(int score)
    {
        Score += score;
    }


    public void PlayerTurnEnd()
    {
        StartCoroutine(SetEnemiesTurn());
    }

    //раздача флагов действий монстрам
    IEnumerator SetEnemiesTurn()
    {
        //сообщаем каждому мобу по очереди разрешение на ход
        for(int i = 0; i < Enemies.Count; i++)
        {
            if(Enemies[i] == null)
            {
                Enemies.Remove(Enemies[i]);
                continue;
            }

            Enemy enemy = Enemies[i].GetComponent<Enemy>();
            
            if (enemy.GetAlive())
            {
                enemy.PlayerInRange(true);
                if ((Enemies[i].transform.position - player.transform.position).magnitude > EnemiesNotInRange)
                {   //игрок слишком далеко
                    enemy.PlayerInRange(false);
                }
                enemy.TurnAllowed();
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
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i] == null)
                {
                    Enemies.Remove(Enemies[i]);
                    continue;
                }

                if (Enemies[i].GetComponent<Enemy>().GetTurnFlag())
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
