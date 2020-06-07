using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    [Header("Объекты")]
    [Tooltip("Список объектов из которых случайно будет выбран один")]
    public List<GameObject> Storage = new List<GameObject>();
    
    /// <summary>
    /// Unpack group - from storage selected one GO
    /// </summary>
    public void Unpack(int choise)
    {
        GameObject stuff = Instantiate(Storage[choise]);
        stuff.transform.position = transform.position;
        stuff.transform.parent = transform.parent;
        Destroy(gameObject);
    }
}
