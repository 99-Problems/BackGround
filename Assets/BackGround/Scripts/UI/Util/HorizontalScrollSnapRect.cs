using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Sirenix.OdinInspector;
using System;

[RequireComponent(typeof(ScrollRect))]
public class HorizontalScrollSnapRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public float fastSwipeThresholdTime = 0.3f;
    public int fastSwipeThresholdDistance = 100;
    public float decelerationRate = 10f;
    private int _fastSwipeThresholdMaxLimit;

    private ScrollRect _scrollRectComponent;
    private RectTransform _container;

    [NonSerialized]
    public IntReactiveProperty pageCount = new IntReactiveProperty(0);
    [NonSerialized]
    public IntReactiveProperty currentPage = new IntReactiveProperty(0);

    [NonSerialized]
    public BoolReactiveProperty _init = new BoolReactiveProperty(false);
    private bool _lerp;
    private Vector2 _lerpTo;

    [NonSerialized]
    public List<float> pagePositions = new List<float>();

    public BoolReactiveProperty dragable = new BoolReactiveProperty(false);
    private float _timeStamp;
    private Vector2 _startPosition;

    private bool _showPageSelection;

    private Subject<Transform> focusObject = new Subject<Transform>();
    public IObservable<Transform> OnFocusObject { get { return focusObject.AsObservable(); } }

    private Subject<Transform> detachFocusObject = new Subject<Transform>();
    public IObservable<Transform> OnDetachFocus { get { return detachFocusObject.AsObservable(); } }

    public bool dragLock = false;

    [Button]
    public void Init()
    {
        _init.Value = false;
        _scrollRectComponent = GetComponent<ScrollRect>();
        _container = _scrollRectComponent.content;

        StartCoroutine(InitUI(0));
    }

    public void Init(int page)
    {
        _init.Value = false;
        _scrollRectComponent = GetComponent<ScrollRect>();
        _container = _scrollRectComponent.content;

        currentPage.Value = page;
        StartCoroutine(InitUI(page));
    }

    public void OnEnable()
    {
        StopCoroutine(InitUI(currentPage.Value));
        Init(currentPage.Value);
    }

    private IEnumerator InitUI(int page)
    {
        yield return new WaitForEndOfFrame();

        // init
        SetPagePositions();
        SetPage(page);
        yield return null;

        _lerp = true;
        _init.Value = true;
    }

    void Update()
    {
        if (false == _init.Value)
            return;

        if (_lerp)
        {
            float decelerate = Mathf.Min(decelerationRate * Time.deltaTime, 1f);
            _container.anchoredPosition = Vector2.Lerp(_container.anchoredPosition, _lerpTo, decelerate);
            
            if (Vector2.SqrMagnitude(_container.anchoredPosition - _lerpTo) < 1.0f)
            {
                _container.anchoredPosition = _lerpTo;
                _lerp = false;
                _scrollRectComponent.velocity = Vector2.zero;

                if (_container.childCount > 0)
                {
                    focusObject.OnNext(_container.GetChild(currentPage.Value));
                }
            }
        }
    }

    private void SetPagePositions()
    {
        if (_container.childCount < 1)
            return;

        _fastSwipeThresholdMaxLimit = (int)GetComponent<RectTransform>().rect.width;

        pagePositions.Clear();

        pageCount.Value = _container.childCount;

        var firstChild = _container.GetChild(0);
        for (int i = 0; i < pageCount.Value; i++)
        {
            RectTransform child = _container.GetChild(i).GetComponent<RectTransform>();
            pagePositions.Add(-(child.localPosition.x - firstChild.localPosition.x));
        }
    }

    private void SetPage(int aPageIndex)
    {
        if (_container.childCount < 1)
            return;
        aPageIndex = Mathf.Clamp(aPageIndex, 0, pageCount.Value - 1);
        _lerpTo = _container.anchoredPosition = new Vector2(pagePositions[aPageIndex], 0);
        currentPage.Value = aPageIndex;
    }

    public void LerpToPage(int aPageIndex)
    {
        if (_container.childCount < 1)
            return;
        aPageIndex = Mathf.Clamp(aPageIndex, 0, pageCount.Value - 1);
        _lerpTo = new Vector2(pagePositions[aPageIndex], 0);
        _lerp = true;
        currentPage.Value = aPageIndex;
    }

    [Button]
    public void NextItem()
    {
        LerpToPage(currentPage.Value + 1);
    }

    [Button]
    public void PrevItem()
    {
        LerpToPage(currentPage.Value - 1);
    }

    internal bool IsLastPage()
    {
        return currentPage.Value + 1 == pageCount.Value;
    }

    Vector2 test = new Vector2();
    private int GetNearestPage()
    {
        if (_container.childCount < 1)
            return 0;
        Vector2 currentPosition = _container.anchoredPosition;

        float distance = float.MaxValue;
        int nearestPage = currentPage.Value;

        for (int i = 0; i < pagePositions.Count; i++)
        {
            test.x = pagePositions[i];
            float testDist = Vector2.SqrMagnitude(currentPosition - test);
            if (testDist < distance)
            {
                distance = testDist;
                nearestPage = i;
            }
        }

        return nearestPage;
    }

    public void OnBeginDrag(PointerEventData aEventData)
    {
        if (dragLock)
            return;
        detachFocusObject.OnNext(null);
        _lerp = false;
        dragable.Value = false;

    }

    public void OnEndDrag(PointerEventData aEventData)
    {
        if (dragLock)
            return;
        float difference = _startPosition.x - _container.anchoredPosition.x;

        //빠르게 스왑했을 때 페이지 넘겨주는 기능
        if (Time.unscaledTime - _timeStamp < fastSwipeThresholdTime &&
            Mathf.Abs(difference) > fastSwipeThresholdDistance &&
            Mathf.Abs(difference) < _fastSwipeThresholdMaxLimit)
        {
            if (difference > 0)
            {
                NextItem();
            }
            else
            {
                PrevItem();
            }
        }
        else
        {
            LerpToPage(GetNearestPage());
        }
        dragable.Value = false;
    }

    public void OnDrag(PointerEventData aEventData)
    {
        if (dragLock)
            return;
        if (!dragable.Value)
        {
            dragable.Value = true;
            _timeStamp = Time.unscaledTime;
            _startPosition = _container.anchoredPosition;
        }
        else
        {
        }
    }



} 
