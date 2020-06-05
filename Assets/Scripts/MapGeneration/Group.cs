using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    [Header("Объекты")]
    [Tooltip("Список объектов из которых случайно будет выбран один")]
    public List<GameObject> Storage = new List<GameObject>();

    private System.Random random = new System.Random();
    
    /// <summary>
    /// Unpack group - from storage selected one GO
    /// </summary>
    public void Unpack()
    {
        int choise = random.Next(Storage.Count);
        Instantiate(Storage[choise]).transform.position = transform.position;
        Destroy(gameObject);
    }
}
