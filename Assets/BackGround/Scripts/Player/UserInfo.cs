using Data;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

static public class UserInfo
{
    static long userUID;
    public static AccountInfo accountInfo;
    public static List<UnitData> Units { get; set; }

    public static UnitData GetUnitInfo(int _id) => Units.Find(_1 => _1.UnitID == _id);

    public static int MaxLife = 20;
    public static int Life = 15;
    public static int stageLevel;

    public static void SetLoginData(LoginAccountData _loginAccountData)
    {
        Units = _loginAccountData.units;
        accountInfo = _loginAccountData.accountInfo;
        stageLevel = _loginAccountData.stageLevel;
    }

    static public Int64 GetAccountID()
    {
        return accountInfo?.AccountID ?? 1;
    }
    public static AccountInfo GetAccountInfo()
    {
        return accountInfo;
    }

    static long syncTicks = 0;
    public static DateTime GetTime(Define.TimeType timeType = Define.TimeType.UTC)
    {
        switch (timeType)
        {
            case Define.TimeType.UTC:
                return DateTime.UtcNow;
            case Define.TimeType.Local:
                return DateTime.Now;
            case Define.TimeType.ServerUTC:
                return DateTime.UtcNow.AddTicks(syncTicks);
            default:
                return DateTime.UtcNow.AddTicks(syncTicks);
        }

    }

    #region client subjects
    public static Subject<Unit> OnChangeResult = new Subject<Unit>();
    #endregion

    public static int GetLife()
    {
        return Life;
    }
}
