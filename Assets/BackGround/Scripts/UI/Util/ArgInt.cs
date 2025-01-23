using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArgInt : MonoBehaviour
{

    [SerializeField] [ListDrawerSettings(Expanded = true)]
    private int[] intArray;


    public bool GetInt(int index, out int value)
    {
        value = -1;
        if (intArray.IsNullOrEmpty() == false)
        {
            if (intArray.Length > index)
            {
                value = intArray[index];
                return true;
            }
        }
        return false;
    }
}