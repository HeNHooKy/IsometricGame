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
    [HideInInspector]
    public Room Room = null;
    
    [Header("Тип двери")]
    [Tooltip("сторона света этой двери")]
    public DoorType type = DoorType.North;
    [Header("Свет")]
    [Tooltip("Свет испускаемый этой дверью")]
    public Color LightColor;

    Door Next = null; //Дверь, с которой связана данная

    public Door GetNext()
    {
        return Next;
    }

    public void SetNext(Door Next)
    {
        this.Next = Next;
    }
}
