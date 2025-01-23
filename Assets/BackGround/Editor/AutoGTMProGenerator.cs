using UnityEditor;
using UnityEngine;
using TMPro;
using NUnit.Framework.Internal;

[InitializeOnLoad]
public class AutoGTMProGenerator
{
    static AutoGTMProGenerator()
    {
        ObjectFactory.componentWasAdded += OnComponentWasAdded;
    }

    private static void OnComponentWasAdded(Component component)
    {
        if (component is TextMeshProUGUI)
        {
            var text = component.gameObject.GetOrAddComponent<GTMPro>();
            text.fontType = Data.Define.EFONT_TYPE.Default;
        }
    }
}
