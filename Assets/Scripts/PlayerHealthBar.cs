using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("массив полей сердец")]
    public List<Image> hearts;

    [Header("Спрайты сердец")]
    [Tooltip("Обычное сердце")]
    public Sprite BaseHeart;
    [Tooltip("Половинка сердца")]
    public Sprite HalfHeart;
    [Tooltip("Оверхил сердце")]
    public Sprite OverHeart;
    [Tooltip("Половина оверхил сердца")]
    public Sprite HalfOverHeart;
    [Tooltip("Пустой слот под сердце")]
    public Sprite EmptyHeart;
    [Tooltip("Пустой слот под половину сердца")]
    public Sprite EmptyHalfHeart;
    [Tooltip("Пустой спрайт")]
    public Sprite Empty;

    [HideInInspector]
    public int MaxHealth;       //максимальное здоровье игрока
    [HideInInspector]
    public int CurrentHealth;   //текущее здоровье игрока

    public void DisplayHeart(int max, int cur)
    {
        MaxHealth = max;
        CurrentHealth = cur;

        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].sprite = Empty;
        }

        for (int i = 0; i < MaxHealth / 2; i++)
        {
            hearts[i].sprite = EmptyHeart;
        }
        if (MaxHealth % 2 != 0)
        {
            hearts[MaxHealth / 2].sprite = EmptyHalfHeart;
        }

        for (int i = 0; i < MaxHealth / 2 && i < CurrentHealth / 2; i++)
        {   //отображаем реальное кол-во хп
            hearts[i].sprite = BaseHeart;
        }
        if (CurrentHealth <= MaxHealth && CurrentHealth % 2 != 0)
        {
            hearts[CurrentHealth / 2].sprite = HalfHeart;
        }

        //отображаем overheal
        for (int i = (MaxHealth / 2) + (MaxHealth % 2) ; i < CurrentHealth / 2; i++)
        {
            hearts[i].sprite = OverHeart;
        }

        if (MaxHealth < CurrentHealth && CurrentHealth % 2 != 0)
        {
            hearts[CurrentHealth / 2].sprite = HalfOverHeart;
        }
    }

}
/*
        float paddingLeft = heartSocket.rect.width * 0.02f;
        float paddingTop = heartSocket.rect.height * 0.02f;
        float width = heartSocket.rect.width - (paddingLeft*2);
        float height = heartSocket.rect.height - (paddingTop*2);

        float heartPaddingLeft = (width / MaxHeartsInRow) * 0.05f;
        float heartPaddingTop = (width / MaxRows) * 0.05f;
        float heartWidth = width / MaxHeartsInRow;
        float heartHeight = height / MaxRows;

        for (int i = 0; i < MaxHeartsInRow*MaxRows; i++)
        {
            hearts.Add(Instantiate(BasePrefab));
            
            RectTransform t = hearts[i].GetComponent<RectTransform>();
            Rect rect = t.rect;
            rect.width = heartWidth - (heartPaddingLeft * 2);
            rect.height = heartHeight - (heartPaddingTop * 2);
            rect.x = (heartWidth * i) + (heartWidth/2);
            rect.y = (heartHeight * (i / MaxHeartsInRow)) + (heartHeight/2);
            t. = rect;

        }
        */
