using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene, IGameData
{
    public InGamePlayInfo PlayInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IObservable<bool?> OnLoadingComplete => throw new NotImplementedException();

    public Action<float> OnFrameMove { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Define.ECONTENT_TYPE ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected override void Init()
    {
        base.Init();

        Managers.Scene.CurrentSceneType = Define.Scene.Lobby;
        ContentType = Define.ECONTENT_TYPE.LOBBY;
    }


    public override void Clear()
    {
    }
}
