using System.Linq;
using UnityEngine;

public class BatController : Enemy
{
    
    void Start()
    {
        StartHP = Health;
        hp = transform.Find("Character").Find("HealthBar").GetComponent<HealthBar>();
        eAnimator = transform.Find("Character").Find("Sprite").GetComponent<Animator>();
        controller = transform.Find("/GameController").GetComponent<GameController>();
        eSprite = transform.Find("Character").Find("Sprite").GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isAlive)
        {
            hp.Set(Health / StartHP);
        }

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
            PathToPlayer = null;    //сбрасываем путь (на всякий случай)
            //релизуем AI
            FindPath(); //ищем путь до игрока
            
            if (PathToPlayer != null)
            {
                /*
                //рисуем путь
                Vector3 prev = PathToPlayer[0];
                for (int i = 1; i < PathToPlayer.Count; i++)
                {
                    Debug.DrawLine(prev + Vector3.up * 1, PathToPlayer[i] + Vector3.up * 1, Color.green, 1f);
                    prev = PathToPlayer[i];
                }*/
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
            {   //пути до игрока не существует. Конец хода
                isMyTurn = false;
                Energy = 0;
            }
             
        }
    }
}
