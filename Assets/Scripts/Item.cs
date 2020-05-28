using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int Rarity = 1;  //ценность предмета (1-common)
    public bool isActivate = true; //предмет активируемый?
    public string PlayerTag = "Player"; //тег игрока
    public Sprite ItemSprite; //спрайт, который будет отображаться в HUD
    public int count = 1;   //количество предметов в стаке
    public int maxStack = 4;
    public int id;  //номер предмета
    private HUD HUD;

    public static int Sum(int count, Item item)
    {
        int c = (item.count - (item.maxStack - count) > 0) ? item.maxStack - count : item.count;
        count += c;
        item.count -= c;
        return count;
    }

    void Start()
    {
        HUD = transform.Find("/HUD").GetComponent<HUD>();
    }

    private void OnTriggerEnter(Collider player)
    {
        if(player.tag == PlayerTag)
        {   //сообщим HUD, что игрок вошел в зону предмета
            HUD.ItemStaySet(this.gameObject);
        }
    }

    private void OnTriggerExit(Collider player)
    {
        if (player.tag == PlayerTag)
        {   //сообщим HUD, что игрок вышел из зоны предмета
            HUD.ItemStayClear();
        }
    }

    public void PickUp(bool isNeedDelete = true)
    {   //вызов анимции подъема предмета
        Destroy(this.gameObject);
    }

    IEnumerator PickedUp()
    {   //анимация подъема, божественное свечение
        yield return null;
    }
}
