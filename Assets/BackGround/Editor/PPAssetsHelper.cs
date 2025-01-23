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
    private string folderPath = "Assets/BackGround/Prefabs"; // �⺻ ���� ���
    private AddressableAssetGroup targetGroup; // Addressable �׷� ������ ���� ����
    private string uiPath = "Assets/BackGround/ui";



    // ������ â�� ���� ���� �޴� �׸� �߰�
    [MenuItem("BackGround/Addressable Helper")]
    public static void ShowWindow()
    {
        GetWindow<PPAssetsHelper>("Addressable Helper");
    }

    // ������ â�� UI�� ǥ��
    private void OnGUI()
    {
        GUILayout.Label("Addressable Helper Tool", EditorStyles.boldLabel);

        // ���� ��� �Է� �ʵ�
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if(targetGroup == null)
        {
            targetGroup = AddressableAssetSettingsDefaultObject.GetSettings(false).DefaultGroup;
        }
        // Addressable �׷� ���� �ʵ�
        targetGroup = (AddressableAssetGroup)EditorGUILayout.ObjectField(
            "Target Addressable Group",
            targetGroup,
            typeof(AddressableAssetGroup),
            false
        );
        DrawCatalogNameField();

        // ���� �� ���ϵ��� Addressable�� �߰��ϴ� ��ư
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
    // ���� �� ���ϵ��� Addressable�� �߰��ϴ� �Լ�
    private void AddFilesToAddressables()
    {
        if (string.IsNullOrEmpty(folderPath) || targetGroup == null)
        {
            Debug.LogError("Please specify a valid folder path and Addressable Group.");
            return;
        }

        // ���� ���� ��� ���ϰ� ������ GUID�� ������
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

        // Addressable ���� ��������
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);

        // ���� ���� ���ϵ��� Addressable�� ���
        foreach (string assetGUID in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // ������ �����ϰ� ���ϸ� �߰�
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // ������ Addressable �׷쿡 �߰�
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // ���� ��θ� �ּҷ� ���
                entry.address = assetPath;

                // JSON ���Ͽ� 'script' �� �߰�
                if (assetPath.EndsWith(".json"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Script.GetLabelString()}", true); // 'Script' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Script'");
                }
                // ���� �̸��� 'Popup'�� ���Ե� �����տ� 'Popup' �� �߰�
                else if (assetPath.EndsWith(".prefab") && assetPath.Contains("Popup"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Popup.GetLabelString()}", true); // 'Popup' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Popup'");
                }
                else if (Path.GetDirectoryName(assetPath).EndsWith("Effect") && assetPath.EndsWith(".prefab"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Particle.GetLabelString()}", true);
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Particle'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.Default.GetLabelString()}", true); // 'default' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address}");
                }
            }
        }
        foreach (string assetGUID in UIGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // ������ �����ϰ� ���ϸ� �߰�
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // ������ Addressable �׷쿡 �߰�
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // ���� ��θ� �ּҷ� ���
                entry.address = assetPath;
                if (assetPath.EndsWith(".mat"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Material.GetLabelString()}", true); // 'Mat' �� �߰�
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

            // ������ �����ϰ� ���ϸ� �߰�
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // ������ Addressable �׷쿡 �߰�
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // ���� ��θ� �ּҷ� ���
                entry.address = assetPath;
                if (assetPath.EndsWith(".mat"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Material.GetLabelString()}", true); // 'Mat' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Mat'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.Font.GetLabelString()}", true);
                }
            }
        }

        // ���� ���� ����
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
