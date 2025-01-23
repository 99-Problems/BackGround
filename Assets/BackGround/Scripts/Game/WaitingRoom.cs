using Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRoom : BaseTrigger
{
    public int staffCount = 2;
   
    public enum RoomType
    {
        Default,
        Waiting,
    }

    public RoomType roomType = RoomType.Waiting;
    public Define.EUNIT_DIRECTION direction;

    [ShowInInspector]
    private List<int> unitList = new List<int>();
    public List<int> unitIds => unitList;

    public static int defaultIndex = -1;

    private void Start()
    {
        triggerType = Define.ETRIGGER_TYPE.WaitingRoom;
    }

    public override void Enter(UnitLogic _unit)
    {
        if (!_unit.IsTarget(triggerType, index))
            return;

        _unit.SetState(Define.EUNIT_STATE.Idle);
    }

    public override void OpenInfo(List<UnitLogic> units)
    {
        Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupSelectStaff, new PBSelectStaff
        {
            unitList = units,
            type = triggerType,
            maxUnitCount = staffCount,
            index = index,

        });
    }

    public override List<int> GetUnitList()
    {
        return unitIds;
    }
}