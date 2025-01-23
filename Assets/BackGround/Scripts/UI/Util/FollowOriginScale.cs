using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowOriginScale : MonoBehaviour
{

    [Header("기본값을 1600,900 해상도로 설정")]
    public Vector2 originScreenSize = new Vector2(1600, 900);
    public RectTransform targetTrans;
    public void Init()
    {
        if (targetTrans == null)
            targetTrans = transform.parent.GetComponent<RectTransform>();
        if (targetTrans== null)
            return;
        var scaleIncrese = targetTrans.rect.width / originScreenSize.x;

        transform.localScale = new Vector3(scaleIncrese, scaleIncrese, scaleIncrese);
    }
}
