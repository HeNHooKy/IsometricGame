using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    /// <summary>
    /// The room types storage
    /// </summary>
    public enum RoomType
    {
        Spawn = 0,
        Coridor = 1, 
        Mobalnya = 2,
        Arena = 3
    }

    [Header("Освещение")]
    [Tooltip("Цвет освещения в комнате")]
    public Color Light;
    [Tooltip("Интенсивность света в комнате")]
    public float LightIntensity;

    [Header("Сетки")]
    [Tooltip("Список всех сеток комнаты")]
    public List<DataGrid> Grids = new List<DataGrid>();
    [Header("Тип комнаты")]
    [Tooltip("Тип этой комнаты")]
    public RoomType TypeOfRoom = RoomType.Mobalnya;

    [Header("Двери")]
    [Tooltip("Северная дверь")]
    public Door NorthDoor;
    [Tooltip("Западная дверь")]
    public Door WestDoor;
    [Tooltip("Восточная дверь")]
    public Door EastDoor;
    [Tooltip("Южная дверь")]
    public Door SouthDoor; 

    /// <summary>
    /// Connect Room to room
    /// </summary>
    /// <param name="door">Door, wich connected to the opposite door in the room</param>
    public bool Connect(Door door)
    {
        //прикрепляем дверь к двери
        if (GetOpposite(door.type) != null)
        {
            GetOpposite(door.type).SetNext(door);
            //и наоборот
            door.SetNext(GetOpposite(door.type));
            //распаковываем комнату
            foreach (DataGrid dg in Grids)
            {
                dg.Randomize();
            }
            return true;
        }
        else
        {
            return false;
        }
        //иначе свободных дверей нет, добавить путь невозможно
    }

    /// <summary>
    /// Get the opposite door
    /// </summary>
    /// <param name="type">world side</param>
    /// <returns></returns>
    public Door GetOpposite(Door.DoorType type)
    {
        switch (type)
        {
            case Door.DoorType.North:
                return SouthDoor;
            case Door.DoorType.South:
                return NorthDoor;
            case Door.DoorType.East:
                return WestDoor;
            case Door.DoorType.West:
                return EastDoor;
            default:
                return null;
        }
    }
    
}
