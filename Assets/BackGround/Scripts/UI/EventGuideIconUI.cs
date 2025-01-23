using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using Sirenix.OdinInspector;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

public class EventGuideIconUI : MonoBehaviour
{
    public Button selectBtn;
    public Vector3 addedMovePos;
    public RectTransform rectTransform;
    [Title("UI ¼³Á¤")]
    public RectTransform arrowRect;
    public float zRotationOffset = 90f;
    public float radius = 50f;
    public float maxDistance = 60f;
    public float minDistance = 40f;
    public float minBonus = 30f;

    public Transform target;
    public float edgeBuffer = 50f;

    private Camera cam;
    private RectTransform canvasRect;
    private Vector3 screenPos = Vector3.zero;
    private EventSpot Info;

    private bool isEnd;
    private IDisposable dispoasable;


    private void Start()
    {
        if (selectBtn == null)
            selectBtn.GetOrAddComponent<Button>();

        selectBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            if (Info == null)
                return;

            InGamePlayInfo.OnMoveCam.OnNext(Info.transform.position + addedMovePos);
        });
    }
    public void Init(EventSpot info)
    {
        Info = info;
        dispoasable = Managers.Input.dragDir.Where(_ => isEnd == false).Merge(InGamePlayInfo.OnMoveCam).DelayFrame(2).Subscribe(dragDir =>
        {
            if (target == null || Info.GetEnd)
            {
                isEnd = true;
                if(dispoasable != null)
                    dispoasable.Dispose();
                gameObject.SetActive(false);
                return;
            }

            SetPosition();

        }).AddTo(this);

        Info.IsClear.Subscribe(_ =>
        {
            isEnd = true;
            if(dispoasable != null) 
                dispoasable.Dispose();
            gameObject.SetActive(false);
        }).AddTo(this);

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (canvasRect == null)
            canvasRect = UICanvas.MasterCanvas.safeArea.GetComponent<RectTransform>();
        if (cam == null)
            cam = Camera.main;
        target = info.gameObject.transform;
        isEnd = false;

        SetPosition();
    }

    public bool IsOffScreen()
    {
        if (target == null || cam == null)
            return false;

        screenPos = cam.WorldToViewportPoint(target.position);
        return screenPos.x <= 0 || screenPos.x >= 1 || screenPos.y <= 0 || screenPos.y >= 1 || screenPos.z < 0;
    }
    public void SetPosition()
    {
        var isOff = IsOffScreen();
        gameObject.SetActive(isOff);
        if (!isOff)
            return;

        if(screenPos.z < 0)
        {
            screenPos.x = 1f - screenPos.x;
            screenPos.y = 1f - screenPos.y;
            screenPos.z = Mathf.Abs(screenPos.z);
        }

        Vector3 viewPos = ClampToEdge(screenPos);
        var canvasPos = ViewportToCanvasPosition(viewPos);
        rectTransform.anchoredPosition = canvasPos;

        var screenCenter = new Vector3(0.5f, 0.5f, 0);
        var fromCenterToTarget = screenPos - screenCenter;
        float angle = Mathf.Atan2(fromCenterToTarget.y, fromCenterToTarget.x) * Mathf.Rad2Deg;
        angle -= zRotationOffset;
        arrowRect.localEulerAngles = new Vector3(0, 0, angle);
        var distance = fromCenterToTarget.normalized * radius;
        var arrowRectPosition = Vector2.zero;
        bool isNegativeX = distance.x < 0;
        bool isNegativeY = distance.y < 0;
        var normalX = Mathf.Abs(fromCenterToTarget.normalized.x);
        var normalY = Mathf.Abs(fromCenterToTarget.normalized.y);
        var distanceX = Mathf.Min(maxDistance * normalX, Mathf.Max(Math.Abs(distance.x), minDistance * normalX));
        var distanceY = Mathf.Min(maxDistance * normalY, Mathf.Max(Math.Abs(distance.y), minDistance * normalY));
        var arrowVector = new Vector2(isNegativeX ? -distanceX : distanceX, isNegativeY ? -distanceY : distanceY);

        arrowRectPosition = arrowVector.normalized * radius;

        arrowRect.anchoredPosition = arrowRectPosition;
    }

    Vector2 ViewportToCanvasPosition(Vector3 pos)
    {
        var rect = canvasRect.rect;

        Vector2 result = new Vector2(
            (pos.x - 0.5f) * rect.width,
            (pos.y - 0.5f) * rect.height
        );

        result.x = Mathf.Clamp(result.x, -rect.width / 2 + rectTransform.rect.width / 2 + edgeBuffer,
                            rect.width / 2 - rectTransform.rect.width / 2 - edgeBuffer);
        result.y = Mathf.Clamp(result.y, -rect.height / 2 + rectTransform.rect.height / 2 + edgeBuffer,
                            rect.height / 2 - rectTransform.rect.height / 2 - edgeBuffer);
        return result;
    }

    Vector3 ClampToEdge(Vector3 pos)
    {
        if (Mathf.Abs(pos.x - 0.5f) > Mathf.Abs(pos.y - 0.5f))
        {
            pos.x = pos.x > 0.5f ? 0.95f : 0.05f;
        }
        else
        {
            pos.y = pos.y > 0.5f ? 0.95f : 0.05f;
        }

        return pos;
    }
}
