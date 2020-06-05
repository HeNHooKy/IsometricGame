using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        North = 0, 
        South = 1,
        West = 2,
        East = 3
    }

    [Header("Тип двери")]
    [Tooltip("сторона света этой двери")]
    public DoorType type = DoorType.North;

    Door Next; //Дверь, с которой связана данная

    public Door GetNext()
    {
        return Next;
    }

    public void SetNext(Door Next)
    {
        this.Next = Next;
    }
}
