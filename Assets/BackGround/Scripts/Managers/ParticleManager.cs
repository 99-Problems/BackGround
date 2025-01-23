using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using AssetKits.ParticleImage;
using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ParticleManager
{
    private Dictionary<string, ObjectPool<GameObject>> particles = new Dictionary<string, ObjectPool<GameObject>>();
    private List<GameObject> particleAssets;
    private Dictionary<GameObject, string> particlesToRelease = new Dictionary<GameObject, string>();
    private Transform particlePoolRoot;
    private int particlesCount;
    private bool isCompleteLoading = false;



    public void Init()
    {
        LoadParticleAsset();
        particlePoolRoot = new GameObject("ParticlePool").transform;
        UnityEngine.Object.DontDestroyOnLoad(particlePoolRoot);
    }



    
    private void LoadParticleAsset()
    {
        AsyncOperationHandle asyncOperationHandle = Addressables.LoadResourceLocationsAsync("Particle");
        asyncOperationHandle.Completed += LoadParticleLocations;
    }
    private void LoadParticleLocations(AsyncOperationHandle locationAsyncOperationHandle)
    {
        IList<IResourceLocation> locations = locationAsyncOperationHandle.Result as IList<IResourceLocation>;
        particlesCount = locations.Count;
        particleAssets = new List<GameObject>(locations.Count);
        foreach (var item in locations)
        {
            AsyncOperationHandle asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(item);
            asyncOperationHandle.Completed += LoadParticleAsset;
        }
    }
    private void LoadParticleAsset(AsyncOperationHandle particleAssetAsyncOperationHandle)
    {
        GameObject particleAsset = particleAssetAsyncOperationHandle.Result as GameObject;
        particleAssets.Add(particleAsset);
        particles.Add(particleAsset.name, new ObjectPool<GameObject>(createFunc: CreateFunc, actionOnGet: ActionOnGet, actionOnRelease: ActionOnRelease,
            actionOnDestroy: ActionOnDestroy));

        if (particleAssets.Count == particlesCount)
        {
            isCompleteLoading = true;
            Debug.Log("complete loading");
        }



        
        
        GameObject CreateFunc()
        {
            GameObject instantiatedParticle = UnityEngine.Object.Instantiate(particleAsset, particlePoolRoot);
            ParticleStopCallback particleStopCallback = instantiatedParticle.AddComponent<ParticleStopCallback>();
            particleStopCallback.Initialize(particles[particleAsset.name]);

            if (particleAsset.TryGetComponent(out ParticleImage particleImage))
            {
                ParticleImage instantiatedParticleImage = instantiatedParticle.GetComponent<ParticleImage>();
                instantiatedParticleImage.onParticleStop.AddListener(() =>
                {
                    particles[particleAsset.name].Release(instantiatedParticleImage.gameObject);
                });

                return instantiatedParticle;
            }
            else if (particleAsset.TryGetComponent(out ParticleSystem particleSystem))
            {
                return instantiatedParticle;
            }
            else
            {
                Debug.LogError($"{particleAsset.name} is not particle.");
                return null;
            }
        }
        void ActionOnGet(GameObject particle)
        {
            particle.SetActive(true);
            particlesToRelease.Add(particle, particleAsset.name);

            if (particle.TryGetComponent(out ParticleImage particleImage))
            {
                particleImage.Play();
            }
            else if (particle.TryGetComponent(out ParticleSystem particleSystem))
            {
                particleSystem.Play();
            }
            else
            {
                Debug.LogError("No particle in this GameObject.");
            }
        }
        void ActionOnRelease(GameObject particle)
        {
            particlesToRelease.Remove(particle);
            particle.SetActive(false);
        }
        void ActionOnDestroy(GameObject particle)
        {
            UnityEngine.Object.Destroy(particle);
        }
    }
    public void PlayParticleSystem(string particleName, Vector3 position)
    {
        if (TryGetParticleFromPool(particleName, out GameObject particleSystem) == false)
        {
            return;
        }

        particleSystem.transform.position = position;
    }
    public void PlayParticleSystem(string particleName, Transform root, Vector3 position)
    {
        if (TryGetParticleFromPool(particleName, out GameObject particleSystem) == false)
        {
            return;
        }

        particleSystem.transform.SetParent(root);
        particleSystem.transform.position = position;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="particleName"></param>
    /// <param name="root">It should be UI object.</param>
    public void PlayParticleImage(string particleName, Transform root)
    {
        if (TryGetParticleImage(particleName, root, out ParticleImage particleImage) == false)
        {
            return;
        }

        particleImage.transform.SetParent(root);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="particleName"></param>
    /// <param name="root">It should be UI object.</param>
    /// <param name="anchoredPosition"></param>
    public void PlayParticleImage(string particleName, Transform root, Vector2 anchoredPosition)
    {
        if (TryGetParticleImage(particleName, root, out ParticleImage particleImage) == false)
        {
            return;
        }

        particleImage.transform.SetParent(root);
        particleImage.rectTransform.anchoredPosition = anchoredPosition;
    }
    private bool TryGetParticlePool(string particleName, out ObjectPool<GameObject> pool)
    {
        if (particles.TryGetValue(particleName, out pool) == false)
        {
            Debug.LogError($"Can't find {particleName} pool.");
            return false;
        }

        return true;
    }
    private bool TryGetParticleFromPool(string particleName, out GameObject particle)
    {
        if (TryGetParticlePool(particleName, out ObjectPool<GameObject> pool) == false)
        {
            particle = null;
            return false;
        }

        particle = pool.Get();
        return true;
    }
    private bool TryGetParticleImage(string particleName, Transform root, out ParticleImage particleImage)
    {
        if (TryGetParticleFromPool(particleName, out GameObject particleObject) == false)
        {
            particleImage = null;
            return false;
        }

        if (particleObject.TryGetComponent(out particleImage) == false)
        {
            Debug.LogError($"{particleObject.name} doesn't have ParticleImage.");
            particleImage = null;
            return false;
        }

        if (root == null || root.TryGetComponent(out RectTransform rectTransform) == false)
        {
            Debug.LogWarning("Root should be canvas or child of them.");
        }

        return true;
    }
    public void ReleaseAll()
    {
        Dictionary<GameObject, string> copiedParticlesToRelease = new Dictionary<GameObject, string>(particlesToRelease.Count);
        foreach (var item in particlesToRelease)
        {
            copiedParticlesToRelease.Add(item.Key, item.Value);
        }

        foreach (var item in copiedParticlesToRelease)
        {
            if (particles.TryGetValue(item.Value, out ObjectPool<GameObject> pool))
            {
                pool.Release(item.Key);
            }
        }
    }
    public void ClearAll()
    {
        Clear(particleAssets.Select((x) => x.name).ToArray());
    }
    public void Clear(params string[] particleNames)
    {
        foreach (var item in particleNames)
        {
            if (TryGetParticlePool(item, out ObjectPool<GameObject> pool))
            {
                pool.Clear();
            }
        }
    }
}