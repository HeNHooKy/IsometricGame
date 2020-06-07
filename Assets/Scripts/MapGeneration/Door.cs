using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Environment
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

    private void Start()
    {
        //DO nothing!
    }

    public void Load()
    {
        HUD = transform.parent.Find("/HUD").GetComponent<HUD>();
    }

    public Door GetNext()
    {
        return Next;
    }

    public void SetNext(Door Next)
    {
        this.Next = Next;
    }

    public override void Use(PlayerController player)
    {
        //Телепортирует игрока на другую локацию
        Room.gameObject.SetActive(false);
        Next.Room.gameObject.SetActive(true);
        HUD.GameController.GetEnemiesList();
        //перемещаем персонажа
        player.transform.position =
            Next.transform.Find("SpawnPoint").transform.position;
        player.ClearFreeLoc();
        player.GetFreeLoc();
    }
}
