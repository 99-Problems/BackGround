using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArgGameObject : MonoBehaviour
{
    [SerializeField]
    public GameObject arg;

    [SerializeField] [ListDrawerSettings(Expanded = true)]
    private GameObject[] argArray;

    public T GetArgComponent<T>() where T : Component
    {
        return arg.GetComponent<T>();
    }

    public T GetArgComponent<T>(int index) where T : Component
    {
        return argArray[index].GetComponent<T>();
    }
}