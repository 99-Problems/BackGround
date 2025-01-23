using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Data;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Cysharp.Text;
using Data.Managers;

public class ResourceDownloadSceneInit : MonoBehaviour
{
    [Title("패치인포")]
    public GameObject patchInfoObj;
    public Button confirmBtn;
    public Text downloadDataText;
    public long downloadMB;

    public Slider downloadBarPer;
    public Text downloadProgressText;
    public Text downloadNotice;
    private bool bCheckDownload;
    private int labelIndex;
    private List<string> labelList = new List<string>();
    private IEnumerator coroutine;

    public long GetDownloadSize()
    {
        return downloadMB;
    }

    void Start()
    {
#if TEST_DOWNLOAD || !UNITY_EDITOR
        GetLabels();
        CheckAndUpdateAllResources();
#else
        LoadNextStep();
#endif
    }

    public void ShowPatchInfo()
    {
        patchInfoObj.SetActive(true);
        downloadDataText.text = string.Format("patch files : {0}MB ", downloadMB);
        confirmBtn.OnClickAsObservableThrottleFirst().Subscribe(_=>
        {
            patchInfoObj.SetActive(false);
            StartDownload();
        });
    }

    public void GetLabels()
    {
        var labels = new List<Define.AssetLabel>((Define.AssetLabel[])Enum.GetValues(typeof(Define.AssetLabel)));
        labelList.Clear();
        foreach (var label in labels)
        {
            labelList.Add(label.GetLabelString());
        }
    }
    private async void StartDownload()
    {
        labelIndex = 0;
        downloadProgressText.gameObject.SetActive(true);
        downloadBarPer.value = 0;
        downloadBarPer.gameObject.SetActive(true);

        {
            var download = Addressables.DownloadDependenciesAsync(labelList, Addressables.MergeMode.Union);
            download.Completed += OnDownloadComplete;
            if(coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            coroutine = DownloadProgress(download);
            StartCoroutine(coroutine);

            await download.Task;
        }
        LoadNextStep();

    }

    public IEnumerator DownloadProgress(AsyncOperationHandle download)
    {
        UpdateDownloadBar(download);
        yield return new WaitForFixedUpdate();
    }

    public void UpdateDownloadBar(AsyncOperationHandle download)
    {
        Debug.Log($"download: {download.PercentComplete * 100f} percent({(long)(downloadMB * download.PercentComplete)}MB) downloaded.");
        downloadBarPer.value = download.PercentComplete;
        downloadProgressText.text = string.Format("{0} / {1}", (long)(downloadMB * download.PercentComplete), downloadMB);
    }

    private void CheckAndUpdateAllResources()
    {
        Addressables.CheckForCatalogUpdates().Completed += OnCatalogUpdatesChecked;
    }

    private void OnCatalogUpdatesChecked(AsyncOperationHandle<List<string>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogsToUpdate = handle.Result;

            if (!catalogsToUpdate.IsNullOrEmpty())
            {
                Debug.Log("Catalog updates available. Updating catalogs...");
                Addressables.UpdateCatalogs(catalogsToUpdate).Completed += OnCatalogsUpdated;
            }
            else
            {
                Debug.Log("No catalog updates needed. Checking total download size...");
                CheckTotalDownloadSize();
            }
        }
        else
        {
            Debug.LogError("Failed to check for catalog updates: " + handle.OperationException);
        }
    }

    private void OnCatalogsUpdated(AsyncOperationHandle<List<IResourceLocator>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Catalogs updated successfully. Checking total download size...");
            // 카탈로그 업데이트 후 전체 리소스 다운로드 크기 확인
            CheckTotalDownloadSize();
        }
        else
        {
            Debug.LogError("Failed to update catalogs: " + handle.OperationException);
        }
    }

    private async void CheckTotalDownloadSize()
    {
        labelIndex = 0;
        //foreach (var label in labelList)
        {
            var download = Addressables.LoadResourceLocationsAsync(labelList.FirstOrDefault());
            download.Completed += OnResourceLocationsLoaded;
        }

        await UniTask.WaitWhile(() => !bCheckDownload/*labelIndex < labelList.Count*/);

        if (downloadMB > 0)
        {
            ShowPatchInfo();
        }
        else
        {
            LoadNextStep();
        }
    }

    private void OnResourceLocationsLoaded(AsyncOperationHandle<IList<IResourceLocation>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<IResourceLocation> allLocations = handle.Result;
            
            if (allLocations != null && allLocations.Count > 0)
            {
                Addressables.GetDownloadSizeAsync(allLocations).Completed += OnDownloadSizeChecked;
            }
            else
            {
                Debug.Log("No resource locations found.");
                labelIndex++;
            }
        }
        else
        {
            Debug.LogError("Failed to load resource locations: " + handle.OperationException);
        }
    }

    private void OnDownloadSizeChecked(AsyncOperationHandle<long> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            long downloadSize = handle.Result;

            if (downloadSize > 0)
            {
                //downloadSize => bytes
                downloadMB += downloadSize / (1024 * 1024);

                Debug.Log($"Total download size: {downloadSize / (1024 * 1024)} mb. Downloading now...");

            }
            else
            {
                Debug.Log($"{handle.DebugName} assets are up to date and already downloaded. Loading assets...");
            }
        }
        else
        {
            Debug.LogError("Failed to check total download size: " + handle.OperationException);
        }
        bCheckDownload = true;
        labelIndex++;
    }

    private void OnDownloadComplete(AsyncOperationHandle handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Download complete. Loading all assets now.");
        }
        else
        {
            Debug.LogError("Failed to download assets: " + handle.OperationException);
        }
        UpdateDownloadBar(handle);
        StopCoroutine(coroutine);
    }

    private async UniTaskVoid LoadNextStep()
    {
        Debug.Log("Assets are ready and loaded.");
       
        ResourceManager.OnLoadTextAsset.Subscribe((_) => UpdateScriptProgressUI(_)).AddTo(this);
        ResourceManager.OnLoadByteAsset.Subscribe((_) => UpdateScriptProgressUI(_)).AddTo(this);

        await Managers.String.LoadStringInfo(); //스트링 로딩

        downloadProgressText.text = "";
        downloadProgressText.gameObject.SetActive(true);
        downloadBarPer.value = 0;
        downloadBarPer.gameObject.SetActive(true);

        await Managers.Data.LoadScript(); //스크립트 로딩 
        SceneManager.LoadScene(Managers.Scene.GetSceneName(Define.Scene.Lobby));

    }

    private void Load()
    {

    }

    private void UpdateScriptProgressUI(string _scriptName)
    {
        if (downloadProgressText)
        {
            Managers.Data.Complete();
            string str = $"({Managers.Data.cntLoad} / {Managers.Data.maxCnt})";
            downloadProgressText.text = str;
            downloadBarPer.value =  Managers.Data.cntLoad / Managers.Data.maxCnt;
            Debug.Log($"{str} {_scriptName}");
        }
    }
}
