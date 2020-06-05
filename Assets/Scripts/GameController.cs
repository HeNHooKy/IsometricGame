using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController : MonoBehaviour
{
    public static int MaxItemsListLength = 100;

    [Header("Настройки игры")]
    [Tooltip("Список всех предметов в игре")]
    public Item[] ItemsList = new Item[MaxItemsListLength];
    [Tooltip("Указатель на игрока")]
    public PlayerController player;
    [Tooltip("Тег монстров")]
    public string EnemyTag = "Enemy";
    [Tooltip("Время на которое становится доступен флаг хода монстра")]
    public float FlagTimer = 0.5f;
    [Tooltip("Длина игрового поля")]
    public float Length = 100f;
    [Tooltip("Ширина игрового поля")]
    public float Witdh = 100f;
    [HideInInspector]
    public long Score = 0;
    [Tooltip("Расстояение на котором противники становятся невидимы")]
    public float EnemiesNotInRange;

    private List<GameObject> enemies = new List<GameObject>();  //все монстры на карте
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
    }

    //пересобрать данные о всех монстрах на карте 
    public void GetEnemiesList()
    {
        //очистим данные о всех монстрах
        enemies.Clear();
        GameObject[] enemiesGO = GameObject.FindGameObjectsWithTag(EnemyTag);
        foreach (GameObject e in enemiesGO)
        {   //добавляем только активных монстров
            if (e.activeInHierarchy)
            {
                enemies.Add(e);
            }
        }
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
        for(int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i] == null)
            {
                enemies.Remove(enemies[i]);
                continue;
            }

            Enemy enemy = enemies[i].GetComponent<Enemy>();
            
            if (enemy.GetAlive())
            {
                enemy.PlayerInRange(true);
                if ((enemies[i].transform.position - player.transform.position).magnitude > EnemiesNotInRange)
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
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null)
                {
                    enemies.Remove(enemies[i]);
                    continue;
                }

                if (enemies[i].GetComponent<Enemy>().GetTurnFlag())
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
