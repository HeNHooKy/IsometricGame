﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    //анимация
    public float DamageAnimationShift = 2;  //сдвиг анимации при получении урона
    public float DamageAnimationSpeedShift = 2f;//скорость сдвига анимации при получении урона

    public float AttackAnimationSpeedShift = 2f; //скорость сдвига анимации при нанесении урона
    public float AttackAnimationShift = 2f; //сдвиг анимации при нанесении урона

    public float TextMoveSpeed = 1f;

    //атрибуты
    public float RangeMeasure = 2f; //делить дальних атак. Уменьшает-увеличивает урон от дальних атак

    public float AttackPower = 1f;  //количество урона, который получит игрок от атаки этого монстра
    public float Health = 2f; //здоровье противника
    public float Energy = 0f;   //количество действий за ход
    public float AttackChance = 0.5f;   //шанс урона
    public float CriticalChance = 0.15f;    //шанс крита
    public float CriticalMultiply = 1.5f; //множитель критического удара
    public float BiasChance = 0.15f; //шанс уклона

    public int Score = 500; //количество 

    public float EnergyReload = 1f; //количество восполняемой в ход энергии
    public float MoveSpeed = 1f; //скорость анимирования передвижения
    public float DieTime = 3f;  //время, спустя которое противник пропадет после смерти
    public int FarVision = 1; //зона видимости (использует сложные алгоритмы поиска - высокая нагрузка)
    public GameObject Ball; //снаряд для дальней атаки
    public GameObject MoveBlock; //GameObject with colider wich block move other enemies
    public string PlayerTag = "Player"; //тег игрока
    public string FloorTag = "Floor"; //тег пола
    public string ItemTag = "Item"; //тег предметов, чтобы они не считались стеной

    protected SpriteRenderer eSprite;
    protected GameController controller; //указатель на управляющий игрой компонент
    protected bool turnCatch = false;
    protected bool isMyTurn = false;
    protected Animator eAnimator;    //указатель на аниматор (У каждого enemy должен быть animator
    protected bool isAlive = true; //отображает способность двигаться, наносить урон(жить)
    protected PlayerController player; //здесь окажется игрок, если будет обнаружен
    protected List<Vector3> PathToPlayer = null; //после вызова FindPath путь будет храниться в этой перменной
    protected float StartHP; //стартовое количество здоровья (полная шкала здоровья)

    protected bool isBeingStep { get; private set; } = false; //блокировка вызова хода
    private GameObject LastMoveBlock; //указатель на блокировку хода
    private System.Random random = new System.Random();
    

    public bool GetAlive()
    {
        return isAlive;
    }

    //поиск пути к персонажу
    protected void FindPath()
    {
        int[,] map = GetMap(new Vector3(transform.position.x, transform.position.z), FarVision);
        System.Drawing.Point PlayerPoint = System.Drawing.Point.Empty;

        for(int i = 0; i < (2 * FarVision) + 1; i++)
        {
            for(int j = 0; j < (2 * FarVision) + 1; j++)
            {
                if (map[i, j] == 2)
                {
                    map[i, j] = 0;
                    PlayerPoint = new System.Drawing.Point(i, j);
                    goto end;   //выход из циклов
                }
            }
        }
        end:        //ВОТ ТУТ ВОТ GOTO


        if (PlayerPoint == System.Drawing.Point.Empty)
        {   //игрок за пределами видимости
            return;
        }

        //получаем путь до игрока(если он есть)
        PathToPlayer = ConvertToVector3(FindClosePath.FindPath(map, new System.Drawing.Point(FarVision, FarVision), PlayerPoint),
            new Vector2(transform.position.x, transform.position.z));
    }

    private List<Vector3> ConvertToVector3(List<System.Drawing.Point> path, Vector2 sPos)
    {
        if(path == null)
        {   //no way
            return null;
        }

        List<Vector3> newPath = new List<Vector3>();
        //возможно стоит добавить в newPath точку начала?
        //вычисляем смещение
        float dx = (float) path[0].X - sPos.x;
        float dy = (float)path[0].Y - sPos.y;

        for(int i = 0; i < path.Count; i++)
        {   //формируем вектор
            newPath.Add(new Vector3(path[i].X - dx, 0f, path[i].Y - dy));
        }
        return newPath;
    }

    //построение integer карты перемещений
    int[,] GetMap(Vector2 sPos, int n)
    {
        int[,] map = new int[(2 * n) + 1, (2 * n) + 1];
        for(int i = 0; i < (2*n)+1; i++)
        {
            for(int j = 0; j < (2*n)+1; j++)
            {
                map[i, j] = -1;
            }
        }

        for(int i = 0; i < (2 * n) + 1; i++)
        {
            for(int j = 0; j < (2 * n) + 1; j++)
            {
                if(FarVision >= (new Vector2(FarVision - i, FarVision - j).magnitude))
                {
                    map[i, j] = GetCell(new Vector3(sPos.x - n + i, -0.2f, sPos.y - n + j));
                }
            }
        }
        return map;
    }

    //определение допустимости движения по блоку
    protected int GetCell(Vector3 pos)
    {
        pos.y = -0.2f;
        RaycastHit[] hit = Physics.RaycastAll(pos, Vector3.up * 2f, 2f);

        foreach(RaycastHit h in hit)
        {
            if(h.transform == transform)
            {
                return 0; //тут я
            }
            if (h.collider.tag == PlayerTag)
            {
                return 2;   //тут игрок!
            }
            if(h.collider.tag == FloorTag && hit.Length == 1)
            {
                return 0;   //ходить можно
            }
            if(h.collider.tag == ItemTag && hit.Length == 2)
            {
                return 0;   //тут лежит какой-то предмет, но ходить можно
            }
        }
        return -1;   //в остальных случаях, ходить нельзя
    }

    //двигаться
    protected void Move(Vector3 tPos)
    {
        if (!isAlive && isBeingStep) return;

        //создаем стоп-блок
        LastMoveBlock = Instantiate(MoveBlock);
        LastMoveBlock.transform.position = tPos;

        isBeingStep = true;
        eSprite.flipX = (tPos - transform.position).x < 0 || (tPos - transform.position).z < 0;
        Vector3 sPos = transform.position;
        //eAnimator.SetBool("IsWalking", true);
        StartCoroutine(_Move(sPos, tPos));
    }

    //ударить в ближнем бою
    protected void CloseAttack(Vector3 direct)
    {
        if (!isAlive) return;
        int layerMask = (1 << 10); //ищем только игрока
        //пускаем луч и собираем данные
        RaycastHit hit;
        bool isHit = Physics.Raycast(direct, Vector3.up, out hit, 1f, layerMask);
        Debug.DrawRay(direct, Vector3.up, Color.yellow, 1f);
        //в мире только один игрок
        PlayerController player;

        if (isHit)
        {   //попався
            player = hit.transform.gameObject.GetComponent<PlayerController>();
            if(player != null)
            {   //точно попався
                eSprite.flipX = (hit.transform.position - transform.position).x < 0 || (hit.transform.position - transform.position).z < 0;
                isBeingStep = true;
                StartCoroutine(CloseAttackAnimation(player.transform));

            }
        }
    }

    

    //запустить стрелу
    protected void RangeAttack(Vector3 direct)
    {
        if (!isAlive) return;
        eAnimator.SetBool("IsRangeAttack", true);
        GameObject arrow = Instantiate(Ball); //вызов стрелы
        arrow.transform.position = direct;  //определяем новое положение стрелы
        arrow.transform.rotation = Quaternion.Euler(0f, Vector3.Angle(transform.position, direct), 0f); //поворот стрелы
    }
    
    /// <summary>
    /// Register damage to enemy
    /// </summary>
    /// <param name="damage">Attack Power</param>
    /// <param name="AC">Attack Chance</param>
    /// <param name="pPos">My position</param>
    public void Damaged(float damage, float AC, Vector3 pPos, bool trueSight = false)
    {   //AC - AttackChance
        if (!isAlive) return;
        //пересчет шанса урона, уклонения
        float chance = !trueSight ? (float) random.NextDouble() : 0;
        
        if (chance > (AC - BiasChance))
        {   //промах!
            return;
        }

        StartCoroutine(DamageAnimation(transform.position + (transform.position - pPos)));//берем обратную от положения персонажа
        Health -= damage;
        if(Health <= 0f)
        {
            controller.AddScore(Score); //дать игроку заработанные очки
            isAlive = false;
            //если здоровье упало слишком низко наступает смерть
            eAnimator.Play("Die");
            this.GetComponent<Collider>().enabled = false;
            Invoke("Die", DieTime);
        }
    }

    //смерть
    protected void Die()
    {
        if (!isAlive) return;   //То, что мертво, умереть не может
        isMyTurn = false;
        Destroy(this.gameObject);
    }

    //получение разрешения на ход
    public void TurnAllowed()
    {   //начало хода
        this.isMyTurn = true;
        Energy += EnergyReload;
    }

    public bool GetTurnFlag()
    {
        return isMyTurn;
    }

    //перемещение
    IEnumerator _Move(Vector3 sPos, Vector3 tPos)
    {
        for (float i = 0; i < 1; i += Time.deltaTime * MoveSpeed)
        {
            transform.position = Vector3.Lerp(sPos, tPos, i);
            yield return null;
        }
        transform.position = tPos;
        //eAnimator.SetBool("IsWalking", false);
        Destroy(LastMoveBlock); //удаляем MoveBlock
        Energy--;
        isBeingStep = false;
    }

    //Анимация ближней атаки
    IEnumerator CloseAttackAnimation(Transform p)
    {
        //eAnimator.SetBool("IsAttack", true);
        Transform sprite = transform.Find("Character");
        Vector3 sPos = sprite.position;
        Vector3 tPos = p.position;
        tPos -= (p.position - sPos) / AttackAnimationShift;
        tPos.y = sPos.y;

        for (float i = 0; i < 1f; i += AttackAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(sPos, tPos, easeInOutQuart(i));
            yield return null;
        }

        //крит
        float CC = (float)random.NextDouble();
        float attack = CC <= CriticalChance ? AttackPower * CriticalMultiply : AttackPower;

        PlayerController player = p.GetComponent<PlayerController>();
        player.Damaged(attack, AttackChance, transform.position); //нанесение урона

        for (float i = 0; i < 1; i += AttackAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(tPos, sPos, easeOutQuint(i));
            yield return null;
        }

        sprite.position = sPos;
        isBeingStep = false;
        Energy--;
    }


    //плавно Анимация получения урона
    IEnumerator DamageAnimation(Vector3 tPos)
    {
        eAnimator.Play("Damaged");//проиграть анимацию в аниматоре
        Transform sprite = transform.Find("Character"); //найдем спрайт, который будем двигать

        SpriteRenderer sp = sprite.GetComponentInChildren<SpriteRenderer>();

        Vector3 sPos = sprite.position;//получим позицию спрата
        //поворот спрайта
        sp.flipX = (sPos - tPos).x < 0 || (tPos - sPos).z > 0;

        tPos -= (tPos - sPos) / DamageAnimationShift;
        tPos.y = sPos.y;

        for (float i = 0; i <= 0.4; i += DamageAnimationSpeedShift * Time.deltaTime)
        {   //анимация
            sprite.position = Vector3.Lerp(sPos, tPos, easeOutExpo(i));
            yield return null;
        }
        for (float i = 0.4f; i < 1; i += DamageAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(tPos, sPos, easeInOutQuad(i));
            yield return null;
        }
        sprite.position = sPos;
    }


    float easeOutExpo(float x)
    {
        return (float)(x == 1f ? 1f : 1f - Math.Pow(2f, -10f * x));
    }
    float easeInOutQuad(float x)
    {
        return (float) (x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2);
    }

    float easeInOutQuart(float x)
    {
        return x < 0.5 ? (float)(8 * x * x * x * x) : (float)(1 - Math.Pow(-2 * x + 2, 4) / 2);
    }

    float easeOutQuint(float x)
    {
        return (float)(1 - Math.Pow(1 - x, 5));
    }

}


/*  отображение поиска пути
 * FindPath();
        if(PathToPlayer != null)
        {
            Vector3 prev = PathToPlayer[0];
            for (int i = 1; i < PathToPlayer.Count; i++)
            {
                Debug.DrawLine(prev + Vector3.up * 1, PathToPlayer[i] + Vector3.up * 1, UnityEngine.Color.green, 1f);
                prev = PathToPlayer[i];
            }
        }
*/
