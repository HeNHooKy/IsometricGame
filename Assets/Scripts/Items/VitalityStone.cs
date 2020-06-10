using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitalityStone : Item
{
    [Header("Настройка эффективности")]
    [Tooltip("На сколько увеличится ХП")]
    public float HPUP = 4f;

    protected override IEnumerator Do(PlayerController player, GameObject item)
    {
        yield return null;
        player.MaxHPUp(HPUP);
    }
}
