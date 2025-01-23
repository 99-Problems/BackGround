using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Triggers;
using UniRx;
using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using System.Linq;
using Unity.Linq;
using UnityEngine.EventSystems;
using MessagePack;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Playables;



#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IGameData
{
    InGamePlayInfo PlayInfo { get; set; }
    IObservable<bool?> OnLoadingComplete { get; }
    Action<float> OnFrameMove { set; get; }
    Define.ECONTENT_TYPE ContentType { get; set; }
}

public interface ICharacterManager
{
    void RefreshStaffs();
}

public interface IStageClearCondition
{
    bool IsStageEndCondition();
}

public interface IGameState
{
    IObservable<Define.EGAME_STATE> OnStateObservable { get; }

    IObservable<bool> MenuVisibleObservable();
    void SetMenuVisible(bool _b);
    bool GetMenuVisible();
    Define.EGAME_STATE GetGameState { get; }
    bool IsShowClearSequence { get; }
    void Result();

}



public class GameSceneInit : BaseScene, IGameData, ICharacterManager, IStageClearCondition, IGameState
{
    public static float playTime = 60f;
    public LevelManager level;
    [HideInInspector]
    public GameObject roomObj;
    public RoomContainer roomContainer;
    public GameObject eventObj;
    public List<EventController> events;

    private OneReplaySubject<bool?> loadingComplete = new OneReplaySubject<bool?>(null);
    private InGamePlayerInfo localPlayer;
    private InGamePlayInfo playInfo;
    [HideInInspector]
    public List<InGamePlayerInfo> gamePlayerInfo = new List<InGamePlayerInfo>();
    [SerializeField]
    private CameraController camController;

    InGamePlayInfo IGameData.PlayInfo
    {
        get => playInfo;
        set => playInfo = value;
    }

    #region 에디터 설정
#if UNITY_EDITOR
    private const string SpeedDebugger = "Menu/스피드토글";
    public static bool isSpeedDebugger => EditorPrefs.GetBool(SpeedDebugger);


    [MenuItem(SpeedDebugger)]
    private static void DebuggerToggle()
    {
        var isDebug = !isSpeedDebugger;
        Menu.SetChecked(SpeedDebugger, isDebug);
        EditorPrefs.SetBool(SpeedDebugger, isDebug);
        SceneView.RepaintAll();
    }
    private static Rect windowRect = new Rect(360, 70, 120, 115+ 30);

    private void OnGUI()
    {
        if (!isSpeedDebugger)
        {
            return;
        }

        var preRect = windowRect;
        windowRect = GUI.Window(0, windowRect, DebugWindow, "Debug");
        if (preRect != windowRect)
        {
            EditorPrefs.SetFloat("GameX", windowRect.x);
            EditorPrefs.SetFloat("GameY", windowRect.y);
        }
    }
#endif
    #endregion

    //IGameData
    public IObservable<bool?> OnLoadingComplete => loadingComplete;
    public Action<float> OnFrameMove { get; set; }
    public Define.ECONTENT_TYPE ContentType { get; set; }

    //IGameState
    [NonSerialized]
    public ReactiveProperty<Define.EGAME_STATE> gameState =
        new ReactiveProperty<Define.EGAME_STATE>(Define.EGAME_STATE.LOADING);

    Define.EGAME_STATE IGameState.GetGameState => gameState.Value;
    private BoolReactiveProperty menuVisible = new BoolReactiveProperty(false);
    private Define.EGAME_STATE prevState;


    public IObservable<Define.EGAME_STATE> OnStateObservable => gameState.AsObservable();

    public bool IsShowClearSequence => showEndSequence;


    Action endSequence;
    private bool showEndSequence;
    private bool isGameStart;


    void Start()
    {
        Loading();
    }

