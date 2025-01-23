using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;

public struct IngamePlayData
{
    public int stageLevel;
    public long score;
    public long gold;
    public float currentTime;
    public float limitTime;
    public int life;
}

public class InGamePlayInfo : MonoBehaviour
{
    public enum EPLAY_STATE
    {
        SORT,
        READY,
        PLAY,
        PAUSE,
        STOP,
        END,
    }
    public ReactiveProperty<EPLAY_STATE> playState = new ReactiveProperty<EPLAY_STATE>(EPLAY_STATE.SORT);

    protected IGameData gameData;
    private GameObject particleParentObj;
    private IStageClearCondition condition;
    [HideInInspector]
    public List<InGamePlayerInfo> listPlayer = new List<InGamePlayerInfo>();

    private List<StageInfoScript> stageInfo = new List<StageInfoScript>();
    public List<EventController> events = new List<EventController>();
    private RoomContainer roomInfo;
    private CameraController cam;

    public bool isLastEvent { get; private set; } = false;

    private StageInfoScript curStageInfo;
    private StageInfoScript lastInfo;

    public IngamePlayData playData;
    private bool isStop;
    private bool isEventPlaying;


    public static Subject<(Define.ETRIGGER_TYPE type,int index, UnitLogic unit, Define.EVENT_TYPE eventType)> OnUnitSelect = new Subject<(Define.ETRIGGER_TYPE, int, UnitLogic unit, Define.EVENT_TYPE eventType)>();
    public static Subject<(Define.ETRIGGER_TYPE type, int index, UnitLogic unit, Define.EVENT_TYPE eventType)> OnUnitDeSelect = new Subject<(Define.ETRIGGER_TYPE, int, UnitLogic unit, Define.EVENT_TYPE eventType)>();
    public static Subject<(Define.EVENT_TYPE eventType, int index)> OnExitEvent = new Subject<(Define.EVENT_TYPE eventType, int index)>();
    public static Subject<Vector3> OnMoveCam = new Subject<Vector3>();
    public static Subject<int> OnHpChange = new Subject<int>();

    private static List<EventSpot> eventList = new List<EventSpot>();
    public static List<EventSpot> GetEventSpotList => eventList;

    private void Start()
    {
        gameData = Managers.Scene.CurrentScene as IGameData;

        gameData.OnLoadingComplete.Subscribe(_ =>
        {

        }).AddTo(this);

        OnUnitSelect.Subscribe(_ =>
        {
            if(_.type == Define.ETRIGGER_TYPE.WaitingRoom)
            {
                var room = roomInfo.GetRoomInfo(_.index);
                _.unit.SetDestination(_.type, _.index);
                _.unit.SetTarget(room.transform);
                _.unit.SetState(Define.EUNIT_STATE.Move);
            }
            else if (_.type == Define.ETRIGGER_TYPE.EventSpot)
            {
                var controller = events.Find(_1 => _1.eventType == _.eventType);
                var _event = controller.GetEventSpot(_.index);
                _.unit.SetDestination(_.type, _.index);
                _.unit.SetTarget(_event.Item1.transform);
                _.unit.SetState(Define.EUNIT_STATE.Move);
                _.unit.curEventType = _.eventType;
            }
        }).AddTo(this);

        OnUnitDeSelect.Subscribe(_ =>
        {
            if (_.type == Define.ETRIGGER_TYPE.WaitingRoom)
            {
                _.unit.SetState(Define.EUNIT_STATE.Idle);
            }
            else if (_.type == Define.ETRIGGER_TYPE.EventSpot)
            {
               _.unit.SetState(Define.EUNIT_STATE.Idle);
            }
        }).AddTo(this);

        OnExitEvent.Subscribe(_ =>
        {
            foreach (var player in listPlayer)
            {

                foreach (var unit in player.listUnit)
                {
                    if (unit.curEventType == _.eventType && unit.destination.index == _.index)
                    {
                        unit.Clear();
                    }
                }
                
            }
        }).AddTo(this);

        OnMoveCam.Subscribe(pos =>
        {
            if (cam.Camera == null)
                return;

            var position = pos;
            position.y = cam.Camera.transform.position.y;
            if (cam.Camera.draglockZ)
                position.z = cam.Camera.transform.position.z;
            cam.Camera.Move(position);
        }).AddTo(this);

        OnHpChange.Subscribe(calcHp =>
        {
            playData.life += calcHp;
        }).AddTo(this);
    }

