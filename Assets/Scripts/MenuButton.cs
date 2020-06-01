using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public ButConst butId;

    public Sprite NonActive;
    public Sprite Selected;
    public Sprite Activated;

    public UnityEvent PressButton = new UnityEvent();
    public UnityEvent UnPressButton = new UnityEvent();

    private Image butImage;//указатель на картинку кнопки
    private HUD HUD;


    private void Start()
    {
        butImage = transform.Find("Base").GetComponent<Image>();
        HUD = transform.parent.parent.GetComponent<HUD>();
        PressButton.AddListener(Pressed);
        UnPressButton.AddListener(Action);
    }

    public void Press()
    {
        PressButton.Invoke();
    }

    public void UnPress()
    {
        UnPressButton.Invoke();
    }

    public void Pressed()
    {
        butImage.sprite = Selected;
        HUD.TurnedBut = butId;
    }

    public void UnPressed()
    {   //кнопку просто отжали (без использования)
        butImage.sprite = NonActive;
    }

    public void Action()
    {   //выполняется действие назначенное на кнопку
        butImage.sprite = Activated;
        HUD.ClickedButton(butId);
    }

}
