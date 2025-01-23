using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using UnityEngine.UI;
using System;
using Data;
using Cysharp.Threading.Tasks;

public class PBPause : PopupArg
{
    public int stageLevel;

    public Action onClose;
}

public class PopupPause : PopupBase
{
    public Button resumeBtn;
    public Button settingBtn;
    public Button exitBtn;
    public Button restartBtn;
    public GTMPro stageText;

    public CanvasGroup canvasGroup;

    protected PBPause arg;

    private bool bLoad;

    private void Start()
    {
        resumeBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            PressBackButton();
        });

        settingBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            Managers.Time.Resume();
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupSetting, PopupArg.empty);
            await UniTask.WaitUntil(() => Managers.Popup.IsPopupActive(Define.EPOPUP_TYPE.PopupSetting));
            Managers.Time.Pause();
        });

        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            if (bLoad)
                return;
            Managers.Time.Resume();
            Loading();
        });

        restartBtn.OnClickAsObservableThrottleFirst().Subscribe(async _ =>
        {
            var gameState = Managers.Scene.CurrentScene as IGameState;
            gameState.SetMenuVisible(false);
            await UniTask.WaitForEndOfFrame();
            Managers.Popup.ClosePopupBox(this);

            Managers.Scene.LoadScene(Define.Scene.GameScene);
        });
    }

    private async UniTaskVoid Loading()
    {
        bLoad = true;
        canvasGroup.alpha = 0;
        Managers.Scene.LoadScene(Define.Scene.Lobby);
        await UniTask.WaitForEndOfFrame();
        await UniTask.WaitUntil(() => Managers.Scene.moveScene == false);
        Managers.Popup.ClosePopupBox(this);
        // ·Îºñ¾À ·Îµù
        IngameLoadingImage.LoadingEvent.OnNext(10);
        await UniTask.DelayFrame(100);
        IngameLoadingImage.LoadingEvent.OnNext(30);
        await UniTask.DelayFrame(50);
        IngameLoadingImage.LoadingEvent.OnNext(50);
        await UniTask.DelayFrame(50);
        IngameLoadingImage.LoadingEvent.OnNext(70);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        IngameLoadingImage.LoadingEvent.OnNext(100);

    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);
        arg = (PBPause)_popupData;

        if (arg == null)
            return;


        stageText.SetText(arg.stageLevel);

        Managers.Time.Pause();
    }

    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(this);
    }

    public override void OnClosePopup()
    {
        base.OnClosePopup();
        Managers.Time.Resume();
    }
}
