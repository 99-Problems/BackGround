using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Linq;
using UnityEngine;

public class RoomContainer : MonoBehaviour
{
    private List<WaitingRoom> roomList;
    private WaitingRoom defaultRoom;

    public UniTask Init()
    {
        var list = gameObject.GetComponentsInChildren<WaitingRoom>();
        roomList = list.Where(room =>
        {
            if (room.roomType == WaitingRoom.RoomType.Default)
            {
                defaultRoom = room;
                defaultRoom.SetIndex(WaitingRoom.defaultIndex);
                return false;
            }

            return true;
        }).ToList();

        return UniTask.CompletedTask;
    }

    public WaitingRoom GetRoomInfo(int _index)
    {
        var room = roomList.Find(_ => _.GetIndex == _index);
        if(room == null)
            return defaultRoom;

        return room;
    }
    
}
