using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public List<Image> hearts;     //массив указателей на сердца

    public Sprite BaseHeart;    //обычное сердце
    public Sprite HalfHeart;    //половинка сердца
    public Sprite OverHeart;    //оверхил сердце
    public Sprite HalfOverHeart;//половина оверхил сердца
    public Sprite EmptyHeart;   //пустой слот под сердце
    public Sprite EmptyHalfHeart;//пустой слот под половину сердца
    public Sprite Empty;    //пустой спрайт

    public int MaxHealth;       //максимальное здоровье игрока
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
            hearts[MaxHealth / 2].sprite = EmptyHeart;
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
