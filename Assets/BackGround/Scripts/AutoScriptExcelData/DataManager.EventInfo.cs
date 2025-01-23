
/********************************************************/
/*Auto Create File*/
/*Source : ExcelToJsonConvert*/
/********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MessagePack;
using UnityEngine;
using UniRx;
using Data;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable][MessagePackObject]
public class EventInfoScript
{

    [Key(0)]public Define.EVENT_TYPE eventType;
    [Key(1)]public float limitTime;
    [Key(2)]public float needTime;
    [Key(3)]public Int64 eventScore;
    [Key(4)]public Int64 bonusScore;
    [Key(5)]public int staffCount;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class EventInfoScriptAll
    {
        [Key(0)]public List<EventInfoScript> result;
    }



    private List<EventInfoScript> listEventInfoScript = null;


    public EventInfoScript GetEventInfoScript(Predicate<EventInfoScript> predicate)
    {
        return listEventInfoScript?.Find(predicate);
    }
    public List<EventInfoScript> GetEventInfoScriptList { 
        get { 
                return listEventInfoScript;
        }
    }



    void ClearEventInfo()
    {
        listEventInfoScript?.Clear();
    }


    async UniTask LoadScriptEventInfo()
    {
        List<EventInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/game", "EventInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("EventInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<EventInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listEventInfoScript = resultScript;


//#endif
    }
#if UNITY_EDITOR
    public static async UniTask ConvertBinaryEventInfo()
    {
        var path = "Assets/BackGround/Prefabs/scripts/game/EventInfo.json";
        var load = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
        var resultScript = JsonUtility.FromJson<EventInfoScriptAll>("{ \"result\" : " + load + "}");
        var convertBytes = MessagePackSerializer.Serialize(resultScript);
        var convertPath = "Assets/BackGround/Prefabs/scripts/game/EventInfo.bytes";
        File.WriteAllBytes(convertPath, convertBytes);
        try
        {
            UnityEngine.Windows.File.Delete(path);
        }
        catch
        {
        }
    }
#endif
}


