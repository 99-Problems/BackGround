using Cysharp.Threading.Tasks;
using Data;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using System.Threading.Tasks;
using Unity.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

public class EventController : MonoBehaviour
{
    public Define.EVENT_TYPE eventType;
    public List<GameObject> eventSpots;

    public (GameObject, int spawnIndex) GetEventSpot(int index)
    {
        if (index < 0 || eventSpots.Count - 1 < index)
            return GetRandomEventSpot();

        return (eventSpots[index], index);
    }

    public (GameObject, int spawnIndex) GetRandomEventSpot()
    {
        var index = Random.Range(0, eventSpots.Count);
       
        return (eventSpots[index], index);
    }

    public async UniTask<(bool isSpawn, int index)> GetEmptySpace(int _index)
    {
        var indexes = InGamePlayInfo.GetEventSpotList.Where(_=> _.Info.eventType == eventType).Select(_1 => _1.GetIndex);
        if (!indexes.Contains(_index))
            return (false, _index);

        //var randIndex = 0;

        //do
        //{
        //    randIndex = Random.Range(0, eventSpots.Count);
        //}
        //while (indexes.Contains(randIndex));

        return (true, _index);
    }

    public int GetEmptySpaceIndex()
    {
        var indexes = InGamePlayInfo.GetEventSpotList.Where(_ => _.Info.eventType == eventType).Select(_1 => _1.GetIndex);
        var randIndex = 0;

        do
        {
            randIndex = Random.Range(0, eventSpots.Count);
        }
        while (indexes.Contains(randIndex));

        return randIndex;
    }

    public bool IsEmptySpace(int _index)
    {
        var indexes = InGamePlayInfo.GetEventSpotList.Where(_ => _.Info.eventType == eventType).Select(_1 => _1.GetIndex);
        return !indexes.Contains(_index);
    }

    public bool IsFullSpace()
    {
        var indexes = InGamePlayInfo.GetEventSpotList.Where(_ => _.Info.eventType == eventType).Select(_1 => _1.GetIndex);
        var randIndex = 0;
        var randList = new List<int>();
        bool isContains = true;
        do
        {
            randIndex = Random.Range(0, eventSpots.Count);
            isContains = indexes.Contains(randIndex);
            if (isContains && randList.Contains(randIndex) == false)
            {
                randList.Add(randIndex);
            }
        }
        while (isContains && (indexes.Count() > 0 && randList.Count() != indexes.Count()));

        return isContains && indexes.Count() == randList.Count();
    }


    public async UniTask<EventSpot> EventStart(int index)
    {
        var spot = GetEventSpot(index);
        var spawnInfo = await GetEmptySpace(spot.spawnIndex);
        
        if(spawnInfo.isSpawn)
        {
            Debug.Log($"이미 존재하는 위치라 대기열 추가 eventType: {eventType} index :{spot.spawnIndex}".ToColor(Color.red));
            // 대충 리스트에 넣기

            var eventInfo = new EventSpot();
            await eventInfo.Init(eventType, spot.spawnIndex, false);
            Managers.Pool.EventStart(this, eventInfo);
            return null;
            //Debug.Log($"이미 존재하는 위치라 랜덤지정 {spot.spawnIndex} -> {spawnInfo.index}".ToColor(Color.red));
        }
        

        var clone = Managers.Pool.PopTrigger(eventType);

        if (clone == null)
        {
            clone = await Managers.Pool.CreateEventSpotPool(eventType);
        }
        if (clone == null)
            return null;

        clone.transform.SetParent(spot.Item1.transform);
        clone.transform.position = spot.Item1.transform.position;
        clone.gameObject.SetActive(true);
        var eventSpot = clone.GetComponent<EventSpot>();
        eventSpot.Init(eventType, spot.spawnIndex);
        Debug.Log("Event Start", Color.green);


        var warnClone = Managers.Pool.PopEventGuideIcon();
        if (warnClone == null)
        {
            warnClone = await Managers.Pool.CreateEventGuideIconPool();
        }
        if (warnClone == null)
            return null;
        warnClone.transform.SetParent(UICanvas.MasterCanvas.safeArea.transform);
        warnClone.transform.localScale = Vector3.one;
        var eventGuideIconUI = warnClone.GetComponent<EventGuideIconUI>();
        eventGuideIconUI.Init(clone);

        return eventSpot;
    }

#if UNITY_EDITOR
    [Button(Name = "스폰 지점 생성")]
    public void SpawnEventSpot()
    {
        var item = new GameObject();
        item.name = $"spawnPoint {eventSpots.Count}";
        item.transform.SetParent(this.gameObject.transform);
        item.transform.position = eventSpots.LastOrDefault()?.transform.position ?? transform.position;
        eventSpots.Add(item);

        SceneView.RepaintAll();
    }

    [Button(Name = "마지막 스폰 지점 삭제")]
    public void DeleteEventSpot()
    {
        if(eventSpots.IsNullOrEmpty())
            return;

        var item = eventSpots.LastOrDefault();
        eventSpots.Remove(item);

        if (Application.isPlaying)
        {
            Destroy(item);
        }
        else
        {
            DestroyImmediate(item);
        }

        SceneView.RepaintAll();
    }
#endif

    public virtual void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Managers.isDrawGizmos == false)
            return;
#endif
        foreach (var item in eventSpots)
        {
            if(item == null) continue;

            var color = Color.yellow;
            switch (eventType)
            {
                case Define.EVENT_TYPE.NONE:
                    break;
                case Define.EVENT_TYPE.Ball:
                    color = Color.yellow;
                    break;
                case Define.EVENT_TYPE.Patient:
                    color = Color.red;
                    break;
                default:
                    color = Color.yellow;
                    break;
            }
            
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawSphere(item.transform.position, 5f);

        }
    }
}
