using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Sirenix.OdinInspector;
using UnityEditor;
[CreateAssetMenu(fileName = "BackGroundSetting", menuName = "Create BackGroundSetting")]
public class SettingScriptableObject : ScriptableObject
{
    private const string SettingFolderPath = "Assets/Resources";
    private const string settingFilePath = "Assets/Resources/BackGroundSetting.asset";

    #region 컬러 참조

    /*
        IVORY           f3ead6      아이보리
        CREAM           ffeca5      크림
        YELLOW          ffe275      노란색
        YELLOW2         f0c568      황토색
        ORANGE          ff8b3d      주황색
        ORANGE2         ffb77c      주황색
        BROWN           a87c20      갈색
        BROWN2          685a4a      갈색2
        DARK_BROWN      382c1f      진갈색
        CYAN            94FFFF      하늘색
        CYAN2           bad1f7      푸른 하늘
        BLUE            4378d8      파란색
        GREEN           248100      녹색
        RED             ffb9b9      빨강
        RED2            762828      빨강2
        RED3            cc4747      빨강3
        CORAL RED       ff4040      밝은 빨강
        GRAY            919191      회색
        GRAYSCALE       bebebe      그레이스케일      
    */

    #endregion
    [LabelText("밝은 빨강")]
    public Color CoralRed;

    [LabelText("빨강")]
    public Color Red;

    [LabelText("흑백연출")]
    public Material grayScale;
    public Color grayScaleColor;

    private static SettingScriptableObject instance;

    public static SettingScriptableObject Instance
    {
        get
        {
            if (instance != null)
                return instance;
            instance = Resources.Load<SettingScriptableObject>("BackGroundSetting");
#if UNITY_EDITOR
            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFolderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                instance = AssetDatabase.LoadAssetAtPath<SettingScriptableObject>(settingFilePath);
                if (instance == null)
                {
                    instance = CreateInstance<SettingScriptableObject>();
                    AssetDatabase.CreateAsset(instance, settingFilePath);
                }
            }
#endif
            return instance;
        }
    }
}
