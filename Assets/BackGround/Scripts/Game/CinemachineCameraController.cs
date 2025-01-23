using Cinemachine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public class CinemachineCameraController : MonoBehaviour
{
    [LabelText("에디터 이동속도")]
    public float editorSpeed = 15f;
    [LabelText("빌드 이동속도")]
    public float moveSpeed = 5f; // 이동 속도
    [LabelText("에디터 휠 속도")]
    [ShowInInspector]
    public static float wheelSpeed = 20f;
    public CinemachineConfiner confiner;
    public CinemachineVirtualCamera cam;
    public int index { get; private set; }

    public bool draglockZ = true;


    private void Awake()
    {
        if (confiner == null)
            confiner = gameObject.GetOrAddComponent<CinemachineConfiner>();
        if (cam == null)
            cam = gameObject.GetOrAddComponent<CinemachineVirtualCamera>();

#if UNITY_EDITOR
        moveSpeed = editorSpeed;
#endif
    }

    public void Init(InGameRoomInfo _info)
    {
        if (_info == null)
            return;

        index = _info.index;
        confiner.m_BoundingVolume = _info.roomRangeColl;
        cam.m_Lens.FieldOfView = _info.fov;
        gameObject.transform.rotation = Quaternion.Euler(_info.camRotation);
        gameObject.transform.position = _info.cameraPos;
    }

    public void Trasnlate(Vector3 moveDiretion)
    {
        if(draglockZ)
            moveDiretion.z = 0;

        transform.Translate(moveDiretion * moveSpeed * Time.deltaTime, Space.World);
        Vector3 closestPoint = confiner.m_BoundingVolume.ClosestPoint(gameObject.transform.position);

        gameObject.transform.position = closestPoint;
    }

    public void Move(Vector3 pos)
    {
        gameObject.transform.position = pos;
        Vector3 closestPoint = confiner.m_BoundingVolume.ClosestPoint(gameObject.transform.position);

        // 가상 카메라의 실제 위치를 제한된 영역 내의 가장 가까운 점으로 설정
        gameObject.transform.position = closestPoint;
    }
}
