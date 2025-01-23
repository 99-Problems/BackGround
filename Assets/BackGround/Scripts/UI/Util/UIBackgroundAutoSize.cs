using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BGImgFileSizeType
{
    BG_2048x2048 = 0,
    BG_1024x1024,
    BG_512x512,
}



public class UIBackgroundAutoSize : MonoBehaviour
{
    public BGImgFileSizeType bgimgType = BGImgFileSizeType.BG_2048x2048;

    public float originalWidth = 720;
    public float originalHeghit = 1600;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button]
    void SetUISize()
    {
        float originalSize = 2048;
        switch (bgimgType)
        {
            case BGImgFileSizeType.BG_2048x2048:
                originalSize = 2048;
                break;
            case BGImgFileSizeType.BG_1024x1024:
                originalSize = 1024;
                break;
            case BGImgFileSizeType.BG_512x512:
                originalSize = 512;
                break;
            default:
                break;
        }

        transform.localScale = new Vector3(originalWidth / originalSize, originalHeghit / originalSize, 1);
    }
}
