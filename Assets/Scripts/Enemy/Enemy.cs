using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    protected List<Vector3> Path = null; //после вызова FindPath путь будет храниться в этой перменной
    protected float StartHP; //стартовое количество здоровья (полная шкала здоровья)
    protected HealthBar hp; //шкала здоровья

    protected bool isBeingStep { get; private set; } = false; //блокировка вызова хода
    protected GameObject LastMoveBlock; //указатель на блокировку хода
    private bool isPlayerNearby = true;
    private System.Random random = new System.Random();

    void Start()
    {
        StartHP = Health;
        hp = transform.Find("Character").Find("HealthBar").GetComponent<HealthBar>();
        eAnimator = transform.Find("Character").Find("Sprite").GetComponent<Animator>();
        controller = transform.Find("/GameController").GetComponent<GameController>();
        eSprite = transform.Find("Character").Find("Sprite").GetComponent<SpriteRenderer>();
    }


    /// <summary>
    /// Завершение действия. Отнимает стамину, завершает step, удаляет moveBlock (если есть)
    /// </summary>
    protected void StepOut()
    {
        if(LastMoveBlock != null)
        {
            Destroy(LastMoveBlock); //удаляем MoveBlock
        }
        Energy--;
        isBeingStep = false;
    }

    /// <summary>
    /// Set enemies move type
    /// </summary>
    /// <param name="isTrue"></param>
    public void PlayerInRange(bool isTrue)
    {
        isPlayerNearby = isTrue;
    }

    public bool GetAlive()
    {
        return isAlive;
    }

    /// <summary>
    /// Get random position around enemy
    /// if in around enemy exist player - return player position
    /// </summary>
    protected Vector3 GetRandomPosition(out bool isNotExist)
    {
        bool isPlayer;
        List<Vector3> locations = GetFreeLocation(out isPlayer);

        isNotExist = false;
        if (locations.Count == 0)
        {
            isNotExist = true;
            return Vector3.zero;
        }
        if (isPlayer)
        {
            return locations.Last();
        }
        else
        {
            return locations[random.Next(locations.Count)];
        }
    }

    /// <summary>
    ///  Get free location near enemy
    /// </summary>
    /// <returns></returns>
    protected List<Vector3> GetFreeLocation(out bool isPlayer)
    {
        isPlayer = false;
        List<Vector3> locations = new List<Vector3>();
        //перебором каждой локации вокруг найдем все доступные
        int vertPlus = GetCell(transform.position + new Vector3(1, 0, 0));
        int vertMinus = GetCell(transform.position + new Vector3(-1, 0, 0));
        int horizPlus = GetCell(transform.position + new Vector3(0, 0, 1));
        int horizMinus = GetCell(transform.position + new Vector3(0, 0, -1));

        if (vertPlus == 0 || vertPlus == 2)
        {
            locations.Add(transform.position + new Vector3(1, 0, 0));
            if (vertPlus == 2)
            {
                isPlayer = true;
                return locations;
            }
        }
        if(vertMinus == 0 || vertMinus == 2)
        {
            locations.Add(transform.position + new Vector3(-1, 0, 0));
            if (vertMinus == 2)
            {
                isPlayer = true;
                return locations;
            }
        }
        if(horizPlus == 0 || horizPlus == 2)
        {
            locations.Add(transform.position + new Vector3(0, 0, 1));
            if (horizPlus == 2)
            {
                isPlayer = true;
                return locations;
            }
        }
        if(horizMinus == 0 || horizMinus == 2)
        {
            locations.Add(transform.position + new Vector3(0, 0, -1));
            if (horizMinus == 2)
            {
                isPlayer = true;
                return locations;
            }
        }

        return locations;
    }


    /// <summary>
    /// Find player and close path to him
    /// </summary>
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
        Path = ConvertToVector3(FindClosePath.FindPath(map, new System.Drawing.Point(FarVision, FarVision), PlayerPoint),
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
    int[,] GetMap(Vector2 sPos, int n, bool isPlayerTarget = true)
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
                    map[i, j] = GetCell(new Vector3(sPos.x - n + i, -1f, sPos.y - n + j), isPlayerTarget);
                }
            }
        }
        return map;
    }

    //определение допустимости движения по блоку
    protected int GetCell(Vector3 pos, bool isPlayerTarget = true)
    {
        pos.y = -0.2f;
        RaycastHit[] hit = Physics.RaycastAll(pos, Vector3.up * 2f, 2f);

        foreach(RaycastHit h in hit)
        {
            if(h.transform == transform)
            {
                return 0; //тут я
            }
            if (h.collider.tag == PlayerTag && isPlayerTarget)
            {
                return 2;   //тут игрок!
            }
            if (h.collider.tag == PlayerTag && !isPlayerTarget)
            {
                return 0;   //тут игрок, но как-то пофиг.
            }
            if (h.collider.tag == FloorTag && hit.Length == 1)
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

    /// <summary>
    /// Shift the enemy, if player too far
    /// </summary>
    protected void Shift(Vector3 tPos)
    {
        transform.position = tPos;
        StepOut();
    }

    //двигаться
    protected void Move(Vector3 tPos)
    {
        if (!isAlive && isBeingStep) return;

        //создаем стоп-блок
        LastMoveBlock = Instantiate(MoveBlock);
        LastMoveBlock.transform.position = tPos;

        if (!isPlayerNearby)
        {
            Shift(tPos);
            return;
        }


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
        StepOut();  //минус очко действий
    }
    
    /// <summary>
    /// Register damage to enemy
    /// </summary>
    /// <param name="damage">Attack Power</param>
    /// <param name="AC">Attack Chance</param>
    /// <param name="pPos">My position</param>
    public void Damaged(float damage, float AC, Vector3 pPos, bool trueStrike = false)
    {   //AC - AttackChance
        if (!isAlive) return;
        //пересчет шанса урона, уклонения
        float chance = !trueStrike ? (float) random.NextDouble() : 0;
        
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
            Destroy(hp.gameObject);
            Invoke("Die", DieTime);
        }
    }

    //смерть
    protected void Die()
    {
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
    protected virtual IEnumerator _Move(Vector3 sPos, Vector3 tPos)
    {
        for (float i = 0; i < 1; i += Time.deltaTime * MoveSpeed)
        {
            transform.position = Vector3.Lerp(sPos, tPos, i);
            yield return null;
        }
        transform.position = tPos;
        StepOut();
    }

    //Анимация ближней атаки
    IEnumerator CloseAttackAnimation(Transform p)
    {
        Transform sprite = transform.Find("Character");
        Vector3 sPos = sprite.position;
        Vector3 tPos = p.position;
        tPos -= (p.position - sPos) / AttackAnimationShift;
        tPos.y = sPos.y;

        for (float i = 0; i < 1f; i += AttackAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(sPos, tPos, Easing.easeInOutQuart(i));
            yield return null;
        }

        eAnimator.Play("Attack");

        //крит
        float CC = (float)random.NextDouble();
        float attack = CC <= CriticalChance ? AttackPower * CriticalMultiply : AttackPower;

        PlayerController player = p.GetComponent<PlayerController>();
        player.Damaged(attack, AttackChance, transform.position); //нанесение урона

        for (float i = 0; i < 1; i += AttackAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(tPos, sPos, Easing.easeOutQuint(i));
            yield return null;
        }

        sprite.position = sPos;
        StepOut();
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
            sprite.position = Vector3.Lerp(sPos, tPos, Easing.easeOutExpo(i));
            yield return null;
        }
        for (float i = 0.4f; i < 1; i += DamageAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(tPos, sPos, Easing.easeInOutQuad(i));
            yield return null;
        }
        sprite.position = sPos;
    }


    

}


/*  отображение поиска пути
 * FindPath();
        if(Path != null)
        {
            Vector3 prev = Path[0];
            for (int i = 1; i < Path.Count; i++)
            {
                Debug.DrawLine(prev + Vector3.up * 1, Path[i] + Vector3.up * 1, UnityEngine.Color.green, 1f);
                prev = Path[i];
            }
        }
*/
