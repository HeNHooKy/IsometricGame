using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuButton_HUD : MonoBehaviour
{
    [Header("Настройки кнопки")]
    [Tooltip("ID кнопки из возможных")]
    public ButConst ButtonId = 0;
    [Tooltip("Спрайт стекла, которое появится, когда кнопка вжата")]
    public Sprite ActivatedGlass;
    [Tooltip("Сдвиг при нажатии")]
    public float Shift = 0.1f;  //смещение icon и count
    [Tooltip("Скорость сдвига")]
    public float AnimationSpeed = 1f;
    

    [HideInInspector]
    public UnityEvent PressButton = new UnityEvent();
    [HideInInspector]
    public UnityEvent UnPressButton = new UnityEvent();

    private MainMenu menu;
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
        menu = transform.parent.parent.GetComponent<MainMenu>();
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
        AnimationPress();
    }

    void Action()
    {
        //то, что происходит, когда кнопка отжимается
        AnimationUnPress();
        menu.ClickedButton(ButtonId);
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
        DownShift(icon, -Shift);
        DownShift(count, -Shift);
        DownShift(glassTransform, -Shift);
        glassTransform.GetComponent<Image>().sprite = baseGlass;
        background.SetActive(true);
        c.a = 0f;
        activated.color = c;
    }

    private void DownShift(RectTransform trans, float y)
    {
        trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y - y, trans.localPosition.z);
    }
}
