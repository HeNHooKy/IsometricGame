using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUD_Button : MonoBehaviour
{
    public int ButtonId = 0;
    public float AnimationSpeed = 1f;
    public float Shift = 0.1f;  //смещение icon и count

    public Sprite ActivatedGlass;

    public UnityEvent PressButton = new UnityEvent();
    public UnityEvent UnPressButton = new UnityEvent();

    private HUD HUD;
    private Image activated;

    private GameObject background;
    private RectTransform icon;
    private RectTransform count;

    private Sprite baseGlass;

    private RectTransform glassTransform;

    private void Start()
    {   //двигаемся к родителю родителя - это и будет HUD
        background = transform.Find("Background").gameObject;
        icon = transform.Find("Icon").GetComponent<RectTransform>();
        count = transform.Find("Count").GetComponent<RectTransform>();
        HUD = transform.parent.parent.GetComponent<HUD>();
        activated = transform.Find("Activated").GetComponent<Image>();
        glassTransform = transform.Find("Glass").GetComponent<RectTransform>();
        baseGlass = glassTransform.GetComponent<Image>().sprite;

        //необходимо добавить нажатие кнопки только тогда, когда идет фаза начала прикосновения
        //чтобы поймать событие тач создадим event и будем слушать Touch из PlayerController
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

    void Pressed()
    {   //анимация и блокировка при нажатии на кнопку
        HUD.SetLockControll(true);
        AnimationPress();
    }

    void Action()
    {
        //то, что происходит, когда кнопка отжимается
        HUD.SetLockControll(false);
        AnimationUnPress();
        HUD.ClickedButton(ButtonId);
    }

    private void AnimationPress()
    {
        Color c = activated.color;
        DownShift(icon, Shift);
        DownShift(count, Shift);
        DownShift(glassTransform, Shift);
        glassTransform.GetComponent<Image>().sprite = ActivatedGlass;
        background.SetActive(false);    //вырубаем "задний фон" - создает ощущение вжатости кнопки
        c.a = 1f;
        activated.color = c;
    }

    private void AnimationUnPress()
    {
        Color c = activated.color;

        DownShift(icon, - Shift);
        DownShift(count, - Shift);
        DownShift(glassTransform, -Shift);
        background.SetActive(true);
        c.a = 0f;
        activated.color = c;
    }



    private float easeOutQuint(float x) {
        return (float)(1 - Math.Pow(1 - x, 5));
    }

    private void DownShift(RectTransform trans, float y)
    {
        trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y - y, trans.localPosition.z);
    }
}