    protected override void Init()
    {
        base.Init();

        Managers.Scene.CurrentSceneType = Define.Scene.GameScene;
        ContentType = Define.ECONTENT_TYPE.INGAME;
    }
    private void Update()
    {
        if (Managers.Popup.IsShowPopup() || Managers.Popup.IsWaitPopup() || !loadingComplete.Value.HasValue || !isGameStart)
            return;

        #region EndPopup
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (Managers.Popup.IsShowSystemMenu())
            {
                Managers.Popup.ShowSystenMenu(false);
                return;
            }


            var gameData = Managers.Scene.CurrentScene as IGameData;
            if (gameData != null && gameData.ContentType.UseIngameExitBtn())
            {
                return;
            }

            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPause, new PBPause { stageLevel = UserInfo.stageLevel });
            //var pbYesNo = new PBYesNo
            //{
            //    strName = 26254,
            //    strDesc = Managers.String.GetString(26253),
            //    subjectYes = () => { Managers.Device.ApplicationQuit(); }
            //};
            //Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupYesNo, PopupArg.empty);
        }
        #endregion

        #region 이벤트 트리거 터치
        // 마우스 입력 또는 터치 입력 중 하나만 처리
        Vector3 inputPosition = Vector3.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;

            // UI 위에서 터치가 발생하면 무시
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(inputPosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    var trigger = hit.collider.GetComponent<BaseTrigger>();
                    if (trigger != null)
                    {
                        trigger.OpenInfo(localPlayer.listUnit);
                        Debug.ColorLog($"{trigger.name} 터치됨");

                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // UI 위에서 마우스 클릭이 발생하면 무시
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var trigger = hit.collider.GetComponent<BaseTrigger>();
                if (trigger != null)
                {
                    trigger.OpenInfo(localPlayer.listUnit);
                    Debug.ColorLog($"{trigger.name} 클릭됨");
                }
            }

            inputPosition = Input.mousePosition;
        }
        #endregion
    }



    public virtual async UniTaskVoid Loading()
    {
        gameObject.FixedUpdateAsObservable().Subscribe(_ =>
                {
                    FrameMove(Time.fixedDeltaTime);
                }).AddTo(this);

        #region 임시 유저 정보 넣기
        var tmpUnits = new List<UnitData>();
        var tmpAccount = 10001001;
        var maxID = 3;
        for (int i = 1; i <= maxID; i++)
        {
            tmpUnits.Add(new UnitData
            {
                AccountID = tmpAccount,
                UnitID = i,
                RoomIndedx = i,
            });
        }


        var tempLoginData = new LoginAccountData
        {
            AccountID = 10000001,
            units = tmpUnits,
            exp = 0,
            stageLevel = 1,
        };

        UserInfo.SetLoginData(tempLoginData);
        #endregion

        IngameLoadingImage.LoadingEvent.OnNext(10);

        await UniTask.WaitForEndOfFrame();
        if(camController == null)
        {
            var loadedCamera = await Managers.Resource.LoadAsyncGameObject("camera", "Camera.prefab");
            var camera = GameObject.Instantiate(loadedCamera);
            camController = camera.GetComponent<CameraController>();
            if(camController)
                camController.Init();
        }
        else
        {
            camController.Init();
        }

        if (!roomContainer)
        {
            roomObj = GameObject.Instantiate(await Managers.Resource.LoadAsyncGameObject("player/event", "RoomContainer.prefab"));
            roomContainer = roomObj.GetOrAddComponent<RoomContainer>();
        }
        await roomContainer.Init();

        IngameLoadingImage.LoadingEvent.OnNext(20);
        await UniTask.DelayFrame(30);
        var obj = new GameObject { name = "InGamePlayInfo" };
        var _playInfo = obj.AddComponent<InGamePlayInfo>();
        _playInfo.SetStageClearCondition(this);
        playInfo = _playInfo;
        IngameLoadingImage.LoadingEvent.OnNext(40);

        var player = new GameObject("Player1");
        localPlayer = player.AddComponent<InGamePlayerInfo>();
        localPlayer.GameData = this;
        localPlayer.playerData.accountID = UserInfo.GetAccountID();

        var units = UserInfo.Units;
        var unitList = new List<UnitLogic>();
        foreach (var unit in units)
        {
            var _unitInfo = Managers.Data.GetUnitInfo(unit.UnitID);
            if (_unitInfo == null)
            {
                Debug.LogError("No unitInfo");
                continue;
            }
            var unitInitialData = new UnitBaseData(_unitInfo);
            var unitLogic = localPlayer.GetUnitFromID(unit.UnitID);
            var room = roomContainer.GetRoomInfo(unit.RoomIndedx);
            var unitPos = room.transform.position + new Vector3(0, 5, 0);
            if(unitLogic == null)
            {
                unitLogic = await SpawnUnit(localPlayer, unitInitialData, unitPos, room.direction);
                unitLogic.gameObject.SetActive(false);
            }
            else
            {
                unitLogic.SetData(unitInitialData);
            }
            unitList.Add(unitLogic);

        }
        gamePlayerInfo.Add(localPlayer);
        localPlayer.Init(unitList);

        IngameLoadingImage.LoadingEvent.OnNext(50);

        if(eventObj)
        {
            foreach (var mit in eventObj.Children())
            {
                var controller = mit.GetComponent<EventController>();
                if (controller)
                    events.Add(controller);
            }
        }

        await UniTask.Delay(100);
        IngameLoadingImage.LoadingEvent.OnNext(60);
        var stageInfos = Managers.Data.GetStageInfoList(UserInfo.stageLevel);

        playInfo.Init(stageInfos, events, roomContainer, camController);
        playInfo.JoinPlayers(gamePlayerInfo);
        IngameLoadingImage.LoadingEvent.OnNext(70);

        SetEndSequence();

        Resources.UnloadUnusedAssets();
        await UniTask.WaitForEndOfFrame();
        SetGameState(Define.EGAME_STATE.LOADING_COMPLETE);
        if (bgm != null)
        {
            Managers.Sound.Play(bgm, Define.Sound.Bgm);
        }

        loadingComplete.OnNext(true);
        Managers.Input.isInteractable = true;
        IngameLoadingImage.LoadingEvent.OnNext(100);
        Managers.Popup.ShowReservationPopup();

        playInfo.SetStageReady();
    }

    private void FrameMove(float _delta)
    {
        if (!loadingComplete.Value.HasValue)
            return;

        if(Managers.Time.IsPause)
        {
            if (gameState.Value != Define.EGAME_STATE.PAUSE)
            {
                prevState = gameState.Value;
                SetGameState(Define.EGAME_STATE.PAUSE);
            }
            return;
        }

        switch (gameState.Value)
        {
            case Define.EGAME_STATE.LOADING:
                break;
            case Define.EGAME_STATE.LOADING_COMPLETE:
                SetGameState(Define.EGAME_STATE.ENTRY);
                break;
            case Define.EGAME_STATE.ENTRY:
                if(playInfo.playState.Value == InGamePlayInfo.EPLAY_STATE.PLAY)
                    SetGameState(Define.EGAME_STATE.ENTRY_COMPLETE);
                break;
            case Define.EGAME_STATE.PLAY:
                OnFrameMove?.Invoke(_delta);
                playInfo?.FrameMove(_delta);

                if (playInfo.IsStageEndCondition())
                {
                    SetGameState(Define.EGAME_STATE.RESULT);
                }
                break;
            case Define.EGAME_STATE.PAUSE:
                break;
            case Define.EGAME_STATE.RESULT:
                break;
            case Define.EGAME_STATE.COMMANDER:
                break;
            case Define.EGAME_STATE.MANAGE:
                break;
            case Define.EGAME_STATE.ENTRY_COMPLETE:
                SetGameState(Define.EGAME_STATE.PLAY);
                break;
            default:
                break;
        }
    }

    public void SetGameState(Define.EGAME_STATE _state)
    {
        switch (_state)
        {
            case Define.EGAME_STATE.LOADING:
                break;
            case Define.EGAME_STATE.LOADING_COMPLETE:
                break;
            case Define.EGAME_STATE.ENTRY:
                break;
            case Define.EGAME_STATE.PLAY:
                break;
            case Define.EGAME_STATE.PAUSE:
                SetGameState(prevState);
                break;
            case Define.EGAME_STATE.RESULT:
                playInfo?.EndGame();
                Result();
                break;
            case Define.EGAME_STATE.COMMANDER:
                break;
            case Define.EGAME_STATE.MANAGE:
                break;
            case Define.EGAME_STATE.ENTRY_COMPLETE:
                isGameStart = true;
                break;
            default:
                break;
        }

        gameState.Value = _state;
    }

    public override void Clear()
    {
    }

    public void RefreshStaffs()
    {

    }

    private static async UniTask<UnitLogic> SpawnUnit(InGamePlayerInfo _spawnPlayer, UnitBaseData _unitBaseData, Vector3 _pos, Define.EUNIT_DIRECTION direction = Define.EUNIT_DIRECTION.DOWN)
    {
        var _unitInfo = Managers.Data.GetUnitInfo(_unitBaseData.unitID);
#if UNITY_EDITOR
        if (_unitInfo == null)
        {
            Debug.LogError("Spawn Unit Failed " + _unitBaseData.unitID);
        }
#endif

        var clone = Managers.Pool.PopUnit(_unitInfo);
        if (clone == null)
        {
            clone = await Managers.Pool.CreateUnitPool(_unitInfo);
        }

        if (clone == null)
            return null;

        clone.transform.SetParent(_spawnPlayer.transform);
        clone.transform.position = _pos;
        clone.gameObject.SetActive(true);
        clone.RotateUnit(direction);
        var unitLogic = clone.GetComponent<UnitLogic>();
        unitLogic.Init(_unitBaseData, _spawnPlayer);

        return unitLogic;
    }

    public bool IsStageEndCondition()
    {
        return false;
    }

    public void Result()
    {
        endSequence?.Invoke();
        endSequence = null;
    }

    private void SetEndSequence()
    {
        if (endSequence == null)
        {
            endSequence = () =>
            {
                Managers.Popup.CloseAllPopupBox();
                OpenResultPopup();
#if LOG_ENABLE && UNITY_EDITOR
                string log = "";
                log += "GAME END".ToColor(SettingScriptableObject.Instance.CoralRed);

                if (playInfo.IsPlayerDie())
                {
                    log += " : 라이프 0으로 인한 사망".ToColor(SettingScriptableObject.Instance.CoralRed);
                }

                Debug.Log(log);
#endif
            };
        }





        void OpenResultPopup()
        {
            bool isClear = playInfo.playData.life > 0;

            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupInGameResult, new PBInGameResult()
            {
                isClear = isClear,
                record = playInfo.playData.score
            });
        }
    }

    public IObservable<bool> MenuVisibleObservable()
    {
        return menuVisible;
    }

    public void SetMenuVisible(bool _b)
    {
        menuVisible.Value = _b;
    }

    public bool GetMenuVisible()
    {
        return menuVisible.Value;
    }

    void OnApplicationPause(bool isPaused)
    {
        if (gameState.Value != Define.EGAME_STATE.PLAY || gameState.Value == Define.EGAME_STATE.PAUSE)
            return;

        if(!Managers.Time.IsPause && isPaused)
        {
            Debug.ColorLog($"퍼즈팝업 띄움 {isPaused}");
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPause, new PBPause
            { 
                stageLevel = UserInfo.stageLevel,
            });

        }
    }
}
