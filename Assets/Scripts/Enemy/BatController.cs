using System.Linq;
using UnityEngine;

public class BatController : Enemy
{


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
                isMyTurn = false;
                return;
            }
        }

        if (isMyTurn && !isBeingStep)
        {
            Path = null;    //сбрасываем путь (на всякий случай)
            //релизуем AI
            FindPath(); //ищем путь до игрока
            
            if (Path != null)
            {
                /*
                //рисуем путь
                Vector3 prev = Path[0];
                for (int i = 1; i < Path.Count; i++)
                {
                    Debug.DrawLine(prev + Vector3.up * 1, Path[i] + Vector3.up * 1, Color.green, 1f);
                    prev = Path[i];
                }*/
                Path.Remove(Path.First()); //стираем позицию монстра
                if (GetCell(Path.First()) == 0)
                {   //тут нет игрока и можно ходить
                    Move(Path.First());
                }
                else if (GetCell(Path.First()) == 2)
                {   //тут игрок, в атаку!
                    CloseAttack(Path.First());
                }
            }
            else
            {   //пути до игрока не существует. Пойдем куда-нибудь
                bool isNotExist;
                Vector3 target = GetRandomPosition(out isNotExist);
                if (!isNotExist)
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
}
