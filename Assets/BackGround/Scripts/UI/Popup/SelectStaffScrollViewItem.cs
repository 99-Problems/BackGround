using Data;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class SelectStaffInfo
{
    public UnitLogic unitLogic;
    public int triggerIndex;
    public Define.ETRIGGER_TYPE type;
    public Define.EVENT_TYPE eventType;
}

public class SelectStaffScrollViewItem : BaseScrollViewItem<SelectStaffInfo>
{

    public GTMPro numberText;
    public GameObject blockObj;
    public Image staffIcon;
    public Image castingProgressBar;
    public Button selectBtn;
    public Button DeselectBtn;


    private bool isGray;
    public bool GetGray => isGray;

    public bool isSelect { get; private set; }
    public bool isLock;
    public bool isStaffWork;

    private SelectStaffInfo Info;

    private void Start()
    {
        Info.unitLogic.state.Subscribe(state=>
        {
            UpdateUI(state);
        }).AddTo(this);

        gameObject.FixedUpdateAsObservable().Where(_=> isStaffWork).Subscribe(_=>
        {
            UpdateTime();
        }).AddTo(this);
    }

    public void UpdateUI()
    {
        selectBtn.interactable = !isSelect;
        DeselectBtn.gameObject.SetActive(isSelect);
        blockObj.SetActive(!isSelect && isLock);

    }

    public void UpdateUI(Define.EUNIT_STATE state)
    {
        if(!IsOwnUnit())
        {
            if (state == Define.EUNIT_STATE.Move || state == Define.EUNIT_STATE.Wait || state == Define.EUNIT_STATE.Work)
            {
                SetGray();
            }
            else if (isGray)
            {
                SetNormal();
            }

            castingProgressBar.gameObject.SetActive(state == Define.EUNIT_STATE.Work);
            isStaffWork = state == Define.EUNIT_STATE.Work;
            UpdateTime();
        }
        UpdateUI();
    }

    public void UpdateTime()
    {
        if (!isStaffWork)
            return;

        var eventInfo = InGamePlayInfo.GetEventSpotList.Find(_ => _.GetIndex == Info.unitLogic.destination.index && _.Info.eventType == Info.unitLogic.curEventType);
        if(eventInfo == null)
        {
            castingProgressBar.gameObject.SetActive(false);
            return;
        }
        castingProgressBar.fillAmount = Mathf.Clamp01((eventInfo.Info.needTime - eventInfo.workTime) / eventInfo.Info.needTime);
    }

    public override void Init(SelectStaffInfo _info, int _index)
    {
        Info = _info;
        if (_info == null)
            return;

        numberText.SetText(Info.unitLogic.UnitID);
        if(IsOwnUnit())
        {
            if(GetOwnUnitState())
            {
                Select(true);

            }
        }
        UpdateUI(Info.unitLogic.state.Value);
        //SetLock(isSelect);
    }
    public bool IsOwnUnit()
    {
        return Info.unitLogic.destination.targetType == Info.type && Info.unitLogic.destination.index == Info.triggerIndex;
    }

    public bool GetOwnUnitState()
    {
        return Info.unitLogic.state.Value == Define.EUNIT_STATE.Move || Info.unitLogic.state.Value == Define.EUNIT_STATE.Work;
    }
    public void SetGray()
    {
        staffIcon.material = SettingScriptableObject.Instance.grayScale;
        isGray = true;
    }

    public void SetNormal()
    {
        staffIcon.material = null;
        isGray = false;
    }

    public void Select(bool init = false)
    {
        isSelect = true;
        UpdateUI();
        if (init)
            return;

        InGamePlayInfo.OnUnitSelect.OnNext((Info.type, Info.triggerIndex, Info.unitLogic, Info.eventType));
    }

    public void DeSelect(bool init = false)
    {
        isSelect = false;
        UpdateUI();
        if (init)
            return;

        InGamePlayInfo.OnUnitDeSelect.OnNext((Info.type, Info.triggerIndex, Info.unitLogic, Info.eventType));
    }

    public void SetLock(bool _isLock)
    {
        isLock = _isLock;
        UpdateUI();
    }
}
