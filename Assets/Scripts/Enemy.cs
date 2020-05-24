using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    public static bool isEnemiesTurn = false;

    public GameController controller; //указатель на управляющий игрой компонент

    public float DamageAnimationShift = 2;
    public float DamageAnimationSpeedShift = 0.095f;

    public float AttackPower = 1f;  //количество урона, который получит игрок от атаки этого монстра
    public float RangeMeasure = 2f; //делить дальних атак. Уменьшает-увеличивает урон от дальних атак
    public float Health = 2f; //здоровье противника
    public float Energy = 1f;   //количество действий за ход
    public float EnergyReload = 1f; //количество восполняемой в ход энергии
    public float MoveSpeed = 1f; //скорость анимирования передвижения
    public float DieTime = 3f;  //время, спустя которое противник пропадет после смерти
    public int FarVision = 1; //зона видимости (использует сложные алгоритмы поиска - высокая нагрузка)
    public GameObject Ball; //снаряд для дальней атаки
    public string PlayerTag = "Player"; //тег игрока
    public string FloorTag = "Floor"; //тег пола
    public string ItemTag = "Item"; //тег предметов, чтобы они не считались стеной

    protected bool isMyTurn = false;
    protected Animator eAnimator;    //указатель на аниматор (У каждого enemy должен быть animator
    protected bool isAlive = true; //отображает способность двигаться, наносить урон(жить)
    protected PlayerController player; //здесь окажется игрок, если будет обнаружен
    protected List<Vector3> PathToPlayer = null; //после вызова FindPath путь будет храниться в этой перменной

    //поиск пути к персонажу
    void FindPath()
    {
        int[,] map = GetMap(new Vector3(transform.position.x, transform.position.z), FarVision);
        System.Drawing.Point PlayerPoint = System.Drawing.Point.Empty;

        for(int i = 0; i < 2 * FarVision + 1; i++)
        {
            for(int j = 0; j < 2 * FarVision + 1; j++)
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
        PathToPlayer = ConvertToVector3(FindClosePath.FindPath(map, new System.Drawing.Point((FarVision / 2) + 1, (FarVision / 2) + 1), PlayerPoint),
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
        float dx = Math.Abs((float) path[0].X -  sPos.x);
        float dy = Math.Abs((float)path[0].Y - sPos.y);
        for(int i = 0; i < path.Count; i++)
        {   //формируем вектор
            newPath.Add(new Vector3(path[i].X - dx, 0f, path[i].Y - dy));
        }
        return newPath;
    }

    //построение integer карты перемещений
    int[,] GetMap(Vector2 sPos, int n)
    {
        int[,] map = new int[2 * n + 1, 2 * n + 1];
        for(int i = 0; i < 2 * n + 1; i++)
        {
            for(int j = 0; j < 2*n+1; j++)
            {
                map[i, j] = GetCell(new Vector3(sPos.x - n + i, -0.2f, sPos.y - n + j));
            }
        }
        return map;
    }

    //определение допустимости движения по блоку
    int GetCell(Vector3 pos)
    {
        RaycastHit[] hit = Physics.RaycastAll(pos, Vector3.up * 2f, 2f);
        Debug.DrawRay(pos, Vector3.up * 2f, UnityEngine.Color.red, 1f);

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
    void Move(Vector3 tPos)
    {
        if (!isAlive) return;
        Vector3 sPos = transform.position;
        eAnimator.SetBool("IsWalking", true);
        StartCoroutine(_Move(sPos, tPos));
    }

    //ударить в ближнем бою
    void CloseAttack(Vector3 direct)
    {
        if (!isAlive) return;
        int layerMask = (1 << 10); //ищем только игрока
        //пускаем луч и собираем данные
        RaycastHit hit;
        bool isHit = Physics.Raycast(transform.position, direct, out hit, 1f, layerMask);
        //в мире только один игрок
        PlayerController player;

        if (isHit)
        {   //попався
            player = hit.transform.gameObject.GetComponent<PlayerController>();
            if(player != null)
            {   //точно попався
                player.Damaged(AttackPower);
                eAnimator.SetBool("IsAttack", true);
            }
        }
    }

    //запустить стрелу
    void RangeAttack(Vector3 direct)
    {
        if (!isAlive) return;
        eAnimator.SetBool("IsRangeAttack", true);
        GameObject arrow = Instantiate(Ball); //вызов стрелы
        arrow.transform.position = direct;  //определяем новое положение стрелы
        arrow.transform.rotation = Quaternion.Euler(0f, Vector3.Angle(transform.position, direct), 0f); //поворот стрелы
    }
    
    //получение урона
    public void Damaged(float damage, Vector3 pPos)
    {
        if (!isAlive) return;
        StartCoroutine(DamageAnimation(transform.position + (transform.position - pPos)));//берем обратную от положения персонажа
        Health -= damage;
        if(Health <= 0f)
        {   //если здоровье упало слишком низко наступает смерть
            //eAnimator.SetBool("IsDie", true);
            this.GetComponent<Collider>().enabled = false;
            Invoke("Die", DieTime);
        }
    }

    //смерть
    void Die()
    {
        if (!isAlive) return;   //То, что мертво, умереть не может
        Destroy(this.gameObject);
        isAlive = false;
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
        eAnimator.SetBool("IsWalking", false);
    }

    public void EnemiesTurn()
    {
        isEnemiesTurn = true;
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
        
        for (float i = 0; i <= 0.4; i += DamageAnimationSpeedShift)
        {   //анимация
            sprite.position = Vector3.Lerp(sPos, tPos, easeOutExpo(i));
            yield return null;
        }
        for (float i = 0.4f; i < 1; i += DamageAnimationSpeedShift)
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