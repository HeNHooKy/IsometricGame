using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicHorn : Item
{
    [Header("Эффективность предмета")]
    [Tooltip("Увеличение крита")]
    public float CriticalChanceUp = 0.05f;
    [Tooltip("Увеличение дамага")]
    public float DamageUp = 2f;

    protected override IEnumerator Do(PlayerController player, GameObject item)
    {
        yield return null;
        player.CriticalChance += CriticalChanceUp;
        player.AttackPower += DamageUp;
        Destroy(item);
    }
}
