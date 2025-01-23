using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using UnityEngine.UI;

public class PopupSetting : PopupBase
{
    public Button exitBtn;

    private void Start()
    {
        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            PressBackButton();
        });
    }
    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(this);
    }
}
