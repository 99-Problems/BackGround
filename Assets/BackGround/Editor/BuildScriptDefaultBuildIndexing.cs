using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.Build.Pipeline;
using UnityEditor.AddressableAssets.Build;
using System.IO;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "BuildScriptDefaultBuildIndexing.asset", menuName = "Addressables/Content Builders/BuildScriptDefaultBuildIndexing")]
public class BuildScriptDefaultBuildIndexing : BuildScriptPackedMode
{
    private static readonly string separator = "_";



    protected override string ConstructAssetBundleName(AddressableAssetGroup assetGroup, BundledAssetGroupSchema schema, BundleDetails info, string assetBundleName)
    {
        string bundleName = assetBundleName;

        if (assetGroup != null)
        {
            string groupName = assetGroup.Name.Replace(" ", "").Replace('\\', '/').Replace("//", "/").ToLower();
            bundleName = groupName + bundleName;

            string path = schema.BuildPath.GetValue(schema.Group.Settings);
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            string currentDate = DateTime.Now.ToString("yyMMdd");
            int index = GetMinIndex(directoryInfo, bundleName);

            bundleName = GetParsedBundleName(bundleName, currentDate, index);
        }

        string bundleNameWithHashing = BuildUtility.GetNameWithHashNaming(schema.BundleNaming, info.Hash.ToString(), bundleName);
        //For no hash, we need the hash temporarily for content update purposes.  This will be stripped later on.
        if (schema.BundleNaming == BundledAssetGroupSchema.BundleNamingStyle.NoHash)
        {
            bundleNameWithHashing = bundleNameWithHashing.Replace(".bundle", "_" + info.Hash.ToString() + ".bundle");
        }

        return bundleNameWithHashing;
    }
    private string GetParsedBundleName(string bundleName, string date, int index)
    {
        string bundleNameWithoutExtension = bundleName.Replace(".bundle", string.Empty);

        return bundleNameWithoutExtension + separator + date + separator + index.ToString("000") + ".bundle";
    }
    private int GetMinIndex(DirectoryInfo directoryInfo, string bundleName)
    {
        FileInfo[] sameNameAndParsedBundleFiles =
            directoryInfo.GetFiles($"*{bundleName.Replace(".bundle", string.Empty)}*").
            Where(IsParsedBundleFile).
            OrderBy(GetIndexFromParsedBundleFile).ToArray();
        int index = 0;

        foreach (var item in sameNameAndParsedBundleFiles)
        {
            int fileIndex = GetIndexFromParsedBundleFile(item);

            if (fileIndex == index)
            {
                index++;
            }
        }

        return index;





        bool IsParsedBundleFile(FileInfo bundleFileInfo)
        {
            string[] splited = bundleFileInfo.Name.Split(separator);

            return splited.Length >= 3 && int.TryParse(splited[splited.Length - 3], out int date) && int.TryParse(splited[splited.Length - 2], out int index);
        }
        int GetIndexFromParsedBundleFile(FileInfo bundleFileInfo)
        {
            string[] splited = bundleFileInfo.Name.Split(separator);

            return int.Parse(splited[splited.Length - 2]);
        }
    }
    public override string Name { get => "Indexed Default Build Script"; }
}