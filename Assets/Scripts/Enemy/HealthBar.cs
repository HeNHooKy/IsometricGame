using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float Health = 1f; //100%

    private Transform Line;

    private void Start()
    {
        Line = transform.Find("Line").transform;
    }

    public void Display()
    {
        Line.localScale = new Vector3(Health, 1f, 1f);
        Line.localPosition = new Vector3((float)(-((1 - Health) / 2)), 0f, 0f);
    }

    public void Set(float health)
    {
        if (health < 0) 
            health = 0;

        Health = health;
        Display();
    }

}
