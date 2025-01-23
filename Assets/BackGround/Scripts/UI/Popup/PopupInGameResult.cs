using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using Data;
using System;

public class PopupInGameResult : PopupBase
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI record;
    [SerializeField] private TextMeshProUGUI reward;
    [SerializeField] private Button exit;
    [SerializeField] private Button home;
    [SerializeField] private Button restart;
    [SerializeField] private Button scoreAnimationStopper; 
    private PBInGameResult resultPopupArg;
    private ScoreAnimationUI recordScoreAnimation;

    private bool bLoad;



    private new void Awake()
    {
#if UNITY_EDITOR
        base.Awake();
#endif
        exit.OnClickAsObservableThrottleFirst().Merge(home.OnClickAsObservableThrottleFirst()).Subscribe((unit) =>
        {
            if (bLoad)
                return;

            CompleteLoading();
        });

        restart.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            RestartGame();
        });

        record.TryGetComponent(out recordScoreAnimation);
        scoreAnimationStopper.OnClickAsObservableThrottleFirst().Subscribe(StopScoreAnimation);
    }



    private void StopScoreAnimation(Unit unit)
    {
        recordScoreAnimation.SetCurrentScore(recordScoreAnimation.targetScore);

        if (scoreAnimationStopper != null)
        {
            Destroy(scoreAnimationStopper.gameObject);
        }
    }
    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(this);
    }
    private async void CompleteLoading()
    {
        bLoad = true;
   
        Managers.Popup.ClosePopupBox(this);
        await UniTask.WaitForEndOfFrame();

        Managers.Scene.LoadScene(Define.Scene.Lobby);
        var gameState = Managers.Scene.CurrentScene as IGameState;
        if(gameState != null)
            gameState.SetMenuVisible(false);
        await UniTask.WaitUntil(() => Managers.Scene.moveScene == false);
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

    private async UniTaskVoid RestartGame()
    {
        Managers.Popup.ClosePopupBox(this);
        var gameState = Managers.Scene.CurrentScene as IGameState;
        gameState.SetMenuVisible(false);
        await UniTask.WaitForEndOfFrame();

        Managers.Scene.LoadScene(Define.Scene.GameScene);
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);
        resultPopupArg = _popupData as PBInGameResult;
        SetTextByResult();
        StopAutomatically();
    }
    private void SetTextByResult()
    {
        string clearText = "STAGE CLEAR!";
        string gameOverText = "GAME OVER";

        if (resultPopupArg.isClear)
        {
            title.text = clearText;
            title.color = Color.white;
        }
        else
        {
            title.text = gameOverText;
            title.color = Color.red;
        }
        recordScoreAnimation.SetTargetScore(resultPopupArg.record);
        reward.text = resultPopupArg.reward.ToString("N0");
    }
    private async void StopAutomatically()
    {
        if (recordScoreAnimation.targetScore > 0)
        {
            await UniTask.WaitWhile(() => IsScoreAnimationPlaying() == false);
            await UniTask.WaitWhile(() => IsScoreAnimationPlaying());
        }

        await UniTask.WaitForEndOfFrame();
        StopScoreAnimation(default);





        bool IsScoreAnimationPlaying()
        {
            return recordScoreAnimation.currentScore != recordScoreAnimation.targetScore;
        }
    }
}





public class PBInGameResult : PopupArg
{
    public bool isClear = false;
    public long record = 0;
    public int reward = 0;
}