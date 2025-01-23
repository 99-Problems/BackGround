using Cysharp.Threading.Tasks;
using Data;
using Sirenix.OdinInspector;
using System;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public Action KeyAction = null;
    public Action<MouseEvent> MouseAction = null;
    public enum MouseEvent
    {
        Press = 0,
        PointerDown = 1,
        PointerUp = 2,
        Click = 3,
    }
    public enum TouchType
    {
        Stopped = 0,
        Moved = 1,
        SizeUp = 2,
        SizeDown = 3,
    }

    public Subject<Vector3> dragDir = new Subject<Vector3>();
    public Subject<bool> dragSubject = new Subject<bool>();

    bool _pressed = false;
    float _pressedTime = 0;


    [Title("기능 락")]
    public bool dragOff;
#if UNITY_EDITOR
    private const string scrollWheel = "Menu/ScrollWheel";
    public static bool isScrollWheel => EditorPrefs.GetBool(scrollWheel);

    [MenuItem(scrollWheel)]
    private static void ScrollWheelToggle()
    {
        var _isDrawGizmos = !isScrollWheel;
        Menu.SetChecked(scrollWheel, _isDrawGizmos);
        EditorPrefs.SetBool(scrollWheel, _isDrawGizmos);
        SceneView.RepaintAll();
    }
#endif

#if UNITY_EDITOR
    public static int pointerID = -1; //유니티 상에서는 -1
#else
        public static int pointerID = 0;  
#endif

    private Vector3 startPosition;
    private Vector3 lastPosition; // 마지막 위치
    private bool isDragging = false; // 드래그 중인지 여부

    public bool isInteractable;
    public float minDistance = 1f;


    public void Init()
    {
      
    }

    public void OnUpdate()
    {
#if !UNITY_EDITOR
        if (!isInteractable || Managers.Popup.IsWaitPopup())
            return;

        if (Managers.Time.IsPause)
            return;

        if (Managers.Popup.IsShowPopupWithoutBlock())
            return;
#endif

#if UNITY_EDITOR
        #region scroll wheel
        if(isScrollWheel)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            dragDir.OnNext(new Vector3(0, scroll * CinemachineCameraController.wheelSpeed, 0));
        }
        #endregion

        if (dragOff)
            return;
#endif

        #region 키 입력 및 이동
        // 마우스 입력 또는 터치 입력 중 하나만 처리
        Vector3 inputPosition = Vector3.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;

            // UI 위에서 터치가 발생하면 무시
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                StartDrag(inputPosition);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                PerformDrag(inputPosition);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndDrag();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // UI 위에서 마우스 클릭이 발생하면 무시
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            inputPosition = Input.mousePosition;
            StartDrag(inputPosition);
        }
        else if (Input.GetMouseButton(0))
        {
            inputPosition = Input.mousePosition;
            PerformDrag(inputPosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
        #endregion

        //if (EventSystem.current == null || EventSystem.current.IsPointerOverGameObject(InputManager.pointerID))
        //    return;

        //if (Input.anyKey && KeyAction != null)
        //    KeyAction.Invoke();


        //if (MouseAction != null)
        //{
        //    if (Input.GetMouseButton(0))
        //    {
        //        if (_pressed == false)
        //        {
        //            MouseAction.Invoke(MouseEvent.PointerDown);
        //            _pressedTime = Time.time;
        //        }

        //        MouseAction.Invoke(MouseEvent.Press);
        //        _pressed = true;
        //    }
        //    else
        //    {
        //        if (_pressed)
        //        {
        //            if (Time.time < _pressedTime + 0.2f)
        //            {
        //                MouseAction.Invoke(MouseEvent.Click);
        //            }

        //            MouseAction.Invoke(MouseEvent.PointerUp);
        //        }

        //        _pressed = false;
        //        _pressedTime = 0;
        //    }
        //}
    }

    public void Clear()
    {
        KeyAction = null;
        MouseAction = null;
    }

    void StartDrag(Vector3 inputPosition)
    {
        startPosition = inputPosition;
        lastPosition = inputPosition; // 드래그 시작 시점의 위치 저장
        isDragging = true; // 드래그 시작
    }

    void PerformDrag(Vector3 inputPosition)
    {
        if (!isDragging)
            return;

        var dragDistance = inputPosition - lastPosition;
        if (dragDistance.magnitude < minDistance)
        {
            return;
        }

        dragDir.OnNext(new Vector3(dragDistance.x, 0, dragDistance.y));

        lastPosition = inputPosition; // 현재 위치를 마지막 위치로 갱신
    }

    void EndDrag()
    {
        isDragging = false; // 드래그 종료
        var dragDistance = lastPosition - startPosition;
        if (Mathf.Abs(dragDistance.y) > 200 && Mathf.Abs(dragDistance.x) < 200)
        {
            dragSubject.OnNext(dragDistance.y > 0);
            string drag = dragDistance.y > 0 ? "Up" : "Down";
            Debug.Log($"{drag}", Color.green);
        }
    }
}
