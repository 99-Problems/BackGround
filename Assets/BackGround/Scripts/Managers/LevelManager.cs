using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Linq;
using UnityEngine;
using Utils.Common;

public class LevelManager : MonoBehaviour
{
    [ShowInInspector]
    public Collider boundsCollider { get; protected set; }
    
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        if (boundsCollider == null)
        {
            var coll = GetComponent<Collider>();
            if (coll)
            {
                boundsCollider = coll;
            }
            else
            {
                foreach (var item in gameObject.Children())
                {
                    coll = item.GetComponent<Collider>();
                    if (coll == null)
                        continue;

                    boundsCollider = coll;
                    break;
                }
            }
        }
    }
}
