using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Data;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using Unity.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventSpot : BaseTrigger
{

    [ShowInInspector]
    private List<int> workUnitList = new List<int>();
    public List<int> unitIds => workUnitList;
    private bool isWork = false;
    private Subject<bool> isClear = new Subject<bool>();
    public IObservable<bool> IsClear => isClear;
    private float curTime = 0f;
    public float workTime { get; private set; } = 0f;
    private bool isEnd;
    public bool GetEnd => isEnd;

    private EventObject eventObj;
    public EventInfoScript Info { get; private set; }
    private List<UnitLogic> waitList = new List<UnitLogic>();

    public Canvas canvas;
    public Image progressBar;
    private IGameData gameData;
    private IDisposable stateSub;

    public GameObject clearParticleObj;

    private void Start()
    {
        triggerType = Define.ETRIGGER_TYPE.EventSpot;
        

        

        isClear.Subscribe(async clear =>
        {
            InGamePlayInfo.RemoveEventSpot(this);
            InGamePlayInfo.OnExitEvent.OnNext((Info.eventType, index));
            if (gameData == null)
                return;

            if (clear)
            {
                gameData.PlayInfo.playData.score += AddScore();
                clearParticleObj.SetActive(true);
                Debug.ColorLog($"이벤트 {Info.eventType} 성공 : +{Info.eventScore}", Color.green);
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
            }
            else
            {
                Debug.ColorLog($"이벤트 {Info.eventType} 실패", SettingScriptableObject.Instance.CoralRed);
                InGamePlayInfo.OnHpChange.OnNext(-1);
            }


            await Managers.Pool.PushEventObject(eventObj);
            Managers.Pool.PushTrigger(this);
        }).AddTo(this);

       

        canvas.worldCamera = Camera.main;
        canvas.gameObject.transform.rotation = Camera.main.transform.rotation;
    }

    public async UniTask Init(Define.EVENT_TYPE eventType, int _index, bool initialize = true)
    {
        Clear(initialize);
        Info = Managers.Data.GetEventInfoScript(_ => _.eventType == eventType);
        if(Info == null)
        {
            return;
        }

        index = _index;

        if (initialize == false)
            return;

        gameData = Managers.Scene.CurrentScene as IGameData;
        var gameState = Managers.Scene.CurrentScene as IGameState;
        stateSub = gameState.OnStateObservable.Subscribe(state =>
        {
            if (state == Define.EGAME_STATE.RESULT)
            {
                isEnd = true;
            }
        }).AddTo(this);

        gameObject.SetActive(false);
       
        await SpawnEventObejct(eventType);

        gameObject.SetActive(true);
    }

    protected async UniTask SpawnEventObejct(Define.EVENT_TYPE eventType)
    {
        EventObject obj = Managers.Pool.PopEventObject(eventType);
        if(obj == null)
        {
            obj = await Managers.Pool.CreateEventObjectPool(eventType);
        }

        obj.transform.SetParent(this.gameObject.transform);
        obj.transform.position = transform.position;
        obj.gameObject.SetActive(true);
        eventObj = obj;
    }

    public EventSpot CopyInstance()
    {
        return new EventSpot()
        {
            Info = this.Info,
            index = this.index,
        };
    }

    private void Update()
    {
        if (isEnd || Info == null)
            return;

        curTime += Time.deltaTime;
        UpdateUI();

        if (isWork)
        {
            workTime += Time.deltaTime;
            if (workTime >= Info.needTime)
            {
                isWork = false;
                isClear.OnNext(true);
                isEnd = true;
                return;
            }
        }

        if (curTime >= Info.limitTime)
        {
            isWork = false;
            isClear.OnNext(false);
            isEnd = true;
            foreach (var unit in waitList)
            {
                unit.Clear();
            }
            return;
        }
    }

    public override void Enter(UnitLogic _unit)
    {
        if (!_unit.IsTarget(triggerType, index))
            return;

        if (IsWorkCondition())
        {
            if(AddWorkList(_unit))
            {
                isWork = true;
                foreach (var unit in waitList)
                {
                    unit.SetState(Define.EUNIT_STATE.Work);
                    Debug.Log($"unitID: {unit.UnitID} 일 시작");
                }
            }
        }
    }

    public bool IsWorkCondition()
    {
        return workUnitList.Count < Info.staffCount;
    }

    public bool AddWorkList(UnitLogic _unit)
    {
        workUnitList.Add(_unit.UnitID);
        _unit.SetState(Define.EUNIT_STATE.Wait);
        waitList.Add(_unit);
        return !IsWorkCondition();
    }

    public void Clear(bool init)
    {
        workUnitList.Clear();
        waitList.Clear();
        curTime = 0f;
        isWork = false;
        isEnd = false;

        if (init == false)
            return;

        if(stateSub != null)
        {
            stateSub.Dispose();
        }

        clearParticleObj.SetActive(false);
    }

    public long AddScore()
    {
        long score = Info.eventScore;
         score += (long)(Info.bonusScore * ((Info.limitTime - workTime) * 10d).DecimalRound(Define.DECIMALROUND.RoundDown,0));

        return score;
    }
    public override void OpenInfo(List<UnitLogic> units)
    {
        Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupSelectStaff, new PBSelectStaff
        {
            unitList = units,
            type = triggerType,
            maxUnitCount = Info.staffCount,
            index = index,
            limitTime = Info.limitTime,
            curTime = curTime,
            eventType = Info.eventType,
        });
    }

    public override List<int> GetUnitList()
    {
        return unitIds;
    }

    public void UpdateUI()
    {
        progressBar.fillAmount = Mathf.Clamp01((Info.limitTime - curTime) / Info.limitTime);
    }
}
