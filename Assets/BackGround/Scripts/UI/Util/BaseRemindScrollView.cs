using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BaseRemindScrollView<T, TInfo> : BaseScrollView<T, TInfo> where T : BaseScrollViewItem<TInfo>
{

    Vector3 contentPosition;

    protected virtual void Start()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
        scrollRect.OnValueChangedAsObservable().Subscribe(_ =>
        {
            contentPosition = scrollRect.content.position;

        });
    }

    public void SetContentPos()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
        scrollRect.content.position = contentPosition;
    }
}