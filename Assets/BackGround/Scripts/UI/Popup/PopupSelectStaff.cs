using Data;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PBSelectStaff : PopupArg
{
    public float limitTime;
    public float curTime;
    public List<UnitLogic> unitList;
    public Define.ETRIGGER_TYPE type;
    public int maxUnitCount;
    public int index;
    public Define.EVENT_TYPE eventType = Define.EVENT_TYPE.NONE;
}

public class PopupSelectStaff : PopupBase
{
  
    public Button exitBtn;
    public GameObject warnIconObj;
    public Image progressBar;
    public GTMPro unitCountText;
    public SelectStaffScrollView scrollView;
    public TimeSpan remainTime;

    private float toColorTime;
    public Color redColor;

    protected PBSelectStaff arg;
    private float curTime;
    private bool bColorChange;

    private void Start()
    {
        if(exitBtn)
        {
            exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
            {
                PressBackButton();
            });
        }

        scrollView.OnItemClick.Subscribe(item =>
        {
            if(!scrollView.SetLock(scrollView.unitCount >= arg.maxUnitCount) && scrollView.unitCount == arg.maxUnitCount - 1)
            {
                foreach (var mit in scrollView.GetItem())
                {
                    if(mit.isLock)
                        mit.SetLock(false);
                }
            }
            UpdateUI();
            Managers.Popup.ClosePopupBox(this); // 전체 배치시 자동 닫기 임시
        }).AddTo(this);

        var staffInfoList = new List<SelectStaffInfo>();
        foreach (var unit in arg.unitList)
        {
            var item = new SelectStaffInfo
            {
                unitLogic = unit,
                triggerIndex = arg.index,
                type = arg.type,
                eventType = arg.eventType,
            };
            staffInfoList.Add(item);
        }
        scrollView.SetItemList(staffInfoList);
        var count = scrollView.GetSelectCount();
        scrollView.SetLock(count >= arg.maxUnitCount);

        UpdateUI();

        gameObject.FixedUpdateAsObservable().Where(_ => !Managers.Time.IsPause).Subscribe(_ =>
        {
            curTime += Time.fixedDeltaTime;
            UpdateTime();
        }).AddTo(this);

        InGamePlayInfo.OnExitEvent.Subscribe(info =>
        {
            if(info.eventType == arg.eventType && info.index == arg.index)
                PressBackButton();
        }).AddTo(this);
    }

    public void UpdateUI()
    {
        if(IsEventSpot())
        {
            UpdateTime();
        }

        unitCountText.SetText(scrollView.unitCount, arg.maxUnitCount);

    }

    public void UpdateTime()
    {
        if (arg.limitTime <= 0)
            return;

        var reamainTime = Mathf.Max(0,arg.limitTime - curTime);

        progressBar.fillAmount = Mathf.Clamp01(reamainTime / arg.limitTime);
        if(bColorChange == false && reamainTime <= toColorTime)
        {
            progressBar.DOColor(redColor, Mathf.Min(toColorTime / 2, reamainTime)).SetEase(Ease.Linear);
            bColorChange = true;
        }
        
        if(progressBar.fillAmount == 0)
        {
            Managers.Popup.ClosePopupBox(this);
        }
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);
        arg = (PBSelectStaff) _popupData;

        warnIconObj.SetActive(IsEventSpot());
        curTime = arg.curTime;
        toColorTime = arg.limitTime / 2;

        var clampTime = (curTime - toColorTime) / toColorTime;

        Color initialColor = Color.Lerp(progressBar.color, redColor, clampTime);
        progressBar.color = initialColor;
    }

    public bool IsEventSpot()
    {
        return arg.type == Define.ETRIGGER_TYPE.EventSpot;
    }

    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(this);
    }
}
