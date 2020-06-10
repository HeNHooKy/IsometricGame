using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsLine : MonoBehaviour
{
    [HideInInspector]
    public List<Item> Items = new List<Item>();

    //указатели на спрайты
    [HideInInspector]
    public List<ItemInfo> ItemsInfo = new List<ItemInfo>();

    private void Awake()
    {
        ItemsInfo.AddRange(GetComponentsInChildren<ItemInfo>());
    }

    /// <summary>
    /// Adding item to the line
    /// </summary>
    /// <param name="item"></param>
    public void AddItems(Item[] item)
    {
        Items.Clear();
        Items.AddRange(item);
        DisplayItems();
    }

    /// <summary>
    /// Display all items on the line
    /// </summary>
    public void DisplayItems()
    {
        for(int i = 0; i < ItemsInfo.Count; i++)
        {
            ItemsInfo[i].SetItem(null);
        }

        for(int i = 0; i < Items.Count; i++)
        {
            ItemsInfo[i].SetItem(Items[i]);
        }
    }
}
