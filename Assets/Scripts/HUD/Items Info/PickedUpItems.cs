using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickedUpItems : MonoBehaviour
{
    private List<Item> AllPickedUpItems = new List<Item>();
    private List<ItemsLine> Lines = new List<ItemsLine>();
    private Information infoWindow;

    //номер строки отображаемой первой
    private int line = 0;
    //количество предметов в одной строке
    private int LineLength = 0;

    private void Start()
    {
        Lines.AddRange(GetComponentsInChildren<ItemsLine>());
        infoWindow = transform.Find("Information").GetComponent<Information>();
        LineLength = Lines[0].ItemsInfo.Count;
    }

    private void Update()
    {
        FillLines(0);
    }

    /// <summary>
    /// Up line counter by 1
    /// </summary>
    public void Up()
    {   //если предметов слишком мало кнопка не нажмется
        if (line < (AllPickedUpItems.Count / 4) - Lines.Count)
            return;
        //если стартовая линия равна последеней доступной
        line = line < (AllPickedUpItems.Count / LineLength) ? line++ : line;
        FillLines(line);
    }

    /// <summary>
    /// Down line counter by 1
    /// </summary>
    public void Down()
    {
        line = line > 0 ? line-- : line;
        FillLines(line);
    }

    /// <summary>
    /// Fill all lines with items
    /// </summary>
    public void FillLines(int startLine)
    {
        for (int k = startLine * LineLength, i = 0; i < Lines.Count; i++)
        {
            Item[] items = new Item[LineLength];
            for(int j = 0; j < LineLength; j++)
            {
                items[j] = null;
            }

            for(int j = 0; j < LineLength && k < AllPickedUpItems.Count; j++)
            {
                items[j] = AllPickedUpItems[k++];
            }

            Lines[i].AddItems(items);
        }
    }

    /// <summary>
    /// Droped the selections board from all Items in window
    /// </summary>
    public void DropAllSelected()
    {
        for(int i = 0; i < Lines.Count; i++)
        {
            for(int j = 0; j < Lines[i].ItemsInfo.Count; j++)
            {
                Lines[i].ItemsInfo[j].SelectedOff();
            }
        }
        infoWindow.DropInfo();
    }

    /// <summary>
    /// add item to picked up item list
    /// </summary>
    /// <param name="item">picked up item</param>
    public void PickedUp(Item item)
    {
        AllPickedUpItems.Add(item);
    }

    /// <summary>
    /// Write information about item in information window
    /// </summary>
    public void WriteInfo(Item selectItem)
    {
        infoWindow.WriteAbout(selectItem);
    }

}
