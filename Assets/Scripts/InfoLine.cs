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
    [Tooltip("Максимум энергии. Настраивается только для поля Str")]
    public int MaxEnergy;
    [Tooltip("Максимум хелсы. Настраивается только для поля HP")]
    public int MaxHp;

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

    //Умеет отображать только энергию
    private void DisplayAgi()
    {
        float energy = player.EnergyReload;

    }

    //Умеет отображать только стамину
    private void DisplayStr()
    {

    }

    //Умеет отображать только атаку
    private void DisplayAtc()
    {

    }

    //умеет отображать только хп
    private void DisplayHP()
    {

    }

    //умеет отображать только криты
    private void DisplayCrt()
    {

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
