using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Linq;
using System;
using UniRx.Triggers;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

public class DebugSceneInit : BaseScene
{
    [Button]
    public void DebugSceneLoad()
    {
        Managers.Scene.LoadScene(Define.Scene.Debug);
    }
    [Button]

    public override void Clear()
    {
       
    }
}
