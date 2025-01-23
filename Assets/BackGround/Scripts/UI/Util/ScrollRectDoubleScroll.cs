using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[DisallowMultipleComponent]
public class ScrollRectDoubleScroll : ScrollRect
{    
    [Header("상위 ScrollRect TransFrom")]
    public Transform targetTransFrom;

    private bool routeToParent = false;
    private void DoForParents<T>(Action<T> action) where T:IEventSystemHandler
    {
        if (targetTransFrom == null)
            return;

        var component = targetTransFrom.GetComponent<T>();
        action((T)(IEventSystemHandler)component);
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        DoForParents<IInitializePotentialDragHandler>((parent) =>
        {
            parent.OnInitializePotentialDrag(eventData);
            base.OnInitializePotentialDrag(eventData);
        });
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (routeToParent)
        {
            DoForParents<IDragHandler>((parent) =>
            {
                parent.OnDrag(eventData);
            });
        }
        else
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!horizontal && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
        {
            routeToParent = true;
        }
        else if (!vertical && Math.Abs (eventData.delta.x) < Math.Abs(eventData.delta.y))
        {
            routeToParent = true;
        }
        else
        {
            routeToParent = false;
        }

        if(routeToParent)
        {
            DoForParents<IBeginDragHandler>((parent) =>
            {
                parent.OnBeginDrag(eventData);
            });
        }
        else
        {
            base.OnBeginDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (routeToParent)
        {
            DoForParents<IEndDragHandler>((parent) =>
            {
                parent.OnEndDrag(eventData);
            });
        }
        else
        {
            base.OnEndDrag(eventData);
        }

        routeToParent = false;
    }
}
