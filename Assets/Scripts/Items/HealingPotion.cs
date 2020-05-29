using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : Item
{
    public float HealthPower = 5f;
    protected override IEnumerator Do(PlayerController player, GameObject item)
    {
        //player.Drink();
        player.Health += HealthPower;
        yield return null;
        Destroy(item);
    }
}
