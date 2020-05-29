using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Button : MonoBehaviour
{
    public int ButtonId = 0;
    public float AnimationSpeed = 1f;

    private HUD HUD;
    private Image activated;
    private Button button;
    private void Start()
    {   //двигаемся к родителю родителя - это и будет HUD
        HUD = transform.parent.parent.GetComponent<HUD>();
        activated = transform.Find("Activated").GetComponent<Image>();
        button = transform.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {   //была нажата кнопка
        StartCoroutine(Animation());
        HUD.ClickedButton(ButtonId);
    }

    private IEnumerator Animation()
    {
        Color c = activated.color;

        //тут какая-то анимация
        for (float i = 0; i < 1; i += Time.deltaTime * AnimationSpeed)
        {
            c.a = easeOutQuint(i);
            activated.color = c;
            yield return null;
        }
        c.a = 1f;
        for (float i = 0; i < 1; i += Time.deltaTime * AnimationSpeed)
        {
            c.a = easeOutQuint(1 - i);
            activated.color = c;
            yield return null;
        }
        c.a = 0f;
        activated.color = c;
    }


    private float easeOutQuint(float x) {
        return (float)(1 - Math.Pow(1 - x, 5));
    }
}
