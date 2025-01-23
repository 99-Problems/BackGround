using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using TMPro;
using DG.Tweening;

using System.Linq;
using Unity.Linq;
using Sirenix.OdinInspector;

public class ImageSwap : MonoBehaviour
{
    public List<Image> listImage;
    public float delayTime = 1.5f;
    public bool automatic = true;
    public Button prevBtn;
    public Button nextBtn;

    int index = 0;
    void Start()
    {
        if(prevBtn)prevBtn.OnClickAsObservable().Subscribe(_ =>
        {
            index = MovePage(index - 1);

        });
        if(nextBtn)nextBtn.OnClickAsObservable().Subscribe(_ =>
        {
            index = MovePage(index + 1);
        });
        if (automatic)
            StartCoroutine(Swap());
    }

    private IEnumerator Swap()
    {
        yield return new WaitForSeconds(delayTime);

        while (true)
        {
            index = MovePage(index + 1);
            yield return new WaitForSeconds(delayTime);
        }
    }

    private int MovePage(int _index)
    {
        if (listImage.Count <= _index)
            _index = 0;
        if (_index < 0)
        {
            _index = listImage.Count - 1;
        }
        listImage[_index].color = new Color(1, 1, 1, 0);
        listImage[_index].DOFade(1, 0.5f);
        listImage[_index].transform.SetAsLastSibling();
        return _index;
    }
}
