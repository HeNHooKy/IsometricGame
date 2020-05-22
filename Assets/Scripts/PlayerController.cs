using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    /*The players health*/
    public float Health = 4f;
    public float Energy = 2f;
    public float EnergyReload = 2f;
    public float Speed = 1f;
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
    bool isWalking = false;
    bool isMyTurn = true;
    bool beingStep = false;

    public void Damaged(float damage)
    {
        //TODO: получение урона
    }

    public void TurnStart()
    {
        Energy += EnergyReload;
        isMyTurn = true;
        Debug.Log("Ход вернулся к игроку");
        GetFreeLoc();
    }


    void Start()
    {
        pAnimator = GetComponentInChildren<Animator>();
        pSprite = GetComponentInChildren<SpriteRenderer>();
        freeLocs = new GameObject[4];
        GetFreeLoc(true);
    }

    void Update()
    {
        
        //получаем объект, на который кликнул пользователь
        turnObj = Touch();

        if(isMyTurn)
        {
            if (turnObj != null)
            {
                if (turnObj.tag == FloorTag)
                    TurnFloor(turnObj);
            }

            if (Energy < 1f)
            {   //энергия кончилась. Конец хода
                TurnEnd();
            }
        }

        //выполняется независимо от блокировок
        Walking();
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
        RaycastHit hit;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
            if (hit.transform == null)
                return null;
            return hit.transform;
        }
        else
        {   //тач отжат
            if(turnFloor != null)
            {
                if(!beingStep)
                {
                    //Сдлеай шаг!
                    beingStep = true;
                    DoStep();
                }
            }
        }
        return null;
    }

    void DoStep()
    {
        //этот метод выполняет любое взаимодействие с окружением
        //проверим свободна ли ячейка
        int layerMask = ~(1 << 10 | 1 << 8); // ~(1 << 10 | 1 << 8)
        //пускаем луч и собираем данные
        RaycastHit hit;
        Physics.Raycast(turnFloor.transform.position, Vector3.up, out hit, 1f, layerMask, QueryTriggerInteraction.Ignore);

        if (RaycastHit.Equals(hit.collider, null))
        {   //клетка пуста
            //Тут свободно. Можно идти
            isWalking = true;
            pAnimator.SetBool("IsWalking", true);
            //забираем энергию за ход
        }
        else
        {
            //Кажется тут занято. Идти нельзя, нужно попробовать другое действие
        }
        Destroy(selected);

        
    }


    void Walking() 
    {   //вызывается только если персонаж находится в движении
        if(turnFloor == null)
        {   //если пол не выбран - метод не выполняется
            return;
        }
        if (isWalking)
        {
            ClearFreeLoc();
            //поворачиваем персонажа
            pSprite.flipX = (turnFloor.transform.position - transform.position).x < 0 || (turnFloor.transform.position - transform.position).z < 0;
            float step = Speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, turnFloor.transform.position, step);
        }

        if(GetPosition(turnFloor.transform) == GetPosition(transform))
        {   //сбрасываем флажок ходьбы, когда цель достигнута
            pAnimator.SetBool("IsWalking", false);
            beingStep = false;
            isWalking = false;
            turnFloor = null;
            Energy--;
            GetFreeLoc();
        }
    }

   

    //конец хода
    void TurnEnd()
    {
        isMyTurn = false;
        Debug.Log("Игрок завершил ход. Передача управления");
        ClearFreeLoc();
        GameController.TurnEnd();
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
    /// 1 - free
    /// 2 - enemy
    /// 3 - environment
    /// 4 - item
    /// </summary>
    /// <param name="position"></param>
    void SetLoc(Vector3 position, int n, bool isStart)
    {
        //находим все позиции, на которые можно ступить
        //пускаем лучи в 4 стороны и собираем данные 
        //первая сторона
        int floorType = 0; //нет пола
        RaycastHit[] hit = Physics.BoxCastAll(position, new Vector3(0.25f, 0, 0.25f), Vector3.up, transform.rotation, 1f);
        foreach(RaycastHit h in hit)
        {
            if (h.collider.tag == FloorTag)
                floorType = 1; //тут можно ходить
            if (h.collider.tag == EnvironmentTag)
                floorType = 3; //тут есть что-то с чем можно взаимодейстовать
            if (h.collider.tag == ItemTag)
                floorType = 4; //тут лежит что-то ценное
            if (h.collider.tag == EnemyTag)
                floorType = 2; //тут стоит враг

        }
        switch(floorType)
        {
            case 1:
                freeLocs[n] = Instantiate(FreeFloor);
                freeLocs[n].transform.position = position;
                SpriteRenderer sp = freeLocs[n].GetComponentInChildren<SpriteRenderer>();
                if(isStart)
                {   //если метод вызван со старта
                    sp.color = new Color(1f, 1f, 1f, 0.5f);
                }
                else
                {
                    StartCoroutine(Show(sp));
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
                StartCoroutine(Hide(freeLocs[i]));
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
    IEnumerator Show(SpriteRenderer sp)
    {
        for(float i = 0; i < 0.5; i += Time.deltaTime * OpacitySpeed)
        {
            sp.color = new Color(1f, 1f, 1f, i);
            yield return null;
        }
        sp.color = new Color(1f, 1f, 1f, 0.5f);
    }

    //коротина анимации исчезновения спрайта
    //СПРАЙТ ДОЛЖЕН НАХОДИТЬСЯ У ДОЧЕРНЕГО ЭЛЕМЕНТА
    IEnumerator Hide(GameObject go)
    {
        SpriteRenderer sp = go.GetComponentInChildren<SpriteRenderer>();

        for (float i = 0.5f; i >= 0; i -= Time.deltaTime * OpacitySpeed)
        {
            sp.color = new Color(1f, 1f, 1f, i);
            yield return null;
        }

        Destroy(go);
    }

}


