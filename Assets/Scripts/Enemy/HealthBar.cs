using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("Здоровье ")]
    [Tooltip("Отображаемое в % здоровье")]
    public float Health = 1f; //100%
    [Header("Поправка сдвига влево")]
    [Tooltip("Минимальная поправка")]
    public float MinFix = 0.01f;
    [Tooltip("Максимальная поправка")]
    public float MaxFix = 0.02f;

    private Transform Line;
    private float fix;

    private void Start()
    {
        Line = transform.Find("Line").transform;
    }

    private void RecalculateFix()
    {
        fix = MinFix + (MaxFix - MinFix) * (1 - Health);
    }

    public void Display()
    {
        RecalculateFix();
        Line.localScale = new Vector3(Health, 1f, 1f);
        Line.localPosition = new Vector3((float)(-((1 - Health) / 2) - fix) , 0f, 0f);
    }

    public void Set(float health)
    {
        if (health < 0) 
            health = 0;

        Health = health;
        Display();
    }

}
