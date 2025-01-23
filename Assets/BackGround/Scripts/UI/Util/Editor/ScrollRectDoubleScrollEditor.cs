using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(ScrollRectDoubleScroll))]
public class ScrollRectDoubleScrollEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }
}
