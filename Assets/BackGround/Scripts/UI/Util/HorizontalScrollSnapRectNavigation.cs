using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalScrollSnapRectNavigation : MonoBehaviour
{
    public HorizontalScrollSnapRect snapRect;

    public Toggle originNavigationIcon;

    private List<Toggle> navigations = new List<Toggle>();

    private void Start()
    {
        originNavigationIcon.gameObject.SetActive(false);

        snapRect._init.Subscribe(init =>
        {
            snapRect.pageCount.Subscribe(_ =>
            {
                navigations.Clear();
                gameObject.Children().Where(_1 => _1.gameObject.activeInHierarchy).Destroy();
                for (int i = 0; i < _; ++i)
                {
                    var clone = gameObject.Add(originNavigationIcon, TransformCloneType.KeepOriginal, true);
                    navigations.Add(clone);

                    int page = i;
                    clone.OnValueChangedAsObservable().DistinctUntilChanged().Where(_1 => _1).Subscribe(_1 => { snapRect.LerpToPage(page); });
                }
            });
            snapRect.currentPage.Subscribe(_ =>
            {
                if (navigations.Count <= _)
                    return;
                navigations[_].isOn = true;
            });
        });
    }
}