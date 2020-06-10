using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PickedUpInfo : MonoBehaviour
{
    [Header("Настройки отображения")]
    [Tooltip("Скорость появления")]
    public float ShowSpeed = 1f;
    [Tooltip("Скорость исчезновения")]
    public float HideSpeed = 1f;

    private Text title;
    private Text description;

    private void Awake()
    {
        title = transform.Find("Title").GetComponent<Text>();
        description = transform.Find("Description").GetComponent<Text>();
        gameObject.SetActive(false);
    }

    public void DisplayAbout(Item item)
    {
        title.text = item.Name;
        description.text = item.Description;
        gameObject.SetActive(true);
        StartCoroutine(Display());
    }

    //long coroutine
    IEnumerator Display()
    {
        Color ct = title.color;
        Color cd = description.color;
        for (float i = 0; i < 1; i += Time.deltaTime * ShowSpeed)
        {
            ct.a = Easing.easeOutExpo(i);
            cd.a = ct.a;
            title.color = ct;
            description.color = cd;
            yield return null;
        }

        ct.a = 1;
        cd.a = ct.a;
        title.color = ct;
        description.color = cd;
        
        for (float i = 1; i >= 0; i -= Time.deltaTime * HideSpeed)
        {
            ct.a = Easing.easeOutSine(i);
            cd.a = ct.a;
            title.color = ct;
            description.color = cd;
            yield return null;
        }

        ct.a = 0;
        cd.a = ct.a;
        title.color = ct;
        description.color = cd;

        gameObject.SetActive(false);
    }
}
