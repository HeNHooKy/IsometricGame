using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Information : MonoBehaviour
{
    [Header("Пустой спрайт")]
    public Sprite UIMask;

    private Image image;
    private Text title, description;

    private void Awake()
    {
        title = transform.Find("Title").GetComponent<Text>();
        image = transform.Find("Picture").GetComponent<Image>();
        description = transform.Find("Description").GetComponent<Text>();
    }

    /// <summary>
    /// Droped information about selected item
    /// </summary>
    public void DropInfo()
    {
        image.sprite = UIMask;
        title.text = "";
        description.text = "";
    }

    /// <summary>
    /// Write information about selected item in information window (sprite, title, description)
    /// </summary>
    public void WriteAbout(Item selectedItem)
    {
        image.sprite = selectedItem.ItemSprite;
        title.text = selectedItem.Name;
        description.text = selectedItem.Description;
    }
}
