using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using TMPro;
using DG.Tweening;

using System.Linq;
using Unity.Linq;
using Sirenix.OdinInspector;
public class NavigationUI : MonoBehaviour
{
    public Button prevBtn;
    public Button nextBtn;
    public GameObject iconParent;
    public Toggle iconOrigin;
    public Action<int> OnSelect;

    List<Toggle> listNaviIcon = new List<Toggle>();
    IntReactiveProperty cur = new IntReactiveProperty(0);
    int max = 0;
    void Start()
    {
        iconOrigin.gameObject.SetActive(false);

        cur.Subscribe(_ =>
        {
            prevBtn.gameObject.SetActive(_ > 0);
            nextBtn.gameObject.SetActive(max - 1 > _);
            OnSelect?.Invoke(cur.Value);

            int i = 0;
            foreach(var mit in listNaviIcon)
            {
                mit.isOn = i == _;
                i++;
            }
        });
        prevBtn.OnClickAsObservable().Subscribe(_ =>
        {
            cur.Value = Math.Max(0, cur.Value - 1);
        });
        nextBtn.OnClickAsObservable().Subscribe(_ =>
        {
            cur.Value = Math.Min(max - 1, cur.Value + 1);
        });
    }


    public void Init(int maxCount, int cursor = 0)
    {
        cur.Value = cursor;
        max = maxCount;
        foreach (var mit in listNaviIcon)
        {
            mit.gameObject.Destroy();
        }
        listNaviIcon.Clear();

        for (int i = 0; i < maxCount; ++i)
        {
            var clone = iconParent.Add(iconOrigin);
            clone.gameObject.SetActive(true);
            listNaviIcon.Add(clone);
            clone.isOn = i == cursor;
        }

        prevBtn.gameObject.SetActive(cur.Value > 0);
        nextBtn.gameObject.SetActive(max - 1 > cur.Value);
    }

    internal int GetIndex()
    {
        return cur.Value;
    }
}
