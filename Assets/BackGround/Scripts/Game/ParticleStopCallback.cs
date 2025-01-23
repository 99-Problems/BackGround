using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticleStopCallback : MonoBehaviour
{
    private ObjectPool<GameObject> particles;



    private void OnParticleSystemStopped()
    {
        particles.Release(gameObject);
    }



    public void Initialize(ObjectPool<GameObject> particles)
    {
        this.particles = particles;
    }
}