using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public bool isCanDo = true;
    public GameController gameController;
    public Color turnColor = Color.red;
    public string floorTag = "Floor";
    public float energy = 1f;
    public float speed = 1f;
    public float height = 0.5f;


    Transform turnObj;
    Material turnFloorMateraial;
    GameObject turnFloor = null;
    Animator pAnimator;
    Color prevColor;
    bool isWalking = false;
    bool isMyTurn = true;
    bool beingStep = false;

    void Start()
    {
        pAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        
        //получаем объект, на который кликнул пользователь
        turnObj = Touch();

        if(isMyTurn)
        {
            if (turnObj != null)
            {
                if (turnObj.tag == floorTag)
                    TurnFloor(turnObj);
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

            //ИЗМЕНИТЬ НА ТЕКСТУРУ
            turnFloorMateraial.color = prevColor; //смена цвета больее не выбранного объекта на предыдущий цвет
        }

        float distance = Math.Abs((GetPosition(obj) - GetPosition(transform)).magnitude);
        if (distance > 1 || distance < 0.01)
        {   //выполняется, если был выбран floor на растоянии привышающем максимальную дистанцию шага

            //ИЗМЕНИТЬ НА ТЕКСТУРУ
            if(turnFloor != null)
                turnFloorMateraial.color = prevColor;

            turnFloorMateraial = null;
            turnFloor = null;
            return;
        }

        turnFloor = obj.gameObject; //сохранение выбранного игроком объекта
        turnFloorMateraial = turnFloor.GetComponent<MeshRenderer>().material; //доступ к материалу


        //ИЗМЕНИТЬ НА ТЕКСТУРУ
        prevColor = turnFloorMateraial.color; //сохранение обычного цвета
        turnFloorMateraial.color = turnColor; //смена цвета
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
            energy--;
        }
        else
        {
            //Кажется тут занято. Идти нельзя, нужно попробовать другое действие
        }
        turnFloorMateraial.color = prevColor;

        if(energy < 1f)
        {   //энергия кончилась. Конец хода
            TurnEnd();
        }
    }


    void Walking() 
    {   //вызывается только если персонаж находится в движении
        if(turnFloor == null)
        {   //если пол не выбран - метод не выполняется
            return;
        }
        if (isWalking)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.localPosition, (Vector3.up * height) + turnFloor.transform.position, step);
        }

        if(GetPosition(turnFloor.transform) == GetPosition(transform))
        {   //сбрасываем флажок ходьбы, когда цель достигнута
            pAnimator.SetBool("IsWalking", false);
            beingStep = false;
            isWalking = false;
            turnFloor = null;
        }
    }

    void TurnEnd()
    {
        isMyTurn = false;
        gameController.TurnEnd();
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
}
