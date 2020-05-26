using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float DamageAnimationShift = 2;  //сдвиг анимации при получении урона
    public float DamageAnimationSpeedShift = 2f;//скорость сдвига анимации при получении урона

    public float AttackAnimationSpeedShift = 0.1f;
    public float AttackAnimationShift = 2f;

    public float Health = 4f;
    public float Energy = 0f;
    public float EnergyReload = 2f;
    public float DieTime = 10f;
    public float AttackPower = 1f; //сила атаки
    public float MoveSpeed = 1f;
    public GameController GameController;
    public GameObject FreeFloor;
    public GameObject SelectFloor;
    public float OpacitySpeed = 2f;
    public string FloorTag = "Floor";
    public string EnemyTag = "Enemy";
    public string EnvironmentTag = "Environment";
    public string ItemTag = "Item";


    Transform turnObj;
    GameObject turnFloor = null;
    Animator pAnimator;
    SpriteRenderer pSprite;
    GameObject selected;
    GameObject[] freeLocs;
    private bool isMyTurn = false;
    private bool beingStep = false;
    private bool isAlive = true;

    private bool isDamaged = false;

    void Start()
    {
        pAnimator = GetComponentInChildren<Animator>();
        pSprite = transform.Find("Character").Find("Sprite").GetComponent<SpriteRenderer>();
        freeLocs = new GameObject[4];
        TurnStart();
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

    public void Damaged(float damage, Vector3 pPos)
    {
        if (!isAlive || isDamaged) return;
        isDamaged = true;   //сейчас персонаж уже получает урон (исключение необратимых сдвигов спрайта)
        Health -= damage;
        if(Health > 0f)
        {   //показвыаем анимацию получения урона
            StartCoroutine(DamageAnimation(transform.position + (transform.position - pPos)));//берем обратную от положения персонажа
        }
        if (Health <= 0f)
        {   //если здоровье упало слишком низко наступает смерть
            //тут показываем анимацию смерти
            //pAnimator.Play("Die");
            ClearFreeLoc(); //очищаем все свободные пути
            isAlive = false;
            Invoke("Die", DieTime);
        }
    }

    public void TurnStart()
    {
        if (!isAlive)
            return; //смерть. Ход не возвращается

        ClearFreeLoc();
        Energy += EnergyReload;
        isMyTurn = true;
        Debug.Log("Ход вернулся к игроку");
        GetFreeLoc();
    }

    //смерть
    private void Die()
    {
        if (!isAlive) return;   //То, что мертво, умереть не может!
        
        Destroy(this.gameObject);
        //вызов окна конца игры
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
        if(isMyTurn && !beingStep)
        {
            RaycastHit hit;
            int layerMask = (1 << 8);
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, layerMask);
                if (hit.transform == null)
                    return null;
                return hit.transform;
            }
            else
            {   //тач отжат
                if (turnFloor != null)
                {
                    if (!beingStep)
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
        //этот метод выполняет любое взаимодействие с окружением
        //проверим свободна ли ячейка
        int layerMask = ~(1 << 10 | 1 << 8); // ~(1 << 10 | 1 << 8)
        //пускаем луч и собираем данные
        RaycastHit hit;
        Physics.Raycast(turnFloor.transform.position, Vector3.up, out hit, 1f, layerMask, QueryTriggerInteraction.Ignore);

        if (RaycastHit.Equals(hit.collider, null))
        {   //клетка пуста
            //Тут свободно. Можно идти
            Move(turnFloor.transform.position);
        }
        else if (hit.collider.tag == EnemyTag)
        {
            Attack(hit.collider.gameObject);
        }
        else 
        { 
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

    

    void Attack(GameObject e)
    {
        ClearFreeLoc();
        //поворачиваем персонажа
        pSprite.flipX = (e.transform.position - transform.position).x < 0 || (e.transform.position - transform.position).z < 0;
        //запускаем анимацию
        StartCoroutine(AttackAnimation(e.transform));
    }
   

    //конец хода
    void TurnEnd()
    {
        isMyTurn = false;
        Debug.Log("Игрок завершил ход. Передача управления");
        ClearFreeLoc();
        GameController.PlayerTurnEnd();
    }

    //началао хода - вызывается из GameController
    

    //Устанавливает подсветку ячеек, по которым можно ходить
    void GetFreeLoc(bool isStart = false)
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
        //находим все позиции, на которые можно ступить
        //пускаем лучи в 4 стороны и собираем данные 
        //первая сторона
        int floorType = 0; //нет пола
        RaycastHit[] hit = Physics.RaycastAll(position - Vector3.up, Vector3.up*2f);
        
        foreach (RaycastHit h in hit)
        {
            if (h.collider.tag == EnemyTag)
            {
                floorType = 2; //тут стоит враг
                break;
            }
            if (h.collider.tag == EnvironmentTag)
            {
                floorType = 3; //тут есть что-то с чем можно взаимодейстовать
                break;
            }
            if (h.collider.tag == ItemTag)
            {
                floorType = 4; //тут лежит что-то ценное
                break;
            }
            if (h.collider.tag == FloorTag && hit.Length == 1)
            {
                floorType = 1; //тут можно ходить
                break;
            }
        }

        SpriteRenderer sp;
        switch (floorType)
        {
            case 1: //тут свободно
                freeLocs[n] = Instantiate(FreeFloor);
                freeLocs[n].transform.position = position;
                sp = freeLocs[n].GetComponentInChildren<SpriteRenderer>();
                if (isStart)
                {   //если метод вызван со старта
                    sp.color = new Color(1f, 1f, 1f, 0.5f);
                }
                else
                {
                    StartCoroutine(Show(sp, 0.5f));
                }
                break;
            case 2: //тут противник
                freeLocs[n] = Instantiate(FreeFloor);
                freeLocs[n].transform.position = position;
                sp = freeLocs[n].GetComponentInChildren<SpriteRenderer>();
                sp.color = Color.red;

                if (isStart)
                {   //если метод вызван со старта
                    //делай красным
                    sp.color = new Color(1f, 0f, 0f, 0.5f);
                }
                else
                {
                    //делай красным
                    sp.color = new Color(1f, 0f, 0f, 0f);
                    StartCoroutine(Show(sp, 0.5f));
                }
                break;
        }
    }

    //очистка массива подсветки пола
    void ClearFreeLoc()
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
            sprite.position = Vector3.Lerp(sPos, tPos, easeInOutQuart(i));
            yield return null;
        }

        Enemy enemy = e.GetComponent<Enemy>();
        enemy.Damaged(AttackPower, transform.position); //нанесение урона

        for (float i = 0; i < 1; i += AttackAnimationSpeedShift * Time.deltaTime)
        {
            sprite.position = Vector3.Lerp(tPos, sPos, easeOutQuint(i));
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
        
        pAnimator.Play("Damaged");//проиграть анимацию в аниматоре
        Transform sprite = transform.Find("Character"); //найдем спрайт, который будем двигать

        SpriteRenderer sp = sprite.GetComponentInChildren<SpriteRenderer>();

        Vector3 sPos = sprite.position;//получим позицию спрайта
        //поворот спрайта
        sp.flipX = (transform.position - tPos).x < 0 || (tPos - transform.position).z > 0;

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
        isDamaged = false;
    }

    float easeOutExpo(float x)
    {
        return (float)(x == 1f ? 1f : 1f - Math.Pow(2f, -10f * x));
    }
    float easeInOutQuad(float x)
    {
        return (float)(x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2);
    }
    float easeInOutQuart(float x) {
        return  x< 0.5 ? (float) (8 * x* x* x* x) : (float) (1 - Math.Pow(-2 * x + 2, 4) / 2);
    }

    float easeOutQuint(float x)
    {
        return (float) (1 - Math.Pow(1 - x, 5));
    }
}