    public void Init(List<StageInfoScript> stageInfos, List<EventController> _events, RoomContainer _roomContainer, CameraController _cam)
    {
        playData.stageLevel = UserInfo.stageLevel;
        stageInfo = stageInfos;
        curStageInfo = stageInfo.FirstOrDefault();
        lastInfo = stageInfo.LastOrDefault();
        playData.limitTime = GameSceneInit.playTime;
        playData.life = UserInfo.GetLife();
        events = _events;
        roomInfo = _roomContainer;
        cam = _cam;
        Clear();
    }

    public virtual void FrameMove(float _deltaTime)
    {
        if(playState.Value != EPLAY_STATE.END)
        {
            if (IsEndCondition() || (condition != null && condition.IsStageEndCondition()))
            {
                playState.Value = EPLAY_STATE.END;
                Managers.Time.SetGameSpeed(1);
                Debug.ColorLog("스테이지 종료", SettingScriptableObject.Instance.CoralRed);
                return;
            }
        }
        switch (playState.Value)
        {
            case EPLAY_STATE.SORT:
                return;
            case EPLAY_STATE.READY:
                break;
            case EPLAY_STATE.PLAY: 
                playData.currentTime = Math.Min(playData.currentTime + _deltaTime, playData.limitTime);
                if(!isLastEvent && !isEventPlaying)
                {
                    if(playData.currentTime >= curStageInfo.eventTime)
                    {
                        isEventPlaying = true;
                        SpawnEventSpot(curStageInfo);
                        
                    }
                }
                break;
            case EPLAY_STATE.PAUSE:
            case EPLAY_STATE.STOP:
                break;
            case EPLAY_STATE.END:
                return;
            default:
                break;
        }

        foreach (var mit in listPlayer) 
        {
            mit.FrameMove(_deltaTime);
        }
    }

    public void EndGame()
    {
        foreach (var player in listPlayer)
        {
            foreach (var unit in player.listUnit)
            {
                unit.Clear();
            }
        }
    }
    public void Clear()
    {
        eventList.Clear();
    }

    protected async UniTask SpawnEventSpot(StageInfoScript eventInfo)
    {
        var eventController = events.Find(_ => _.eventType == eventInfo.eventType);
        if (eventController == null)
        {
            Debug.LogError("이벤트 정보 없음");
            return;
        }
        EventSpot _event = await eventController.EventStart(eventInfo.spawnIndex);
        if (_event != null)
        {
            eventList.Add(_event);
        }

        var nextScript = stageInfo.Find(_ => _.eventTime > curStageInfo.eventTime);
        curStageInfo = nextScript != null ? nextScript : curStageInfo;

        if (curStageInfo == lastInfo)
        {
            isLastEvent = true;
            Debug.ColorLog("스테이지 스폰 종료", SettingScriptableObject.Instance.CoralRed);
        }
        isEventPlaying = false;
    }

    public static void AddWaitEvent(EventSpot _event)
    {
        eventList.Add(_event);
    }

    internal bool IsStageEndCondition()
    {
        return playState.Value == EPLAY_STATE.END;
    }

    public void SetStageClearCondition(IStageClearCondition _condition)
    {
        condition = _condition;
    }

    public void JoinPlayers(IEnumerable<InGamePlayerInfo> players)
    {
        foreach (var mit in players)
        {
            mit.transform.SetParent(gameObject.transform);
            mit.Entry();
            listPlayer.Add(mit);
        }
    }

    public async UniTask SetStageReady()
    {
        await UniTask.WaitUntil(() => IngameLoadingImage.instance != null ? IngameLoadingImage.instance.isloadingComplete : true);
        playState.Value = EPLAY_STATE.READY;
        bool isReady = false;
        Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupCountDown, new PBCountDown
        {
            limitTime = 3f,
            onClose = () =>
            {
                Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPush, new PBPush
                {
                    strDesc = "start",
                    pushTime = 1f,
                    isTextAni = true,
                    
                }, false);
                isReady = true;
            },
        }, false);
        await UniTask.WaitUntil(() => isReady);
        StageStart();
    }
    public void StageStart()
    {
        playState.Value = EPLAY_STATE.PLAY;
        

        Debug.ColorLog("게임 시작", Color.green);
    }

    protected virtual bool IsEndCondition()
    {
#if UNITY_EDITOR
        if (Managers.isinfinityMode) return false;
#endif
        return IsPlayerDie() || playData.currentTime >= playData.limitTime || isStop;
    }

    internal bool IsPlayerDie()
    {
        return playData.life <= 0;
    }

    public static bool IsExistEventSpot(int index)
    {
        return eventList.Find(_ => _.GetIndex == index) != null;
    }

    public static void RemoveEventSpot(EventSpot _event)
    {
        eventList.Remove(_event);
    }
}
