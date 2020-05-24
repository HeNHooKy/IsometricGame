using UnityEngine;

public class BatController : Enemy
{
    void Start()
    {
        eAnimator = transform.Find("Character").Find("Sprite").GetComponent<Animator>();
        controller = transform.Find("/GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {

        if(isMyTurn)
        {
            //EnemiesStillTurn();
        }
    }
}
