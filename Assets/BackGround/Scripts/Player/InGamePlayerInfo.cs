using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class InGamePlayerInfo : MonoBehaviour
{
    public readonly List<UnitLogic> listUnit = new List<UnitLogic>();

    public IGameData GameData { get; set; }
    public static Subject<InGamePlayerInfo> OnInitPlayer = new Subject<InGamePlayerInfo>();

    public struct PlayerData
    {
        public int playerIndex;
        public string nick;
        public long accountID;
    }
    public PlayerData playerData;

    [Button]
    public void MoveOnOff(bool _move)
    {
        foreach (var unit in listUnit)
        {
            unit.isMove = _move;
        }
    }
    [Button]
    public void SetRoomTarget(int index)
    {
        var sceneInit = GameData as GameSceneInit;
        if(sceneInit == null)
        {
            Debug.ColorLog("게임씬 못받음");
            return;
        }
        var transform = sceneInit.roomContainer.GetRoomInfo(index).transform;
        foreach (var unit in listUnit)
        {
            unit.target = transform;
            unit.state.Value = Data.Define.EUNIT_STATE.Move;
        }
    }

    public void Init(IEnumerable<UnitLogic> _listMyUnit)
    {
        listUnit.Clear();
        if (_listMyUnit != null)
        {
            foreach (var mit in _listMyUnit)
            {
                mit.Reset();
                if (!mit.gameObject.activeSelf)
                    mit.gameObject.SetActive(true);
            }

            listUnit.AddRange(_listMyUnit);
        }
    }

    public UnitLogic GetUnitFromID(int _mitUnitID)
    {
        for (var index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (mit.UnitID == _mitUnitID)
                return mit;
        }

        return null;
    }

    public void FrameMove(float _deltaTime)
    {
        for (int index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (!mit.gameObject.activeInHierarchy)
                continue;

            mit.FrameMove(_deltaTime);
        }
    }

    public void Entry()
    {

    }


    [Button]
    public void Test(string assetPath = "unit", string prefabName = "staff", int index = 1)
    {
        Managers.Pool.CreateUnitPool(assetPath, $"{prefabName}{index}");
    }
}
