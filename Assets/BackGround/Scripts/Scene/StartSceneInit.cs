using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Data;

public class StartSceneInit : MonoBehaviour
{
    public Button startButton;
    public Button settingBtn;
    public Button exitBtn;

    private void Start()
    {
        if(startButton)
        {
            startButton.OnClickAsObservableThrottleFirst().Subscribe(_ =>
            {
                //SceneManager.LoadScene("GameScene");
                Managers.Scene.LoadScene(Define.Scene.GameScene);
                startButton.interactable = false;

            }).AddTo(this);
        }

        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }).AddTo(this);

        settingBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupSetting, PopupArg.empty);
        }).AddTo(this);
    }
}
