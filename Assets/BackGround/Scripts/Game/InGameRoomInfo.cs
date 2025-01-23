using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using Sirenix.OdinInspector;

public class InGameRoomInfo : MonoBehaviour
{
    [Title("Info")]
    public int index;
    [LabelText("제한 범위(콜라이더)")]
    public BoxCollider roomRangeColl;
    [Tooltip("구역 진입시 카메라 포지션")]
    public Vector3 cameraPos;
    [Tooltip("카메라 회전각 설정")]
    public Vector3 camRotation = Vector3.zero;
    public float moveTime = 1f;
    public float fov = 60f;
}
