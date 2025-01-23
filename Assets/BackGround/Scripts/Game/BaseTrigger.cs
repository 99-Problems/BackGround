using Cysharp.Threading.Tasks;
using Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTrigger : MonoBehaviour
{
    [ShowInInspector]
    [ReadOnly]
    protected bool isDone;

    public Define.ETRIGGER_TYPE triggerType;
    public BoxCollider collider;

    [Title("ColliderInfo")]
    [Tooltip("사이즈 조절")]
    public bool useSizing;
    [ShowIf("useSizing")]
    [PropertySpace(0,20)]
    public Vector3 size;

    [SerializeField]
    protected int index;
    public int GetIndex => index;
    public virtual bool IsDone => isDone;

    public virtual void Awake()
    {
        if (collider)
        {
            //collider.enabled = false;
            collider.isTrigger = true;
            if(useSizing)
            {
                collider.size = size;
                collider.center= new Vector3(0,size.y/2,0);
            }
        }
    }

    public virtual UniTask Load()
    {
        return UniTask.CompletedTask;
    }

    public abstract void Enter(UnitLogic _unit);

    public abstract void OpenInfo(List<UnitLogic> units);

    public abstract List<int> GetUnitList();

    public virtual void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Managers.isDrawGizmos == false)
            return;
#endif
        if (collider)
        {
            var color = Color.blue;
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawCube(transform.position + collider.center, collider.size);
            color = Color.yellow;
        }
    }

    public virtual void SetIndex(int _index)
    {
        index = _index;
    }
}
