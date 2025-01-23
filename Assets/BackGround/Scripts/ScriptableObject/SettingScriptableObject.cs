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

    #region �÷� ����

    /*
        IVORY           f3ead6      ���̺���
        CREAM           ffeca5      ũ��
        YELLOW          ffe275      �����
        YELLOW2         f0c568      Ȳ���
        ORANGE          ff8b3d      ��Ȳ��
        ORANGE2         ffb77c      ��Ȳ��
        BROWN           a87c20      ����
        BROWN2          685a4a      ����2
        DARK_BROWN      382c1f      ������
        CYAN            94FFFF      �ϴû�
        CYAN2           bad1f7      Ǫ�� �ϴ�
        BLUE            4378d8      �Ķ���
        GREEN           248100      ���
        RED             ffb9b9      ����
        RED2            762828      ����2
        RED3            cc4747      ����3
        CORAL RED       ff4040      ���� ����
        GRAY            919191      ȸ��
        GRAYSCALE       bebebe      �׷��̽�����      
    */

    #endregion
    [LabelText("���� ����")]
    public Color CoralRed;

    [LabelText("����")]
    public Color Red;

    [LabelText("��鿬��")]
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
