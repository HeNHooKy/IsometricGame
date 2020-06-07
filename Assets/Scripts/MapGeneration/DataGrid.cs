using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DataGrid : MonoBehaviour
{
    [Header("Настройки сетки")]
    [Tooltip("Необходимо ли случайное перемешивание")]
    public bool IsRandomize = false;
    [Tooltip("Максимальное количество объектов в сетке")]
    public int MaxObjects = 5; 

    

    private System.Random random = new System.Random();

    

    /// <summary>
    /// Try to randomize grid objects
    /// </summary>
    public void Randomize()
    {
        if(IsRandomize)
        {
            List<Group> groups = new List<Group>();
            groups.AddRange(GetComponentsInChildren<Group>());

            MaxObjects = MaxObjects > groups.Count ? groups.Count : MaxObjects;

            for (int i = groups.Count - 1; i >= 1; i--)
            {   //перемешиваем
                int j = random.Next(i + 1);
                // обменять значения data[j] и data[i]
                var temp = groups[j];
                groups[j] = groups[i];
                groups[i] = temp;
            }

            for(int i = 0; i < MaxObjects; i++)
            {   //разбираем группы счастливчиков
                int choise = random.Next(groups[i].Storage.Count);
                groups[i].Unpack(choise);
            }

            for(int i = MaxObjects; i < groups.Count; i++)
            {   //уничтожаем бедолаг
                Destroy(groups[i].gameObject);
            }
        }
    }
}
