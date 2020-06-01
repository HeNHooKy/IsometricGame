using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AmbienceGenerator : MonoBehaviour
{
    public float Chance = 0.4f;
    System.Random random = new System.Random();


    //простейший скрипт. Отображает или скрывает предмет окружения с заданной вероятностью
    public void Generate()
    {
        Generate(Chance);
    }

    public void Generate(float chance)
    {
        if (random.NextDouble() <= chance)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
        
    
}
