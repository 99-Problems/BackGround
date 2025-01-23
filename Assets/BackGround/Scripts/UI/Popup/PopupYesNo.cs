using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using Unity.Linq;

public class PBYesNo : PopupArg
{
    public string title;
    public string desc;
    public string rewardTitleText;
    public string yesBtnText;
    public string noBtnText;
    public string warningInfoText;

    public Action subjectYes;
    public Action subjectNo;
}

public class PopupYesNo : PopupBase
{
    public GameObject materialLayout;
    public Button okBtn;
    public Button cancelBtn;
    public GTMPro topTitle;
    public GTMPro panelDesc;

    public TMP_Text textRewardTitle;
    public TMP_Text textOkBtn;
    public TMP_Text textCancelBtn;
    public TMP_Text textWarning;
    public GameObject back;

    protected PBYesNo arg;

    public CanvasGroup titmeImage;
    RectTransform rect;
    CanvasGroup backImage;
    [NonSerialized]
    public bool clickYes = false;

    public virtual void Start()
    {
        rect = back.GetComponent<RectTransform>();
        backImage = back.GetComponent<CanvasGroup>();
        okBtn.OnClickAsObservable().Subscribe(_ =>
        {
            clickYes = true;
            Managers.Popup.ClosePopupBox(gameObject);
        });

        cancelBtn.OnClickAsObservable().Subscribe(_ =>
        {
            PressBackButton();
        });

        Initialization();
    }

    public virtual void Initialization()
    {
        if (topTitle != null)
            topTitle.SetText(arg.title);


        panelDesc.SetText(arg.desc);

        if (!arg.rewardTitleText.IsNullOrWhitespace())
        {
            textRewardTitle.text = arg.rewardTitleText;
        }

        if (!arg.noBtnText.IsNullOrWhitespace())
        {
            textCancelBtn.text = arg.noBtnText;
        }

        if (!arg.yesBtnText.IsNullOrWhitespace())
        {
            textOkBtn.text = arg.yesBtnText;
        }

        if (!arg.warningInfoText.IsNullOrWhitespace())
        {
            textWarning.text = arg.warningInfoText;
        }
    }
    public override void InitPopupbox(PopupArg popupData)
    {
        arg = (PBYesNo)popupData;
    }

    public override void OnClosePopup()
    {
        base.OnClosePopup();
        if (clickYes)
            arg.subjectYes?.Invoke();
        else
            arg.subjectNo?.Invoke();
    }

    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(gameObject);
    }
}
