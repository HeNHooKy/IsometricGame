using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoLine : MonoBehaviour
{
    [Header("Пустой спрайт")]
    public Sprite UIMask;
    [Header("Белый квадрат")]
    public Sprite WhiteSquare;
    [Header("Отображаемые данные")]
    public Fields Field;
    [Header("Вариативные параметры")]
    [Tooltip("Отрицательный сдвиг")]
    public int Shift = 2;
    [Tooltip("Максимум в этом поле")]
    public int Max;

    private List<Image> points = new List<Image>();
    private PlayerController player;

    public enum Fields
    {
        HP = 0,
        Str = 1,
        Atc = 2,
        Crt = 3, 
        Agi = 4
    }

    private void Start()
    {
        //ищем батю
        player = transform.parent.parent.GetComponent<HUD>().Player.GetComponent<PlayerController>();
        //собираем все точки в список
        points.AddRange(GetComponentsInChildren<Image>());
    }

    private void Update()
    {
        switch (Field)
        {
            case Fields.HP:
                DisplayHP();
                break;
            case Fields.Str:
                DisplayStr();
                break;
            case Fields.Atc:
                DisplayAtc();
                break;
            case Fields.Crt:
                DisplayCrt();
                break;
            case Fields.Agi:
                DisplayAgi();
                break;
        }
    }

    //Умеет отображать только уклонение
    private void DisplayAgi()
    {
        float agi = player.BiasChance * 10 * 2;
        DisplayCount((int)agi);
    }

    //Умеет отображать только стамину
    private void DisplayStr()
    {
        float energy = player.EnergyReload;
        DisplayCount((int)energy - Shift);
    }

    //Умеет отображать только атаку
    private void DisplayAtc()
    {
        float atcPw = player.AttackPower;
        DisplayCount((int)atcPw - Shift);
    }

    //умеет отображать только хп
    private void DisplayHP()
    {
        float hp = player.MaxHealth;
        DisplayCount((int)hp * 10 / Max);
    }

    //умеет отображать только криты
    private void DisplayCrt()
    {
        float crtCh = player.CriticalChance * 10 * 2;
        DisplayCount((int) crtCh);
    }

    //отображает заданное число точек
    private void DisplayCount(int count)
    {
        if (count > 10)
            return; //не может отобразить больше положенного

        //обнуляй!
        for(int i = 0; i < 10; i++)
        {
            points[i].sprite = UIMask;
        }
        //пиши!
        for(int i = 0; i < count; i++)
        {
            points[i].sprite = WhiteSquare;
        }
    }
}
