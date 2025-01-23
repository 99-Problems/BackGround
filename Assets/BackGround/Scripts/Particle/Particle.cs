using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetKits.ParticleImage;
using System;
using Sirenix.OdinInspector;

public class Particle : MonoBehaviour
{
    private new ParticleSystem particleSystem = null;
    private ParticleImage particleImage = null;
    private Action<Particle> release;



    private void Awake()
    {
        if (TryGetComponent(out particleSystem) == false)
        {
            particleImage = GetComponent<ParticleImage>();
        }
    }
    private void OnParticleSystemStopped()
    {
        if (particleImage)
        {
            return;
        }

        release.Invoke(this);
    }
    private void OnDestroy()
    {
        if (particleImage)
        {
            particleImage.onParticleStop.RemoveListener(ReleaseInvoke);
        }
    }



    public void Play()
    {
        if (particleSystem)
        {
            particleSystem.Play();
        }
        else
        {
            particleImage.Play();
        }
    }
    public Action<Particle> Release
    {
        set
        {
            if (release != null)
            {
                return;
            }

            release = value;

            if (particleImage)
            {
                particleImage.onParticleStop.AddListener(ReleaseInvoke);
            }
        }
    }
    private void ReleaseInvoke()
    {
        release.Invoke(this);
    }
}