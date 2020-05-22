using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float AttackPower = 1f;
    public float RangeMeasure = 2f;
    public float Health = 2f;
    public float Energy = 1f;
    public float EnergyReload = 1f;
    public float MoveSpeed = 1f;
    public float DieTime = 3f;
    public float FarVision = 5f;
    public GameObject ball;
    public string PlayerTag = "Player";
    public string FloorTag = "Floor";
    public string ItemTag = "Item";

    private Animator eAnimation;
    private bool isAlive = true;
    private PlayerController player;

    void Start()
    {
        eAnimation = GetComponent<Animator>();
    }

    private void Update()
    {
        FindPath();
    }

    //поиск пути к персонажу
    Vector3[] FindPath()
    {
        GetMap(new Vector3(transform.position.x, transform.position.z), 1);

        return null;
    }

    //построение integer карты перемещений
    int[,] GetMap(Vector2 sPos, int n)
    {
        int[,] map = new int[2 * n + 1, 2 * n + 1];
        for(int i = 0; i < 2*n+1; i++)
        {
            string s = "";
            for(int j = 0; j < 2*n+1; j++)
            {
                map[i, j] = GetCell(new Vector3(sPos.x - n + i, 0f, sPos.y - n + j));
                s += map[i, j];
            }
            Debug.Log(s);
        }
        return map;
    }

    //определение допустимости движения по блоку
    int GetCell(Vector3 pos)
    {
        RaycastHit[] hit = Physics.RaycastAll(pos, Vector3.up * 1f, 1f);

        foreach(RaycastHit h in hit)
        {
            if (h.transform.name == PlayerTag)
            {
                return 2;   //тут игрок!
            }
            if(h.transform.name == FloorTag && hit.Length == 1)
            {
                return 1;   //ходить можно
            }
            if(h.transform.name == ItemTag && hit.Length == 2)
            {
                return 1;   //тут лежит какой-то предмет, но ходить можно
            }
        }
        return 0;   //в остальных случаях, ходить нельзя
    }

    //двигаться
    void Move(Vector3 tPos)
    {
        Vector3 sPos = transform.position;
        eAnimation.SetBool("IsWalking", true);
        StartCoroutine(_Move(sPos, tPos));
    }

    //ударить в ближнем бою
    void CloseAttack(Vector3 direct)
    {
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
                eAnimation.SetBool("IsAttack", true);
            }
        }
    }

    //запустить стрелу
    void RangeAttack(Vector3 direct)
    {
        eAnimation.SetBool("IsRangeAttack", true);
        GameObject arrow = Instantiate(ball); //вызов стрелы
        arrow.transform.position = direct;  //определяем новое положение стрелы
        arrow.transform.rotation = Quaternion.Euler(0f, Vector3.Angle(transform.position, direct), 0f); //поворот стрелы
    }
    
    //получение урона
    void Damaged(float damage)
    {
        eAnimation.SetBool("IsDamaged", true);
        Health -= damage;
        if(Health < 0f)
        {   //если здоровье упало слишком низко наступает смерть
            eAnimation.SetBool("IsDie", true);
            Invoke("Die", DieTime);
        }
    }

    //смерть
    void Die()
    {
        Destroy(this.gameObject);
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
        eAnimation.SetBool("IsWalking", false);
    }

}
