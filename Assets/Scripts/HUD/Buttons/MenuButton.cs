﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [Header("Найстройки кнопки")]
    [Tooltip("ID кнопки из доступных")]
    public ButConst butId;
    [Tooltip("Спрайт не активированной кнопки")]
    public Sprite NonActive;
    [Tooltip("Спрайт выбранной кнопки")]
    public Sprite Selected;
    [Tooltip("Спрайт вжатой кнопки")]
    public Sprite Activated;

    [HideInInspector]
    public UnityEvent PressButton = new UnityEvent();
    [HideInInspector]
    public UnityEvent UnPressButton = new UnityEvent();

    private Image butImage;//указатель на картинку кнопки
    private MainMenu Menu;


    private void Awake()
    {
        butImage = transform.Find("Base").GetComponent<Image>();
        Menu = transform.parent.parent.GetComponent<MainMenu>();
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
        DownShift(transform.GetComponent<RectTransform>(), -5);
        Menu.TurnedBut = butId;
    }

    public void UnPressed()
    {   //кнопку просто отжали (без использования)
        DownShift(transform.GetComponent<RectTransform>(), 5);
        butImage.sprite = NonActive;
    }

    public void Action()
    {   //выполняется действие назначенное на кнопку
        DownShift(transform.GetComponent<RectTransform>(), 5);
        butImage.sprite = Activated;
        Menu.ClickedButton(butId);
    }

    private void DownShift(RectTransform trans, float y)
    {
        trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y - y, trans.localPosition.z);
    }
}
