﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : Item
{
    [Header("Количество восстанавливаемого здоровья")]
    [Tooltip("Мощьность хила")]
    public float HealPower = 5f;
    protected override IEnumerator Do(PlayerController player, GameObject item)
    {
        //player.Drink();
        player.Heal(HealPower);
        player.DisplayHearts();
        Destroy(item);
        yield return null;
        
    }
}
