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


    private Transform turnObj;
    private Material turnFloorMateraial;
    private GameObject turnFloor = null;
    private Color prevColor;


    void Update()
    {
        //получаем объект, на который кликнул пользователь
        turnObj = Touch();
        if (turnObj != null)
        {
            if (turnObj.tag == floorTag)
                TurnFloor(turnObj);
        }
    }


    /// <summary>
    /// This method change the floor texture
    /// </summary>
    void TurnFloor(Transform obj)
    {
        if (turnFloor == obj.gameObject)
            return;

        if (turnFloor != null)
        {   //выполняется только если это не первый выделенный объект 
            //или указатель не был смещен на неактивную область

            //ИЗМЕНИТЬ НА ТЕКСТУРУ
            turnFloorMateraial.color = prevColor; //смена цвета больее не выбранного объекта на предыдущий цвет
        }

        float distance = Math.Abs((GetPosition(obj) - GetPosition(transform)).magnitude);
        if (distance > energy || distance < 0.01)
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
        {
            if(turnFloor != null)
            {
                //Сдлеай шаг!
                DoStep();
            }
        }
        return null;
    }

    void DoStep()
    {
        //этот метод выполняет любое взаимодействие с окружением
        //проверим свободна ли ячейка
        RaycastHit[] allColliders = Physics.BoxCastAll(turnFloor.transform.position, new Vector3(0.25f, 0.25f, 0.25f), Vector3.up, transform.rotation, 5); //строим луч
        if(allColliders.Length <= 1)
        {
            //Тут свободно. Можно идти                                                  ТУТ МОЖНО МЕНЯТЬ ВЫСОТУ
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, (Vector3.up * 0.15f) + turnFloor.transform.position, 0.1f);
        }
        else
        {
            //Кажется занято. Идти нельзя, нужно попробовать другое действие
        }
        turnFloorMateraial.color = prevColor;
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
