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

    /// <summary>
    /// Get random position around slime,
    /// if around slime exist player - return player position
    /// </summary>
    Vector3 GetRandomPosition(out bool isNotExist)
    {
        bool isPlayer;
        List<Vector3> locations = GetFreeLocation(out isPlayer);

        isNotExist = false;
        if(locations.Count == 0)
        {
            isNotExist = true;
            return Vector3.zero;
        }
        if(isPlayer)
        {
            return locations.Last();
        }
        else
        {
            return locations[random.Next(locations.Count)];
        }    
    }
}
