using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Data;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PoolManager : MonoBehaviour
{
    #region Pool

    class Pool<T> where T : MonoBehaviour
    {
        public T Original { get; private set; }
        public Transform Root { get; set; }
        public bool isDebug;
        Queue<T> _poolStack = new Queue<T>();

        public void Init(T original, string poolName, int count = 5)
        {
            Original = original;
            Original.gameObject.SetActive(false);
            Original.name = poolName + "_Origin";
            Root = new GameObject().transform;
            Root.gameObject.SetActive(false);
            Root.name = poolName;

            Original.transform.SetParent(Root);
            for (int i = 0; i < count; i++)
                Push(Create());
        }

        T Create()
        {
            GameObject go = GameObject.Instantiate(Original.gameObject);
            go.name = Root.name;
            return go.GetComponent<T>();
        }

        public void Push(T poolable)
        {
            if (poolable == null)
                return;
            poolable.transform.SetParent(Root);
            if (Original != null)
            {
                poolable.transform.localScale = Original.transform.localScale;
            }

            poolable.gameObject.SetActive(false);
            _poolStack.Enqueue(poolable);
        }

        public T Pop(Transform parent)
        {
            T poolable = null;
            int cnt = 0;
            while (true)
            {
                if (Original == null)
                    break;
                if (_poolStack.Count > 0)
                    poolable = _poolStack.Dequeue();
                else
                {
                    poolable = Create();
                    if (poolable == null)
                        return null;
                }

                if (poolable != null)
                    break;
            }

            if (poolable == null)
                return null;

            // DontDestroyOnLoad 해제 용도
            if (parent == null)
            {
                parent = Managers.Scene.CurrentScene.transform;
            }

            poolable.transform.parent = parent;

            poolable.gameObject.SetActive(true);

            return poolable;
        }

        public int GetCount()
        {
            return _poolStack.Count;
        }
    }
    #endregion

    Dictionary<string, Pool<UnitLogic>> unitLogicPool = new Dictionary<string, Pool<UnitLogic>>();
    Pool<EventSpot> triggerPool = new Pool<EventSpot>();
    Pool<EventGuideIconUI> guideIconUIPool =  new Pool<EventGuideIconUI>();
    Dictionary<Define.EVENT_TYPE, Pool<EventObject>> eventObjectPool = new Dictionary<Define.EVENT_TYPE, Pool<EventObject>>();
    private ParticlePool particlePool = new ParticlePool();

    Transform _root;

    public class EventWaitData
    {
        public EventController controller;
        public EventSpot spot;
    }
    private List<EventWaitData> waitActiveEvent = new List<EventWaitData>();

    public async UniTask<bool> SpawnWaitEvent(EventWaitData data)
    {
        
        var eventInfo = InGamePlayInfo.GetEventSpotList?.Find(_ => _.GetIndex == data.spot.GetIndex && _.Info.eventType == data.spot.Info.eventType);
        if(eventInfo == null)
        {
            if(data.controller.IsEmptySpace(data.spot.GetIndex))
            {
                var _event = await data.controller.EventStart(data.spot.GetIndex);
                InGamePlayInfo.AddWaitEvent(_event);
                Debug.ColorLog($"대기된 Index : {data.spot.GetIndex} 생성");
            }
            else if(data.controller.IsFullSpace())
            {
                return false;
            }
            else
            {
                var randIndex = data.controller.GetEmptySpaceIndex();
                var _event = await data.controller.EventStart(randIndex);
                InGamePlayInfo.AddWaitEvent(_event);
                Debug.ColorLog($"Index : {data.spot.GetIndex} -> {randIndex} 로 대체 생성");
            }
            spawnEventLock = true;
            return true;
        }
        else if (data.controller.IsFullSpace())
        {
            return false;
        }
        else
        {
            var randIndex = data.controller.GetEmptySpaceIndex();
            var _event = await data.controller.EventStart(randIndex);
            InGamePlayInfo.AddWaitEvent(_event);
            Debug.ColorLog($"Index : {data.spot.GetIndex} -> {randIndex} 로 대체 생성");
            spawnEventLock = true;
            return true;
        }
    }

    public void EventStart(EventController _controller, EventSpot _event)
    {
        var data = new EventWaitData 
        {
            controller = _controller,
            spot = _event,
        };

        waitActiveEvent.Add(data);
    }
    private bool spawnEventLock = false;
    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
            Object.DontDestroyOnLoad(_root);
        }

        MainThreadDispatcher.UpdateAsObservable().Where(_=> spawnEventLock == false).Subscribe(async _ =>
        {
            var gameState = Managers.Scene.CurrentScene as IGameState;
            if (gameState == null || gameState.GetGameState != Define.EGAME_STATE.PLAY || Managers.Time.IsPause)
                return;

            if (waitActiveEvent.Count > 0)
            {
                var removeData = new EventWaitData();
                bool isRemove = false;

                foreach (var _data in waitActiveEvent)
                {
                    if(await SpawnWaitEvent(_data))
                    {
                        isRemove = true;
                        removeData =_data;
                        Debug.Log("대기 이벤트 스폰", Color.green);
                        break;
                    }
                }

                if(isRemove)
                {
                    waitActiveEvent.Remove(removeData);
                    spawnEventLock = false;
                }
            }
        });
    }

    public void Clear()
    {
        waitActiveEvent.Clear();
    }

    public UnitLogic PopUnit(UnitInfoScript unitInfo)
    {
        if (unitInfo == null)
            return null;

        return PopUnit(unitInfo.assetPath, unitInfo.prefabName);
    }

    public UnitLogic PopUnit(string assetPath, string prefabName)
    {
        var key = ZString.Concat(assetPath, "/", prefabName);
        if (unitLogicPool.TryGetValue(key, out var pool))
        {
            var unitLogic = pool.Pop(null);
            unitLogic.gameObject.SetActive(false);
            return unitLogic;
        }

        //풀이 없음
        return null;
    }

    public async UniTask<UnitLogic> CreateUnitPool(UnitInfoScript unitInfo)
    {
        if (unitInfo == null)
            return null;

        return await CreateUnitPool(unitInfo.assetPath, unitInfo.prefabName);
    }

    public async UniTask<UnitLogic> CreateUnitPool(string assetPath, string prefabName)
    {
        var key = ZString.Concat(assetPath, "/", prefabName);
        var loadedUnit = await Managers.Resource.LoadAsyncGameObject(
            $"player/{assetPath}",
            $"{prefabName}.prefab");
        if (loadedUnit == null)
        {
            Debug.LogWarning(key);
            return null;
        }

        if (Managers.Scene.moveScene)
        {
            return null;
        }

        if (unitLogicPool.TryGetValue(key, out var _logic))
            return _logic.Pop(null);

        var clone = GameObject.Instantiate(loadedUnit).GetComponent<UnitLogic>();
        clone.gameObject.SetActive(false);

        List<GameObject> removeClone = new List<GameObject>();
        {
            var origin = loadedUnit.GetComponent<UnitLogic>();
        }
        await UniTask.DelayFrame(5);
        removeClone.Destroy(true);

        if (unitLogicPool.TryGetValue(key, out var pool))
        {
            return clone;
        }

        var newPool = new Pool<UnitLogic>();
        newPool.Init(clone, prefabName, 1);
        newPool.Root.transform.SetParent(_root);

        unitLogicPool[key] = newPool;
        return newPool.Pop(null);
    }

    public void PushUnit(UnitLogic _logic)
    {
        PushUnit(_logic.assetPath, _logic.prefabName, _logic);
    }

    public void PushUnit(string assetPath, string prefabName, UnitLogic _logic)
    {
        if (_logic == null) return;

        if (!(assetPath.IsNullOrEmpty() || prefabName.IsNullOrEmpty()))
        {
            var key = ZString.Concat(assetPath, "/", prefabName);
            if (unitLogicPool.TryGetValue(key, out var pool))
            {
                //if (_logic.navMesh)
                //    _logic.navMesh.enabled = false;
                _logic.transform.position = new Vector3(9999, 9999, 9999);
                pool.Push(_logic);
                return;
            }
        }

        GameObject.Destroy(_logic.gameObject);
    }

    public void PushTrigger(EventSpot trigger)
    {
        if (trigger == null) return;

        triggerPool.Push(trigger);

        //GameObject.Destroy(trigger.gameObject);
    }


    public EventSpot PopTrigger(Define.EVENT_TYPE eventType)
    {
        var trigger = triggerPool.Pop(null);
        if(trigger == null) return null;

        return trigger;
    }

    public async UniTask<EventSpot> CreateEventSpotPool(Define.EVENT_TYPE eventType)
    {
        var _prefab = await Managers.Resource.LoadAsync<GameObject>("player/room", "EventSpot.prefab");
        var clone = GameObject.Instantiate(_prefab).GetComponent<EventSpot>();
        if (triggerPool.Original != null)
        {
            return clone;
        }
        triggerPool.Init(clone.GetComponent<EventSpot>(), "eventSpot", 10);
        triggerPool.Root.transform.SetParent(_root);
        return triggerPool.Pop(null);
    }

    public async UniTask PushEventObject(EventObject obj)
    {
        if (obj == null) return;

        var key = obj.eventType;
        if (eventObjectPool.TryGetValue(key, out var pool))
        {
            pool.Push(obj);
            return;
        }
    }

    public EventObject PopEventObject(Define.EVENT_TYPE eventType)
    {
        var key = eventType;
        if (eventObjectPool.TryGetValue(key, out var pool))
        {
            var obj = pool.Pop(null);
            obj.gameObject.SetActive(false);
            return obj;
        }
        
        return null;
    }

    public async UniTask<EventObject> CreateEventObjectPool(Define.EVENT_TYPE eventType)
    {
        var key = eventType;
        var _eventObj = await Managers.Resource.LoadAsyncGameObject("player/event", $"Event_{eventType}.prefab");
        if (_eventObj == null)
        {
            Debug.LogWarning(key);
            return null;
        }

        if (eventObjectPool.TryGetValue(key, out var _ret))
        {
            return _ret.Pop(null);
        }

        var clone = GameObject.Instantiate(_eventObj).GetOrAddComponent<EventObject>();
        clone.eventType = eventType;
        clone.gameObject.SetActive(false);

        List<GameObject> removeClone = new List<GameObject>();
        {
            var origin = _eventObj.GetComponent<EventObject>();
        }
        await UniTask.DelayFrame(5);
        removeClone.Destroy(true);

        if (eventObjectPool.TryGetValue(key, out var pool))
        {
            return clone;
        }

        var newPool = new Pool<EventObject>();
        newPool.Init(clone, $"{eventType.ToString()}_Object", 1);
        newPool.Root.transform.SetParent(_root);

        eventObjectPool[key] = newPool;
        return newPool.Pop(null);
    }

    public void PushEventGuideIcon(EventGuideIconUI icon)
    {
        if (icon == null) return;

        guideIconUIPool.Push(icon);
    }


    public EventGuideIconUI PopEventGuideIcon()
    {
        var icon = guideIconUIPool.Pop(null);
        if (icon == null) return null;

        return icon;
    }

    public async UniTask<EventGuideIconUI> CreateEventGuideIconPool()
    {
        var _prefab = await Managers.Resource.LoadAsync<GameObject>("sceneUI", "warnIconObj.prefab");
        var clone = GameObject.Instantiate(_prefab).GetComponent<EventGuideIconUI>();
        if (guideIconUIPool.Original != null)
        {
            return clone;
        }
        guideIconUIPool.Init(clone.GetComponent<EventGuideIconUI>(), "eventGuideIconUI");
        guideIconUIPool.Root.transform.SetParent(_root);
        return guideIconUIPool.Pop(null);
    }
    public async void PlayParticle(string particleName, Transform root)
    {
        await particlePool.PlayParticle(particleName, root);
    }



    #region Particle test
    public void ParticleNameValidTest()
    {
        PlayParticle("Highlight", null);
        PlayParticle("Highlight", null);
    }
    #endregion





    private class ParticlePool
    {
        private Dictionary<string, Pool<Particle>> particles = new Dictionary<string, Pool<Particle>>();
        private readonly static string particleBundleName = "Effect";



        public async UniTask<Particle> PlayParticle(string particleName, Transform root)
        {
            if (string.IsNullOrEmpty(particleName))
            {
                Debug.LogError("Particle name should be not null or empty.");
                return null;
            }

            if (particles.ContainsKey(particleName) == false || (IsParticleAssetLoading() == false && IsValidPool() == false))
            {
                bool isCreatedParticlePool = await CreateParticlePool(particleName);

                if (isCreatedParticlePool == false)
                {
                    return null;
                }

            }
            await UniTask.WaitWhile(IsParticleAssetLoading);

            if (particles.ContainsKey(particleName) == false)
            {
                Debug.LogError($"{particleName} doesn't exist.");
                return null;
            }

            Particle particle = GetParticle(particleName, root);
            particle.Play();

            return particle;





            bool IsParticleAssetLoading()
            {
                return particles.ContainsKey(particleName) && particles[particleName] == null;
            }
            bool IsValidPool()
            {
                return particles.ContainsKey(particleName) && particles[particleName] != null &&
                    particles[particleName].Original != null && particles[particleName].Root != null;
            }
        }
        private Particle GetParticle(string particleName, Transform parent)
        {
            Particle particle = particles[particleName].Pop(parent);

            particle.Release = (x) =>
            {
                ReleaseParticle(particleName, x);
            };
            particle.gameObject.OnDestroyAsObservable().Subscribe((unit) => ReleaseParticle(particleName, particle)).AddTo(particle.gameObject);

            return particle;
        }
        private async UniTask<bool> CreateParticlePool(string particleName)
        {
            if (particles.TryAdd(particleName, null) == false)
            {
                particles[particleName] = null;
            }

            GameObject particleAsset = await Managers.Resource.LoadAsyncParticle(particleBundleName, particleName + ".prefab");

            if (particleAsset == null)
            {
                Debug.LogError($"{particleName} doesn't exist.");
                particles.Remove(particleName);
                return false;
            }
            if (particleAsset.TryGetComponent(out Particle particleAssetComponent) == false)
            {
                Debug.LogError($"{particleAsset} doesn't have Particle component.");
                particles.Remove(particleName);
                return false;
            }

            particles[particleName] = new Pool<Particle>();
            Particle instance = Instantiate(particleAssetComponent);
            particles[particleName].Init(instance, $"{particleName}{nameof(Particle)}", 4);

            return true;
        }
        private void ReleaseParticle(string particleName, Particle particle)
        {
            particles[particleName].Push(particle);
        }
    }
}