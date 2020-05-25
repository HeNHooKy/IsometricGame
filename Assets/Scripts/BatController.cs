﻿using System.Linq;
using UnityEngine;

public class BatController : Enemy
{

    
    void Start()
    {
        eAnimator = transform.Find("Character").Find("Sprite").GetComponent<Animator>();
        controller = transform.Find("/GameController").GetComponent<GameController>();
        eSprite = transform.Find("Character").Find("Sprite").GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isEnemiesTurn && !turnCatch && isAlive)
        {
            isMyTurn = true;
            turnCatch = true;
            
            Energy += EnergyReload; //восполняем энергию
        }

        if(isMyTurn)
        {
            if (Energy < 1f)
            {   //конец хода
                isMyTurn = false;
                turnCatch = false;
                controller.EnemyTurnEnd();
                return;
            }
            //каждый фрейм сообщаем контроллеру, что наш ход еще не окончен
            controller.EnemyTurnIsNotEnd();
        }

        if (isMyTurn && !isBeingStep)
        {
            //релизуем AI
            FindPath(); //ищем путь до игрока
            if (PathToPlayer != null)
            {
                //рисуем путь
                Vector3 prev = PathToPlayer[0];
                for (int i = 1; i < PathToPlayer.Count; i++)
                {
                    Debug.DrawLine(prev + Vector3.up * 1, PathToPlayer[i] + Vector3.up * 1, Color.green, 1f);
                    prev = PathToPlayer[i];
                }

                PathToPlayer.Remove(PathToPlayer.First()); //стираем позицию монстра
                if (GetCell(PathToPlayer.First()) == 0)
                {   //тут нет игрока и можно ходить
                    Move(PathToPlayer.First());
                }
                else if (GetCell(PathToPlayer.First()) == 2)
                {   //тут игрок, в атаку!
                    CloseAttack(PathToPlayer.First());
                }
            }
            else
            {
                isMyTurn = false;
                turnCatch = false;
                Energy = 0;
                controller.EnemyTurnEnd();
            }
             
        }
    }
}
