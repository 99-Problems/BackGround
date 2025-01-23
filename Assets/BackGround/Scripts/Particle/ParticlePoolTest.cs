using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ParticlePoolTest : MonoBehaviour
{
    [Button]
    private void ParticleNameValidTest()
    {
        Managers.Pool.ParticleNameValidTest();
    }
}