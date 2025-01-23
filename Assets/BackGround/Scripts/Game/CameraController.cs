using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using Cinemachine;
using Sirenix.OdinInspector;
using Unity.Linq;
using UnityEngine.UIElements;
using System;

public class CameraController : MonoBehaviour
{
    public CinemachineBrain brain;
    [BoxGroup("Room Info")]
    public int curIndex;
    [BoxGroup("Room Info")]
    public List<InGameRoomInfo> roomList = new List<InGameRoomInfo>();
    [ShowInInspector]
    [ReadOnly]
    private CinemachineCameraController curCam;
    private List<CinemachineCameraController> cameras = new List<CinemachineCameraController>();
    public CinemachineCameraController Camera => curCam;
    public async void Init()
    {
        foreach (var room in roomList)
        {
            var virtualCam = await Managers.Resource.LoadAsyncGameObject("camera", "RoomCamera.prefab");
            var camera = gameObject.Add(virtualCam);
            camera.SetActive(false);
            camera.name = $"room_{room.index}_cam";
            var cinemachineCam = camera.GetOrAddComponent<CinemachineCameraController>();
            cinemachineCam.Init(room);
            cameras.Add(cinemachineCam);
        }

        ChangeCam(0);

        Action<Vector3> dragAction = moveDirection =>
        {
            if (curCam == null)
                return;

            curCam.Trasnlate(moveDirection);
        };
        Managers.Input.dragDir.Subscribe(dragAction).AddTo(this);
        Managers.Input.dragSubject.Subscribe(dir =>
        {
            if (dir)
            {
                ChangeCam(curIndex + 1);
            }
            else
            {
                ChangeCam(curIndex - 1);
            }

        }).AddTo(this);
    }

    [Button]
    public void ChangeCam(int index)
    {
        var room = roomList.Find(_=>_.index == index);
        var cam = cameras.Find(_=>_.index == index);
        if (room == null || cam == null)
            return;

        curIndex = index;
        
        brain.m_DefaultBlend.m_Time = room.moveTime;


        cam.gameObject.SetActive(true);

        if (curCam != null && curCam != cam)
            curCam.gameObject.SetActive(false);

        curCam = cam;
    }
}
