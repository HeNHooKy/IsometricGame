using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlimeController : Enemy
{
    System.Random random = new System.Random();

    void Start()
    {
        eAnimator = transform.Find("Character").Find("Sprite").GetComponent<Animator>();
        controller = transform.Find("/GameController").GetComponent<GameController>();
        eSprite = transform.Find("Character").Find("Sprite").GetComponent<SpriteRenderer>();
    }
    
    

    // Update is called once per frame
    void Update()
    {

        if (isMyTurn)
        {   //проверка на конец хода
            if (Energy < 1f)
            {   //конец хода
                isMyTurn = false;
                return;
            }
        }

        if (isMyTurn && !isBeingStep)
        {
            //релизуем AI
            bool isNotExist;
            Vector3 target = GetRandomPosition(out isNotExist);
            if(!isNotExist)
            {   //путь есть
                if (GetCell(target) == 0)
                {   //тут нет игрока и можно ходить
                    Move(target);
                }
                else if (GetCell(target) == 2)
                {   //тут игрок, в атаку!
                    CloseAttack(target);
                }
            }
            else
            {   //нечего делать - конец хода
                isMyTurn = false;
                Energy = 0;
            }
        }
    }

    
}
