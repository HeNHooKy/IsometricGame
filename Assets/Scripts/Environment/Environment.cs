using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Environment : MonoBehaviour
{
    [Header("Спрайт окружения")]
    [Tooltip("Спрайт, который будет отображаться на кнопке действия")]
    public Sprite Sprite;
    [Tooltip("Тег игрока")]
    public string PlayerTag = "Player";

    protected HUD HUD;

    void Start()
    {
        HUD = transform.parent.parent.parent.Find("/HUD").GetComponent<HUD>();
    }

    public void UsePointTriggered(Collider player)
    {
        if (player.tag == PlayerTag)
        {   //сообщим HUD, что игрок вошел в зону предмета
            HUD.ItemStaySet(gameObject);
        }
    }

    public void UsePointUnTriggered(Collider player)
    {
        if (player.tag == PlayerTag)
        {   //сообщим HUD, что игрок вошел в зону предмета
            HUD.ItemStayClear();
        }
    }

    public virtual void Use(PlayerController player)
    {
        Debug.Log("Использован предмет окружения без реализации!");
    }

}
