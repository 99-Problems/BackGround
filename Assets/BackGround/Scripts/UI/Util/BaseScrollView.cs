using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Unity.Linq;
using System;
using System.Linq;
using DG.Tweening;

public abstract class BaseScrollViewItem<T> : MonoBehaviour
{
    public abstract void Init(T _info, int _index);
}

[RequireComponent(typeof(ScrollRect))]
public abstract class BaseScrollView<T, TInfo> : MonoBehaviour
    where T : BaseScrollViewItem<TInfo>
{
    public T Origin;

    protected List<T> item = new List<T>();

    protected Subject<T> itemClickSubject = new Subject<T>();

    [ReadOnly]
    public Queue<T> recycleQueue = new Queue<T>();

    public IObservable<T> OnItemClick
    {
        get { return itemClickSubject.AsObservable(); }
    }

    protected Subject<int> itemListReplaceSubject = new Subject<int>();

    public IObservable<int> OnItemListReplace
    {
        get { return itemListReplaceSubject.AsObservable(); }
    }

    protected Subject<T> itemInitSubject = new Subject<T>();

    public IObservable<T> OnItemInit
    {
        get { return itemInitSubject.AsObservable(); }
    }

    [ReadOnly]
    public ScrollRect scrollRect;

    //아이템 생성방식
    public bool ImmediateRefresh = true;
    private Coroutine refreshCoroutine;
    public IDisposable refreshFunc;

    [ShowIf("@ImmediateRefresh == false")]
    [BoxGroup("FadeAni")]
    [InfoBox("itemFadeAni 아이템을 부드럽게 생성 (생성 아이템이 많으면 사용하지 말것.)")]
    public bool itemFadeAni = false;
    [ShowIf("@ImmediateRefresh == false")]
    [BoxGroup("FadeAni")]
    public float fadeTime = 0.3f;
    [ShowIf("@ImmediateRefresh == false")]
    [BoxGroup("FadeAni")]
    public int onceRenderItem = 1;
    [ShowIf("@ImmediateRefresh == false")]
    [BoxGroup("FadeAni")]
    public float delayTime = 0.0f;

    private void Awake()
    {
        if (Origin)
            Origin.gameObject.SetActive(false);

    }

    public void ClearItem()
    {
        if (refreshFunc != null)
            refreshFunc.Dispose();

        int delSize = item.Count;
        for (int j = 0; j < delSize; ++j)
        {
            GameObject.Destroy(item[j].gameObject);
        }

        item.Clear();
    }

    public virtual void DeleteItem(IEnumerable<TInfo> _list)
    {
        List<T> temp = new List<T>();
        temp.AddRange(item);

        var enumerator = _list.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            var clone = RecycleItem(ref temp, current);

            if (clone != null)
            {
                item.Remove(clone);
                clone.transform.SetAsLastSibling();
                clone.gameObject.SetActive(false);
            }
        }
        itemListReplaceSubject.OnNext(_list.Count());
    }

    public void SetItemList(IEnumerable<TInfo> _list)
    {

        if (!ImmediateRefresh)
        {
            if (refreshFunc != null)
                refreshFunc.Dispose();
            refreshFunc = Observable.FromCoroutine(_ => DelayRefresh(_list)).Subscribe(_ => Debug.Log("UpdateRefresh")).AddTo(this);
            return;
        }

        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
        scrollRect.content.anchoredPosition3D = Vector3.zero;

        List<T> temp = new List<T>();
        temp.AddRange(item);

        item.Clear();

        int i = 0;
        if (_list != null)
        {
            var enumerator = _list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var clone = RecycleItem(ref temp, current);
                if (clone == null)
                {
                    if (recycleQueue.Count == 0)
                    {
                        clone = scrollRect.content.gameObject.Add(CreateItemAsset(current), TransformCloneType.KeepOriginal, true);
                        InitFirstItem(clone);
                    }
                    else
                    {
                        clone = recycleQueue.Dequeue();
                        clone.transform.SetAsLastSibling();
                    }
                }
                else
                {
                    temp.Remove(clone);
                    clone.transform.SetAsLastSibling();
                }

                clone.gameObject.SetActive(true);

                InitItem(clone, current, i);
                item.Add(clone);
                i++;
                itemInitSubject.OnNext(clone);
            }
        }

        int delSize = temp.Count;
        for (int j = 0; j < delSize; ++j)
        {
            temp[j].gameObject.SetActive(false);
            recycleQueue.Enqueue(temp[j]);
        }
        itemListReplaceSubject.OnNext(i);
    }

    private IEnumerator DelayRefresh(IEnumerable<TInfo> _list)
    {
        if(itemFadeAni && refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }

        int delSize = item.Count;
        for (int j = 0; j < delSize; ++j)
        {
            item[j].gameObject.SetActive(false);
            recycleQueue.Enqueue(item[j]);
        }

        item.Clear();
        yield return null;

        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
        scrollRect.content.anchoredPosition3D = Vector3.zero;

        if (itemFadeAni)
        {
            int i = 0;
            var enumerator = _list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                T profile;
                if (recycleQueue.Count == 0)
                {
                    var newObj = scrollRect.content.gameObject.Add(CreateItemAsset(current), TransformCloneType.KeepOriginal, true);
                    InitFirstItem(newObj);
                    profile = newObj.GetComponent<T>();
                }
                else
                {
                    profile = recycleQueue.Dequeue();
                    profile.transform.SetAsLastSibling();
                }

                var canvasGroup = profile.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = profile.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.DOKill();
                canvasGroup.alpha = 0;

                profile.gameObject.SetActive(true);
                InitItem(profile, current, i);
                item.Add(profile);
                i++;
                itemInitSubject.OnNext(profile);
            }
            yield return new WaitForFixedUpdate();
            itemListReplaceSubject.OnNext(i);
            PlayRefreshAni();
        }
        else
        {
            int i = 0;
            var enumerator = _list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                T profile;
                if (recycleQueue.Count == 0)
                {
                    var newObj = scrollRect.content.gameObject.Add(CreateItemAsset(current), TransformCloneType.KeepOriginal, true);
                    InitFirstItem(newObj);
                    profile = newObj.GetComponent<T>();
                }
                else
                {
                    profile = recycleQueue.Dequeue();
                    profile.transform.SetAsLastSibling();
                    profile.gameObject.SetActive(true);
                }

                InitItem(profile, current, i);
                item.Add(profile);
                i++;
                itemInitSubject.OnNext(profile);
                yield return null;
            }
            yield return null;
            itemListReplaceSubject.OnNext(i);
        }
        refreshFunc = null;
    }

    // 특정 상황에서 스크롤뷰를 강제로 다른데로 이동 시킬때 한번에 완성 시켜버림
    // 스크롤뷰를 시작 지점에서 멀리 이동시키면 fade 동작하는데까지 오래걸리기 때문에 만듬.
    public void SkipRefreshAni()
    {
        if (ImmediateRefresh)
            return;

        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }

        foreach (var obj in item)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            canvasGroup.DOKill();
            canvasGroup.alpha = 1;
        }
    }

    public void ReLoadRefreshAni()
    {
        foreach (var obj in item)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                continue;

            canvasGroup.DOKill();
            canvasGroup.alpha = 0;
        }

        if(gameObject.activeSelf)
            PlayRefreshAni();
    }

    public void PlayRefreshAni()
    {
        if (!itemFadeAni)
            return;

        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }

        if (scrollRect == null || scrollRect.IsActive() == false)
            return;

        refreshCoroutine = StartCoroutine(RefreshAni());
    }

    private IEnumerator RefreshAni()
    {
        int i = 0;
        foreach (var obj in item)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
            {
            	canvasGroup = obj.gameObject.AddComponent<CanvasGroup>();
            }

            if (i != 0 && i % onceRenderItem == 0)
            {
                if (delayTime > 0)
                {
                    yield return new WaitForSeconds(delayTime);
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            canvasGroup.DOFade(1, fadeTime);
            i++;
        }
    }



    //복사용 object
    protected virtual T CreateItemAsset(TInfo _current)
    {
        return Origin;
    }

    //최초 생성시
    protected virtual void InitFirstItem(T _obj)
    {
    }

    //재활용 가능 아이템 확인
    protected virtual T RecycleItem(ref List<T> _items, TInfo _info)
    {
        return null;
    }

    //정보 갱신이 필요
    protected virtual void InitItem(T _obj, TInfo _info, int _index)
    {
        _obj.Init(_info, _index);
    }

    public T GetFirstItem()
    {
        return item.FirstOrDefault();
    }

    public int GetItemCount()
    {
        return item.Count;
    }
    
    public List<T> GetItem()
    {
        return item;
    }

#if UNITY_EDITOR
    //기본 마스크를 RectMask로 변경한다
    [Button]
    public void DefaultBaseSetting()
    {
        var component = gameObject.Child("Viewport");
        if (component)
        {
            var mask = component.GetComponent<Mask>();
            DestroyImmediate(mask, true);
            var Image = component.GetComponent<Image>();
            DestroyImmediate(Image, true);
            component.AddComponent<RectMask2D>();
        }

        var image = gameObject.GetComponent<Image>();
        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
        }

        image.color = new Color(0, 0, 0, 0);

        var _scrollRect = GetComponent<ScrollRect>();
        _scrollRect.movementType = ScrollRect.MovementType.Clamped;
    }
#endif

    public void OnDisable()
    {
        if (!ImmediateRefresh && itemFadeAni)
        {
            if (refreshCoroutine != null)
            { 
                StopCoroutine(refreshCoroutine);
                refreshCoroutine = null;
            }
        }
    }
}