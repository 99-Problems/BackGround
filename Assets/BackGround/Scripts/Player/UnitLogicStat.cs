using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
[System.Serializable]
public class UnitLogicStat
{
    public long atk;
    public long pierce;
    public long life;
    public long def;
    public long critical;
    public long critical_regist;
    public long atk_enhance;
    public long pierce_enhance;
    public long life_enhance;
    public long def_enhance;
    public long critical_enhance;
    public long critical_regist_enhance;
    public long damage_up;
    public long reduce_damage;
    public long take_reduce_damage;
    public long critical_damage_up;
    public long redeuce_critical_damage;
    public long atk_speed;
    public long move_speed;


    public void Clear()
    {
        atk = 0;
        pierce = 0;
        life = 0;
        def = 0;
        critical = 0;
        critical_regist = 0;
        atk_enhance = 0;
        pierce_enhance = 0;
        life_enhance = 0;
        def_enhance = 0;
        critical_enhance = 0;
        critical_regist_enhance = 0;
        damage_up = 0;
        reduce_damage = 0;
        take_reduce_damage = 0;
        critical_damage_up = 0;
        redeuce_critical_damage = 0;
        atk_speed = 0;
        move_speed = 0;
    }
    public void Set()
    {
        Clear();
    }

}
