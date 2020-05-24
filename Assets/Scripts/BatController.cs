using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BatController : Enemy
{
    void Start()
    {
        eAnimator = transform.Find("Character").Find("Sprite").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isEnemiesTurn)
        {
            //EnemiesStillTurn();
        }
    }
}
