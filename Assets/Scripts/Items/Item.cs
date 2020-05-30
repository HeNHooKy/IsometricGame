using System.Collections;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public int Rarity = 1;  //ценность предмета (1-common)
    public bool isActivate = true; //предмет активируемый?
    public string PlayerTag = "Player"; //тег игрока
    public Sprite ItemSprite; //спрайт, который будет отображаться в HUD
    public int count = 1;   //количество предметов в стаке
    public int maxStack = 4;
    public int id;  //номер предмета
    private HUD HUD;

    private GameObject that;

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

    //вызывается в HUD принимает указатель на игрока. 
    //в корутине выолняет переопределенные действия
    public void Use(PlayerController player)
    {
        that = Instantiate(this.gameObject);
        that.GetComponent<Item>().StartCoroutine(that.GetComponent<Item>().Do(player, that));
    }

    public void PickUp(bool isNeedDelete = true)
    {   //вызов анимции подъема предмета
        StartCoroutine(PickedUp());
    }

    IEnumerator PickedUp()
    {   //анимация подъема, божественное свечение
        yield return null;
        Destroy(this.gameObject);
    }

    //выполняет объявленные действия с игроком
    protected abstract IEnumerator Do(PlayerController player, GameObject item);
}
