using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ImagePreserveAspectScaler : MonoBehaviour
{
    void Start()
    {
        Managers.Device.OnChangeScreenSize.Subscribe(_1 => { test(); }).AddTo(this);
        test();
    }

    [Button]
    public void test()
    {
        var image = GetComponent<Image>();
        if (image == null || image.sprite == null)
        {
            // GameObject.Destroy(gameObject);
            return;
        }

        image.preserveAspect = true;
        var rectTransform = image.rectTransform;
        // Vector2 canvasSize = Managers.Device.canvasSize;

        var canvasSize = Managers.Device.canvasSize;
        var textureRect = image.sprite.rect;
        float scaleSize = canvasSize.x / textureRect.width;
        var divid = canvasSize.y / textureRect.height;
        if (scaleSize < divid)
        {
            scaleSize = divid;
        }

        if (scaleSize != 0)
        {
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(textureRect.width * scaleSize, textureRect.height * scaleSize);
        }
    }
}