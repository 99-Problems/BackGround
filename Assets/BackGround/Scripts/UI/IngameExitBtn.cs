using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using UnityEngine.UI;
using Data;

public class IngameExitBtn : MonoBehaviour
{
    public Button exitBtn;

    private void Start()
    {
        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPause, new PBPause
            {
                stageLevel = UserInfo.stageLevel,
            });
        });
    }
}
