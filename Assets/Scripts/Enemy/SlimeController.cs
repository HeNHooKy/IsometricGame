using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlimeController : Enemy
{

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

    
    //в этом блоке переопределяется анимация ходьбы слизня(т.к. его перемещение специфично)
    protected override IEnumerator _Move(Vector3 sPos, Vector3 tPos)
    {
        eAnimator.Play("JumpPrepare");
        //ждём пока анимация будет завершена
        while (!eAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            yield return null;
        }

        for (float i = 0; i < 1; i += Time.deltaTime * MoveSpeed)
        {
            transform.position = Vector3.Lerp(sPos, tPos, i);
            yield return null;
        }

        transform.position = tPos;
        StepOut();
    }


}
