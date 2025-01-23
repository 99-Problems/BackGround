using Data;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

[MessagePackObject]
public class LoginAccountData
{
    [Key(0)]
    public Int64 AccountID { get; set; }
    [Key(1)]
    public AccountInfo accountInfo { get; set; }
    [Key(2)]
    public List<UnitData> units { get; set; }
    [Key(3)]
    public long exp { get; set; }
    [Key(4)]
    public int stageLevel { get; set; }

    public LoginAccountData CopyInstance()
    {
        return new LoginAccountData()
        {
            AccountID = this.AccountID,
            accountInfo = this.accountInfo,
            units = this.units,
            exp = this.exp,
            stageLevel = stageLevel,
        };
    }

    public bool CompareKey(Int64 AccountID)
    {
        return this.AccountID == AccountID;
    }

    public bool CompareKey(LoginAccountData rdata)
    {
        return AccountID == rdata.AccountID;
    }
}
[MessagePackObject]
public class AccountInfo
{
    [Key(0)]
    public Int64 AccountID { get; set; }
    [Key(1)]
    public Int64 UserUID { get; set; }
    [Key(2)]
    public int Channel { get; set; }
    [Key(3)]
    public int ChannelGroup { get; set; }
    [Key(4)]
    public string NickName { get; set; }
    [Key(5)]
    public DateTime RegTime { get; set; }
    [Key(6)]
    public DateTime LogoutTime { get; set; }


    public AccountInfo CopyInstance()
    {
        return new AccountInfo()
        {
            AccountID = this.AccountID,
            UserUID = this.UserUID,
            Channel = this.Channel,
            ChannelGroup = this.ChannelGroup,
            NickName = this.NickName,
            RegTime = this.RegTime,
            LogoutTime = this.LogoutTime
        };
    }

    public bool CompareKey(Int64 AccountID)
    {
        return this.AccountID == AccountID;
    }

    public bool CompareKey(AccountInfo rdata)
    {
        return AccountID == rdata.AccountID;
    }
}

[MessagePackObject]
public class UnitData
{
    [Key(0)]
    public Int64 AccountID { get; set; }
    [Key(1)]
    public int UnitID { get; set; }
    [Key(2)]
    public int RoomIndedx { get; set; }

    public UnitData CreateAddUnitData()
    {
        return new UnitData()
        {
            AccountID = this.AccountID,
            UnitID = this.UnitID,
            RoomIndedx = this.RoomIndedx,
        };
    }


    public UnitData CopyInstance()
    {
        return new UnitData()
        {
            AccountID = this.AccountID,
            UnitID = this.UnitID,
            RoomIndedx = this.RoomIndedx,
        };
    }

    public bool CompareKey(Int64 AccountID, int UnitID)
    {
        return this.AccountID == AccountID
             && this.UnitID == UnitID;
    }

    public bool CompareKey(UnitData rdata)
    {
        return AccountID == rdata.AccountID
             && UnitID == rdata.UnitID;
    }
}



