using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyStone : Item
{
    [Header("Эффективность предмета")]
    public float EnergyUp = 2f;

    protected override IEnumerator Do(PlayerController player, GameObject item)
    {
        player.MaxEnergyUp(EnergyUp);
        yield return null;
    }
}
