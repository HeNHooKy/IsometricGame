using System.CodeDom;
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
        Mobalnya = 2,
        Arena = 3,
        MainArena = 4
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

    private void Refresh()
    {
        //скрываем все двери на карте и привязываем их к этой комнате
        NorthDoor.gameObject.SetActive(false);
        NorthDoor.Room = this;
        NorthDoor.Load();

        WestDoor.gameObject.SetActive(false);
        WestDoor.Room = this;
        WestDoor.Load();

        EastDoor.gameObject.SetActive(false);
        EastDoor.Room = this;
        EastDoor.Load();

        SouthDoor.gameObject.SetActive(false);
        SouthDoor.Room = this;
        SouthDoor.Load();
    }


    public Room Unpack()
    {
        GameObject inst = Instantiate(this.gameObject);
        inst.GetComponent<Room>().Refresh();
        inst.SetActive(false);
        //распаковываем сетки
        foreach (DataGrid dg in inst.GetComponent<Room>().Grids)
        {
            dg.Randomize();
        }
        return inst.GetComponent<Room>();
    }

    /// <summary>
    /// Connect Room to room
    /// </summary>
    /// <param name="door">Door, wich connected to the opposite door in the room</param>
    public bool Connect(Door door, Color light)
    {
        Door myDoor = GetOpposite(door.type);
        //прикрепляем дверь к двери
        if ((myDoor != null) && (myDoor.GetNext() == null))
        {
            door.LightColor = Light;
            myDoor.SetNext(door);
            myDoor.LightColor = light;
            //и наоборот
            door.SetNext(myDoor);
            //отображаем двери
            door.gameObject.SetActive(true);
            myDoor.gameObject.SetActive(true);
            return true;
        }
        else
        {
            return false;
        }
        //иначе свободных дверей нет, добавить путь невозможно
    }

    public Door GetDoorByNumber(int number)
    {
        switch(number)
        {
            case (int)Door.DoorType.North:
                return NorthDoor;
            case (int)Door.DoorType.South:
                return SouthDoor;
            case (int)Door.DoorType.West:
                return WestDoor;
            case (int)Door.DoorType.East:
                return EastDoor;
            default:
                return null;
        }
    }

    public List<Door> GetFreeDoors()
    {
        List<Door> freeDoors = new List<Door>();
        //есть ли хоть одна свободная дверь
        if (EastDoor != null && EastDoor.GetNext() == null)
        {
            freeDoors.Add(EastDoor);
        }
        if (NorthDoor != null && NorthDoor.GetNext() == null)
        {
            freeDoors.Add(NorthDoor);
        }
        if (SouthDoor != null && SouthDoor.GetNext() == null)
        {
            freeDoors.Add(SouthDoor);
        }
        if (WestDoor != null && WestDoor.GetNext() == null)
        {
            freeDoors.Add(WestDoor);
        }
        return freeDoors;
    }

    public List<Door> GetBuzyDoors()
    {
        List<Door> buzyDoors = new List<Door>();
        //есть ли хоть одна свободная дверь
        if (EastDoor.GetNext() != null)
        {
            buzyDoors.Add(EastDoor);
        }
        if (NorthDoor.GetNext() != null)
        {
            buzyDoors.Add(NorthDoor);
        }
        if (SouthDoor.GetNext() != null)
        {
            buzyDoors.Add(SouthDoor);
        }
        if (WestDoor.GetNext() != null)
        {
            buzyDoors.Add(WestDoor);
        }
        return buzyDoors;
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
