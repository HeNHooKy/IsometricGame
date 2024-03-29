﻿using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Анимация")]
    [Tooltip("Cдвиг при получении урона")]
    public float DamageAnimationShift = 2;
    [Tooltip("Cкорость сдвига при получении урона")]
    public float DamageAnimationSpeedShift = 2f;
    [Tooltip("Сдвиг при нанесении урона")]
    public float AttackAnimationSpeedShift = 0.1f;
    [Tooltip("Cкорость сдвига при нанесении урона")]
    public float AttackAnimationShift = 2f;


    [Header("Аттрибуты")]
    [Tooltip("Здоровье игрока")]
    public float Health = 4f;
    [Tooltip("Максимальное здоровье")]
    public float MaxHealth = 10f;
    [Tooltip("Количество действий за ход")]
    public float EnergyReload = 2f;
    [Tooltip("Сила атаки")]
    public float AttackPower = 1f;
    [Tooltip("Шанс урона")]
    public float AttackChance = 0.5f;
    [Tooltip("Шанс крита")]
    public float CriticalChance = 0.15f;
    [Tooltip("Множитель критического удара")]
    public float CriticalMultiply = 1.5f;
    [Tooltip("Шанс уклона")]
    public float BiasChance = 0.15f;

    [Header("Настройки игрока")]
    [Tooltip("Максимально возможное количество здоровья")]
    public float MaxHealthOver = 16f;
    [Tooltip("Максимально возможное количество энергии")]
    public float MaxEnergyOver = 12f;
    [Tooltip("Время, через которое пропадет GO игрока после смерти")]
    public float DieTime = 2f;
    [Tooltip("Скорость анимации перемещения игрока")]
    public float MoveSpeed = 1f;
    [Tooltip("Указатель на Player Health Bar в HUD")]
    public PlayerHealthBar phb;
    [Tooltip("Указатель на GameController")]
    public GameController GameController;
    [Tooltip("Префаб свободной для движения ячейки")]
    public GameObject FreeFloor;
    [Tooltip("Префаб выбранной ячейки")]
    public GameObject SelectFloor;
    [Tooltip("Скорость появления/исчезания свободной для движения ячейки")]
    public float OpacitySpeed = 2f;

    [Header("Теги")]
    [Tooltip("Тег пола доступного для перемещения")]
    public string FloorTag = "Floor";
    [Tooltip("Тег противников")]
    public string EnemyTag = "Enemy";
    [Tooltip("Тег окружения, с которым можно взаимодействовать")]
    public string EnvironmentTag = "Environment";
    [Tooltip("Тег предметов")]
    public string ItemTag = "Item";

    [HideInInspector]
    public bool isPressedButton = false;

    Transform turnObj;
    GameObject turnFloor = null;
    Animator pAnimator;
    SpriteRenderer pSprite;
    GameObject selected;
    GameObject[] freeLocs;
    private bool isMyTurn = false;
    private bool beingStep = false;
    private bool isAlive = true;
    private float Energy = 0f;
    private bool isDefending = false;
    private Vector3 spritePosition;

    private System.Random random = new System.Random();

    void Start()
    {
        pAnimator = GetComponentInChildren<Animator>();
        pSprite = transform.Find("Character").Find("Sprite").GetComponent<SpriteRenderer>();
        freeLocs = new GameObject[4];
        spritePosition = transform.Find("Character").localPosition;   //собираем данные о стартовой позиции спрайта персонажа для того,
                                                                      //чтобы сдвигать персонажа при получении урона относительно неё
        TurnStart();
        DisplayHearts();
    }

    void Update()
    {
        if (!isAlive)
            return; //мертвый персонаж больше не обрабатывается 
        //получаем объект, на который кликнул пользователь

        turnObj = Touch();
        if (isMyTurn && !beingStep)
        {
            if (Energy < 1f)
            {   //энергия кончилась. Конец хода
                TurnEnd();
            }

            if (turnObj != null)
            {
                if (turnObj.tag == FloorTag)
                    TurnFloor(turnObj);
            }
        }
    }

    /// <summary>
    /// Player take shield and defending(englesh..)
    /// </summary>
    public void SetBlock()
    {   //должна проиграться анимация: игрок достает щит.
        //ход заканчивается
        //игрок держит щит, пока ему не нанесут первый удар - первый удар полностью заблокируется
        //после первого удара персонаж спрячет щит
        if (!isAlive || isDefending) return;
        isDefending = true;
        
        
        StartCoroutine(_SetBlock());
    }

    IEnumerator _SetBlock()
    {
        pAnimator.Play("SetShield");
        //ждём пока анимация будет завершена
        while (!pAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShieldIdle"))
        {
            yield return null;
        }
        Energy = 0f;
    }

    //Снятие щита в начале хода
    IEnumerator _DisBlock()
    {   //вызывается только в начале хода
        pAnimator.Play("HideShield");
        while (!pAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayerIdle"))
        {
            yield return null;
        }

        //начинаем ход
        ClearFreeLoc();
        Energy += EnergyReload;
        isMyTurn = true;
        Debug.Log("Ход вернулся к игроку");
        GetFreeLoc();
    }

    /// <summary>
    /// Setup player health point
    /// </summary>
    public void MaxHPUp(float hp)
    {
        if (MaxHealth + hp >= MaxHealthOver)
        {
            MaxHealth = MaxEnergyOver - MaxHealth > hp ? hp : MaxEnergyOver - MaxHealth;
        }
        else
        {
            MaxHealth += hp;
        }
        DisplayHearts();
    }

    public void MaxEnergyUp(float energy)
    {
        //TODO: Energy UP
    }

    /// <summary>
    /// Set player Health Point to Health plus HealthPower, but lower than 16
    /// </summary>
    /// <param name="HealthPower">Heal power</param>
    public void Heal(float healPower, bool isOverHeal = false)
    {
        float needHeal;
        if (isOverHeal)
        {
            needHeal = 16 > Health ? 16 - Health : 0;
            Health += healPower > needHeal ? needHeal : healPower;
            return;
        }
        needHeal = MaxHealth > Health ? MaxHealth - Health : 0;
        Health += healPower > needHeal ? needHeal : healPower;
    }

    /// <summary>
    /// Base on phb - Display player health
    /// </summary>
    public void DisplayHearts()
    {
        phb.DisplayHeart((int)Math.Ceiling(MaxHealth), (int)Math.Ceiling(Health));
    }

    /// <summary>
    /// Register damage to player
    /// </summary>
    /// <param name="damage">Attack Power</param>
    /// <param name="AC">Attack Chance</param>
    /// <param name="pPos">Enemy position</param>
    public void Damaged(float damage, float AC, Vector3 pPos)
    {
        if (!isAlive) return;
        //пересчет шанса урона, уклонения
        float chance = (float)random.NextDouble();
        if (chance > (AC - BiasChance))
        {   //промах!
            return;
        }
        if(!isDefending)
        {
            Health -= damage; //сначала снимаем HP потом проверяем
        }
        //отображаем актуальное хп
        DisplayHearts();
        if(Health > 0f)
        {   //показвыаем анимацию получения урона
            StartCoroutine(DamageAnimation(transform.position - pPos));//берем вектор направления удара
        }
        if (Health <= 0f)
        {   //если здоровье упало слишком низко наступает смерть
            //тут показываем анимацию смерти
            StartCoroutine(DieAnimation(transform.position + (transform.position - pPos)));
            transform.Find("Point Light").gameObject.SetActive(false);
            ClearFreeLoc(); //очищаем все свободные пути
            isAlive = false;
            Invoke("Die", DieTime);
        }
    }

    public void TurnStart()
    {
        if (!isAlive)
            return; //смерть. Ход не возвращается
        if(isDefending)
        {   //игрок держит щит
            isDefending = false;
            StartCoroutine(_DisBlock());
        }
        else
        {
            ClearFreeLoc();
            Energy += EnergyReload;
            isMyTurn = true;
            Debug.Log("Ход вернулся к игроку");
            GetFreeLoc();
        }
    }

    //смерть
    private void Die()
    {
        //вызов окна конца игры
        GameController.GameOver();
    }

    /// <summary>
    /// Player capacity to do something
    /// </summary>
    /// <returns></returns>
    public bool GetCapacity()
    {   //способность игрока выполнить какое-то действие
        return isMyTurn && isAlive && !beingStep;
    }

    /// <summary>
    /// This method change the floor texture
    /// </summary>
    void TurnFloor(Transform obj)
    {
        if (turnFloor == obj.gameObject || beingStep)
            return;

        if (turnFloor != null)
        {   //выполняется только если это не первый выделенный объект 
            //или указатель не был смещен на неактивную область
            Destroy(selected);
        }

        float distance = Math.Abs((GetPosition(obj) - GetPosition(transform)).magnitude);
        if (distance > 1 || distance < 0.01)
        {   //выполняется, если был выбран floor на растоянии привышающем максимальную дистанцию шага

            Destroy(selected);
            turnFloor = null;
            return;
        }

        turnFloor = obj.gameObject; //сохранение выбранного игроком объекта

        selected = Instantiate(SelectFloor);
        selected.transform.position = turnFloor.transform.position;
    }

    /// <summary>
    /// Catch the user clicks and return the gameObject which clicked.
    /// </summary>
    Transform Touch()
    {
        if(isMyTurn && !beingStep && !isPressedButton)
        {
            if (Input.touchCount > 0)
            {
                //ловим пол
                RaycastHit hitFloor;
                int layerMask = (1 << 8);
                Touch touch = Input.GetTouch(0);
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitFloor, layerMask);
                if (hitFloor.transform == null)
                    return null;

                return hitFloor.transform;
            }
            else
            {   //тач отжат
                if (turnFloor != null)
                {
                    if (isMyTurn && !beingStep && !isDefending)
                    {
                        //Сдлеай шаг!
                        beingStep = true;
                        DoStep();
                    }
                }
            }
        }
        return null;
    }

    void DoStep()
    {
        if (!isAlive) return;
        int floorType = GetLoc(turnFloor.transform.position);

        if (floorType == 1 || floorType == 2 || floorType == 3)
        {
            //Тут свободно. Можно идти
            Move(turnFloor.transform.position);
        }
        else if (floorType == 4)
        {
            Attack(turnFloor.transform.position);
        }
        else
        {
            turnFloor = null;
            beingStep = false;
            //Кажется тут занято. Идти нельзя, нужно попробовать другое действие
        }
        Destroy(selected);
    }

    //двигаться
    void Move(Vector3 tPos)
    {
        ClearFreeLoc();
        if (!isAlive) return;
        Vector3 sPos = transform.position;
        pAnimator.SetBool("IsWalking", true);
        StartCoroutine(_Move(sPos, tPos));
    }

    //перемещение
    IEnumerator _Move(Vector3 sPos, Vector3 tPos)
    {
        pSprite.flipX = (tPos - sPos).x < 0 || (tPos - sPos).z < 0;
        for (float i = 0; i < 1; i += Time.deltaTime * MoveSpeed)
        {
            transform.position = Vector3.Lerp(sPos, tPos, i);
            yield return null;
        }
        transform.position = tPos;
        pAnimator.SetBool("IsWalking", false);
        beingStep = false;
        turnFloor = null;
        Energy--;
        GetFreeLoc();
    }

    

    void Attack(Vector3 position)
    {
        RaycastHit hit; int enemyLayer = (1 << 11);

        if (Physics.Raycast(position, Vector3.up * 2f, out hit, 2f, enemyLayer))
        {
            ClearFreeLoc();
            //поворачиваем персонажа
            pSprite.flipX = (position - transform.position).x < 0 || (position - transform.position).z < 0;
            //запускаем анимацию
            StartCoroutine(AttackAnimation(hit.transform));
        }
    }
   

    //конец хода
    void TurnEnd()
    {
        isMyTurn = false;
        Debug.Log("Игрок завершил ход. Передача управления");
        turnObj = null; //блокируем возможность сходить еще
        Destroy(selected);
        ClearFreeLoc();
        GameController.PlayerTurnEnd();
    }

    //началао хода - вызывается из GameController
    

    //Устанавливает подсветку ячеек, по которым можно ходить
    public void GetFreeLoc(bool isStart = false)
    {
        if(Energy >= 1f)
        {
            SetLoc(transform.position + new Vector3(0, 0, 1), 0, isStart);
            SetLoc(transform.position + new Vector3(0, 0, -1), 1, isStart);
            SetLoc(transform.position + new Vector3(1, 0, 0), 2, isStart);
            SetLoc(transform.position + new Vector3(-1, 0, 0), 3, isStart);
        }
    }


    /// <summary>
    /// Set the floor light
    /// 1 - free. 
    /// 2 - enemy. 
    /// 3 - environment. 
    /// 4 - item.
    /// </summary>
    void SetLoc(Vector3 position, int n, bool isStart)
    {
        int floorType = GetLoc(position);
        SpriteRenderer sp;
        switch (floorType)
        {
            case 1: //тут свободно
                freeLocs[n] = Instantiate(FreeFloor);
                freeLocs[n].transform.position = position;
                sp = freeLocs[n].GetComponentInChildren<SpriteRenderer>();
                StartCoroutine(Show(sp, 0.5f));
                break;
            case 4: //тут противник
                freeLocs[n] = Instantiate(FreeFloor);
                freeLocs[n].transform.position = position;
                sp = freeLocs[n].GetComponentInChildren<SpriteRenderer>();
                //делай красным
                sp.color = new Color(1f, 0f, 0f, 0f);
                StartCoroutine(Show(sp, 0.5f));
                break;
            case 2: //тут лежит что-то ценное
                freeLocs[n] = Instantiate(FreeFloor);
                freeLocs[n].transform.position = position;
                sp = freeLocs[n].GetComponentInChildren<SpriteRenderer>();
                //делай зеленым
                sp.color = new Color(0f, 1f, 0f, 0f);
                StartCoroutine(Show(sp, 0.5f));
                break;
            case 3: //тут окружение
                freeLocs[n] = Instantiate(FreeFloor);
                freeLocs[n].transform.position = position;
                sp = freeLocs[n].GetComponentInChildren<SpriteRenderer>();
                //делай синим
                sp.color = new Color(0f, 0f, 1f, 0f);
                StartCoroutine(Show(sp, 0.5f));
                break;
        }
    }

    int GetLoc(Vector3 position)
    {
        //находим все позиции, на которые можно ступить
        //пускаем лучи в 4 стороны и собираем данные 
        //первая сторона
        int floorType = 0; //нет пути
        RaycastHit[] hit = Physics.RaycastAll(position - Vector3.up, Vector3.up * 2f);
        Debug.DrawRay(position - Vector3.up, Vector3.up * 2f, Color.green, 2f);

        foreach (RaycastHit h in hit)
        {
            if (h.collider.tag == EnemyTag && floorType < 4)
            {
                floorType = 4; //тут стоит враг
                continue;
            }
            if (h.collider.tag == EnvironmentTag && floorType < 3)
            {
                floorType = 3; //тут есть что-то с чем можно взаимодейстовать
                continue;
            }
            if (h.collider.tag == ItemTag && floorType < 2)
            {
                floorType = 2; //тут лежит что-то ценное
                continue;
            }
            if (h.collider.tag == FloorTag && hit.Length == 1 && floorType < 1)
            {
                floorType = 1; //тут просто можно ходить
                continue;
            }
        }

        return floorType;
    }

    //очистка массива подсветки пола
    public void ClearFreeLoc()
    {   
        for(int i = 0; i < 4; i++)
        {
            if (freeLocs[i] != null)
            {
                StartCoroutine(Hide(freeLocs[i], 0.5f));
                freeLocs[i] = null;
            }
        }
    }

    /// <summary>
    /// Get the Vector2 from Transform
    /// </summary>
    /// <param name="t">Transform parametr</param>
    /// <returns>Vector2</returns>
    public Vector2 GetPosition(Transform t)
    {
        return new Vector2(t.position.x, t.position.z);
    }

    //коротина анимации появления спрайта
    IEnumerator Show(SpriteRenderer sp, float value)
    {
        for(float i = 0; i < value; i += Time.deltaTime * OpacitySpeed)
        {
            setAlpha(sp, i);
            yield return null;
        }
        setAlpha(sp, value);
    }

    //коротина анимации исчезновения спрайта
    //СПРАЙТ ДОЛЖЕН НАХОДИТЬСЯ У ДОЧЕРНЕГО ЭЛЕМЕНТА
    IEnumerator Hide(GameObject go, float value)
    {
        SpriteRenderer sp = go.GetComponentInChildren<SpriteRenderer>();
        for (float i = value; i >= 0; i -= Time.deltaTime * OpacitySpeed)
        {
            setAlpha(sp, i);
            yield return null;
        }
        setAlpha(sp, 0);
        Destroy(go);
    }

    void setAlpha(SpriteRenderer sp, float value)
    {
        Color color = sp.color;
        color.a = value;
        sp.color = color;
    }

    IEnumerator DieAnimation(Vector3 tPos)
    {
        pAnimator.Play("Die");
        Transform sprite = transform.Find("Character"); //найдем спрайт, который будем двигать
        SpriteRenderer sp = sprite.GetComponentInChildren<SpriteRenderer>();

        Vector3 sPos = sprite.position;//получим позицию спрайта
        //поворот спрайта
        sp.flipX = (transform.position - tPos).x < 0 || (tPos - transform.position).z > 0;

        tPos -= (tPos - sPos) / DamageAnimationShift;
        tPos.y = sPos.y;

        for (float i = 0; i <= 0.4; i += DamageAnimationSpeedShift * Time.deltaTime)
        {   //анимация
            sprite.position = Vector3.Lerp(sPos, tPos, Easing.easeOutExpo(i));
            yield return null;
        }
    }

    IEnumerator AttackAnimation(Transform e)
    {
        pAnimator.Play("PlayerAttack");

        Transform sprite = transform.Find("Character");
        Vector3 sPos = sprite.position;
        Vector3 tPos = e.position;
        tPos -= (e.position - sPos) / AttackAnimationShift;
        tPos.y = sPos.y;

        for(float i = 0; i < 1f; i += AttackAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(sPos, tPos, Easing.easeInOutQuart(i));
            yield return null;
        }

        Enemy enemy = e.GetComponent<Enemy>();

        //крит
        float CC = (float) random.NextDouble();
        float attack = AttackPower;
        bool trueSight = false;

        if (CC <= CriticalChance)
        {   //произошел крит. Вызываем анимацию крита и сообщаем противнику повышенный урон
            trueSight = true;
            attack = AttackPower * CriticalMultiply;
            transform.Find("Character").Find("CRIT").Find("Text").GetComponent<Animator>().Play("Text");
        }
        

        enemy.Damaged(attack, AttackChance, transform.position, trueSight); //нанесение урона

        for (float i = 0; i < 1; i += AttackAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(tPos, sPos, Easing.easeOutQuint(i));
            yield return null;
        }
        sprite.position = sPos;
        beingStep = false;
        turnFloor = null;
        Energy--;
        GetFreeLoc();
    }

    //плавно Анимация получения урона
    IEnumerator DamageAnimation(Vector3 tPos)
    {
        float damageShift = DamageAnimationShift;
        int floorType = GetLoc(transform.position + tPos);

        

        //если позади занято или если игрок защищается (наоборот)
        if (isDefending )
        {   //анимация снятия щита 
            pAnimator.Play("HideShieldDamaged");
            isDefending = false;    //теперь игрок будет получать урон
        }
        else
        {   //анимация получения урона
            pAnimator.Play("Damaged");//проиграть анимацию в аниматоре
        }
        
        Transform sprite = transform.Find("Character"); //найдем спрайт, который будем двигать

        SpriteRenderer sp = sprite.GetComponentInChildren<SpriteRenderer>();

        Vector3 sPos = sprite.localPosition;//получим позицию спрайта
        //поворот спрайта
        sp.flipX = tPos.x > 0 || tPos.z > 0;

        if (isDefending || (floorType != 1 && floorType != 2))
        {
            damageShift *= 2;
        }

        tPos = sPos + (tPos/damageShift);
        tPos.y = sPos.y;

        for (float i = 0; i <= 0.4; i += DamageAnimationSpeedShift * Time.deltaTime)
        {   //анимация
            sprite.localPosition = Vector3.Lerp(sPos, tPos, Easing.easeOutExpo(i));
            yield return null;
        }
        for (float i = 0.4f; i < 1; i += DamageAnimationSpeedShift * Time.deltaTime)
        {
            sprite.localPosition = Vector3.Lerp(tPos, spritePosition, Easing.easeInOutQuad(i));
            yield return null;
        }
        sprite.localPosition = spritePosition;
    }

    
}


