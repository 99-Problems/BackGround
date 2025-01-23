using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using System.Linq;
using Data;
using Data.Managers;
using System.IO;
using UnityEditor.AddressableAssets.Build;
using System;
using UnityEditor.AddressableAssets.Build.DataBuilders;

public class PPAssetsHelper : EditorWindow
{
    [SerializeField] private string catalogName;
    private string folderPath = "Assets/BackGround/Prefabs"; // 기본 폴더 경로
    private AddressableAssetGroup targetGroup; // Addressable 그룹 선택을 위한 변수
    private string uiPath = "Assets/BackGround/ui";



    // 에디터 창을 열기 위한 메뉴 항목 추가
    [MenuItem("BackGround/Addressable Helper")]
    public static void ShowWindow()
    {
        GetWindow<PPAssetsHelper>("Addressable Helper");
    }

    // 에디터 창에 UI를 표시
    private void OnGUI()
    {
        GUILayout.Label("Addressable Helper Tool", EditorStyles.boldLabel);

        // 폴더 경로 입력 필드
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if(targetGroup == null)
        {
            targetGroup = AddressableAssetSettingsDefaultObject.GetSettings(false).DefaultGroup;
        }
        // Addressable 그룹 선택 필드
        targetGroup = (AddressableAssetGroup)EditorGUILayout.ObjectField(
            "Target Addressable Group",
            targetGroup,
            typeof(AddressableAssetGroup),
            false
        );
        DrawCatalogNameField();

        // 폴더 내 파일들을 Addressable로 추가하는 버튼
        if (GUILayout.Button("Add Files to Addressables"))
        {
            AddFilesToAddressables();
            BuildAddressables();
        }
        if(GUILayout.Button("Addressables Build"))
        {
            BuildAddressables();
        }
    }



    private void DrawCatalogNameField()
    {
        AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
        Undo.RecordObject(addressableAssetSettings, "Change catalog name");
        addressableAssetSettings.OverridePlayerVersion =
            EditorGUILayout.TextField("Catalog name", addressableAssetSettings.OverridePlayerVersion);
    }
    // 폴더 내 파일들을 Addressable로 추가하는 함수
    private void AddFilesToAddressables()
    {
        if (string.IsNullOrEmpty(folderPath) || targetGroup == null)
        {
            Debug.LogError("Please specify a valid folder path and Addressable Group.");
            return;
        }

        // 폴더 내의 모든 파일과 폴더의 GUID를 가져옴
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { folderPath });
        var UIGuids = AssetDatabase.FindAssets("", new[] { uiPath });
        UIGuids = UIGuids
            .Where(guid => !AssetDatabase.GUIDToAssetPath(guid).Contains($"{uiPath}/font"))
            .ToArray();
        var fontGuids = AssetDatabase.FindAssets("", new[] {$"{uiPath}/font"});

        if (assetGuids.Length == 0)
        {
            Debug.LogWarning($"No files found in folder: {folderPath}");
            return;
        }
        else if(UIGuids.Length == 0)
        {
            Debug.LogWarning($"No files found in folder: {uiPath}");
            return;
        }
        else if(fontGuids.Length == 0)
        {
            Debug.LogWarning($"No files found in folder: {uiPath}/font");
            return;
        }

        // Addressable 설정 가져오기
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);

        // 폴더 내의 파일들을 Addressable로 등록
        foreach (string assetGUID in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // 폴더는 제외하고 파일만 추가
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // 에셋을 Addressable 그룹에 추가
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // 파일 경로를 주소로 사용
                entry.address = assetPath;

                // JSON 파일에 'script' 라벨 추가
                if (assetPath.EndsWith(".json"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Script.GetLabelString()}", true); // 'Script' 라벨 추가
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Script'");
                }
                // 파일 이름에 'Popup'이 포함된 프리팹에 'Popup' 라벨 추가
                else if (assetPath.EndsWith(".prefab") && assetPath.Contains("Popup"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Popup.GetLabelString()}", true); // 'Popup' 라벨 추가
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Popup'");
                }
                else if (Path.GetDirectoryName(assetPath).EndsWith("Effect") && assetPath.EndsWith(".prefab"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Particle.GetLabelString()}", true);
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Particle'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.Default.GetLabelString()}", true); // 'default' 라벨 추가
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address}");
                }
            }
        }
        foreach (string assetGUID in UIGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // 폴더는 제외하고 파일만 추가
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // 에셋을 Addressable 그룹에 추가
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // 파일 경로를 주소로 사용
                entry.address = assetPath;
                if (assetPath.EndsWith(".mat"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Material.GetLabelString()}", true); // 'Mat' 라벨 추가
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Mat'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.UI.GetLabelString()}", true);
                }
            }
        }

        foreach (string assetGUID in fontGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // 폴더는 제외하고 파일만 추가
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // 에셋을 Addressable 그룹에 추가
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // 파일 경로를 주소로 사용
                entry.address = assetPath;
                if (assetPath.EndsWith(".mat"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Material.GetLabelString()}", true); // 'Mat' 라벨 추가
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Mat'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.Font.GetLabelString()}", true);
                }
            }
        }

        // 변경 사항 저장
        AssetDatabase.SaveAssets();
        Debug.Log("Addressables updated successfully.");
    }
    private void BuildAddressables()
    {
        AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
        IDataBuilder dataBuilder = addressableAssetSettings.GetDataBuilder(4);
        AddressablesDataBuilderInput addressablesDataBuilderInput = new AddressablesDataBuilderInput(addressableAssetSettings);
        dataBuilder.BuildData<AddressablesPlayerBuildResult>(addressablesDataBuilderInput);
    }
}

//public class AddressableBuildPreprocessor : IPreprocessBuildWithReport
//{
//    public int callbackOrder => 0;

//    public void OnPreprocessBuild(BuildReport report)
//    {
//        AddressableAssetSettings.BuildPlayerContent();
//        Debug.Log("Addressables built before Unity build.");
//    }
//}
