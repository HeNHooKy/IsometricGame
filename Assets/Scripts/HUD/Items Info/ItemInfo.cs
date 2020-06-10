using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour
{
    [Header("Пустой спрайт")]
    public Sprite UIMask;

    private GameObject Selected;
    private Item item;
    private Image image;
    private PickedUpItems controller;
    private Button button;

    private void Awake()
    {
        image = GetComponent<Image>();
        controller = transform.parent.parent.parent.GetComponent<PickedUpItems>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        Selected = transform.Find("Selected").gameObject;
    }

    public void SelectedOff()
    {
        Selected.SetActive(false);
    }

    public void OnClick()
    {
        controller.DropAllSelected();

        if (item != null)
        {
            Selected.SetActive(true);
            controller.WriteInfo(item);
        }
    }

    /// <summary>
    /// Set item on item button
    /// </summary>
    /// <param name="item">picked up item</param>
    public void SetItem(Item item)
    {
        if(item == null)
        {
            this.item = item;
            image.sprite = UIMask;
        }
        else
        {
            this.item = item;
            image.sprite = item.ItemSprite;
        }
    }

}
