
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
public class StageInfoScript
{

    [Key(0)]public int stageLevel;
    [Key(1)]public float eventTime;
    [Key(2)]public Define.EVENT_TYPE eventType;
    [Key(3)]public int spawnIndex;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class StageInfoScriptAll
    {
        [Key(0)]public List<StageInfoScript> result;
    }



    private List<StageInfoScript> listStageInfoScript = null;


    public StageInfoScript GetStageInfoScript(Predicate<StageInfoScript> predicate)
    {
        return listStageInfoScript?.Find(predicate);
    }
    public List<StageInfoScript> GetStageInfoScriptList { 
        get { 
                return listStageInfoScript;
        }
    }



    void ClearStageInfo()
    {
        listStageInfoScript?.Clear();
    }


    async UniTask LoadScriptStageInfo()
    {
        List<StageInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/game", "StageInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("StageInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<StageInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listStageInfoScript = resultScript;


//#endif
    }
#if UNITY_EDITOR
    public static async UniTask ConvertBinaryStageInfo()
    {
        var path = "Assets/BackGround/Prefabs/scripts/game/StageInfo.json";
        var load = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
        var resultScript = JsonUtility.FromJson<StageInfoScriptAll>("{ \"result\" : " + load + "}");
        var convertBytes = MessagePackSerializer.Serialize(resultScript);
        var convertPath = "Assets/BackGround/Prefabs/scripts/game/StageInfo.bytes";
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


