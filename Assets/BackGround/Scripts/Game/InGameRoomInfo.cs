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
    [LabelText("���� ����(�ݶ��̴�)")]
    public BoxCollider roomRangeColl;
    [Tooltip("���� ���Խ� ī�޶� ������")]
    public Vector3 cameraPos;
    [Tooltip("ī�޶� ȸ���� ����")]
    public Vector3 camRotation = Vector3.zero;
    public float moveTime = 1f;
    public float fov = 60f;
}
