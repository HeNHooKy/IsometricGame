using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Настройки количества комнат")]
    [Tooltip("Количество мини-боссов на уровне")]
    public int ArenaRoomsCounter = 1;
    [Tooltip("Количество мобален на уровне")]
    public int MobalnyaRoomsCounter = 5;
    [Tooltip("Все доступные для генератора этого уровня комнаты")]
    public List<Room> Rooms = new List<Room>();

    private System.Random random = new System.Random();
    private int roomIter = 1;   //т.к. спавн уже есть


    /// <summary>
    /// Create new level. In active will be only spawn room
    /// </summary>
    public void GenerateLevel()
    {
        //этап 1. Поиск спавна, главной арены и коридора, и запись их в отдельные элементы (если таких объектов нет -> кинуть exception)
        //Из основного списка эти элементы должны быть удалены
        //этап 2. Генерация спавна
        //этап 3. Перемешиваем список карт
        //этап 4. К случайной стороне спавна клеим первую карту
        //этап 5. К случайной стороне кроме стороны спавна клеим вторую карту к первой
        //этап 5. Генерируем число. Если число четное -> наращиваем в ширину, нет -> переход к другой комнате
        //Наращивание в ширину - присоединение комнаты к той комнате, в которой находится итератор
        //Переход к другой комнате - переход итератора к случайной комнате, кроме спавна
        //Если все двери комнаты уже заняты -> переход к случайной комнате, кроме спавна
        //этап 6. Наращиваение в ширину. К случайной свободной двери прикрепляем случайную карту из списка
        //Если количество созданных комнат равно количеству мобален + 1 (спавн) -> Перейти к этапу 7, иначе -> к этапу 5
        //этап 7. К случайной стороне комнаты, в которой находится итератор, если есть свободные двери, прикрепить коридор
        //и перевести итератор в него
        //к пустой двери прикрепить карту главной арены.
        //Скрыть все карты, кроме спавна.
        //Передать управление в вызвавший метод

        //этап 1
        Room spawnRoom = null, mainArenaRoom = null, coridor = null;
        List<Room> miniBossRooms = new List<Room>();
        foreach (Room room in Rooms)
        {
            if (room.TypeOfRoom == Room.RoomType.Arena)
            {   //т.к. мини-боссы не должны оказаться на пути к МБ
                miniBossRooms.Add(room);
                Rooms.Remove(room);
                continue;
            }

            if (room.TypeOfRoom == Room.RoomType.MainArena)
            {
                mainArenaRoom = room;
                Rooms.Remove(room);
                continue;
            }

            if (room.TypeOfRoom == Room.RoomType.Spawn)
            {
                spawnRoom = room;
                Rooms.Remove(room);
                continue;
            }

        }

        //этап 3
        for (int i = Rooms.Count - 1; i >= 1; i--)
        {
            int j = random.Next(i + 1);
            // обменять значения data[j] и data[i]
            var temp = Rooms[j];
            Rooms[j] = Rooms[i];
            Rooms[i] = temp;
        }

        if (spawnRoom == null || mainArenaRoom == null || coridor == null)
        {
            throw new Exception("Одной из основных карт нет в списке на генерацию!" +
                "Проверьте наличие карт:  Spawn, MainArena, Coridor");
        }

        //этап 2
        spawnRoom.Unpack();

        //этап 4
        
        spawnRoom.Connect(Rooms[roomIter].GetDoorByNumber(random.Next(4)), spawnRoom.Light);
        //этап 5 - 6
    }

    //расширяет карту
    private void Builder(Room room)
    {
        if(roomIter >= MobalnyaRoomsCounter)
        {   //больше комнат не поставить
            return;
        }

        List<Door> freeDoors = room.GetFreeDoors();
        List<Door> buzyDoors = room.GetBuzyDoors();
        Door bd, fd;

        if (freeDoors.Count == 0)
        {   //свободных комнат нет -> переходим в случайную комнату
            bd = buzyDoors[random.Next(buzyDoors.Count)];
            Builder(bd.Room);
            return;
        }

        if(freeDoors.Count != 4)
        {
            //есть свободные комнаты
            if (random.Next() % 2 == 0)
            {   //бог играет в кости. И выпала 2-ка
                //к случайной свободной двери прикрепим комнату
                fd = freeDoors[random.Next(freeDoors.Count)];
                room.Connect(fd, room.Light);
            }
            else
            {   //выпала 1-ца
                //переходим к случайной двери из списка занятых дверей
                bd = buzyDoors[random.Next(buzyDoors.Count)];
                Builder(bd.Room);
            }
            return;
        }

        bd = buzyDoors[random.Next(buzyDoors.Count)];
        Builder(bd.Room);
    }
}
