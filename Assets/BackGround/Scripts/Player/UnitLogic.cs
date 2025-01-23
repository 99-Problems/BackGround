using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Data;
using UnityEngine.AI;
using TMPro;
using static UnityEngine.UI.GridLayoutGroup;
using UniRx.Triggers;
using UniRx;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using Unity.Burst.CompilerServices;
using Data.Managers;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using static Data.Define;

public struct UnitBaseData
{
    public int unitID;
    public float speed;
    public UnitBaseData(UnitInfoScript unitInfo)
    {
        unitID = unitInfo.unitID;
        speed = unitInfo.speed;
    }
}


public class UnitLogic : MonoBehaviour
{
    [SerializeField]
    internal UnitLogicStat stat;

    public int curIndex;

    [ShowInInspector]
    public UnitInfoScript unitInfo;
    public ReactiveProperty<EUNIT_STATE> state = new ReactiveProperty<EUNIT_STATE>(EUNIT_STATE.Idle);
    public IObservable<EUNIT_STATE> unitState => state.AsObservable();
    private protected UnitBaseData unitBaseData;
    private protected InGamePlayerInfo owner;


    [HideInInspector]
    [NonSerialized]
    public NavMeshPath path;
    private NavMeshAgent navMesh;
    public CapsuleCollider sphere { get; private set; }

    public Transform target;
    [NonSerialized]
    public Vector3 targetPosition = Vector3.positiveInfinity;
    protected bool initialized;
    public bool isMove;
    public float minRotateDistance = 2f;
    public ObstacleAvoidanceType avoidType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
    private float avoidRadius = 2.5f;

    private float rotateSpeed = 240f;

    public string assetPath => unitInfo.assetPath;
    public string prefabName => unitInfo.prefabName;
    public int UnitID => unitBaseData.unitID;

    public (Define.ETRIGGER_TYPE targetType, int index) destination { get; private set; }
    public Define.EVENT_TYPE curEventType = Define.EVENT_TYPE.NONE;

    public float remainWorkTime;

    private Animator anim;

    private void Awake()
    {
        var radius = 0.5f;
            
        sphere = gameObject.GetOrAddComponent<CapsuleCollider>();
        //sphere.radius = radius * 0.5f;
        //sphere.height = 1.5f;
        //sphere.center = new Vector3(0, 0.5f, -0.1f);
        sphere.material = new PhysicMaterial
        {
            dynamicFriction = 0,
            staticFriction = 0,
        };

        var rigidBody = gameObject.GetOrAddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;
        sphere.enabled = false;
        anim = GetComponent<Animator>();
    }

    //protected virtual void Start()
    //{
    //    Init();
    //    Reset();
    //}

    //[Button]
    public virtual void Init(UnitBaseData _unitBaseData, InGamePlayerInfo _owner)
    {
        stat = new UnitLogicStat();
        path = new NavMeshPath();

        sphere.enabled = true;
        initialized = true;
        unitBaseData = _unitBaseData;
        owner = _owner;

        SetOnNavMesh();
    }

    public virtual void Reset()
    {
        stat.Set();
        state.Value = Define.EUNIT_STATE.Idle;
        remainWorkTime = 0f;
        destination = (ETRIGGER_TYPE.None, -1);
        target = null;
        curEventType = Define.EVENT_TYPE.NONE;
    }

    public void Clear()
    {
        SetState(EUNIT_STATE.Idle);
        ClearDestination();
    }

    public void ClearDestination()
    {
        destination = (ETRIGGER_TYPE.None, -1);
        navMesh.isStopped = true;
    }


#if LOG_ENABLE
    private void Update()
    {
        if (target!=null && isMove)
        {
            UnitMove(target);
            
        }
    }
#endif

    public virtual void FrameMove(float _deltaTime)
    {
        navMesh.isStopped = true;
        if (!initialized)
            return;

        if (Managers.Time.GetGameSpeed() <= 0f)
        {
            return;
        }

        switch (state.Value)
        {   
            case Define.EUNIT_STATE.Idle:
               
                break;
            case Define.EUNIT_STATE.Move:
                UnitMove(target);
                break;
            case Define.EUNIT_STATE.Work:
                break;
            case Define.EUNIT_STATE.Wait:
                break;
            default:
                break;
        }

    }


    public void UnitMove(Transform _moveTarget)
    {
        if (_moveTarget == null)
            return;
        navMesh.isStopped = false;
        navMesh.SetDestination(_moveTarget.position);

        return;
    }

    public void RotateUnit(Define.EUNIT_DIRECTION _dir)
    {
        var direction = _dir.GetDirecton();
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void SetState(Define.EUNIT_STATE _state)
    {
        state.Value = _state;
        anim.SetBool("Idle", _state == EUNIT_STATE.Idle || _state == EUNIT_STATE.Wait);
        anim.SetBool("Move", _state == EUNIT_STATE.Move);
        anim.SetBool("Pickup", _state == EUNIT_STATE.Work);
    }

    public async UniTaskVoid SetOnNavMesh()
    {
        NavMeshHit hit;
        var range = 100.0f;
        var isPlace = NavMesh.SamplePosition(transform.position, out hit, range, NavMesh.AllAreas);
        if (isPlace)
        {
            transform.position = hit.position;
            //Debug.ColorLog($"네브메쉬에 없어서 이동 \npos : {hit.position}");
        }
        else
        {
            Debug.ColorLog($"네브메쉬 이동 실패", Color.red);
        }
        if(navMesh == null)
        {
            navMesh = gameObject.GetOrAddComponent<NavMeshAgent>();
            navMesh.enabled = true;
            navMesh.baseOffset = 0.15f;
            navMesh.obstacleAvoidanceType = avoidType;
            navMesh.angularSpeed = rotateSpeed;
            navMesh.speed = unitBaseData.speed;
            navMesh.acceleration = 1000;
            navMesh.stoppingDistance = 0.1f;
            navMesh.autoBraking = false;
            navMesh.updateRotation = false;
            navMesh.destination = transform.position;
            navMesh.isStopped = true;
            navMesh.radius = avoidRadius;
            navMesh.updateRotation = true;
            //Debug.ColorLog($"네브메쉬 세팅후 SetDestination", Color.green);
        }
    }

    public void SetTarget(Transform _tranform)
    {
        target = _tranform;
    }

    public void SetDestination(Define.ETRIGGER_TYPE type, int index)
    {
        destination = (type, index);
    }

    public bool IsTarget(Define.ETRIGGER_TYPE type, int index)
    {
        return type == destination.targetType && index == destination.index;
    }

    public void SetData(UnitBaseData _unitInitialData)
    {
        unitBaseData = _unitInitialData;
    }

    private void OnTriggerEnter(Collider other)
    {
        var trigger = other.GetComponent<BaseTrigger>();
        if (trigger != null) 
        {
            trigger.Enter(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var trigger = other.GetComponent<BaseTrigger>();
        if(trigger && state.Value == EUNIT_STATE.Move)
        {
            if(IsTarget(trigger.triggerType,trigger.GetIndex))
            {
                trigger.Enter(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
