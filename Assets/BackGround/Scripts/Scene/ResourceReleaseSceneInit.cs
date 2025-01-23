using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using Cysharp.Threading.Tasks;

public class ResourceReleaseSceneInit : MonoBehaviour
{
    public static Subject<Unit> releaseComplete = new Subject<Unit>();
    private void Start()
    {
        Release();
    }
    private async void Release()
    {
        await UniTask.DelayFrame(1);
        await Resources.UnloadUnusedAssets();
        Managers.Resource.ReleaseAllAssets();

        await UniTask.DelayFrame(1);
        releaseComplete.OnNext(Unit.Default);
    }
}
