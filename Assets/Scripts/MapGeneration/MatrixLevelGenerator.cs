using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixLevelGenerator : MonoBehaviour
{
    [Header("Настройки генерации уровня")]
    [Tooltip("Количество мини-боссов на уровне")]
    public int ArenaRoomsCounter = 1;
    [Tooltip("Количество мобален на уровне")]
    public int MobalnyaRoomsCounter = 5;
    [Tooltip("Все доступные для генератора этого уровня комнаты")]
    public List<Room> Rooms = new List<Room>();

    [HideInInspector]
    public Room[,] GameMap;

    private System.Random random = new System.Random();

    private int x, y;

    private int length;

    /// <summary>
    /// Level complete or gameover
    /// </summary>
    public void EndLevel()
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                if(GameMap[i, j] != null)
                {
                    Destroy(GameMap[i, j].gameObject);
                }
                GameMap[i, j] = null;
            }
        }
    }

    public Vector3 GenerateLevel()
    {
        length = MobalnyaRoomsCounter + ArenaRoomsCounter + 2;
        //зануляем карту
        GameMap = new Room[length, length];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                GameMap[i, j] = null;
            }
        }
        
        //перемешиваем карту
        for (int i = Rooms.Count - 1; i >= 1; i--)
        {
            int j = random.Next(i + 1);
            // обменять значения data[j] и data[i]
            var temp = Rooms[j];
            Rooms[j] = Rooms[i];
            Rooms[i] = temp;
        }

        Room spawnRoom = null, mainArenaRoom = null;
        List<Room> miniBossRooms = new List<Room>();
        for (int i = 0; i < Rooms.Count; i++)
        {
            if (Rooms[i].TypeOfRoom == Room.RoomType.Arena)
            {   //т.к. мини-боссы не должны оказаться на пути к МБ
                miniBossRooms.Add(Rooms[i]);
                Rooms.Remove(Rooms[i]);
                i--;
                continue;
            }

            if (Rooms[i].TypeOfRoom == Room.RoomType.MainArena)
            {
                mainArenaRoom = Rooms[i];
                Rooms.Remove(Rooms[i]);
                i--;
                continue;
            }

            if (Rooms[i].TypeOfRoom == Room.RoomType.Spawn)
            {
                spawnRoom = Rooms[i];
                Rooms.Remove(Rooms[i]);
                i--;
                continue;
            }
        }

        

        if (spawnRoom == null || mainArenaRoom == null)
        {
            throw new Exception("Одной из основных карт нет в списке на генерацию!\n" +
                "Проверьте наличие карт:  Spawn, MainArena");
        }
        //ставим спавн
        
        x = length / 2; y = length / 2;
        GameMap[x, y] = spawnRoom.Unpack();
        spawnRoom = GameMap[x, y];

        int mobalnyas = MobalnyaRoomsCounter;
        if (mobalnyas > Rooms.Count)
        {
            throw new Exception("Недостаточно мобален в списке на генерацию!\n" +
                "Снизьте количество мобален в настройках или добавьте карт в список на генерацию!");
        }

        //добавляем все мобальни
        while (mobalnyas > 0)
        {   //если зашли в спираль
            if (GameMap[x, y].GetFreeDoors().Count == 0)
            {   //сделай шаг в любую сторону
                FreshXY(random.Next(4));
            }
            if(AddRoom(Rooms[0]))
            {
                Rooms.Remove(Rooms[0]);
                mobalnyas--;
            }
        }

        int miniarenas = ArenaRoomsCounter;
        if(miniarenas > miniBossRooms.Count)
        {
            throw new Exception("Недостаточно карт мини-боссов в списке на генерацию.\n" +
                "Снизьте количество мини-боссов в настройках или добавьте карт в список на генерацию!");
        }

        //добавляем все мини-арены
        while (miniarenas-- > 0)
        {   //если зашли в спираль
            if (GameMap[x, y].GetFreeDoors().Count == 0)
            {   //сделай шаг в любую сторону
                FreshXY(random.Next(4));
            }
            if(AddRoom(miniBossRooms[0]))
            {
                Rooms.Remove(miniBossRooms[0]);
                miniarenas--;
            }

            //шагнем в любую занятую дверь (скорее всего назад), чтобы выйти из комнты 
            List<Door> buzyDoor = GameMap[x, y].GetBuzyDoors();
            if (buzyDoor.Count > 0)  //если эти двери есть
            {
                FreshXY((int)buzyDoor[random.Next(buzyDoor.Count)].type);
            }
        }

        
            

        bool isAdded = false;
        while (!isAdded)
        {   //если зашли в спираль
            if (GameMap[x, y].GetFreeDoors().Count == 0)
            {   //сделай шаг в любую сторону
                FreshXY(random.Next(4));
            }
            if (AddRoom(mainArenaRoom))
            {
                isAdded = true;
            }
        }

        spawnRoom.gameObject.SetActive(true);
        return spawnRoom.gameObject.transform.Find("SpawnPosition").position;
    }



    //добавить новую комнату на карту
    private bool AddRoom(Room room)
    {
        //найти все свободные ячейки вокруг генерируемой комнаты
        //перемешать
        //сохранить данные x, y
        //взять первую дверь и в её сторону сместить координаты
        //обновить данные x, y
        //если обновилось успешно -> вернуть true
        //если обновиться неудалось -> взять следующее значение
        //если значения кончились, а обновиться не удалось -> вернуть false
        List<Door> freeDoors = GameMap[x, y].GetFreeDoors();
        ShakeDoors(freeDoors);

        int lastX = x, lastY = y; int i;
        for (i = 0; i < freeDoors.Count && (!FreshXY((int)freeDoors[i].type)); i++) ;

        //обновиться не удалось
        if(lastX == x && lastY == y)
        {
            return false;
        }

        //обновиться удалось. Нужно присоединить комнату
        GameMap[x, y] = room.Unpack();   //распакуем карту 
        room = GameMap[x, y];  //поместим комнату на карту
        //обновим связи
        Door opDoor = GameMap[x, y].GetOpposite(freeDoors[i].type);//берем дверь напротив
        GameMap[lastX, lastY].Connect(opDoor, GameMap[x, y].Light);

        //проверим: нет ли рядом комнат неподключенных к новой
        //и подключим, если такие нашлись
        freeDoors = GameMap[x, y].GetFreeDoors();
        lastX = x; lastY = y;   //сбросим данные
        for(i = 0; i < freeDoors.Count; i++)
        {   //осмотрим все точки
            if(FreshXY((int)freeDoors[i].type))
            {
                if (GameMap[x, y] != null)
                {
                    //тут есть комната, которая никак не связана с настоящей
                    room.Connect(GameMap[x, y].GetOpposite(freeDoors[i].type), GameMap[x, y].Light);
                }
                //возвращаемся назад
                FreshXY((int)room.GetOpposite(freeDoors[i].type).type);
            }
        }
        //по окончании всех манипуляций x и y должны остаться на позиции последней добавленой карты

        return true;
    }

    //перемещает координату x, y в сторону side
    //Вернет true - значения обновлено успешно
    //Вернет false - строить в эту сторону невозможно
    private bool FreshXY(int side)
    {
        switch(side)
        {
            case (int)Door.DoorType.North:
                if (y - 1 >= 0)
                    y--;
                else
                    return false;
                break;
            case (int)Door.DoorType.South:
                if (y + 1 < length)
                    y++;
                else
                    return false;
                break;
            case (int)Door.DoorType.West:
                if (x - 1 >= 0)
                    x--;
                else
                    return false;
                break;
            case (int)Door.DoorType.East:
                if (x + 1 < length)
                    x++;
                else
                    return false;
                break;
        }
        return true;
    }

    private void ShakeDoors(List<Door> doors)
    {
        for (int i = doors.Count - 1; i >= 1; i--)
        {   //перемешиваем
            int j = random.Next(i + 1);
            // обменять значения data[j] и data[i]
            var temp = doors[j];
            doors[j] = doors[i];
            doors[i] = temp;
        }
    }
    
}
